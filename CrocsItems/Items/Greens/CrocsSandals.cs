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

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier2;

        public override ItemTag[] ItemTags => [ItemTag.Healing, ItemTag.Damage];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("CrocsSandalsHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texCrocsSandals.png");

        public override bool IsCroc => true;

        public static BuffDef attackSpeedBuff;
        public static Material matAttackSpeedOverlay;

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
                    effectData.scale = 24f;
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

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var crocsSandalsIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("CrocsSandalsHolder.prefab"), "CrocsSandalsIDRS", false);
            var itemDisplay = crocsSandalsIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. crocsSandalsIDRS.GetComponentsInChildren<Renderer>()];
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
                    localPos = new Vector3(-0.0035F, 0.46714F, 0.02476F),
                    localAngles = new Vector3(45.46881F, 182.0074F, 184.4055F),
                    localScale = new Vector3(0.16331F, 0.16331F, 0.16331F),

                    followerPrefab = crocsSandalsIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("HuntressBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.01359F, 0.36697F, -0.01548F),
        localAngles = new Vector3(45.53463F, 186.5002F, 185.7617F),
        localScale = new Vector3(0.15496F, 0.15496F, 0.15496F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("Bandit2Body",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.00525F, 0.29683F, 0.0875F),
        localAngles = new Vector3(32.62676F, 180.2419F, 182.4019F),
        localScale = new Vector3(0.2027F, 0.2027F, 0.2027F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.00285F, 2.27212F, 1.97661F),
        localAngles = new Vector3(59.88783F, 174.3331F, 350.1672F),
        localScale = new Vector3(1.65036F, 1.65036F, 1.65036F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "HeadCenter",
        localPos = new Vector3(0.00424F, 0.23585F, 0.05142F),
        localAngles = new Vector3(38.95258F, 180.429F, 180.6613F),
        localScale = new Vector3(0.19436F, 0.19436F, 0.19436F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.00004F, 0.24619F, -0.03354F),
        localAngles = new Vector3(29.12964F, 181.8412F, 180.358F),
        localScale = new Vector3(0.13534F, 0.13534F, 0.13534F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.00916F, 0.29329F, 0.13914F),
        localAngles = new Vector3(16.23968F, 185.2051F, 182.059F),
        localScale = new Vector3(0.15367F, 0.15367F, 0.15367F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootBackR",
        localPos = new Vector3(-0.17829F, 1.5229F, 0.22961F),
        localAngles = new Vector3(5.38809F, 143.3998F, 179.6215F),
        localScale = new Vector3(0.36804F, 0.36804F, 0.36804F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.0118F, 0.30149F, 0.10755F),
        localAngles = new Vector3(19.90068F, 180.8563F, 188.5628F),
        localScale = new Vector3(0.16696F, 0.16696F, 0.16696F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.13397F, 0.91924F, 2.33611F),
        localAngles = new Vector3(51.30529F, 170.2407F, 352.4607F),
        localScale = new Vector3(1.79975F, 1.79975F, 1.79975F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.00003F, 0.36572F, 0.04139F),
        localAngles = new Vector3(307.8225F, 1.07182F, 179.3283F),
        localScale = new Vector3(0.18118F, 0.18118F, 0.18118F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.01503F, 0.28854F, -0.01491F),
        localAngles = new Vector3(34.73861F, 181.2098F, 180.3323F),
        localScale = new Vector3(0.16999F, 0.16999F, 0.16999F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.0862F, 0.16076F, -0.17777F),
        localAngles = new Vector3(57.56286F, 65.53618F, 72.12031F),
        localScale = new Vector3(0.20458F, 0.20458F, 0.20458F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(0.02103F, 0.31652F, 0.08969F),
        localAngles = new Vector3(20.10568F, 182.2416F, 183.3654F),
        localScale = new Vector3(0.15274F, 0.17222F, 0.15274F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Head",
        localPos = new Vector3(-0.98358F, 0.21098F, 0.01558F),
        localAngles = new Vector3(82.39269F, 132.3183F, 43.69971F),
        localScale = new Vector3(0.28965F, 0.305F, 0.28965F),

        followerPrefab = crocsSandalsIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }

    public class CrocsSandalsController : CharacterBody.ItemBehavior
    {
        public int counter = 0;
    }
}