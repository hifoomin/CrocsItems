using CrocsItems.Items;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using static R2API.DirectorAPI;
using static Rewired.UI.ControlMapper.ControlMapper;
using Stage = R2API.DirectorAPI.Stage;

namespace CrocsItems.Interactables
{
    [ConfigSection("Interactables :: Shrine of Crocs")]
    internal class ShrineofCrocs : InteractableBase<ShrineofCrocs>
    {
        public override string Name => "Shrine of Crocs";

        public override InteractableCategory Category => InteractableCategory.Shrines;

        public override int MaxSpawnsPerStage => 1;

        public override int CreditCost => 35;

        public override HullClassification Size => HullClassification.Golem;

        public override int MinimumStageToAppearOn => 1;

        public override int SpawnWeight => 1;

        public GameObject prefab;

        public override bool OrientToFloor => true;
        public override bool SkipOnSacrifice => true;

        public override bool SpawnInVoid => true;

        public override bool SpawnInSimulacrum => true;

        public override bool SlightlyRandomizeOrientation => false;

        public override string inspectInfoDescription => $"When activated by a survivor, the Shrine of Crocs drops a random Croc item, or a Jibbit if they already have a Croc item.";

        public static GameObject shrineVFX;

        public static CostTypeIndex costTypeIndex = CostTypeIndex.Money;

        public override List<Stage> Stages { get; } = new() { Stage.AbandonedAqueduct, Stage.AbyssalDepths, Stage.AphelianSanctuary, Stage.DistantRoost, Stage.DisturbedImpact, Stage.GildedCoast, Stage.GoldenDieback, Stage.HelminthHatchery, Stage.RallypointDelta, Stage.ReformedAltar, Stage.ScorchedAcres, Stage.ShatteredAbodes, Stage.SiphonedForest, Stage.SirensCall, Stage.SkyMeadow, Stage.SulfurPools, Stage.SunderedGrove, Stage.TitanicPlains, Stage.TreebornColony, Stage.VerdantFalls, Stage.ViscousFalls, Stage.WetlandAspect };

        public override void Init()
        {
            base.Init();

            prefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("afcf09fce0fd504498c42cc15c6e77ef").WaitForCompletion(), "Shrine of Crocs", true);
            // guid is shrine blood
            var baseline = prefab.transform.Find("Base");
            baseline.localPosition = new Vector3(0f, 2.55f, 0f);

            var mdl = baseline.Find("mdlShrineHealing");
            mdl.name = "mdlShrineCrocs";
            mdl.localPosition = Vector3.zero;
            mdl.localScale = Vector3.one * 1.75f;

            baseline.Find("Decal").gameObject.SetActive(false);

            var meshFilter = mdl.GetComponent<MeshFilter>();
            meshFilter.sharedMesh = Main.bundle.LoadAsset<Mesh>("meshShrineOfCrocs.fbx");

            var meshRenderer = mdl.GetComponent<MeshRenderer>();
            var sharedMaterials = meshRenderer.sharedMaterials; // dumbass moment
            Array.Resize(ref sharedMaterials, 2);

            sharedMaterials[0] = Main.bundle.LoadAsset<Material>("matShrineOfCrocsPillar.mat");
            sharedMaterials[1] = Main.bundle.LoadAsset<Material>("matShrineOfCrocsCroc.mat");

            meshRenderer.sharedMaterials = sharedMaterials;

            var symbol = prefab.transform.Find("Symbol");
            symbol.localPosition = new Vector3(0f, 10f, 0f);
            symbol.localScale = Vector3.one * 4f;

            var symbolMeshRenderer = symbol.GetComponent<MeshRenderer>();
            symbolMeshRenderer.material.mainTexture = Main.bundle.LoadAsset<Texture2D>("texShrineOfCrocsSymbol.png");
            symbolMeshRenderer.material.SetColor("_TintColor", new Color32(255, 122, 0, 255));
            symbolMeshRenderer.material.SetTexture("_RemapTex", Main.texRampTritone2);

            shrineVFX = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("0fa235b9d7e778f4ba2cd8f2437f72d9").WaitForCompletion(), "Shrine of Crocs VFX", false);
            // guid is shrine use effect
            shrineVFX.GetComponent<EffectComponent>().soundName = "Play_item_void_clover";
            ContentAddition.AddEffect(shrineVFX);

            var purchaseInteraction = prefab.GetComponent<PurchaseInteraction>();
            purchaseInteraction.displayNameToken = "CROCSITEMS_SHRINE_CROCS_NAME";
            purchaseInteraction.contextToken = "CROCSITEMS_SHRINE_CROCS_CONTEXT";
            purchaseInteraction.Networkavailable = true;
            purchaseInteraction.costType = costTypeIndex;
            purchaseInteraction.cost = 75;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();
            genericDisplayNameProvider.displayToken = "CROCSITEMS_SHRINE_CROCS_NAME";

            UnityEngine.Object.DestroyImmediate(prefab.GetComponent<ShrineBloodBehavior>());

            prefab.AddComponent<ShrineOfCrocsController>();

            prefab.AddComponent<ShrineOfCrocsUnityEventActivator>();

            PrefabAPI.RegisterNetworkPrefab(prefab);

            LanguageAPI.Add("CROCSITEMS_SHRINE_CROCS_NAME", "Shrine of Crocs");
            LanguageAPI.Add("CROCSITEMS_SHRINE_CROCS_CONTEXT", "Offer to Shrine of Crocs");

