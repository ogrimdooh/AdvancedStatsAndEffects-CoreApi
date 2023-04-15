using System;
using System.Collections.Generic;
using VRageMath;
using Sandbox.ModAPI;
using VRage.Serialization;
using VRage.Game;
using VRage;
using System.IO;
using ProtoBuf;
using VRage.ModAPI;
using Sandbox.Game;
using VRage.ObjectBuilders;
using System.Linq;
using VRage.Utils;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.Game.Entities;
using Sandbox.Game.Components;

namespace AdvancedStatsAndEffects
{

    //Do not include this file in your project modders
    public class AdvancedStatsAndEffectsAPIBackend
    {

        public const int MinVersion = 1;
        public const ushort ModHandlerID = 35875;

        private static readonly Dictionary<string, Delegate> ModAPIMethods = new Dictionary<string, Delegate>()
        {
            ["VerifyVersion"] = new Func<int, string, bool>(VerifyVersion),
            ["GetGameTime"] = new Func<long>(GetGameTime),
            ["SetStatAsConsumableTrigger"] = new Func<string, bool>(SetStatAsConsumableTrigger),
            ["ConfigureConsumable"] = new Func<string, bool>(ConfigureConsumable),
            ["ConfigureFixedStat"] = new Func<string, bool>(ConfigureFixedStat),
            ["ConfigureVirtualStat"] = new Func<string, bool>(ConfigureVirtualStat),
            ["AddBeforeCycleStatCallback"] = new Func<string, Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>(AddBeforeCycleStatCallback),
            ["AddStartStatCycleCallback"] = new Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>(AddStartStatCycleCallback),
            ["AddEndStatCycleCallback"] = new Func<string, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat>, int, bool>(AddEndStatCycleCallback),
            ["AddBeforePlayersUpdateCallback"] = new Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>(AddBeforePlayersUpdateCallback),
            ["AddAfterPlayersUpdateCallback"] = new Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>(AddAfterPlayersUpdateCallback),
            ["AddBeforeCycleCallback"] = new Func<Func<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>(AddBeforeCycleCallback),
            ["AddAfterCycleCallback"] = new Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>(AddAfterCycleCallback),
            ["AddFixedStatCycleCallback"] = new Func<string, Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent>, int, bool>(AddFixedStatCycleCallback),
            ["AddVirtualStatAbsorptionCicle"] = new Func<string, Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent>, int, bool>(AddVirtualStatAbsorptionCicle),            
            ["AddAfterPlayerReset"] = new Func<Action<long, IMyCharacter, MyCharacterStatComponent>, int, bool>(AddAfterPlayerReset),
            ["AddAfterPlayerRespawn"] = new Func<Action<long, IMyCharacter, MyCharacterStatComponent, bool>, int, bool>(AddAfterPlayerRespawn),
            ["AddAfterBotAdd"] = new Func<Action<long, IMyCharacter>, int, bool>(AddAfterBotAdd),
            ["AddFixedEffect"] = new Func<long, string, byte, bool, bool>(AddFixedEffect),
            ["RemoveFixedEffect"] = new Func<long, string, byte, bool, bool>(RemoveFixedEffect),
            ["ClearOverTimeConsumable"] = new Func<long, bool>(ClearOverTimeConsumable),
            ["GetRemainOverTimeConsumable"] = new Func<long, string, float>(GetRemainOverTimeConsumable),
            ["GetLastHealthChange"] = new Func<long, Vector2>(GetLastHealthChange),
            ["DoPlayerConsume"] = new Func<long, MyDefinitionId, bool>(DoPlayerConsume),
            ["GetPlayerFixedStatStack"] = new Func<long, string, byte>(GetPlayerFixedStatStack),
            ["GetPlayerFixedStatRemainTime"] = new Func<long, string, long>(GetPlayerFixedStatRemainTime),
            ["GetPlayerFixedStatUpdateHash"] = new Func<long, int>(GetPlayerFixedStatUpdateHash)
        };

        public static void BeforeStart()
        {
            MyAPIGateway.Utilities.SendModMessage(ModHandlerID, ModAPIMethods);
        }

        public static bool VerifyVersion(int ModAPIVersion, string ModName)
        {
            if (ModAPIVersion < MinVersion)
            {
                return false;
            }
            return true;
        }

