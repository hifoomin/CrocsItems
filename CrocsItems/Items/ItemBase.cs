using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace CrocsItems.Items
{
    public abstract class ItemBase<T> : ItemBase where T : ItemBase<T>
    {
        public static T instance { get; private set; }

        public ItemBase()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inherting ItemBase was instantiated twice");
            }
            instance = this as T;
        }
    }

    public abstract class ItemBase
    {
        public abstract string ItemName { get; }
        public abstract string ItemLangTokenName { get; }
        public abstract string ItemPickupDesc { get; }
        public abstract string ItemFullDescription { get; }
        public abstract string ItemLore { get; }

        public abstract ItemTier Tier { get; }
        public abstract ItemTag[] ItemTags { get; }

        public abstract GameObject ItemModel { get; }
        public abstract Sprite ItemIcon { get; }
        public abstract bool IsCroc { get; }

        public virtual bool CanRemove { get; } = true;
        public virtual bool IsJibbit { get; } = false;

        public virtual float modelPanelParametersMinDistance { get; } = 2f;
        public virtual float modelPanelParametersMaxDistance { get; } = 6f;

        public ItemDef ItemDef;

        public static bool DefaultEnabledCallback(ItemBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind<bool>(attribute.name, "Enabled", true, "Allow this item to appear in runs?").Value;
                if (isValid)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public virtual void Init()
        {
            CreateItem();
            Hooks();
        }

        public virtual void Hooks()
        { }

        protected void CreateItem()
        {
            // var temporaryItemIcon = Main.bundle.LoadAsset<Sprite>("texItemTemp.png");
            // var temporaryItemModel = Main.bundle.LoadAsset<GameObject>("TempHolder.prefab");

            ItemDef = ScriptableObject.CreateInstance<ItemDef>();
            ItemDef.name = "ITEM_CROCSITEMS_" + ItemLangTokenName;
            ItemDef.nameToken = "ITEM_CROCSITEMS_" + ItemLangTokenName + "_NAME";
            ItemDef.pickupToken = "ITEM_CROCSITEMS_" + ItemLangTokenName + "_PICKUP";
            ItemDef.descriptionToken = "ITEM_CROCSITEMS_" + ItemLangTokenName + "_DESCRIPTION";
            ItemDef.loreToken = "ITEM_CROCSITEMS_" + ItemLangTokenName + "_LORE";
#pragma warning disable
            ItemDef.pickupModelPrefab = ItemModel ?? null;
            ItemDef.pickupIconSprite = ItemIcon ?? null;
            ItemDef.hidden = false;
            ItemDef.canRemove = CanRemove;

            ItemDef.deprecatedTier = Tier;
            // ItemDef.requiredExpansion = Main.CROCSExpansionDef;
            if (ItemTags.Length > 0)
            {
                ItemDef.tags = ItemTags;
            }

            LanguageAPI.Add("ITEM_CROCSITEMS_" + ItemLangTokenName + "_NAME", ItemName);
            LanguageAPI.Add("ITEM_CROCSITEMS_" + ItemLangTokenName + "_PICKUP", ItemPickupDesc);
            LanguageAPI.Add("ITEM_CROCSITEMS_" + ItemLangTokenName + "_DESCRIPTION", ItemFullDescription);
            LanguageAPI.Add("ITEM_CROCSITEMS_" + ItemLangTokenName + "_LORE", ItemLore);

            /*
            if (AchievementName != string.Empty && AchievementDesc != string.Empty)
            {
                LanguageAPI.Add("ACHIEVEMENT_ITEM_CROCSITEMS" + ItemLangTokenName + "_NAME", AchievementName);
                LanguageAPI.Add("ACHIEVEMENT_ITEM_CROCSITEMS" + ItemLangTokenName + "_DESCRIPTION", AchievementDesc);

                ItemDef.unlockableDef = CreateUnlock();
            }
            */

            if (ItemModel != null)
            {
                CreateModelPanelParameters(ItemModel);
            }

            ItemAPI.Add(new CustomItem(ItemDef, CreateItemDisplayRules()));

            if (IsJibbit)
            {
                Main.jibbitzList.Add(ItemDef);
            }

            if (IsCroc)
            {
                Main.crocsList.Add(ItemDef);
            }
        }

        private void CreateModelPanelParameters(GameObject itemModel)
        {
            if (itemModel.GetComponent<ModelPanelParameters>() != null)
            {
                return;
            }

            GameObject model = PrefabAPI.InstantiateClone(itemModel, itemModel.name + "-fixed", false);
            GameObject focus = new("Focus");
            GameObject camera = new("Camera");
            MeshRenderer biggestRenderer = model.GetComponentsInChildren<MeshRenderer>().ToList().OrderByDescending(x => ToFloat(x.bounds.size)).First();
            float mult = ToFloat(biggestRenderer.bounds.size) / 3f;
            float min = mult;
            float max = 3f * mult;
            focus.transform.parent = model.transform;
            camera.transform.parent = model.transform;
            focus.transform.position = biggestRenderer.bounds.center;
            camera.transform.localPosition = focus.transform.position + (model.transform.forward * max);

            var modelPanelParameters = model.AddComponent<ModelPanelParameters>();
            modelPanelParameters.focusPointTransform = focus.transform;
            modelPanelParameters.cameraPositionTransform = camera.transform;
            modelPanelParameters.minDistance = min;
            modelPanelParameters.maxDistance = max;

            ItemDef.pickupModelPrefab = model;
        }

        public static float ToFloat(Vector3 vec)
        {
            vec.x = Mathf.Abs(vec.x);
            vec.y = Mathf.Abs(vec.y);
            vec.z = Mathf.Abs(vec.z);
            return vec.x + vec.y + vec.z;
        }

        public int GetCount(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCount(ItemDef);
        }

        public int GetCountEffective(CharacterBody body)
        {
            if (!body || !body.inventory) { return 0; }

            return body.inventory.GetItemCountEffective(ItemDef);
        }

        public string GetConfName()
        {
            ConfigSectionAttribute attribute = this.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                return attribute.name;
            }
            else
            {
                return "Items :: " + ItemName;
            }
        }

        public abstract ItemDisplayRuleDict CreateItemDisplayRules();
    }
}