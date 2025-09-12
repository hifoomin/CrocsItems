using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CrocsItems.Items.Greens
{
    [ConfigSection("Items :: Crocs Sandals")]
    public class CrocsSandals : ItemBase<CrocsSandals>
    {
        public override string ItemName => "Crocs Sandals";

        public override string ItemLangTokenName => "CROCS_SANDALS";

        public override string ItemPickupDesc => "Attacks heal all nearby allies for a percentage of damage dealt. Gain attack speed the longer you are in combat.";

        public override string ItemFullDescription => "Attacks <style=cIsHealing>heal</style> all nearby allies for <style=cIsHealing>1.5%</style> <style=cStack>(+1.5% per stack)</style> of damage dealt. Gain <style=cIsDamage>2%</style> <style=cIsDamage>attack speed</style> on hit, up to <style=cIsDamage>+40%</style> <style=cStack>(+40% per stack)</style>.";

        public override string ItemLore => "This item should have lore.";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => [ItemTag.Healing, ItemTag.Damage];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("CrocsSandalsHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texCrocsSandals.png");

        public override bool IsCroc => true;

        public static BuffDef attackSpeedBuff;
        public static Material matAttackSpeedOverlay;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpBuff();
            SetUpVFX();
        }

        public void SetUpBuff()
        {
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.canStack = true;
            attackSpeedBuff.buffColor = new Color32(227, 169, 45, 255);
            attackSpeedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("2caf2c471e1682249a666ba7ce277eac").WaitForCompletion();
            // guid is attack speed on crit
            attackSpeedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            attackSpeedBuff.ignoreGrowthNectar = false;
            attackSpeedBuff.isDOT = false;
            attackSpeedBuff.isCooldown = false;

            ContentAddition.AddBuffDef(attackSpeedBuff);
        }

        public void SetUpVFX()
        {
            matAttackSpeedOverlay = new Material(Addressables.LoadAssetAsync<Material>("a3a110f394481d346979d76f8d20138d").WaitForCompletion());
            // guid is mat huntress flash expanded
            matAttackSpeedOverlay.SetFloat("_InvFade", 1f);
            matAttackSpeedOverlay.SetFloat("_Boost", 1.5f);
            matAttackSpeedOverlay.SetFloat("_AlphaBoost", 2f);
            matAttackSpeedOverlay.SetFloat("_AlphaBias", 0f);
            matAttackSpeedOverlay.SetInt("_Cull", 0);
            matAttackSpeedOverlay.SetColor("_TintColor", new Color32(227, 169, 45, 255));
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += CalcAttackSpeedBoost;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;

        }
        private void OnInventoryChangedGlobal(CharacterBody body)
        {
            body.AddItemBehavior<CrocsSandalsController>(GetCount(body));
        }

        private void CalcAttackSpeedBoost(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                args.baseAttackSpeedAdd += 0.02f * sender.GetBuffCount(attackSpeedBuff);
            }
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            var attackerBody = report.attackerBody;
            if (!attackerBody)
            {
                return;
            }

            var stack = GetCount(attackerBody);

            if (stack > 0)
            {
                var healValue = report.damageDealt * 0.015f * stack;

                var teamMask = default(TeamMask);
                teamMask.AddTeam(attackerBody.teamComponent.teamIndex);

                List<HurtBox> hurtBoxBuffer = new();

                var sphereSearch = new SphereSearch();
                sphereSearch.origin = attackerBody.corePosition;
                sphereSearch.mask = LayerIndex.entityPrecise.mask;
                sphereSearch.radius = 20f;
                sphereSearch.RefreshCandidates();
                sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
                sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                sphereSearch.OrderCandidatesByDistance();
                sphereSearch.GetHurtBoxes(hurtBoxBuffer);
                sphereSearch.ClearCandidates();

                for (int i = 0; i < hurtBoxBuffer.Count; i++)
                {
                    var hurtBox = hurtBoxBuffer[i];
                    var healthComponent = hurtBox.healthComponent;
                    if (healthComponent)
                    {
                        healthComponent.Heal(healValue, default);
                    }
                }
                hurtBoxBuffer.Clear();

                if (!report.damageInfo.damageType.IsDamageSourceSkillBased)
                {
                    return;
                }

                var crocsSandalsController = attackerBody.GetComponent<CrocsSandalsController>();
                if (!crocsSandalsController)
                {
                    return;
                }

                var buffCount = attackerBody.GetBuffCount(attackSpeedBuff);
                var maxBuffCount = 20 * stack;

                RefreshTimedBuffs(attackerBody, attackSpeedBuff, 3f);

                if (buffCount < maxBuffCount)
                {
                    attackerBody.AddTimedBuff(attackSpeedBuff, 3f);
                    crocsSandalsController.counter++;
                }

                var modelLocator = attackerBody.modelLocator;
                if (!modelLocator)
                {
                    return;
                }
                var modelTransform = modelLocator.modelTransform;
                if (!modelTransform)
                {
                    return;
                }

                if (buffCount > 0 && crocsSandalsController.counter >= maxBuffCount && buffCount % maxBuffCount == 0) // add overlay vfx and play sound every 20 buff count
                {
                    var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlay.duration = buffCount / maxBuffCount; // decrease overlay duration the more buffs you get, because it gets a lot easier with more stacks
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = matAttackSpeedOverlay;
                    temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();

                    Util.PlaySound("Play_vagrant_attack1_pop", attackerBody.gameObject);
                    Util.PlaySound("Play_bison_step_charge", attackerBody.gameObject);
                    var effectData = new EffectData();
                    effectData.scale = 12f;
                    effectData.origin = attackerBody.corePosition;

                    EffectManager.SpawnEffect(Reds.CrocsEchoWave.passiveParticles, effectData, true);

                    crocsSandalsController.counter = 0;
                }
            }
        }

        private void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0)
            {
                return;
            }

            for (int i = 0; i < body.timedBuffs.Count; i++)
            {
                var buff = body.timedBuffs[i];
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    buff.timer = duration;
                }
            }
        }
    }

    public class CrocsSandalsController : CharacterBody.ItemBehavior
    {
        public int counter = 0;
    }
}