        public static int GetPlayerFixedStatUpdateHash(long playerId)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                return player.GetPlayerFixedStatUpdateHash();
            }
            return 0;
        }

        public static byte GetPlayerFixedStatStack(long playerId, string fixedStat)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                if (player.FixedStatStack.ContainsKey(fixedStat))
                {
                    return player.FixedStatStack[fixedStat];
                }
            }
            return 0;
        }

        public static long GetPlayerFixedStatRemainTime(long playerId, string fixedStat)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                if (player.FixedStatTimer.ContainsKey(fixedStat))
                {
                    return player.FixedStatTimer[fixedStat];
                }
            }
            return 0;
        }

        public static long GetGameTime()
        {
            if (AdvancedStatsAndEffectsTimeManager.Instance != null)
                return AdvancedStatsAndEffectsTimeManager.Instance.GameTime;
            return 0;
        }

        public static bool DoPlayerConsume(long playerId, MyDefinitionId consumableId)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                if (AdvancedStatsAndEffectsSession.Static.ConsumablesInfo.ContainsKey(consumableId))
                {
                    player.DoConsumeItem(AdvancedStatsAndEffectsSession.Static.ConsumablesInfo[consumableId]);
                    return true;
                }
            }
            return false;
        }

        public static bool AddAfterBotAdd(Action<long, IMyCharacter> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.AfterBotAdd.Add(new AdvancedStatsAndEffectsSession.OnBotAdd()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.AfterBotAdd.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddAfterPlayerRespawn(Action<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.PlayerRespawn.Add(new AdvancedStatsAndEffectsSession.OnPlayerRespawn()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.PlayerRespawn.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddAfterPlayerReset(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.PlayerReset.Add(new AdvancedStatsAndEffectsSession.OnPlayerReset()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.PlayerReset.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddVirtualStatAbsorptionCicle(string targetStat, Action<string, float, MyDefinitionId, long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            if (callback != null && AdvancedStatsAndEffectsSession.Static.IsVirtualStat(targetStat))
            {
                if (!AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle.ContainsKey(targetStat))
                    AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle[targetStat] = new List<AdvancedStatsAndEffectsSession.OnVirtualStatAbsorptionCicle>();
                AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle[targetStat].Add(new AdvancedStatsAndEffectsSession.OnVirtualStatAbsorptionCicle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.VirtualStatAbsorptionCicle[targetStat].Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
            }
            return false;
        }

        public static bool AddBeforeCycleStatCallback(string targetStat, Action<long, long, long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            if (callback != null)
            {
                if (!AdvancedStatsAndEffectsSession.Static.StatBeforeCycle.ContainsKey(targetStat))
                    AdvancedStatsAndEffectsSession.Static.StatBeforeCycle[targetStat] = new List<AdvancedStatsAndEffectsSession.OnStatBeforeCycle>();
                AdvancedStatsAndEffectsSession.Static.StatBeforeCycle[targetStat].Add(new AdvancedStatsAndEffectsSession.OnStatBeforeCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.StatBeforeCycle[targetStat].Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
            }
            return false;
        }

        public static bool AddStartStatCycleCallback(string targetStat, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            if (callback != null)
            {
                if (!AdvancedStatsAndEffectsSession.Static.StartStatCycle.ContainsKey(targetStat))
                    AdvancedStatsAndEffectsSession.Static.StartStatCycle[targetStat] = new List<AdvancedStatsAndEffectsSession.OnStatCycle>();
                AdvancedStatsAndEffectsSession.Static.StartStatCycle[targetStat].Add(new AdvancedStatsAndEffectsSession.OnStatCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.StartStatCycle[targetStat].Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
            }
            return false;
        }

        public static bool AddEndStatCycleCallback(string targetStat, Action<long, IMyCharacter, MyCharacterStatComponent, MyEntityStat> callback, int priority)
        {
            if (callback != null)
            {
                if (!AdvancedStatsAndEffectsSession.Static.EndStatCycle.ContainsKey(targetStat))
                    AdvancedStatsAndEffectsSession.Static.EndStatCycle[targetStat] = new List<AdvancedStatsAndEffectsSession.OnStatCycle>();
                AdvancedStatsAndEffectsSession.Static.EndStatCycle[targetStat].Add(new AdvancedStatsAndEffectsSession.OnStatCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.EndStatCycle[targetStat].Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
            }
            return false;
        }

        public static bool AddFixedEffect(long playerId, string fixedEffectId, byte stacks, bool max)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                player.AddFixedEffect(fixedEffectId, stacks, max);
            }
            return false;
        }

        public static bool RemoveFixedEffect(long playerId, string fixedEffectId, byte stacks, bool max)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                player.RemoveFixedEffect(fixedEffectId, stacks, max);
            }
            return false;
        }

        public static Vector2 GetLastHealthChange(long playerId)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                return player.lastHealthChanged;
            }
            return Vector2.Zero;
        }

        public static float GetRemainOverTimeConsumable(long playerId, string stat)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                return player.GetRemainOverTimeConsumable(stat);
            }
            return 0;
        }

        public static bool ClearOverTimeConsumable(long playerId)
        {
            var player = AdvancedStatsAndEffectsEntityManager.Instance.GetPlayerCharacter(playerId);
            if (player != null)
            {
                player.DoEmptyConsumables();
            }
            return false;
        }

        public static bool AddBeforeCycleCallback(Func<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.BeforeCycle.Add(new AdvancedStatsAndEffectsSession.OnCanCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.BeforeCycle.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddFixedStatCycleCallback(string fixedStat, Action<string, byte, long, long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            if (callback != null)
            {
                if (!AdvancedStatsAndEffectsSession.Static.FixedStatsInfo.ContainsKey(fixedStat))
                    return false;
                if (!AdvancedStatsAndEffectsSession.Static.FixedStatCycle.ContainsKey(fixedStat))
                    AdvancedStatsAndEffectsSession.Static.FixedStatCycle[fixedStat] = new List<AdvancedStatsAndEffectsSession.OnFixedStatCycle>();
                AdvancedStatsAndEffectsSession.Static.FixedStatCycle[fixedStat].Add(new AdvancedStatsAndEffectsSession.OnFixedStatCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.FixedStatCycle[fixedStat].Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddAfterCycleCallback(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.AfterCycle.Add(new AdvancedStatsAndEffectsSession.OnCycle()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.AfterCycle.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddBeforePlayersUpdateCallback(Func<long, IMyCharacter, MyCharacterStatComponent, bool> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.BeforePlayersUpdate.Add(new AdvancedStatsAndEffectsSession.OnPlayerCanUpdate()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.BeforePlayersUpdate.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool AddAfterPlayersUpdateCallback(Action<long, IMyCharacter, MyCharacterStatComponent> callback, int priority)
        {
            if (callback != null)
            {
                AdvancedStatsAndEffectsSession.Static.AfterPlayersUpdate.Add(new AdvancedStatsAndEffectsSession.OnPlayersUpdate()
                {
                    Action = callback,
                    Priority = priority
                });
                AdvancedStatsAndEffectsSession.Static.AfterPlayersUpdate.Sort((x, y) => x.Priority.CompareTo(y.Priority) * -1);
                return true;
            }
            return false;
        }

        public static bool SetStatAsConsumableTrigger(string statToBind)
        {
            if (AdvancedStatsAndEffectsSession.Static.BodyStatsConfigs.Keys.Contains(statToBind))
            {
                if (!AdvancedStatsAndEffectsSession.Static.TriggerStats.Contains(statToBind))
                {
                    AdvancedStatsAndEffectsSession.Static.TriggerStats.Add(statToBind);
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Set stat as Consumable Trigger : {statToBind}");
                    return true;
                }
                else
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Stat already set as Consumable Trigger : {statToBind}");
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogInfo(typeof(AdvancedStatsAndEffectsAPIBackend), $"Not a valid stat : {statToBind}");
            }
            return false;
        }

        public static bool ConfigureConsumable(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var consumableInfo = MyAPIGateway.Utilities.SerializeFromXML<ConsumableInfo>(value);
                    AdvancedStatsAndEffectsSession.Static.DoConfigureConsumable(consumableInfo);
                }
                catch (Exception e)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsAPIBackend), e);
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(AdvancedStatsAndEffectsAPIBackend), $"ConfigureConsumable : value is null");
            }
            return false;
        }

        public static bool ConfigureVirtualStat(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var fixedStatInfo = MyAPIGateway.Utilities.SerializeFromXML<VirtualStatInfo>(value);
                    AdvancedStatsAndEffectsSession.Static.DoConfigureVirtualStat(fixedStatInfo);
                }
                catch (Exception e)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsAPIBackend), e);
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(AdvancedStatsAndEffectsAPIBackend), $"ConfigureVirtualStat : value is null");
            }
            return false;
        }

        public static bool ConfigureFixedStat(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    var fixedStatInfo = MyAPIGateway.Utilities.SerializeFromXML<FixedStatInfo>(value);
                    AdvancedStatsAndEffectsSession.Static.DoConfigureFixedStat(fixedStatInfo);
                }
                catch (Exception e)
                {
                    AdvancedStatsAndEffectsLogging.Instance.LogError(typeof(AdvancedStatsAndEffectsAPIBackend), e);
                }
            }
            else
            {
                AdvancedStatsAndEffectsLogging.Instance.LogWarning(typeof(AdvancedStatsAndEffectsAPIBackend), $"ConfigureFixedStat : value is null");
            }
            return false;
        }

    }

}