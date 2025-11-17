using System;
using System.Collections;
using System.Collections.Generic;
using CrocsItems.Items;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CrocsItems
{
    public class JibbitzDropBehavior
    {
        public static List<PickupDropTable> acceptableDropTables = new();
        public static void Init()
        {
            On.RoR2.ChestBehavior.BaseItemDrop += OnChestItemDrop;
            acceptableDropTables.Add(Addressables.LoadAssetAsync<BasicPickupDropTable>("944f7e92dae9bac4d96e05635dcff090").WaitForCompletion());
            // guid is dt tier 1 item
            acceptableDropTables.Add(Addressables.LoadAssetAsync<BasicPickupDropTable>("7a61dd0f9e87dab4d9828371f75bf253").WaitForCompletion());
            // guid is dt tier 2 item
            acceptableDropTables.Add(Addressables.LoadAssetAsync<BasicPickupDropTable>("abd505260a23e9b449202c055554b77b").WaitForCompletion());
            // guid is dt tier 3 item
        }

        private static void OnChestItemDrop(On.RoR2.ChestBehavior.orig_BaseItemDrop orig, ChestBehavior self)
        {
            orig(self);
            if (NetworkServer.active && Util.CheckRoll(10f) && acceptableDropTables.Contains(self.dropTable))
            {
                bool anyoneHasAnyCrocsItem = false;
                for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
                {
                    var body = CharacterBody.readOnlyInstancesList[i];
                    if (Main.HasAnyCrocs(body) || Main.HasAnyCrocsEquipment(body))
                    {
                        anyoneHasAnyCrocsItem = true;
                        break;
                    }
                }

                if (Run.instance && anyoneHasAnyCrocsItem)
                {
                    var randomJibbit = Main.jibbitzList[Run.instance.stageRng.RangeInt(0, Main.jibbitzList.Count)];
                    var randomJibbitPickup = PickupCatalog.FindPickupIndex(randomJibbit.itemIndex);

                    var position = self.dropTransform.position + Vector3.up * 1.5f;
                    var velocity = Vector3.up * self.dropUpVelocityStrength + self.dropTransform.forward * self.dropForwardVelocityStrength;

                    var newPickupInfo = new GenericPickupController.CreatePickupInfo();
                    newPickupInfo.rotation = Quaternion.identity;
                    newPickupInfo.pickupIndex = randomJibbitPickup;
                    newPickupInfo.position = position;
                    PickupDropletController.CreatePickupDroplet(newPickupInfo, position, velocity);
                }
            }
        }
    }
}