            LanguageAPI.Add("CROCSITEMS_SHRINE_CROCS_DESCRIPTION", "When activated by a survivor, the Shrine of Crocs drops a random Croc item, or a Jibbit if they already have a Croc item.");

            LanguageAPI.Add("CROCSITEMS_SHRINE_CROCS_USE_MESSAGE_2P", "<style=cShrine>You offer to the Shrine of Crocs and are greatly rewarded!</color>");
            LanguageAPI.Add("CROCSITEMS_SHRINE_CROCS_USE_MESSAGE", "<style=cShrine>{0} offered to the Shrine of Crocs and was greatly rewarded!.</color>");

            interactableSpawnCard.prefab = prefab;

            PostInit();
        }
    }

    public class ShrineOfCrocsUnityEventActivator : MonoBehaviour
    {
        public PurchaseInteraction purchaseInteraction;
        public ShrineOfCrocsController shrineCrocsBehavior;

        public void Start()
        {
            shrineCrocsBehavior = GetComponent<ShrineOfCrocsController>();
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            purchaseInteraction.costType = ShrineofCrocs.costTypeIndex;
            purchaseInteraction.onPurchase.AddListener(InteractListener);
        }

        public void InteractListener(Interactor interactor)
        {
            shrineCrocsBehavior.AddShrineStack(interactor);
        }
    }

    public class ShrineOfCrocsController : ShrineBehavior
    {
        public int maxPurchaseCount = 1;

        public float costMultiplierPerPurchase;

        public Transform symbolTransform;

        private PurchaseInteraction purchaseInteraction;

        private int purchaseCount;

        private float refreshTimer;

        private const float refreshDuration = 2f;

        private bool waitingForRefresh;

        public int itemCount = 1;

        public override int GetNetworkChannel()
        {
            return RoR2.Networking.QosChannelIndex.defaultReliable.intVal;
        }

        private void Start()
        {
            // Main.ModLogger.LogError("shrine sacrifice behavior start");
            purchaseInteraction = GetComponent<PurchaseInteraction>();
            symbolTransform = transform.Find("Symbol");
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0f && purchaseCount < maxPurchaseCount && Run.instance)
                {
                    purchaseInteraction.SetAvailable(true);
                    purchaseInteraction.Networkcost = Run.instance.GetDifficultyScaledCost(75);
                    waitingForRefresh = false;
                }
            }
        }

        public void AddShrineStack(Interactor interactor)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            waitingForRefresh = true;
            var interactorBody = interactor.GetComponent<CharacterBody>();

            // Main.ModLogger.LogError("random white pickupindex is " + dropPickup);

            ItemDef randomJibbit = null;
            // EquipmentDef randomJibbitEquipment = null;
            ItemDef randomCrocItem = null;
            EquipmentDef randomCrocEquipment = null;

            if (Run.instance)
            {
                if (Main.HasAnyCrocs(interactorBody) || Main.HasAnyCrocsEquipment(interactorBody))
                {
                    randomJibbit = Main.jibbitzList[Run.instance.stageRng.RangeInt(0, Main.jibbitzList.Count)];
                }
                else
                {
                    if (Run.instance.stageRng.RangeInt(0, 100) <= 33)
                    {
                        randomCrocEquipment = Main.crocsListEquipment[Run.instance.stageRng.RangeInt(0, Main.crocsList.Count)];
                    }
                    else
                    {
                        randomCrocItem = Main.crocsList[Run.instance.stageRng.RangeInt(0, Main.crocsList.Count)];
                    }
                }
            }

            PickupIndex finalPickup = PickupIndex.none;

            if (randomJibbit != null)
            {
                finalPickup = PickupCatalog.FindPickupIndex(randomJibbit.itemIndex);
            }
            if (randomCrocItem != null)
            {
                finalPickup = PickupCatalog.FindPickupIndex(randomCrocItem.itemIndex);
            }
            if (randomCrocEquipment != null)
            {
                finalPickup = PickupCatalog.FindPickupIndex(randomCrocEquipment.equipmentIndex);
            }

            float angle = 360f / itemCount;
            Vector3 vector = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * (Vector3.up * 40f + Vector3.forward * 5f);
            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);

            for (int i = 0; i < itemCount; i++)
            {
                GenericPickupController.CreatePickupInfo info = new()
                {
                    position = transform.position + new Vector3(0, 3f, 0),
                    rotation = Quaternion.identity,
                    pickupIndex = finalPickup
                };

                PickupDropletController.CreatePickupDroplet(info, transform.position + new Vector3(0, 3f, 0), vector);
                vector = quaternion * vector;
            }

            Util.PlaySound("Play_item_proc_clover", gameObject);
            Util.PlaySound("Play_item_proc_clover", gameObject);
            Util.PlaySound("Play_item_proc_clover", gameObject);
            Util.PlaySound("Play_item_proc_clover", gameObject);
            Util.PlaySound("Play_item_proc_clover", gameObject);

            EffectManager.SpawnEffect(ShrineofCrocs.shrineVFX, new EffectData
            {
                origin = base.transform.position,
                rotation = Quaternion.identity,
                scale = 1.5f,
                color = new Color32(132, 189, 0, 255)
            }, true);

            purchaseCount++;
            refreshTimer = 2f;
            if (purchaseCount >= maxPurchaseCount)
            {
                symbolTransform.gameObject.SetActive(false);
                CallRpcSetPingable(false);
            }
        }

        private void UNetVersion()
        { }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            return base.OnSerialize(writer, forceAll);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            base.OnDeserialize(reader, initialState);
        }

        public override void PreStartClient()
        {
            base.PreStartClient();
        }
    }
}