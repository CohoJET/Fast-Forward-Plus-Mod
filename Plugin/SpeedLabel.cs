using System;
using System.Globalization;
using FastForwardPlus.Utilities;
using UnityEngine;

namespace FastForwardPlus
{
    /// <summary>
    /// Draws the current multiplier in the top-right corner while the simulation is running above
    /// normal speed. 
    /// </summary>
    public class SpeedLabel : MonoBehaviour
    {
        public SpeedLabel(IntPtr ptr) : base(ptr) { }

        private const float RefreshInterval = 0.25f;

        private GUIStyle _style;
        private string _label;
        private float _nextRefresh;

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh)
                return;

            _nextRefresh = Time.unscaledTime + RefreshInterval;

            var speed = SimulationSpeed.IsWorldReady() ? SimulationSpeed.Current() : SimulationSpeed.NormalSpeed;
            _label = speed > SimulationSpeed.NormalSpeed
                ? speed.ToString("0.##", CultureInfo.InvariantCulture) + "x"
                : null;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_label))
                return;

            if (_style == null)
            {
                _style = new GUIStyle
                {
                    font = GUI.skin.label.font,
                    fontSize = 22,
                    alignment = TextAnchor.UpperRight,
                };
                _style.normal.textColor = Color.white;
            }

            var area = new Rect(Screen.width - 130f, 12f, 118f, 32f);

            // Cheap drop shadow so the label stays readable against a bright planet or something.
            _style.normal.textColor = new Color(0f, 0f, 0f, 0.6f);
            GUI.Label(new Rect(area.x + 1f, area.y + 1f, area.width, area.height), _label, _style);
            _style.normal.textColor = Color.white;

            GUI.Label(area, _label, _style);
        }
    }
}
