using System;
using R2API;
using RoR2;
using UnityEngine;

namespace CrocsItems.Items.Jibbitz
{
    public class BlackHeart : ItemBase<BlackHeart>
    {
        public override string ItemName => "Black Heart";

        public override string ItemLangTokenName => "BLACK_HEART";

        public override string ItemPickupDesc => "While you have a Crocs item, increase health regeneration and maximum health.";

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsHealing>health regeneration</style> by <style=cIsHealing>2.5%</style> <style=cStack>(+2.5% per stack)</style> of your <style=cIsHealing>maximum health</style> and <style=cIsHealing>maximum health</style> by <style=cIsHealing>75</style> <style=cStack>(+75 per stack)</style>.";

        public override string ItemLore => "This item needs lore.";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public override bool CanRemove => false;

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
                args.baseHealthAdd += 75f * stack;
                args.baseRegenAdd += 0.025f * sender.maxHealth * stack;
            }
        }
    }
}