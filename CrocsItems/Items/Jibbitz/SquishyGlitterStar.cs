using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CrocsItems.Items.Jibbitz
{
    [ConfigSection("Items :: Squishy Glitter Star")]
    public class SquishyGlitterStar : ItemBase<SquishyGlitterStar>
    {
        public override string ItemName => "Squishy Glitter Star";

        public override string ItemLangTokenName => "SQUISHY_GLITTER_STAR";

        public override string ItemPickupDesc => "While you have a Crocs item, increase jump height and sprinting speed.";

        public override string ItemFullDescription => $"While you have a <style=cIsUtility>Crocs</style> item, increase <style=cIsUtility>jump height</style> by <style=cIsUtility>{baseJumpHeight * 100f}%</style> <style=cStack>(+{stackJumpHeight * 100f}% per stack)</style> and <style=cIsUtility>sprinting speed</style> by <style=cIsUtility>{baseSprintingSpeed * 100f}%</style> <style=cStack>(+{stackSprintingSpeed * 100f}% per stack)</style>.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier1;

        public override ItemTag[] ItemTags => [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.WorldUnique];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("SquishyGlitterStarHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texSquishyGlitterStar.png");

        public override bool CanRemove => true;

        public override bool IsCroc => false;
        public override bool IsJibbit => true;

        [ConfigField("Base Jump Height", "Decimal.", 0.3f)]
        public static float baseJumpHeight;

        [ConfigField("Stack Jump Height", "Decimal.", 0.3f)]
        public static float stackJumpHeight;

        [ConfigField("Base Sprinting Speed", "Decimal.", 0.2f)]
        public static float baseSprintingSpeed;

        [ConfigField("Stack Sprinting Speed", "Decimal.", 0.2f)]
        public static float stackSprintingSpeed;

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
                args.sprintSpeedAdd += baseSprintingSpeed + stackSprintingSpeed * (stack - 1);
                args.jumpPowerMultAdd += baseJumpHeight + stackJumpHeight * (stack - 1);
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var squishyGlitterStarIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("SquishyGlitterStarHolder.prefab"), "SquishyGlitterStarIDRS", false);
            var itemDisplay = squishyGlitterStarIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. squishyGlitterStarIDRS.GetComponentsInChildren<Renderer>()];
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
                    childName = "FootR",
                    localPos = new Vector3(-0.00452F, 0.20867F, -0.04586F),
                    localAngles = new Vector3(323.2636F, 321.1545F, 359.3052F),
                    localScale = new Vector3(0.0678F, 0.0678F, 0.0678F),

                    followerPrefab = squishyGlitterStarIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("HuntressBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootR",
                    localPos = new Vector3(-0.01449F, 0.14379F, -0.06557F),
                    localAngles = new Vector3(324.7495F, 319.2489F, 355.3762F),
                    localScale = new Vector3(0.07738F, 0.07738F, 0.07738F),

                    followerPrefab = squishyGlitterStarIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("Bandit2Body",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootR",
                    localPos = new Vector3(-0.04968F, 0.19693F, -0.12796F),
                    localAngles = new Vector3(15.672F, 87.32202F, 205.6392F),
                    localScale = new Vector3(0.08714F, 0.08714F, 0.08714F),

                    followerPrefab = squishyGlitterStarIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "MainWheelR",
        localPos = new Vector3(-0.02833F, 1.30067F, 2.00702F),
        localAngles = new Vector3(29.35673F, 97.75612F, 303.2204F),
        localScale = new Vector3(1.08009F, 1.08009F, 1.08009F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.02341F, 0.33068F, -0.12775F),
        localAngles = new Vector3(326.9152F, 313.3573F, 352.6025F),
        localScale = new Vector3(0.13313F, 0.13313F, 0.13313F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.033F, 0.21006F, -0.12245F),
        localAngles = new Vector3(312.159F, 262.5387F, 66.91231F),
        localScale = new Vector3(0.07086F, 0.07086F, 0.07086F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.02016F, 0.1561F, -0.10028F),
        localAngles = new Vector3(307.9617F, 277.4116F, 37.39954F),
        localScale = new Vector3(0.08822F, 0.08822F, 0.08822F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootFrontR",
        localPos = new Vector3(-0.3738F, 1.19165F, -0.36723F),
        localAngles = new Vector3(322.8422F, 266.4492F, 125.6811F),
        localScale = new Vector3(0.21667F, 0.21667F, 0.21667F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.03957F, 0.17147F, -0.19321F),
        localAngles = new Vector3(311.3138F, 274.4646F, 67.32278F),
        localScale = new Vector3(0.1074F, 0.1074F, 0.1074F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.12955F, 0.22561F, -2.71476F),
        localAngles = new Vector3(314.8155F, 236.1199F, 107.5251F),
        localScale = new Vector3(0.85182F, 0.85182F, 0.85182F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }
);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.00686F, 0.0987F, -0.26458F),
        localAngles = new Vector3(352.8398F, 294.7687F, 104.7761F),
        localScale = new Vector3(0.10953F, 0.10953F, 0.10953F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.01337F, 0.24921F, -0.30587F),
        localAngles = new Vector3(303.8278F, 259.0275F, 73.75887F),
        localScale = new Vector3(0.14856F, 0.14856F, 0.14856F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.15892F, 0.20634F, -0.02381F),
        localAngles = new Vector3(301.3812F, 175.3402F, 228.2814F),
        localScale = new Vector3(0.1004F, 0.1004F, 0.1004F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.13305F, 0.13963F, -0.00243F),
        localAngles = new Vector3(302.4132F, 1.30971F, 221.7963F),
        localScale = new Vector3(0.09701F, 0.09735F, 0.09701F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Wheel",
        localPos = new Vector3(-0.63388F, -0.48715F, -0.06084F),
        localAngles = new Vector3(340.674F, 342.4554F, 207.807F),
        localScale = new Vector3(0.12722F, 0.12722F, 0.12722F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);
            /*
            i.Add("HereticBody",

                                                    new ItemDisplayRule()
                                                    {
                                                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                                                        childName = "FootR",
                                                        localPos = new Vector3(0.11327F, 0.65168F, 0.03705F),
                                                        localAngles = new Vector3(84.99532F, 329.3675F, 210.1704F),
                                                        localScale = new Vector3(0.32767F, 0.32767F, 0.32767F),

                                                        followerPrefab = squishyGlitterStarIDRS,
                                                        limbMask = LimbFlags.None,
                                                        followerPrefabAddress = new AssetReferenceGameObject("")
                                                    }

                                                );
            */
            // massive desync

            i.Add("FalseSonBody",

                            new ItemDisplayRule()
                            {
                                ruleType = ItemDisplayRuleType.ParentedPrefab,
                                childName = "FootR",
                                localPos = new Vector3(0.23143F, 0.23851F, 0.00255F),
                                localAngles = new Vector3(358.9553F, 339.3062F, 205.8068F),
                                localScale = new Vector3(0.14548F, 0.14548F, 0.14548F),

                                followerPrefab = squishyGlitterStarIDRS,
                                limbMask = LimbFlags.None,
                                followerPrefabAddress = new AssetReferenceGameObject("")
                            }

                        );

            i.Add("DroneTechBody",

                            new ItemDisplayRule()
                            {
                                ruleType = ItemDisplayRuleType.ParentedPrefab,
                                childName = "FootR",
                                localPos = new Vector3(0.18928F, -0.07418F, 0.01394F),
                                localAngles = new Vector3(332.6079F, 271.2307F, 154.9987F),
                                localScale = new Vector3(0.08752F, 0.08752F, 0.08752F),

                                followerPrefab = squishyGlitterStarIDRS,
                                limbMask = LimbFlags.None,
                                followerPrefabAddress = new AssetReferenceGameObject("")
                            }

                        );

            i.Add("DrifterBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.02879F, 0.20615F, -0.01217F),
        localAngles = new Vector3(81.77789F, 84.55266F, 313.1673F),
        localScale = new Vector3(0.09425F, 0.09425F, 0.09425F),

        followerPrefab = squishyGlitterStarIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }
}