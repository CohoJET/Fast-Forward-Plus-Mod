using System;
using System.Globalization;
using FastForwardPlus.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FastForwardPlus
{
    /// <summary>
    /// Watches the configured keys and applies the speed each one carries.
    /// </summary>
    public class SpeedHotkeys : MonoBehaviour
    {
        public SpeedHotkeys(IntPtr ptr) : base(ptr) { }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            foreach (var binding in SpeedBindings.All)
            {
                if (!keyboard[binding.Key].wasPressedThisFrame)
                    continue;

                Apply(binding.Speed);
                return;
            }
        }

        private static void Apply(float speed)
        {
            if (!SimulationSpeed.IsWorldReady() || !SimulationSpeed.IsFastForwardAllowed())
                return;

            try
            {
                SimulationSpeed.Apply(speed);
                FastForwardPlusPlugin.Logger.LogInfo(
                    "Simulation speed set to " + speed.ToString("0.##", CultureInfo.InvariantCulture) + "x.");
            }
            catch (Exception ex)
            {
                FastForwardPlusPlugin.Logger.LogError($"Could not set the simulation speed: {ex}");
            }
        }
    }
}
