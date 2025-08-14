using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace CrocsItems.Items.Reds
{
    [ConfigSection("Items :: Crocs Sandals")]
    public class CrocsSandals : ItemBase<CrocsSandals>
    {
        public override string ItemName => "Crocs Sandals";

        public override string ItemLangTokenName => "CROCS_SANDALS";

        public override string ItemPickupDesc => "Attacks heal all allies for a percentage of damage dealt. Sustained attacks buff all allies.";

        public override string ItemFullDescription => "Attacks <style=cIsHealing>heal</style> all allies for <style=cIsHealing>1%</style> <style=cStack>(+1% per stack)</style> of damage dealt.";

        public override string ItemLore => "This item should have lore.";

        public override ItemTier Tier => ItemTier.Tier3;

        public override ItemTag[] ItemTags => [ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.Utility, ItemTag.Damage];

        public override GameObject ItemModel => null;

        public override Sprite ItemIcon => null;

        public static BuffDef speedBuff;

        public override ItemDisplayRuleDict CreateItemDisplayRules()
        {
            return new ItemDisplayRuleDict();
        }

        public override void Init()
        {
            base.Init();
            SetUpBuff();
        }

        public void SetUpBuff()
        {
            speedBuff = ScriptableObject.CreateInstance<BuffDef>();
            speedBuff.isHidden = false;
            speedBuff.isDebuff = false;
            speedBuff.canStack = true;
            speedBuff.buffColor = Color.white;
            speedBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("2508a4654959d334aaca7d4321922642").WaitForCompletion();
            speedBuff.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
            speedBuff.ignoreGrowthNectar = false;
            speedBuff.isDOT = false;
            speedBuff.isCooldown = false;

            ContentAddition.AddBuffDef(speedBuff);
        }

        public void SetUpVFX()
        {
        }

        public override void Hooks()
        {
            base.Hooks();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void OnServerDamageDealt(DamageReport report)
        {
            // sphere search?
            // create orbs that heal?
        }
    }
}