using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CrocsItems.Items.Reds
{
    [ConfigSection("Items :: Crocs Echo Wave")]
    public class CrocsEchoWave : ItemBase<CrocsEchoWave>
    {
        public override string ItemName => "Crocs Echo Wave";

        public override string ItemLangTokenName => "CROCS_ECHO_WAVE";

        public override string ItemPickupDesc => "Sprinting builds up movement speed that can be discharged for massive impact damage.";

        public override string ItemFullDescription => "Sprinting builds up to <style=cIsUtility>100% movement speed</style>. <style=cIsDamage>Ramming</style> into an enemy while sprinting deals up to <style=cIsDamage>3000%</style> <style=cStack>(+3000% per stack)</style> <style=cIsDamage>damage</style> based on movement speed.";

        public override string ItemLore => "This item should have lore.";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.Utility, ItemTag.Damage];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("CrocsEchoWaveHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texCrocsEchoWave.png");

        public override bool IsCroc => true;

        public static BuffDef speedBuff;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpBuff();
        }

        public void SetUpBuff()
        {
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.isHidden = false;
            speedBuff.isDebuff = false;
            speedBuff.canStack = true;
            speedBuff.buffColor = Color.white;
            speedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("2508a4654959d334aaca7d4321922642").WaitForCompletion();
            speedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            speedBuff.ignoreGrowthNectar = false;
            speedBuff.isDOT = false;
            speedBuff.isCooldown = false;
            speedBuff.name = "Crocs Echo Wave Movement Speed - 1% Per";

            ContentAddition.AddBuffDef(speedBuff);
        }

        public void SetUpVFX()
        {
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += CalcSpeedBoost;
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;

        }

        private void OnInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            body.AddItemBehavior<CrocsEchoWaveController>(GetCount(body));
        }

        private void CalcSpeedBoost(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                var speedBoostBuffCount = sender.GetBuffCount(speedBuff);
                args.moveSpeedMultAdd += speedBoostBuffCount * 0.01f;
            }
        }
    }

    public class CrocsEchoWaveController : CharacterBody.ItemBehavior
    {
        public float timer;
        public float buffChangeInterval = 0.15f;
        public int buffCountToChange = 2;

        public int maxBuffCount = 100;
        public int minBuffCount = 100;
        public int buffCountAfterImpact = 25;
        public int buffCount;
        public float minImpactDamage = 10f;
        public float maxImpactDamage = 30f;

        public OverlapAttack attackerOverlap;

        public ModelLocator modelLocator;
        public Transform modelTransform;
        public GameObject hitBoxObject;
        public HitBoxGroup hitBoxGroup;
        public HitBox hitBox;
        public float collisionDisableTime = 0.5f;
        public int cachedLayer;
        public bool successfullyHit = false;

        public void Start()
        {
            cachedLayer = gameObject.layer;

            modelLocator = GetComponent<ModelLocator>();
            modelTransform = modelLocator?.modelTransform;
            if (modelTransform && hitBoxObject == null)
            {
                hitBoxObject = new("Croc Echo Waves HitBox")
                {
                    layer = LayerIndex.defaultLayer.intVal
                };

                hitBoxObject.transform.localScale = new Vector3(4f, 8f, 4f);

                hitBox = hitBoxObject.AddComponent<HitBox>();
                hitBoxGroup = hitBoxObject.AddComponent<HitBoxGroup>();
                hitBoxGroup.groupName = "CrocEchoWavesHitBox";
                hitBoxGroup.hitBoxes = [hitBox];
            }
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer >= buffChangeInterval)
            {
                if (body.isSprinting)
                {
                    buffCount += buffCountToChange;
                    buffCount = Mathf.Min(buffCount, maxBuffCount);
                    if (buffCount >= minBuffCount)
                    {
                        CheckImpact();
                    }
                }
                else
                {
                    buffCount -= buffCountToChange;
                    buffCount = Mathf.Max(0, buffCount);
                }

                body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, buffCount);

                timer = 0f;
            }
        }

        public void CheckImpact()
        {
            var sprintingSpeed = 7f * 1.45f;
            var scaledDamage = Util.Remap(body.moveSpeed, sprintingSpeed, sprintingSpeed * 4f, minImpactDamage * stack, maxImpactDamage * stack); // about 50% effectiveness with this item alone -- should be doing nearly 2000% impact damage
            var finalDamage = scaledDamage;
            attackerOverlap = new()
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                damage = body.damage * finalDamage,
                forceVector = Vector3.zero,
                pushAwayForce = 4000f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                // impactSound = null,
                procCoefficient = 1f,
                isCrit = body.RollCrit(),
                hitBoxGroup = hitBoxObject.GetComponent<HitBoxGroup>()
            };

            hitBoxObject.transform.forward = body.inputBank.moveVector;
            hitBoxObject.transform.position = modelTransform.position;

            successfullyHit = attackerOverlap.Fire();
            if (successfullyHit)
            {
                // Main.ModLogger.LogError("attack overlap fire is true");
                buffCount = buffCountAfterImpact;
                body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, buffCountAfterImpact);
                // body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, 0);
                SpawnVFX();
            }
        }

        public void SpawnVFX()
        {
            Util.PlaySound("Play_grandParent_attack1_boulderSmall_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_env_desert_wind_gust", gameObject);
        }
    }
}