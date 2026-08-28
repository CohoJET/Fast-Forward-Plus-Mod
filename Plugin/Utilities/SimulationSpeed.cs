using System;
using Unity.Entities;
using UnityEngine;

namespace FastForwardPlus.Utilities
{
    /// <summary>
    /// Reads and writes the game's one and only speed control: <c>Core.Singleton._simulationSpeed</c>,
    /// which the game mirrors straight into <see cref="Time.timeScale"/>.
    /// </summary>
    internal static class SimulationSpeed
    {
        /// <summary>
        /// Speed the game runs at with no fast forward enabled.
        /// </summary>
        internal const float NormalSpeed = 1f;

        private static EntityManager Entities => World.DefaultGameObjectInjectionWorld.EntityManager;

        internal static bool IsWorldReady()
        {
            try
            {
                var world = World.DefaultGameObjectInjectionWorld;
                return world != null && Utility.HasSingleton<Core.Singleton_23>(world.EntityManager);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The speed the simulation is currently running at.
        /// </summary>
        internal static float Current()
        {
            try
            {
                return Utility.GetSingleton<Core.Singleton_23>(Entities)._simulationSpeed;
            }
            catch (Exception ex)
            {
                FastForwardPlusPlugin.Logger?.LogError($"Could not read the current simulation speed: {ex}");
                return NormalSpeed;
            }
        }

        /// <summary>
        /// Whether this world was created with fast forward enabled.
        /// </summary>
        internal static bool IsFastForwardAllowed()
        {
            try
            {
                return Utility.GetSingleton<Core.Singleton_23>(Entities)._worldSettingsRO._enabledFastForward;
            }
            catch (Exception ex)
            {
                FastForwardPlusPlugin.Logger?.LogError($"Could not read the world's fast forward setting: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Applies <paramref name="speed"/> everywhere the game itself does: the singleton, the Unity
        /// timescale, the ship's "this run used fast forward" flag, and a clock sync broadcast so other
        /// players follow. Mirrors <c>UITimebackMenu.OnClick_FastForward</c> step for step.
        /// </summary>
        internal static void Apply(float speed)
        {
            var em = Entities;

            // Read-modify-write through SetComponentData rather than RefRW.ValueRW: the interop
            // wrapper hands back a copy of the struct, so assigning into it writes to a temporary and
            // the singleton never changes.
            var coreEntity = Utility.GetSingletonEntity<Core.Singleton_23>(em);
            var core = em.GetComponentData<Core.Singleton_23>(coreEntity);
            core._simulationSpeed = speed;
            em.SetComponentData(coreEntity, core);

            Time.timeScale = speed;

            // Vanilla sets this on any speed above normal, and it never clears. It is what marks a run
            // as having been sped up, so leaving it alone would quietly un-invalidate that.
            if (speed > NormalSpeed && Utility.TryGetSingletonEntity<SpaceshipSingleton>(em, out var shipEntity))
            {
                var ship = em.GetComponentData<SpaceshipSingleton>(shipEntity);
                ship._usedFastForward = true;
                em.SetComponentData(shipEntity, ship);
            }

            BroadcastClockSync(em, speed);
        }

        /// <summary>
        /// Queues the same <c>SyncFullNetcoreClock</c> event the vanilla button sends. The receiving
        /// end applies the carried float verbatim with no clamping, so unmodded clients follow a modded
        /// host to any speed on the ladder.
        /// </summary>
        private static void BroadcastClockSync(EntityManager em, float speed)
        {
            try
            {
                var netcore = Utility.GetSingleton<Netcore.Singleton_19>(em);
                var universe = Utility.GetSingleton<UniverseCoreSingleton>(em);

                var sync = new NetcoreEvent_SyncFullNetcoreClock
                {
                    _sendTime = NetcoreEvent_SyncFullNetcoreClock.ForceNewSyncSendTime,
                    _serverNetcoreClock = netcore._netcoreClock,
                    _physicsTicksOffsetFromNetcoreClock = universe._physicsTickID - netcore._netcoreClock,
                    _simulationSpeed = speed,
                };

                var buffer = Utility.GetSingletonBuffer<NetcoreNewEvent>(em, false);
                buffer.Add(new NetcoreNewEvent
                {
                    _event = NetcoreEvent.Create(sync, true),
                    _sortValue = 0,
                });
            }
            catch (Exception ex)
            {
                FastForwardPlusPlugin.Logger?.LogError($"Could not broadcast the clock sync, other players will stay at their current speed: {ex}");
            }
        }
    }
}
