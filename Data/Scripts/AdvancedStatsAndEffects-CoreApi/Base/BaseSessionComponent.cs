using System;
using VRage.Game.Components;
using VRage.Game;
using VRage.Utils;
using Sandbox.ModAPI;

namespace AdvancedStatsAndEffects
{

    public abstract class BaseSessionComponent : MySessionComponentBase
    {

        public bool IsServer
        {
            get
            {
                return MyAPIGateway.Multiplayer.IsServer;
            }
        }

        public bool IsClient
        {
            get
            {
                return !IsServer;
            }
        }

        public bool IsDedicated
        {
            get
            {
                return MyAPIGateway.Utilities.IsDedicated;
            }
        }

        protected static int RunCount { get; private set; } = 0;

        public bool IsClosing { get; private set; } = false;

        protected abstract void DoInit(MyObjectBuilder_SessionComponent sessionComponent);

        protected virtual void DoUpdate()
        {

        }

        protected virtual void DoUpdate15()
        {

        }

        protected virtual void DoUpdate60()
        {

        }

        protected override void UnloadData()
        {
            base.UnloadData();

        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            AdvancedStatsAndEffectsLogging.Instance.LogInfo(GetType(), "Init");
            DoInit(sessionComponent);
        }

        public override void UpdateAfterSimulation()
        {
            try
            {
                DoUpdate();

                if (++RunCount % 15 > 0)
                    return;

                DoUpdate15();

                if (RunCount % 60 > 0)
                    return;

                DoUpdate60();

                if (RunCount > 299)
                    RunCount = 0;
            }
            catch (Exception ex)
            {
                AdvancedStatsAndEffectsLogging.Instance.LogError(GetType(), ex);
            }
        }

    }

}
