using System;
using CrocsItems.Items;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace CrocsItems
{
    public class JibbitzDropBehavior
    {
        public static void Init()
        {
            On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += TryDropJibbit;

        }

        private static void TryDropJibbit(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 orig, GenericPickupController.CreatePickupInfo pickupInfo, Vector3 position, Vector3 velocity)
        {
            if (NetworkServer.active && Util.CheckRoll(10f))
            {
                bool anyoneHasAnyCrocsItem = false;
                for (int i = 0; i < CharacterBody.readOnlyInstancesList.Count; i++)
                {
                    var body = CharacterBody.readOnlyInstancesList[i];
                    if (ItemBase.HasAnyCrocs(body))
                    {
                        anyoneHasAnyCrocsItem = true;
                        break;
                    }
                }

                bool isJibbitPickup = false;
                for (int i = 0; i < ItemBase.jibbitzList.Count; i++)
                {
                    var jibbit = ItemBase.jibbitzList[i];
                    var jibbitPickup = PickupCatalog.FindPickupIndex(jibbit.itemIndex);
                    if (jibbitPickup == pickupInfo.pickupIndex)
                    {
                        isJibbitPickup = true;
                        break;
                    }
                }

                if (Run.instance && anyoneHasAnyCrocsItem && !isJibbitPickup)
                {
                    var randomJibbit = ItemBase.jibbitzList[Run.instance.stageRng.RangeInt(0, ItemBase.jibbitzList.Count)];
                    var randomJibbitPickup = PickupCatalog.FindPickupIndex(randomJibbit.itemIndex);

                    var newPickupInfo = new GenericPickupController.CreatePickupInfo();
                    newPickupInfo.rotation = Quaternion.identity;
                    newPickupInfo.pickupIndex = randomJibbitPickup;
                    newPickupInfo.position = position;
                    PickupDropletController.CreatePickupDroplet(newPickupInfo, position, velocity);
                }
            }
            orig(pickupInfo, position, velocity);
        }
    }
}