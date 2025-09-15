using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DirectorAPI;
using static Rewired.UI.ControlMapper.ControlMapper;
using Stage = R2API.DirectorAPI.Stage;

namespace CrocsItems.Interactables
{
    public abstract class InteractableBase<T> : InteractableBase where T : InteractableBase<T>
    {
        public static T Instance { get; private set; }

        public InteractableBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class InteractableBase
    {
        public DirectorCard directorCard;
        public InteractableSpawnCard interactableSpawnCard;
        public DirectorCardHolder directorCardHolder;
        public abstract string Name { get; }
        public abstract InteractableCategory Category { get; }
        public abstract int MaxSpawnsPerStage { get; }
        public abstract int CreditCost { get; }
        public abstract HullClassification Size { get; }
        public abstract int MinimumStageToAppearOn { get; }
        public abstract int SpawnWeight { get; }
        public abstract string inspectInfoDescription { get; }
        public virtual List<Stage> Stages { get; } = new() { Stage.AbandonedAqueduct, Stage.AbyssalDepths, Stage.AphelianSanctuary, Stage.DistantRoost, Stage.DisturbedImpact, Stage.GildedCoast, Stage.GoldenDieback, Stage.HelminthHatchery, Stage.RallypointDelta, Stage.ReformedAltar, Stage.ScorchedAcres, Stage.ShatteredAbodes, Stage.SiphonedForest, Stage.SirensCall, Stage.SkyMeadow, Stage.SulfurPools, Stage.SunderedGrove, Stage.TitanicPlains, Stage.TreebornColony, Stage.VerdantFalls, Stage.ViscousFalls, Stage.WetlandAspect };
        public virtual bool SpawnInSimulacrum { get; } = false;
        public virtual bool SpawnInVoid { get; } = false;
        public virtual bool SpawnOnCommencement { get; } = false;
        public virtual bool SkipOnSacrifice { get; } = false;
        public virtual float SacrificeWeightMultiplier { get; } = 1f;
        public virtual bool OrientToFloor { get; } = true;
        public virtual bool SlightlyRandomizeOrientation { get; } = true;

        public virtual void Init()
        {
            interactableSpawnCard = ScriptableObject.CreateInstance<InteractableSpawnCard>();
        }

        public void PostInit()
        {
            if (SpawnInSimulacrum)
            {
                Stages.Add(Stage.TitanicPlainsSimulacrum);
                Stages.Add(Stage.AbandonedAqueductSimulacrum);
                Stages.Add(Stage.AphelianSanctuarySimulacrum);
                Stages.Add(Stage.RallypointDeltaSimulacrum);
                Stages.Add(Stage.AbyssalDepthsSimulacrum);
                Stages.Add(Stage.SkyMeadowSimulacrum);
                Stages.Add(Stage.CommencementSimulacrum);
            }

            if (SpawnInVoid)
            {
                Stages.Add(Stage.VoidCell);
                Stages.Add(Stage.VoidLocus);
            }

            interactableSpawnCard.skipSpawnWhenSacrificeArtifactEnabled = SkipOnSacrifice;
            interactableSpawnCard.maxSpawnsPerStage = MaxSpawnsPerStage;
            interactableSpawnCard.directorCreditCost = CreditCost;
            interactableSpawnCard.hullSize = Size;
            interactableSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.NoChestSpawn;
            interactableSpawnCard.slightlyRandomizeOrientation = SlightlyRandomizeOrientation;
            interactableSpawnCard.weightScalarWhenSacrificeArtifactEnabled = SacrificeWeightMultiplier;
            interactableSpawnCard.occupyPosition = true;
            interactableSpawnCard.orientToFloor = OrientToFloor;
            interactableSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            interactableSpawnCard.sendOverNetwork = true;
            interactableSpawnCard.name = "isc" + Name.Replace(" ", "");

            directorCard = new() { spawnCard = interactableSpawnCard, minimumStageCompletions = MinimumStageToAppearOn - 1, selectionWeight = SpawnWeight };

            directorCardHolder = new() { Card = directorCard, InteractableCategory = Category };

            if (Stages != null)
            {
                for (int i = 0; i < Stages.Count; i++)
                {
                    var stage = Stages[i];
                    // Main.ModLogger.LogError($"Adding {interactableSpawnCard.name} to stage {stage}");
                    Helpers.AddNewInteractableToStage(directorCardHolder, stage);
                }
            }

            var prefab = interactableSpawnCard.prefab;

            if (prefab.GetComponent<GenericInspectInfoProvider>() != null)
            {
                GameObject.DestroyImmediate(prefab.GetComponent<GenericInspectInfoProvider>());
            }

            var genericInspectInfoProvider = prefab.AddComponent<GenericInspectInfoProvider>();
            genericInspectInfoProvider.enabled = true;

            var genericDisplayNameProvider = prefab.GetComponent<GenericDisplayNameProvider>();

            var descToken = "CROCSITEMS_" + Name.ToUpper();
            descToken = descToken.Replace(" ", "_") + "_DESCRIPTION";

            LanguageAPI.Add(descToken, inspectInfoDescription);

            var shrineIcon = Addressables.LoadAssetAsync<Sprite>("13b0407e61597f24497f3832ad9231d8").WaitForCompletion();
            // guid is tex shrine icon outlined

            var inspectDef = ScriptableObject.CreateInstance<InspectDef>();
            inspectDef.name = prefab.name + "InspectDef";
            var inspectInfo = inspectDef.Info = new()
            {
                TitleToken = genericDisplayNameProvider.displayToken,
                DescriptionToken = descToken,
                FlavorToken = "crocs",
                isConsumedItem = false,
                Visual = shrineIcon,
                TitleColor = Color.white
            };

            genericInspectInfoProvider.InspectInfo = inspectDef;
            genericInspectInfoProvider.InspectInfo.Info = inspectInfo;

            // Main.ModLogger.LogError($"directorCard is {directorCard}");
            // Main.ModLogger.LogError($"directorCardHolder is {directorCardHolder}");
        }

        public static bool DefaultEnabledCallback(InteractableBase self)
        {
            ConfigSectionAttribute attribute = self.GetType().GetCustomAttribute<ConfigSectionAttribute>();
            if (attribute != null)
            {
                bool isValid = Main.config.Bind(attribute.name, "Enabled", true, "Allow this interactable to appear in runs?").Value;
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
    }
}