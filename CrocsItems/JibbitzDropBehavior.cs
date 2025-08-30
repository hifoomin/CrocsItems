using System;
using System.Collections;
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
            On.RoR2.ChestBehavior.BaseItemDrop += OnChestItemDrop;
        }

        private static void OnChestItemDrop(On.RoR2.ChestBehavior.orig_BaseItemDrop orig, ChestBehavior self)
        {
            orig(self);
            if (NetworkServer.active && Util.CheckRoll(12f))
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

                if (Run.instance && anyoneHasAnyCrocsItem)
                {
                    var randomJibbit = ItemBase.jibbitzList[Run.instance.stageRng.RangeInt(0, ItemBase.jibbitzList.Count)];
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