using System;
using System.Collections.Generic;
using System.Linq;
using KolonyTools;
using UnityEngine;

namespace KolonyTools
{

    public class WolfMissionsUi
    {
        private GUIStyle _labelStyle;
        private GUIStyle _scrollStyle;
        private Vector2 _scrollPos = Vector2.zero;

        public WolfMissionsUi()
        {
            _labelStyle = new GUIStyle(HighLogic.Skin.label);
            _scrollStyle = new GUIStyle(HighLogic.Skin.scrollView);
            comboFrom = new InlineComboBox<CelestialBody>(bodies.ToArray(), bodies.Select(b => b.name).ToArray());
            comboTo = new InlineComboBox<CelestialBody>(bodies.ToArray(), bodies.Select(b => b.name).ToArray());
            comboFromS = new InlineComboBox<CaravanParameters.Situation>(situations.ToArray());
            comboToS = new InlineComboBox<CaravanParameters.Situation>(situations.ToArray());

        }

        List<CelestialBody> bodies = ProgressTracking.Instance.celestialBodyNodes.Where(node => node.IsReached).Select(node => node.Body).Where(b => b.GetOrbit() != null).ToList();
        List<CaravanParameters.Situation> situations = Enum.GetValues(typeof(CaravanParameters.Situation)).Cast<CaravanParameters.Situation>().ToList();
        InlineComboBox<CelestialBody> comboFrom;
        InlineComboBox<CelestialBody> comboTo;
        InlineComboBox<CaravanParameters.Situation> comboFromS;
        InlineComboBox<CaravanParameters.Situation> comboToS;

        public void displayAndRun()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, _scrollStyle, GUILayout.Width(680), GUILayout.Height(380));
            GUILayout.BeginVertical();
            GUILayout.Label("Hello, WOLF!", _labelStyle, GUILayout.Width(400));

            GUILayout.Label("Choose missions", _labelStyle, GUILayout.Width(400));
            GUILayout.BeginHorizontal();
            GUILayout.Label("From", _labelStyle, GUILayout.Width(40));
            comboFrom.show(GUILayout.Width(80));
            comboFromS.show(GUILayout.Width(80));
            GUILayout.Label("To", _labelStyle, GUILayout.Width(20));
            comboTo.show(GUILayout.Width(80));
            comboToS.show(GUILayout.Width(80));
            if ((comboFrom.get() != comboTo.get()) || (comboFromS.get() != comboToS.get()))
            {
                if (GUILayout.Button("Initiate"))
                {
                    var p = new CaravanParameters(
                                    comboFrom.get(),
                                    comboFromS.get(),
                                    comboTo.get(),
                                    comboToS.get());
                    WolfContractHandling.MakeContractFor(p);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Accepted missions", _labelStyle, GUILayout.Width(400));
            foreach (var caravanParams in WolfContractHandling.GetActiveCaravanContracts())
            {
                GUILayout.Label("  " + caravanParams.ToString(), _labelStyle, GUILayout.Width(400));
            }

            GUILayout.Label("Completed missions", _labelStyle, GUILayout.Width(400));
            var grouped = WolfCaravanScenario.GetCompletedRoutes().GroupBy(r => r.Parameters.ToString()); 
            if (grouped.Any())
            {
                foreach (var item in grouped)
                {
                    GUILayout.Label(
                        String.Format("  {0}:  {1} ({2} points)", item.Key, item.Count(), item.Sum(r => r.Points)),
                        _labelStyle, GUILayout.Width(400));
                }
            }
            else
            {
                GUILayout.Label("  None");
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }


        private static void MakeContractTo(CelestialBody destination)
        {
            var p = new CaravanParameters(
                FlightGlobals.Bodies.Where(cb => cb.isHomeWorld).First(),
                CaravanParameters.Situation.Surface,
                destination,
                CaravanParameters.Situation.Orbit);
            WolfContractHandling.MakeContractFor(p);
        }

    }
   
}
