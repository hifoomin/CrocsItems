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
            attackSpeedBuff = ScriptableObject.CreateInstance<BuffDef>();
            attackSpeedBuff.isHidden = false;
            attackSpeedBuff.isDebuff = false;
            attackSpeedBuff.canStack = true;
            attackSpeedBuff.buffColor = Color.yellow;
            attackSpeedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("2508a4654959d334aaca7d4321922642").WaitForCompletion();
            attackSpeedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            attackSpeedBuff.ignoreGrowthNectar = false;
            attackSpeedBuff.isDOT = false;
            attackSpeedBuff.isCooldown = false;

            ContentAddition.AddBuffDef(attackSpeedBuff);
        }

        public void SetUpVFX()
        {
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += CalcAttackSpeedBoost;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            // CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;
        }

        private void CalcAttackSpeedBoost(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                args.baseAttackSpeedAdd += 0.02f * sender.GetBuffCount(attackSpeedBuff);
            }
        }

        private void OnInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            body.AddItemBehavior<CrocsSandalsController>(GetCount(body));
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            // sphere search?
            // create orbs that heal?
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

                var buffCount = attackerBody.GetBuffCount(attackSpeedBuff);
                var maxBuffCount = 20 * stack;

                RefreshTimedBuffs(attackerBody, attackSpeedBuff, 3f);

                if (buffCount < maxBuffCount)
                {
                    attackerBody.AddTimedBuff(attackSpeedBuff, 3f);
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
        public int maxBuffCount = 8;
        public int buffCount;
        public float inCombatTimer = 0f;
        public float outOfCombatInterval = 2f;
        public float buffChangeInterval = 1f;

        public void FixedUpdate()
        {
            if (!body)
            {
                return;
            }

            inCombatTimer += Time.fixedDeltaTime;
            if (body.outOfCombatStopwatch <= outOfCombatInterval)
            {
                inCombatTimer += Time.fixedDeltaTime;
                if (inCombatTimer >= buffChangeInterval)
                {
                    buffCount++;
                    buffCount = Mathf.Min(buffCount, maxBuffCount);
                    body.SetBuffCount(CrocsSandals.attackSpeedBuff.buffIndex, buffCount);
                    inCombatTimer = 0f;
                }
            }
            else
            {
                inCombatTimer += Time.fixedDeltaTime;
                if (inCombatTimer >= buffChangeInterval)
                {
                    buffCount--;
                    buffCount = Mathf.Max(0, buffCount);
                    body.SetBuffCount(CrocsSandals.attackSpeedBuff.buffIndex, buffCount);
                    inCombatTimer = 0f;
                }
            }
        }
    }
}