using System;
using System.Collections.Generic;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CrocsItems.Equipment
{
    [ConfigSection("Equipment :: Crocs Classic")]
    public class CrocsClassic : EquipmentBase<CrocsClassic>
    {
        public override string EquipmentName => "Crocs Classic";

        public override string EquipmentLangTokenName => "CROCS_CLASSIC";

        public override string EquipmentPickupDesc => "Switch between an offensive stance and a defensive stance.";

        public override string EquipmentFullDescription => "<style=cIsUtility>Switch</style> between an <style=cIsDamage>offensive stance</style>, gaining <style=cIsDamage>explosive attacks</style> for <style=cIsDamage>20% TOTAL damage</style> and <style=cIsDamage>+20% critical chance</style>, or a <style=cIsHealing>defensive</style> stance, gaining <style=cIsHealing>30</style> armor, <style=cIsHealing>+3 hp/s health regeneration</style> and <style=cIsHealing>40% increased healing</style>.";

        public override string EquipmentLore => "";

        public override GameObject EquipmentModel => Main.bundle.LoadAsset<GameObject>("CrocsClassicHolder.prefab");

        public override Sprite EquipmentIcon => Main.bundle.LoadAsset<Sprite>("texCrocsClassic.png");

        public override float Cooldown => 10f;

        public override bool IsCroc => true;

        public static BuffDef offensiveBuff;
        public static BuffDef defensiveBuff;

        public static GameObject vfx;

        public static ModdedProcType crocsClassic = ProcTypeAPI.ReserveProcType();

        public static Material matOffensiveOverlay;
        public static Material matDefensiveOverlay;

        public override void Init()
        {
            base.Init();
            SetUpBuffs();
            SetUpVFX();
        }

        public void SetUpBuffs()
        {
            offensiveBuff = ScriptableObject.CreateInstance<BuffDef>();
            offensiveBuff.canStack = false;
            offensiveBuff.isCooldown = false;
            offensiveBuff.isDebuff = false;
            offensiveBuff.isDOT = false;
            offensiveBuff.isHidden = false;
            offensiveBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            offensiveBuff.ignoreGrowthNectar = false;
            offensiveBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("f0d295f0817aef341aad8edc6350e781").WaitForCompletion();
            // guid is tex buff full crit icon
            offensiveBuff.buffColor = new Color32(30, 167, 217, 255);
            offensiveBuff.name = "Crocs Classic | Offensive Stance - Explosive Attacks, +20% Movement Speed, +20% Crit Chance";

            ContentAddition.AddBuffDef(offensiveBuff);

            defensiveBuff = ScriptableObject.CreateInstance<BuffDef>();
            defensiveBuff.canStack = false;
            defensiveBuff.isCooldown = false;
            defensiveBuff.isDebuff = false;
            defensiveBuff.isDOT = false;
            defensiveBuff.isHidden = false;
            defensiveBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            defensiveBuff.ignoreGrowthNectar = false;
            defensiveBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("c9ccdef9734715a408aa90e9e37735e4").WaitForCompletion();
            // guid is tex buff body armor
            defensiveBuff.buffColor = new Color32(30, 217, 83, 255);
            defensiveBuff.name = "Crocs Classic | Defensive Stance - +30 Armor, +3 hp/s regeneration, +40% Healing";

            ContentAddition.AddBuffDef(defensiveBuff);
        }

        public void SetUpVFX()
        {
            vfx = Addressables.LoadAssetAsync<GameObject>("851521a751ef1cf45bde684db954c165").WaitForCompletion();
            // guid is omni explosion vfx quick

            matOffensiveOverlay = new Material(Addressables.LoadAssetAsync<Material>("a3a110f394481d346979d76f8d20138d").WaitForCompletion());
            // guid is mat huntress flash expanded
            matOffensiveOverlay.SetFloat("_InvFade", 1f);
            matOffensiveOverlay.SetFloat("_Boost", 1.5f);
            matOffensiveOverlay.SetFloat("_AlphaBoost", 2f);
            matOffensiveOverlay.SetFloat("_AlphaBias", 0f);
            matOffensiveOverlay.SetInt("_Cull", 0);
            matOffensiveOverlay.SetColor("_TintColor", new Color32(30, 167, 217, 255));

            matDefensiveOverlay = new Material(Addressables.LoadAssetAsync<Material>("a3a110f394481d346979d76f8d20138d").WaitForCompletion());
            // guid is mat huntress flash expanded
            matDefensiveOverlay.SetFloat("_InvFade", 1f);
            matDefensiveOverlay.SetFloat("_Boost", 1.5f);
            matDefensiveOverlay.SetFloat("_AlphaBoost", 2f);
            matDefensiveOverlay.SetFloat("_AlphaBias", 0f);
            matDefensiveOverlay.SetInt("_Cull", 0);
            matDefensiveOverlay.SetColor("_TintColor", new Color32(30, 217, 83, 255));
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += AddStats;
            On.RoR2.GlobalEventManager.OnHitAllProcess += OnHitAnything;
            On.RoR2.HealthComponent.Heal += OnHeal;
        }

        private float OnHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var body = self.body;
            if (body && body.HasBuff(defensiveBuff))
            {
                amount *= 1.4f;
            }
            return orig(self, amount, procChainMask, nonRegen);
        }

        private void OnHitAnything(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            var attacker = damageInfo.attacker;
            if (!attacker)
            {
                return;
            }

            if (damageInfo.procChainMask.HasModdedProc(crocsClassic))
            {
                return;
            }

            var attackerBody = attacker.GetComponent<CharacterBody>();
            if (!attackerBody)
            {
                return;
            }

            if (!attackerBody.HasBuff(offensiveBuff))
            {
                return;
            }

            damageInfo.procChainMask.AddModdedProc(crocsClassic);

            var damageCoefficient = 0.2f;
            var aoe = 6f;

            var totalDamage = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient);

            var effectData = new EffectData();
            effectData.origin = damageInfo.position;
            effectData.scale = aoe;
            effectData.rotation = Util.QuaternionSafeLookRotation(damageInfo.force);

            EffectManager.SpawnEffect(vfx, effectData, true);

            var blastAttack = new BlastAttack();
            blastAttack.position = damageInfo.position;
            blastAttack.baseDamage = totalDamage;
            blastAttack.baseForce = 0f;
            blastAttack.radius = aoe;
            blastAttack.attacker = damageInfo.attacker;
            blastAttack.inflictor = null;
            blastAttack.teamIndex = TeamComponent.GetObjectTeam(attacker);
            blastAttack.crit = damageInfo.crit;
            blastAttack.procChainMask = damageInfo.procChainMask;
            blastAttack.procCoefficient = 0f;
            blastAttack.damageColorIndex = DamageColorIndex.Item;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;

            blastAttack.Fire();
        }

        private void AddStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(offensiveBuff))
            {
                args.critAdd += 20f;
            }
            if (sender.HasBuff(defensiveBuff))
            {
                args.armorAdd += 30f;
                args.baseRegenAdd += 3f + (3f * 0.2f * (sender.level - 1));
            }
        }

        protected override bool ActivateEquipment(EquipmentSlot slot)
        {
            slot.subcooldownTimer = 1f;

            var body = slot.characterBody;
            if (!body)
            {
                return false;
            }

            if (!body.HasBuff(offensiveBuff))
            {
                AddOverlay(body, matOffensiveOverlay);

                body.AddBuff(offensiveBuff.buffIndex);

                if (body.HasBuff(defensiveBuff))
                {
                    body.RemoveBuff(defensiveBuff);
                }

                return true;
            }

            if (body.HasBuff(offensiveBuff) && !body.HasBuff(defensiveBuff))
            {
                AddOverlay(body, matDefensiveOverlay);

                body.RemoveBuff(offensiveBuff.buffIndex);
                body.AddBuff(defensiveBuff.buffIndex);

                return true;
            }

            return false;
        }

        private void AddOverlay(CharacterBody body, Material materialToApply)
        {
            var modelLocator = body.modelLocator;
            if (!modelLocator)
            {
                return;
            }

            var modelTransform = modelLocator.modelTransform;
            if (!modelTransform)
            {
                return;
            }

            var temporaryOverlay = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
            temporaryOverlay.duration = body.equipmentSlot.subcooldownTimer;
            temporaryOverlay.animateShaderAlpha = true;
            temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            temporaryOverlay.destroyComponentOnEnd = true;
            temporaryOverlay.originalMaterial = materialToApply;
            temporaryOverlay.inspectorCharacterModel = modelTransform.GetComponent<CharacterModel>();
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var crocsClassicIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("CrocsClassicHolder.prefab"), "CrocsClassicIDRS", false);
            var itemDisplay = crocsClassicIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. crocsClassicIDRS.GetComponentsInChildren<Renderer>()];
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
                    localPos = new Vector3(0.00499F, 0.10819F, -0.01906F),
                    localAngles = new Vector3(78.05534F, 93.50056F, 282.6487F),
                    localScale = new Vector3(0.13345F, 0.13345F, 0.13345F),

                    followerPrefab = crocsClassicIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("HuntressBody",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.02319F, 0.10455F, -0.00065F),
                    localAngles = new Vector3(88.97302F, -0.0031F, 184.6359F),
                    localScale = new Vector3(0.1284F, 0.1284F, 0.1284F),

                    followerPrefab = crocsClassicIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("Bandit2Body",

                new ItemDisplayRule()
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "FootL",
                    localPos = new Vector3(0.01806F, 0.11816F, -0.02683F),
                    localAngles = new Vector3(51.35358F, 8.17412F, 176.2019F),
                    localScale = new Vector3(0.14493F, 0.14493F, 0.14493F),

                    followerPrefab = crocsClassicIDRS,
                    limbMask = LimbFlags.None,
                    followerPrefabAddress = new AssetReferenceGameObject("")
                }

            );

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "MainWheelL",
        localPos = new Vector3(0.36897F, 0.36324F, 1.23123F),
        localAngles = new Vector3(41.33593F, 187.2521F, 346.4221F),
        localScale = new Vector3(2.27587F, 2.27587F, 2.27587F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.0242F, 0.14377F, -0.04946F),
        localAngles = new Vector3(87.06909F, 186.8422F, 8.58135F),
        localScale = new Vector3(0.24249F, 0.26448F, 0.21171F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.01434F, 0.13129F, -0.02574F),
        localAngles = new Vector3(53.08321F, 357.4543F, 175.021F),
        localScale = new Vector3(0.1554F, 0.1554F, 0.1554F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(-0.00028F, 0.08797F, -0.0002F),
        localAngles = new Vector3(52.15151F, 24.80481F, 192.2515F),
        localScale = new Vector3(0.13097F, 0.13097F, 0.13097F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootFrontL",
        localPos = new Vector3(0.15999F, 1.40754F, -0.10921F),
        localAngles = new Vector3(355.2609F, 307.6481F, 176.9572F),
        localScale = new Vector3(0.35525F, 0.35525F, 0.35525F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.00427F, 0.0897F, -0.03953F),
        localAngles = new Vector3(40.3113F, 1.81888F, 178.2181F),
        localScale = new Vector3(0.15767F, 0.23143F, 0.17403F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(-0.12647F, 1.13434F, -1.59064F),
        localAngles = new Vector3(16.7209F, 7.55004F, 179.9179F),
        localScale = new Vector3(2.16119F, 2.16119F, 2.16119F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }
);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.04277F, 0.16665F, -0.05733F),
        localAngles = new Vector3(26.59792F, 335.7987F, 182.9781F),
        localScale = new Vector3(0.17828F, 0.17828F, 0.17828F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.02345F, 0.13827F, -0.08909F),
        localAngles = new Vector3(37.59599F, 9.59647F, 184.4446F),
        localScale = new Vector3(0.23287F, 0.25524F, 0.22414F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.01111F, 0.12926F, 0.00776F),
        localAngles = new Vector3(57.43624F, 288.4187F, 187.9462F),
        localScale = new Vector3(0.17321F, 0.17321F, 0.17321F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootL",
        localPos = new Vector3(0.01803F, 0.10702F, 0.0056F),
        localAngles = new Vector3(50.7015F, 267.4674F, 187.5229F),
        localScale = new Vector3(0.13742F, 0.13742F, 0.13742F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Wheel",
        localPos = new Vector3(0.71467F, -0.08814F, -0.00931F),
        localAngles = new Vector3(293.512F, 301.6905F, 148.5624F),
        localScale = new Vector3(0.21422F, 0.21422F, 0.21422F),

        followerPrefab = crocsClassicIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }
}