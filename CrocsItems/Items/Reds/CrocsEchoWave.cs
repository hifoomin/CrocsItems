using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrocsItems.Items.Greens;
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

        public static GameObject passiveParticles;

        public static GameObject impactParticles;

        public static GameObject impactExplosion;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpVFX();
            SetUpBuff();
        }

        public void SetUpBuff()
        {
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.isHidden = false;
            speedBuff.isDebuff = false;
            speedBuff.canStack = true;
            speedBuff.buffColor = new Color32(36, 47, 82, 255);
            speedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("3e432d63b7c55a544a0f383de7b1f474").WaitForCompletion();
            // guid is tex move speed buff icon
            speedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            speedBuff.ignoreGrowthNectar = false;
            speedBuff.isDOT = false;
            speedBuff.isCooldown = false;
            speedBuff.name = "Crocs Echo Wave Movement Speed - 1% Per";

            ContentAddition.AddBuffDef(speedBuff);
        }

        public void SetUpVFX()
        {
            passiveParticles = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("56e965f822208a1438744bae84358e1d").WaitForCompletion(), "Crocs Echo Wave Passive Particles VFX", false);
            // guid is bandit 2 smoke bomb

            passiveParticles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.ScaleToHierarchy(passiveParticles);

            VFXUtils.RecolorMaterialsAndLights(passiveParticles, Color.yellow, Color.yellow, true, true);

            var passiveParticlesTransform = passiveParticles.transform.Find("Core");
            passiveParticlesTransform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            passiveParticlesTransform.localPosition = Vector3.zero;

            var passiveParticlesSparks = passiveParticlesTransform.Find("Sparks");
            var passiveParticlesSparksPS = passiveParticlesSparks.GetComponent<ParticleSystem>();
            var passiveParticlesSparksMain = passiveParticlesSparksPS.main;
            passiveParticlesSparksMain.maxParticles = 100;
            var passiveParticlesSparksEmission = passiveParticlesSparksPS.emission;
            var passiveParticlesBurst = new ParticleSystem.Burst(0f, 100, 100, 1, 0.01f);
            passiveParticlesBurst.probability = 1f;
            passiveParticlesSparksEmission.SetBurst(0, passiveParticlesBurst);

            var passiveParticlesSparksPSR = passiveParticlesSparks.GetComponent<ParticleSystemRenderer>();
            passiveParticlesSparksPSR.material.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("8d0972db888e4df42814eb4b6178f0e2").WaitForCompletion());
            // guid is tex glow paint mask

            passiveParticlesTransform.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            passiveParticlesTransform.Find("Dust, CenterSphere").gameObject.SetActive(false);
            passiveParticlesTransform.Find("Dust, CenterTube").gameObject.SetActive(false);

            passiveParticlesTransform.Find("Point Light").gameObject.SetActive(false);

            ContentAddition.AddEffect(passiveParticles);

            impactParticles = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("56e965f822208a1438744bae84358e1d").WaitForCompletion(), "Crocs Echo Wave Impact Particles VFX", false);
            // guid is bandit 2 smoke bomb

            impactParticles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.ScaleToHierarchy(impactParticles);

            VFXUtils.RecolorMaterialsAndLights(impactParticles, Color.yellow, Color.yellow, true, true);

            var transform = impactParticles.transform.Find("Core");
            transform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            transform.localPosition = Vector3.zero;

            var sparks = transform.Find("Sparks");
            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 100;
            var sparksEmission = sparksPS.emission;
            var burst = new ParticleSystem.Burst(0f, 100, 100, 1, 0.01f);
            burst.probability = 1f;
            sparksEmission.SetBurst(0, burst);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();
            sparksPSR.material.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("8d0972db888e4df42814eb4b6178f0e2").WaitForCompletion());
            // guid is tex glow paint mask

            // transform.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            transform.Find("Dust, CenterSphere").gameObject.SetActive(false);
            transform.Find("Dust, CenterTube").gameObject.SetActive(false);

            var pointLight = transform.Find("Point Light");

            var light = pointLight.GetComponent<Light>();
            light.intensity = 15f;
            light.range = 16f;

            ContentAddition.AddEffect(impactParticles);

            VFXUtils.MultiplyDuration(impactParticles, 4f);

            impactExplosion = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("26e8d4483ccf1484b98777af3c44ecbb").WaitForCompletion(), "Crocs Echo Wave Impact VFX", false);
            // guid is ice ring explosion
            var effectComponent = impactExplosion.GetComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.soundName = "";

            impactExplosion.transform.Find("RuneRings").gameObject.SetActive(false);

            impactExplosion.transform.Find("IceMesh").GetComponent<ParticleSystemRenderer>().mesh = Addressables.LoadAssetAsync<Mesh>("a66159248c42bad478cf4ce0379ba1ee").WaitForCompletion();
            // guid is mdl special distant planet neb station

            VFXUtils.RecolorMaterialsAndLights(impactExplosion, Color.yellow, Color.yellow, true);
            VFXUtils.MultiplyDuration(impactExplosion, 2.5f);
            VFXUtils.ScaleToHierarchy(impactExplosion);

            ContentAddition.AddEffect(impactExplosion);
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

        public bool addedOverlay = false;

        public List<HurtBox> lastHitHurtBoxes = new();

        public TemporaryOverlayInstance temporaryOverlayInstance;

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
                        // AddOverlay();
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

            successfullyHit = attackerOverlap.Fire(lastHitHurtBoxes);
            if (successfullyHit)
            {
                // Main.ModLogger.LogError("attack overlap fire is true");
                buffCount = buffCountAfterImpact;
                body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, buffCountAfterImpact);
                // body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, 0);
                // RemoveOverlay();
                SpawnVFX(lastHitHurtBoxes);
            }
        }

        public void SpawnVFX(List<HurtBox> lastHitHurtBoxes)
        {
            Util.PlaySound("Play_grandParent_attack1_boulderSmall_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_env_desert_wind_gust", gameObject);
            Util.PlaySound("Play_mage_m2_zap", gameObject);

            var effectData = new EffectData();
            effectData.scale = 16f + body.radius;
            effectData.origin = body.footPosition;

            EffectManager.SpawnEffect(CrocsEchoWave.impactParticles, effectData, true);

            for (int i = 0; i < lastHitHurtBoxes.Count; i++)
            {
                var lastHitHurtBox = lastHitHurtBoxes[i];

                var healthComponent = lastHitHurtBox.healthComponent;
                if (!healthComponent)
                {
                    continue;
                }

                var victimBody = healthComponent.body;
                if (!victimBody)
                {
                    continue;
                }

                var effectData2 = new EffectData();
                effectData2.scale = Mathf.Sqrt(victimBody.radius * 2f);
                effectData2.origin = victimBody.corePosition;

                EffectManager.SpawnEffect(CrocsEchoWave.impactExplosion, effectData2, true);

                var effectData3 = new EffectData();
                effectData3.scale = 8f + victimBody.radius;
                effectData3.origin = victimBody.footPosition;

                EffectManager.SpawnEffect(CrocsEchoWave.impactParticles, effectData3, true);
            }

            lastHitHurtBoxes.Clear();
        }
    }
}