using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CrocsItems.Items.Jibbitz
{
    public class BlackHeart : ItemBase<BlackHeart>
    {
        public override string ItemName => "Black Heart";

        public override string ItemLangTokenName => "BLACK_HEART";

        public override string ItemPickupDesc => "While you have a Crocs item, increase health regeneration and maximum health.";

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsHealing>health regeneration</style> by <style=cIsHealing>1%</style> <style=cStack>(+1% per stack)</style> of your <style=cIsHealing>maximum health</style> and <style=cIsHealing>maximum health</style> by <style=cIsHealing>125</style> <style=cStack>(+125 per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("BlackHeartHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texBlackHeart.png");

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
                args.baseHealthAdd += 125f * stack;
                args.baseRegenAdd += 0.01f * sender.maxHealth * stack;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var blackHeartIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("BlackHeartHolder.prefab"), "BlackHeartIDRS", false);
            var itemDisplay = blackHeartIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. blackHeartIDRS.GetComponentsInChildren<Renderer>()];
            Array.Resize(ref itemDisplay.rendererInfos, rendererList.Count);
            for (int j = 0; j < rendererList.Count; j++)
            {
                var renderer = rendererList[j];
                var defaultMaterial = renderer.material;
                itemDisplay.rendererInfos[j] = new CharacterModel.RendererInfo()
                {
                    renderer = renderer,
                    defaultMaterial = defaultMaterial,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false,
                    hideOnDeath = false,
                    ignoresMaterialOverrides = false
                };
            }

            ItemDisplayRuleDict i = new();

            i.Add("CommandoBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(-0.01792F, 0.20367F, -0.06996F),
                    localAngles = new Vector3(332.0242F, 112.7565F, 193.2354F),
                    localScale = new Vector3(0.08395F, 0.08395F, 0.08395F),

                    followerPrefab = blackHeartIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("HuntressBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.02785F, 0.19306F, -0.05696F),
                    localAngles = new Vector3(356.6767F, 100.7022F, 191.9155F),
                    localScale = new Vector3(0.07839F, 0.07839F, 0.07839F),

                    followerPrefab = blackHeartIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("Bandit2Body",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.02282F, 0.12287F, -0.11877F),
                    localAngles = new Vector3(5.18259F, 106.6233F, 157.8688F),
                    localScale = new Vector3(0.08718F, 0.08718F, 0.08718F),

                    followerPrefab = blackHeartIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "MainWheelL",
        localPos = new Vector3(0.26713F, 2.08687F, 1.64393F),
        localAngles = new Vector3(332.1618F, 109.3373F, 249.1514F),
        localScale = new Vector3(1.15078F, 1.15078F, 1.15078F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.0235F, 0.31043F, -0.15202F),
        localAngles = new Vector3(354.9282F, 110.2459F, 200.05F),
        localScale = new Vector3(0.11534F, 0.11534F, 0.11534F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.01289F, 0.16433F, -0.13385F),
        localAngles = new Vector3(5.17897F, 108.7139F, 170.8345F),
        localScale = new Vector3(0.08995F, 0.08995F, 0.08995F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(-0.02598F, 0.1106F, -0.09919F),
        localAngles = new Vector3(354.5979F, 116.8072F, 168.0664F),
        localScale = new Vector3(0.08472F, 0.08472F, 0.08472F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootFrontL",
        localPos = new Vector3(0.32582F, 1.23417F, -0.20883F),
        localAngles = new Vector3(13.19976F, 41.28339F, 101.703F),
        localScale = new Vector3(0.18216F, 0.18216F, 0.18216F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.00271F, 0.08216F, -0.18706F),
        localAngles = new Vector3(9.07798F, 109.8903F, 161.2258F),
        localScale = new Vector3(0.09995F, 0.09995F, 0.09995F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(-0.23601F, 0.90931F, -3.00773F),
        localAngles = new Vector3(350.3483F, 292.4929F, 239.7709F),
        localScale = new Vector3(0.99672F, 0.99672F, 0.99672F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }
);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.09536F, 0.13318F, -0.18243F),
        localAngles = new Vector3(11.81283F, 80.65681F, 142.466F),
        localScale = new Vector3(0.10102F, 0.10102F, 0.10102F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.0093F, 0.12709F, -0.24745F),
        localAngles = new Vector3(0.69374F, 114.2518F, 156.2798F),
        localScale = new Vector3(0.11933F, 0.11933F, 0.11933F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.12127F, 0.17457F, -0.01218F),
        localAngles = new Vector3(359.493F, 24.40005F, 167.567F),
        localScale = new Vector3(0.10452F, 0.10452F, 0.10452F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.11188F, 0.1233F, 0.00458F),
        localAngles = new Vector3(4.14639F, 17.92205F, 162.0147F),
        localScale = new Vector3(0.0828F, 0.0828F, 0.0828F),

        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Wheel",
        localPos = new Vector3(0.66651F, -0.2417F, -0.02912F),
        localAngles = new Vector3(8.86733F, 342.5343F, 37.52561F),
        localScale = new Vector3(0.15695F, 0.15695F, 0.15695F),
        followerPrefab = blackHeartIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }
}