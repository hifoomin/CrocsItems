using System;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CrocsItems.Items.Jibbitz
{
    public class SweetTreat : ItemBase<SweetTreat>
    {
        public override string ItemName => "Sweet Treat";

        public override string ItemLangTokenName => "SWEET_TREAT";

        public override string ItemPickupDesc => "While you have a Crocs item, increase attack speed and reduce skill cooldowns.";

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsDamage>attack speed</style> by <style=cIsDamage>30%</style> <style=cStack>(+30% per stack)</style> and reduce <style=cIsUtility>skill cooldowns</style> by <style=cIsUtility>15%</style> <style=cStack>(+15% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("SweetTreatHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texSweetTreat.png");

        public override bool CanRemove => true;

        public override bool IsCroc => false;
        public override bool IsJibbit => true;

        public override void Init()
        {
            base.Init();
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += AddStats;
        }

        private void AddStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var stack = GetCount(sender);
            var hasAnyCrocs = Main.HasAnyCrocs(sender) || Main.HasAnyCrocsEquipment(sender);
            if (stack > 0 && hasAnyCrocs)
            {
                args.baseAttackSpeedAdd += 0.3f * stack;
                args.cooldownMultAdd -= Util.ConvertAmplificationPercentageIntoReductionNormalized(0.15f * stack);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict(new ItemDisplayRule()
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = "Base",
                localPos = new Vector3(1, -1, -0.9f),
                localScale = new Vector3(0.5f, 0.5f, 0.5f),
                followerPrefab = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("SweetTreatHolder.prefab"), "S", false),
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}