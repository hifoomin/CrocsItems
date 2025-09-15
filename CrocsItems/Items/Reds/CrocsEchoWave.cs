using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrocsItems.Items.Greens;
using KinematicCharacterController;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CrocsItems.Items.Reds
{
    [ConfigSection("Items :: Crocs Echo Wave")]
    public class CrocsEchoWave : ItemBase<CrocsEchoWave>
    {
        public override string ItemName => "Crocs Echo Wave";

        public override string ItemLangTokenName => "CROCS_ECHO_WAVE";

        public override string ItemPickupDesc => "Sprinting builds up movement speed that can be discharged for massive impact damage.";

        public override string ItemFullDescription => "Sprinting builds up to <style=cIsUtility>100% movement speed</style>. <style=cIsDamage>Ramming</style> into an enemy while sprinting deals up to <style=cIsDamage>3000%</style> <style=cStack>(+3000% per stack)</style> <style=cIsDamage>damage</style> based on movement speed.";

        public override string ItemLore => "";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.Utility, ItemTag.Damage];

        public override GameObject ItemModel => Main.bundle.LoadAsset<GameObject>("CrocsEchoWaveHolder.prefab");

        public override Sprite ItemIcon => Main.bundle.LoadAsset<Sprite>("texCrocsEchoWave.png");

        public override bool IsCroc => true;

        public static BuffDef speedBuff;

        public static GameObject passiveParticles;

        public static GameObject impactParticles;

        public static GameObject impactExplosion;

        public override void Init()
        {
            base.Init();
            SetUpVFX();
            SetUpBuff();
        }

        public void SetUpBuff()
        {
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.isHidden = false;
            speedBuff.isDebuff = false;
            speedBuff.canStack = true;
            speedBuff.buffColor = new Color32(36, 47, 82, 255);
            speedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("3e432d63b7c55a544a0f383de7b1f474").WaitForCompletion();
            // guid is tex move speed buff icon
            speedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            speedBuff.ignoreGrowthNectar = false;
            speedBuff.isDOT = false;
            speedBuff.isCooldown = false;
            speedBuff.name = "Crocs Echo Wave Movement Speed - 1% Per";

            ContentAddition.AddBuffDef(speedBuff);
        }

        public void SetUpVFX()
        {
            passiveParticles = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("56e965f822208a1438744bae84358e1d").WaitForCompletion(), "Crocs Echo Wave Passive Particles VFX", false);
            // guid is bandit 2 smoke bomb

            passiveParticles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.ScaleToHierarchy(passiveParticles);

            VFXUtils.RecolorMaterialsAndLights(passiveParticles, Color.yellow, Color.yellow, true, true);

            var passiveParticlesTransform = passiveParticles.transform.Find("Core");
            passiveParticlesTransform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            passiveParticlesTransform.localPosition = Vector3.zero;

            var passiveParticlesSparks = passiveParticlesTransform.Find("Sparks");
            var passiveParticlesSparksPS = passiveParticlesSparks.GetComponent<ParticleSystem>();
            var passiveParticlesSparksMain = passiveParticlesSparksPS.main;
            passiveParticlesSparksMain.maxParticles = 200;
            var passiveParticlesSparksEmission = passiveParticlesSparksPS.emission;
            var passiveParticlesBurst = new ParticleSystem.Burst(0f, 200, 200, 1, 0.01f);
            passiveParticlesBurst.probability = 1f;
            passiveParticlesSparksEmission.SetBurst(0, passiveParticlesBurst);

            var passiveParticlesSparksPSR = passiveParticlesSparks.GetComponent<ParticleSystemRenderer>();
            passiveParticlesSparksPSR.material.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("8d0972db888e4df42814eb4b6178f0e2").WaitForCompletion());
            // guid is tex glow paint mask

            passiveParticlesTransform.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            passiveParticlesTransform.Find("Dust, CenterSphere").gameObject.SetActive(false);
            passiveParticlesTransform.Find("Dust, CenterTube").gameObject.SetActive(false);

            passiveParticlesTransform.Find("Point Light").gameObject.SetActive(false);

            ContentAddition.AddEffect(passiveParticles);

            impactParticles = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("56e965f822208a1438744bae84358e1d").WaitForCompletion(), "Crocs Echo Wave Impact Particles VFX", false);
            // guid is bandit 2 smoke bomb

            impactParticles.GetComponent<EffectComponent>().applyScale = true;
            VFXUtils.ScaleToHierarchy(impactParticles);

            VFXUtils.RecolorMaterialsAndLights(impactParticles, Color.yellow, Color.yellow, true, true);

            var transform = impactParticles.transform.Find("Core");
            transform.localScale = Vector3.one / 12f;// base radius at 1 scale is 12m according to bandit's util value
            transform.localPosition = Vector3.zero;

            var sparks = transform.Find("Sparks");
            var sparksPS = sparks.GetComponent<ParticleSystem>();
            var sparksMain = sparksPS.main;
            sparksMain.maxParticles = 100;
            var sparksEmission = sparksPS.emission;
            var burst = new ParticleSystem.Burst(0f, 100, 100, 1, 0.01f);
            burst.probability = 1f;
            sparksEmission.SetBurst(0, burst);

            var sparksPSR = sparks.GetComponent<ParticleSystemRenderer>();
            sparksPSR.material.SetTexture("_MainTex", Addressables.LoadAssetAsync<Texture2D>("8d0972db888e4df42814eb4b6178f0e2").WaitForCompletion());
            // guid is tex glow paint mask

            // transform.Find("Smoke, Edge Circle").gameObject.SetActive(false);
            transform.Find("Dust, CenterSphere").gameObject.SetActive(false);
            transform.Find("Dust, CenterTube").gameObject.SetActive(false);

            var pointLight = transform.Find("Point Light");

            var light = pointLight.GetComponent<Light>();
            light.intensity = 15f;
            light.range = 16f;

            ContentAddition.AddEffect(impactParticles);

            VFXUtils.MultiplyDuration(impactParticles, 4f);

            impactExplosion = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("26e8d4483ccf1484b98777af3c44ecbb").WaitForCompletion(), "Crocs Echo Wave Impact VFX", false);
            // guid is ice ring explosion
            var effectComponent = impactExplosion.GetComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.soundName = "";

            impactExplosion.transform.Find("RuneRings").gameObject.SetActive(false);

            impactExplosion.transform.Find("IceMesh").GetComponent<ParticleSystemRenderer>().mesh = Addressables.LoadAssetAsync<Mesh>("a66159248c42bad478cf4ce0379ba1ee").WaitForCompletion();
            // guid is mdl special distant planet neb station

            VFXUtils.RecolorMaterialsAndLights(impactExplosion, Color.yellow, Color.yellow, true);
            VFXUtils.MultiplyDuration(impactExplosion, 2.5f);
            VFXUtils.ScaleToHierarchy(impactExplosion);

            ContentAddition.AddEffect(impactExplosion);
        }

        public override void Hooks()
        {
            base.Hooks();
            RecalculateStatsAPI.GetStatCoefficients += CalcSpeedBoost;
            CharacterBody.onBodyInventoryChangedGlobal += OnInventoryChangedGlobal;

        }

        private void OnInventoryChangedGlobal(CharacterBody body)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            body.AddItemBehavior<CrocsEchoWaveController>(GetCount(body));
        }

        private void CalcSpeedBoost(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender)
            {
                var speedBoostBuffCount = sender.GetBuffCount(speedBuff);
                args.moveSpeedMultAdd += speedBoostBuffCount * 0.01f;
            }
        }

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            var crocsEchoWaveIDRS = PrefabAPI.InstantiateClone(Main.bundle.LoadAsset<GameObject>("CrocsEchoWaveHolder.prefab"), "CrocsEchoWaveIDRS", false);
            var itemDisplay = crocsEchoWaveIDRS.AddComponent<ItemDisplay>();
            List<Renderer> rendererList = [.. crocsEchoWaveIDRS.GetComponentsInChildren<Renderer>()];
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
                                localPos = new Vector3(-0.00387F, 0.11857F, 0.01629F),
                                localAngles = new Vector3(84.61184F, 220.3867F, 47.41245F),
                                localScale = new Vector3(0.14531F, 0.14659F, 0.14531F),

                                followerPrefab = crocsEchoWaveIDRS,
                                limbMask = LimbFlags.None,
                                followerPrefabAddress = new AssetReferenceGameObject("")
                            }

                        );

            i.Add("HuntressBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.01041F, 0.08162F, -0.00924F),
        localAngles = new Vector3(85.0407F, 197.8464F, 22.78797F),
        localScale = new Vector3(0.12683F, 0.11843F, 0.11843F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("Bandit2Body",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.00708F, 0.15599F, -0.01433F),
        localAngles = new Vector3(56.54497F, 15.6954F, 175.5474F),
        localScale = new Vector3(0.16627F, 0.16627F, 0.16627F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ToolbotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "MainWheelR",
        localPos = new Vector3(-0.18462F, -0.25801F, 1.3921F),
        localAngles = new Vector3(35.12162F, 178.6449F, 15.10357F),
        localScale = new Vector3(2.15099F, 2.15099F, 2.15099F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("EngiBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.00137F, 0.18647F, -0.01936F),
        localAngles = new Vector3(80.48362F, 131.4032F, 305.733F),
        localScale = new Vector3(0.26569F, 0.26125F, 0.24179F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MageBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.0208F, 0.17038F, 0.00367F),
        localAngles = new Vector3(50.80023F, 10.96024F, 198.6825F),
        localScale = new Vector3(0.16578F, 0.16718F, 0.16578F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("MercBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.00942F, 0.12823F, 0.00514F),
        localAngles = new Vector3(58.85978F, 356.114F, 183.1814F),
        localScale = new Vector3(0.15804F, 0.16791F, 0.15804F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("TreebotBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootFrontR",
        localPos = new Vector3(-0.23328F, 1.42811F, -0.26013F),
        localAngles = new Vector3(354.3248F, 47.10957F, 177.3136F),
        localScale = new Vector3(0.43013F, 0.43013F, 0.43013F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("LoaderBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.01876F, 0.17012F, -0.04231F),
        localAngles = new Vector3(41.75632F, 13.00825F, 191.4296F),
        localScale = new Vector3(0.20217F, 0.20217F, 0.20217F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CrocoBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.09182F, 0.8062F, -1.41886F),
        localAngles = new Vector3(15.25191F, 3.42294F, 179.4088F),
        localScale = new Vector3(2.44813F, 2.00045F, 1.90371F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("CaptainBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.00953F, 0.17557F, -0.12688F),
        localAngles = new Vector3(9.75157F, 0.82062F, 180.4495F),
        localScale = new Vector3(0.23233F, 0.20145F, 0.20145F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("RailgunnerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.00226F, 0.23116F, -0.08208F),
        localAngles = new Vector3(41.55166F, 2.0483F, 180.5456F),
        localScale = new Vector3(0.32904F, 0.28836F, 0.29128F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("VoidSurvivorBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(-0.027F, 0.16392F, -0.01953F),
        localAngles = new Vector3(56.78973F, 79.39018F, 167.5546F),
        localScale = new Vector3(0.1866F, 0.1866F, 0.1866F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("SeekerBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "FootR",
        localPos = new Vector3(0.02031F, 0.12695F, 0.00215F),
        localAngles = new Vector3(54.21278F, 280.0642F, 187.6502F),
        localScale = new Vector3(0.16319F, 0.16319F, 0.16319F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            i.Add("ChefBody",

    new ItemDisplayRule()
    {
        ruleType = ItemDisplayRuleType.ParentedPrefab,
        childName = "Wheel",
        localPos = new Vector3(-0.72718F, -0.28572F, -0.0641F),
        localAngles = new Vector3(276.9898F, 27.48667F, 240.6015F),
        localScale = new Vector3(0.26742F, 0.26742F, 0.26742F),

        followerPrefab = crocsEchoWaveIDRS,
        limbMask = LimbFlags.None,
        followerPrefabAddress = new AssetReferenceGameObject("")
    }

);

            return i;
        }
    }

    public class CrocsEchoWaveController : CharacterBody.ItemBehavior
    {
        public float timer;
        public float buffChangeInterval = 0.15f;
        public int buffCountToChange = 2;

        public int maxBuffCount = 100;
        public int minBuffCount = 100;
        public int buffCountAfterImpact = 25;
        public int buffCount;
        public float minImpactDamage = 10f;
        public float maxImpactDamage = 30f;

        public OverlapAttack attackerOverlap;

        public ModelLocator modelLocator;
        public Transform modelTransform;
        public GameObject hitBoxObject;
        public HitBoxGroup hitBoxGroup;
        public HitBox hitBox;
        public float collisionDisableTime = 0.5f;
        public int cachedLayer;
        public bool successfullyHit = false;

        public bool addedOverlay = false;

        public List<HurtBox> lastHitHurtBoxes = new();

        public TemporaryOverlayInstance temporaryOverlayInstance;

        public void Start()
        {
            cachedLayer = gameObject.layer;

            modelLocator = GetComponent<ModelLocator>();
            modelTransform = modelLocator?.modelTransform;
            if (modelTransform && hitBoxObject == null)
            {
                hitBoxObject = new("Croc Echo Waves HitBox")
                {
                    layer = LayerIndex.defaultLayer.intVal
                };

                hitBoxObject.transform.localScale = new Vector3(4f, 8f, 4f);

                hitBox = hitBoxObject.AddComponent<HitBox>();
                hitBoxGroup = hitBoxObject.AddComponent<HitBoxGroup>();
                hitBoxGroup.groupName = "CrocEchoWavesHitBox";
                hitBoxGroup.hitBoxes = [hitBox];
            }
        }

        public void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;

            if (timer >= buffChangeInterval)
            {
                if (body.isSprinting)
                {
                    buffCount += buffCountToChange;
                    buffCount = Mathf.Min(buffCount, maxBuffCount);
                    if (buffCount >= minBuffCount)
                    {
                        CheckImpact();
                        // AddOverlay();
                    }
                }
                else
                {
                    buffCount -= buffCountToChange;
                    buffCount = Mathf.Max(0, buffCount);
                }

                body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, buffCount);

                timer = 0f;
            }
        }

        public void CheckImpact()
        {
            var sprintingSpeed = 7f * 1.45f;
            var scaledDamage = Util.Remap(body.moveSpeed, sprintingSpeed, sprintingSpeed * 4f, minImpactDamage * stack, maxImpactDamage * stack); // about 50% effectiveness with this item alone -- should be doing nearly 2000% impact damage
            var finalDamage = scaledDamage;
            attackerOverlap = new()
            {
                attacker = gameObject,
                inflictor = gameObject,
                teamIndex = TeamComponent.GetObjectTeam(gameObject),
                damage = body.damage * finalDamage,
                forceVector = Vector3.zero,
                pushAwayForce = 4000f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                // impactSound = null,
                procCoefficient = 1f,
                isCrit = body.RollCrit(),
                hitBoxGroup = hitBoxObject.GetComponent<HitBoxGroup>()
            };

            hitBoxObject.transform.forward = body.inputBank.moveVector;
            hitBoxObject.transform.position = modelTransform.position;

            successfullyHit = attackerOverlap.Fire(lastHitHurtBoxes);
            if (successfullyHit)
            {
                // Main.ModLogger.LogError("attack overlap fire is true");
                buffCount = buffCountAfterImpact;
                body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, buffCountAfterImpact);
                // body.SetBuffCount(CrocsEchoWave.speedBuff.buffIndex, 0);
                // RemoveOverlay();
                SpawnVFX(lastHitHurtBoxes);
            }
        }

        public void SpawnVFX(List<HurtBox> lastHitHurtBoxes)
        {
            Util.PlaySound("Play_grandParent_attack1_boulderSmall_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_vulture_attack1_impact", gameObject);
            Util.PlaySound("Play_env_desert_wind_gust", gameObject);
            Util.PlaySound("Play_mage_m2_zap", gameObject);

            var effectData = new EffectData();
            effectData.scale = 16f + body.radius;
            effectData.origin = body.footPosition;

            EffectManager.SpawnEffect(CrocsEchoWave.impactParticles, effectData, true);

            for (int i = 0; i < lastHitHurtBoxes.Count; i++)
            {
                var lastHitHurtBox = lastHitHurtBoxes[i];

                var healthComponent = lastHitHurtBox.healthComponent;
                if (!healthComponent)
                {
                    continue;
                }

                var victimBody = healthComponent.body;
                if (!victimBody)
                {
                    continue;
                }

                var effectData2 = new EffectData();
                effectData2.scale = Mathf.Sqrt(victimBody.radius * 2f);
                effectData2.origin = victimBody.corePosition;

                EffectManager.SpawnEffect(CrocsEchoWave.impactExplosion, effectData2, true);

                var effectData3 = new EffectData();
                effectData3.scale = 8f + victimBody.radius;
                effectData3.origin = victimBody.footPosition;

                EffectManager.SpawnEffect(CrocsEchoWave.impactParticles, effectData3, true);
            }

            lastHitHurtBoxes.Clear();
        }
    }
}