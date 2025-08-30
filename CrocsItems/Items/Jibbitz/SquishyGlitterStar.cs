using System;
using R2API;
using RoR2;
using UnityEngine;

namespace CrocsItems.Items.Jibbitz
{
    public class SquishyGlitterStar : ItemBase<SquishyGlitterStar>
    {
        public override string ItemName => "Squishy Glitter Star";

        public override string ItemLangTokenName => "SQUISHY_GLITTER_STAR";

        public override string ItemPickupDesc => "While you have a Crocs item, increase jump height and sprinting speed.";

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsUtility>jump height</style> by <style=cIsUtility>30%</style> <style=cStack>(+30% per stack)</style> and <style=cIsUtility>sprinting speed</style> by <style=cIsUtility>15%</style> <style=cStack>(+15% per stack)</style>.";

        public override string ItemLore => "This item needs lore.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("SquishyGlitterStarHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texSquishyGlitterStar.png");

        public override bool CanRemove => true;

        public override bool IsCroc => false;
        public override bool IsJibbit => true;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

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
            var hasAnyCrocs = HasAnyCrocs(sender);
            if (stack > 0 && hasAnyCrocs)
            {
                args.sprintSpeedAdd += 0.15f * stack;
                args.jumpPowerMultAdd += 0.3f * stack;
            }
        }
    }
}