using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine.InputSystem;

namespace FastForwardPlus
{
    /// <summary>
    /// The key-to-speed table, parsed from a single config line.
    /// </summary>
    internal static class SpeedBindings
    {
        internal const string DefaultBindings = "F1=1, F2=2, F3=4, F4=8, F5=16";

        private static (Key Key, float Speed)[] _bindings = Array.Empty<(Key, float)>();

        internal static IReadOnlyList<(Key Key, float Speed)> All => _bindings;

        internal static void Load(ConfigFile config)
        {
            var setting = config.Bind(
                "Speeds",
                "Bindings",
                DefaultBindings,
                "Comma-separated KEY=MULTIPLIER pairs, using Input System key names " +
                "(F1, Digit1, Numpad0, ...). Multipliers above 18 do not work.");

            _bindings = Parse(setting.Value);
            FastForwardPlusPlugin.Logger.LogInfo(_bindings.Length == 0
                ? "No speed hotkeys are bound."
                : "Speed hotkeys: " + string.Join(", ", _bindings.Select(b =>
                    b.Key + " -> " + b.Speed.ToString("0.##", CultureInfo.InvariantCulture) + "x")));
        }

        private static (Key, float)[] Parse(string raw)
        {
            var bindings = new List<(Key, float)>();
            var bound = new HashSet<Key>();

            foreach (var piece in (raw ?? "").Split(','))
            {
                var trimmed = piece.Trim();
                if (trimmed.Length == 0)
                    continue;

                var split = trimmed.Split('=');
                if (split.Length != 2)
                {
                    Warn(trimmed, "expected KEY=MULTIPLIER.");
                    continue;
                }

                // Key.None is a real enum member but no physical key, and the keyboard has no
                // control for it, so it has to be rejected here rather than at lookup time.
                if (!Enum.TryParse<Key>(split[0].Trim(), true, out var key) || key == Key.None)
                {
                    Warn(trimmed, $"'{split[0].Trim()}' is not an Input System Key name.");
                    continue;
                }

                if (!float.TryParse(split[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var speed))
                {
                    Warn(trimmed, $"'{split[1].Trim()}' is not a number.");
                    continue;
                }

                // Zero is pause, which has its own button.
                if (speed <= 0f)
                {
                    Warn(trimmed, "the multiplier must be above 0.");
                    continue;
                }

                // First binding for a key wins.
                if (!bound.Add(key))
                {
                    Warn(trimmed, $"{key} is already bound.");
                    continue;
                }

                bindings.Add((key, speed));
            }

            return bindings.ToArray();
        }

        private static void Warn(string entry, string reason) =>
            FastForwardPlusPlugin.Logger.LogWarning($"Ignoring '{entry}' in Bindings: {reason}");
    }
}
