using System;
using System.Collections.Generic;
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

        public override string ItemFullDescription => "While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsDamage>attack speed</style> by <style=cIsDamage>30%</style> <style=cStack>(+30% per stack)</style> and reduce <style=cIsUtility>skill cooldowns</style> by <style=cIsUtility>12.5%</style> <style=cStack>(+12.5% per stack)</style>.";

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
                args.cooldownMultAdd -= Util.ConvertAmplificationPercentageIntoReductionNormalized(0.125f * stack);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var sweetTreatIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("SweetTreatHolder.prefab"), "SweetTreatIDRS", false);
            var itemDisplay = sweetTreatIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. sweetTreatIDRS.GetComponentsInChildren<Renderer>()];
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
                    childName = "Head",
                    localPos = new Vector3(-0.00587F, 0.58128F, 0.16511F),
                    localAngles = new Vector3(2.14203F, 108.2614F, 52.9227F),
                    localScale = new Vector3(0.25855F, 0.25855F, 0.25855F),

                    followerPrefab = sweetTreatIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("HuntressBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.02562F, 0.48285F, 0.115F),
                    localAngles = new Vector3(4.51112F, 109.1555F, 44.29779F),
                    localScale = new Vector3(0.21452F, 0.21452F, 0.21452F),

                    followerPrefab = sweetTreatIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("Bandit2Body",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(-0.00781F, 0.40143F, 0.28731F),
                    localAngles = new Vector3(357.2288F, 111.4128F, 56.65021F),
                    localScale = new Vector3(0.25888F, 0.25888F, 0.25888F),

                    followerPrefab = sweetTreatIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.04008F, 3.81882F, 2.72591F),
        localAngles = new Vector3(1.21159F, 112.3926F, 33.91652F),
        localScale = new Vector3(1.84496F, 1.84496F, 1.84496F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "HeadCenter",
        localPos = new Vector3(0.00098F, 0.3581F, 0.22993F),
        localAngles = new Vector3(347.1471F, 111.8147F, 45.71295F),
        localScale = new Vector3(0.27407F, 0.27407F, 0.27407F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.00281F, 0.29843F, 0.09278F),
        localAngles = new Vector3(358.5781F, 115.6455F, 64.65923F),
        localScale = new Vector3(0.16891F, 0.16891F, 0.16891F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.00729F, 0.3155F, 0.27172F),
        localAngles = new Vector3(13.84386F, 259.5506F, 281.8133F),
        localScale = new Vector3(0.24914F, 0.24914F, 0.24914F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootBackR",
        localPos = new Vector3(-0.31406F, 1.45427F, 0.41659F),
        localAngles = new Vector3(337.4986F, 249.3362F, 119.3273F),
        localScale = new Vector3(0.52902F, 0.52902F, 0.52902F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.01331F, 0.3358F, 0.26087F),
        localAngles = new Vector3(15.5291F, 256.3843F, 283.7037F),
        localScale = new Vector3(0.23437F, 0.23437F, 0.23437F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.22456F, 2.07007F, 2.99383F),
        localAngles = new Vector3(13.65421F, 282.1486F, 337.8485F),
        localScale = new Vector3(2.22588F, 2.22588F, 2.22588F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }
);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.01031F, 0.33743F, 0.14904F),
        localAngles = new Vector3(353.4342F, 105.6774F, 66.71175F),
        localScale = new Vector3(0.32152F, 0.32152F, 0.32152F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.02232F, 0.36592F, 0.12821F),
        localAngles = new Vector3(356.6202F, 108.9451F, 56.15862F),
        localScale = new Vector3(0.24114F, 0.24114F, 0.24114F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.20527F, 0.33912F, -0.20474F),
        localAngles = new Vector3(345.1343F, 96.4844F, 349.1574F),
        localScale = new Vector3(0.26322F, 0.26322F, 0.26322F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.0156F, 0.36563F, 0.26061F),
        localAngles = new Vector3(355.1281F, 101.7715F, 66.5113F),
        localScale = new Vector3(0.23289F, 0.23289F, 0.23289F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.97968F, 0.48174F, 0.05943F),
        localAngles = new Vector3(16.72155F, 14.57022F, 8.75761F),
        localScale = new Vector3(0.37293F, 0.37293F, 0.37293F),

        followerPrefab = sweetTreatIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }
}