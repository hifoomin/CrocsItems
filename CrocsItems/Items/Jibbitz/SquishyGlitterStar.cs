using System;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CrocsItems.Items.Jibbitz
{
    public class SquishyGlitterStar : ItemBase<SquishyGlitterStar>
    {
        public override string ItemName => "Squishy Glitter Star";

        public override string ItemLangTokenName => "SQUISHY_GLITTER_STAR";

        public override string ItemPickupDesc => "While you have a Crocs item, increase jump height and sprinting speed.";

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsUtility>jump height</style> by <style=cIsUtility>30%</style> <style=cStack>(+30% per stack)</style> and <style=cIsUtility>sprinting speed</style> by <style=cIsUtility>20%</style> <style=cStack>(+20% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("SquishyGlitterStarHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texSquishyGlitterStar.png");

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
                args.sprintSpeedAdd += 0.2f * stack;
                args.jumpPowerMultAdd += 0.3f * stack;
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
                followerPrefab = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("SquishyGlitterStarHolder.prefab"), "S", false),
                limbMask = LimbFlags.None,
                followerPrefabAddress = new AssetReferenceGameObject("")
            });
        }
    }
}