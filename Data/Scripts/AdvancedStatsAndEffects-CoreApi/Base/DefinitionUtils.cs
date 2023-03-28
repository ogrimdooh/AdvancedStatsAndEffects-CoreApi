using Sandbox.Definitions;
using Sandbox.Game.EntityComponents;
using System;
using System.Linq;
using VRage.Game;
using VRage.Game.ObjectBuilders.ComponentSystem;
using VRage.Utils;
using VRageMath;

namespace AdvancedStatsAndEffects
{
    public static class DefinitionUtils
    {

        public static MyBlueprintDefinitionBase TryGetBlueprintDefinition(string bluePrintName)
        {
            try
            {
                var definitionID = new MyDefinitionId(typeof(MyObjectBuilder_BlueprintDefinition), bluePrintName);
                return MyDefinitionManager.Static.GetBlueprintDefinition(definitionID);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static T TryGetDefinition<T>(string subtype) where T : MyDefinitionBase
        {
            try
            {
                var lista = MyDefinitionManager.Static.GetDefinitionsOfType<T>();
                var subtypehash = MyStringHash.Get(subtype);
                return lista.FirstOrDefault(x => x.Id.SubtypeId == subtypehash);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static T TryGetDefinition<T>(MyDefinitionId id) where T : MyDefinitionBase
        {
            try
            {
                return MyDefinitionManager.Static.GetDefinition(id) as T;
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static bool IsDefinitionExists(MyDefinitionId id)
        {
            try
            {
                return MyDefinitionManager.Static.GetAllDefinitions().Any(x => x.Id == id);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
                return false;
            }
        }

        public static T TryGetDefinitionBySubType<T>(string subtype) where T : MyDefinitionBase
        {
            try
            {
                return MyDefinitionManager.Static.GetAllDefinitions<T>().FirstOrDefault(x => x.Id.SubtypeName == subtype);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static MyBlockVariantGroup GetBlockVariantGroup(string subtype)
        {
            try
            {
                var query = MyDefinitionManager.Static.GetBlockVariantGroupDefinitions();
                if (query.Any(x => x.Key == subtype))
                    return query.FirstOrDefault(x => x.Key == subtype).Value;
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static MySpawnGroupDefinition GetSpawnGroup(string subtype)
        {
            try
            {
                var query = MyDefinitionManager.Static.GetSpawnGroupDefinitions();
                if (query.Any(x => x.Id.SubtypeName == subtype))
                    return query.FirstOrDefault(x => x.Id.SubtypeName == subtype);
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
            return null;
        }

        public static void ChangeContainerTypeDefinition(string lootname, Vector2I count, UniqueEntityId[] lootsToRemove, params MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem[] loots)
        {
            try
            {
                var lootDefinition = MyDefinitionManager.Static.GetContainerTypeDefinition(lootname);
                if (lootDefinition != null)
                {
                    var builder = MyDefinitionManagerBase.GetObjectFactory().CreateObjectBuilder<MyObjectBuilder_ContainerTypeDefinition>(lootDefinition);
                    var baseBuilder = lootDefinition.GetObjectBuilder();
                    builder.Id = baseBuilder.Id;
                    builder.Description = baseBuilder.Description;
                    builder.DisplayName = baseBuilder.DisplayName;
                    builder.Icons = baseBuilder.Icons;
                    builder.Public = baseBuilder.Public;
                    builder.Enabled = baseBuilder.Enabled;
                    builder.AvailableInSurvival = baseBuilder.AvailableInSurvival;
                    builder.CountMin = count.X;
                    builder.CountMax = count.Y;
                    var lista = lootDefinition.Items.Where(x => !lootsToRemove.Any(y => y.DefinitionId == x.DefinitionId))
                        .Select(x => GetLootItem(new Vector2((float)x.AmountMin, (float)x.AmountMax), new UniqueEntityId(x.DefinitionId), x.Frequency))
                        .ToList();
                    lista.AddRange(loots);
                    builder.Items = lista.ToArray();
                    lootDefinition.Init(builder, lootDefinition.Context);
                    lootDefinition.DeselectAll();
                }
                else
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"RemoveItensFromContainer: {lootname} Not Found");
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
        }

        public static void ReplaceContainerTypeDefinition(string lootname, Vector2I count, bool replace, params MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem[] loots)
        {
            try
            {
                var lootDefinition = MyDefinitionManager.Static.GetContainerTypeDefinition(lootname);
                if (lootDefinition != null)
                {
                    var builder = MyDefinitionManagerBase.GetObjectFactory().CreateObjectBuilder<MyObjectBuilder_ContainerTypeDefinition>(lootDefinition);
                    var baseBuilder = lootDefinition.GetObjectBuilder();
                    builder.Id = baseBuilder.Id;
                    builder.Description = baseBuilder.Description;
                    builder.DisplayName = baseBuilder.DisplayName;
                    builder.Icons = baseBuilder.Icons;
                    builder.Public = baseBuilder.Public;
                    builder.Enabled = baseBuilder.Enabled;
                    builder.AvailableInSurvival = baseBuilder.AvailableInSurvival;
                    builder.CountMin = count.X;
                    builder.CountMax = count.Y;
                    if (replace)
                        builder.Items = loots;
                    else
                    {
                        var lista = lootDefinition.Items.Select(x => GetLootItem(new Vector2((float)x.AmountMin, (float)x.AmountMax), new UniqueEntityId(x.DefinitionId), x.Frequency)).ToList();
                        lista.AddRange(loots);
                        builder.Items = lista.ToArray();
                    }
                    lootDefinition.Init(builder, lootDefinition.Context);
                    lootDefinition.DeselectAll();
                }
                else
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"ReplaceLoot: {lootname} Not Found");
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
        }

        public static void ChangeInventoryContainerType(string botname, string lootname)
        {
            try
            {
                var botDef = TryGetDefinition<MyAnimalBotDefinition>(botname);
                if (botDef != null)
                {
                    botDef.InventoryContentGenerated = true;
                    botDef.InventoryContainerTypeId = new MyDefinitionId(typeof(MyObjectBuilder_ContainerTypeDefinition), MyStringHash.GetOrCompute(lootname));
                }
                else
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"ChangeBotLoot: botname={botname} Not Found");
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
        }

        public static void ChangeStatValue(string statname, Vector3 values)
        {
            try
            {
                var statDef = TryGetDefinition<MyEntityStatDefinition>(statname);
                if (statDef != null)
                {
                    statDef.MinValue = values.X;
                    statDef.MaxValue = values.Y;
                    statDef.DefaultValue = values.Z;
                }
                else
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"ChangeStatValue: {statname} Not Found");
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
        }

        public static void AddStatToCharacter(string statname, string charname)
        {
            try
            {
                var def = MyDefinitionManager.Static.GetEntityComponentDefinition(new MyDefinitionId(typeof(MyObjectBuilder_CharacterStatComponent), charname)) as MyEntityStatComponentDefinition;
                if (def != null)
                {
                    var statDef = TryGetDefinition<MyEntityStatDefinition>(statname);
                    if (statDef != null)
                    {
                        def.Stats.Add(statDef.Id);
                    }
                    else
                        AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"AddStatToCharacter: statname={statname} Not Found");
                }
                else
                    AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(DefinitionUtils), $"AddStatToCharacter: charname={charname} Not Found");
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(DefinitionUtils), ex);
            }
        }

        public static MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem GetLootItem(Vector2 ammount, UniqueEntityId id, float frequency)
        {
            return new MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem()
            {
                AmountMin = ammount.X.ToString(),
                AmountMax = ammount.Y.ToString(),
                Id = id.DefinitionId,
                Frequency = frequency
            };
        }

    }

}