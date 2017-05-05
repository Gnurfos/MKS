using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Contracts;

namespace KolonyTools
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class WolfCaravanScenarioRegister : MonoBehaviour
    {
        public void Start()
        {
            GameEvents.Contract.onCompleted.Add(ContractCompleted);
            DontDestroyOnLoad(this);
        }

        public void OnDestroy()
        {
            GameEvents.Contract.onCompleted.Remove(ContractCompleted);
        }

        public void ContractCompleted(Contract c)
        {
            WolfContractHandling.ContractCompleted(c);
        }
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new GameScenes[] {GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.EDITOR})]
    public class WolfCaravanScenario : ScenarioModule
    {
        public WolfCaravanScenario()
        {
            Instance = this;
        }

        public static WolfCaravanScenario Instance { get; private set; }

        private class CompletedRoute
        {
            public CelestialBody FromBody { get; set; }
            public CaravanParameters.Situation FromSituation { get; set; }
            public CelestialBody ToBody { get; set; }
            public CaravanParameters.Situation ToSituation { get; set; }
            public int Points { get; set; }
        }

        private List<CompletedRoute> _completedRoutes = new List<CompletedRoute>();

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            _completedRoutes = new List<CompletedRoute>();
            if (gameNode.HasNode("COMPLETED_ROUTES"))
            {
                var routesNode = gameNode.GetNode("COMPLETED_ROUTES");
                var routeNodes = routesNode.GetNodes("ROUTE");
                foreach (var routeNode in routeNodes)
                {
                    var route = new CompletedRoute
                    {
                        FromBody = BodyFromName(routeNode.GetValue("FromBody")),
                        FromSituation = SituationFromString(routeNode.GetValue("FromSituation")),
                        ToBody = BodyFromName(routeNode.GetValue("ToBody")),
                        ToSituation = SituationFromString(routeNode.GetValue("ToSituation")),
                    };
                    _completedRoutes.Add(route);
                }
            }
        }

        private static CaravanParameters.Situation SituationFromString(string s)
        {
            return (CaravanParameters.Situation) Enum.Parse(typeof(CaravanParameters.Situation), s, true);
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            var routesNode = gameNode.HasNode("COMPLETED_ROUTES") ?
                gameNode.GetNode("COMPLETED_ROUTES") :
                gameNode.AddNode("COMPLETED_ROUTES");

            foreach (var route in _completedRoutes)
            {
                var routeNode = new ConfigNode("ROUTE");
                routeNode.AddValue("FromBody", route.FromBody.name);
                routeNode.AddValue("FromSituation", route.FromSituation.ToString());
                routeNode.AddValue("ToBody", route.ToBody.name);
                routeNode.AddValue("ToSituation", route.ToSituation.ToString());
                routesNode.AddNode(routeNode);
            }
        }

        private static CelestialBody BodyFromName(string name)
        {
            CelestialBody body = FlightGlobals.Bodies.Where(cb => cb.name == name || cb.theName == name).FirstOrDefault();
            if (body == null)
            {
                throw new Exception("Unknown CelestialBody: " + name);
            }
            return body;
        }

        public class CompletedRouteWithParams
        {
            public CaravanParameters Parameters;
            public int Points;
        }

        public static IEnumerable<CompletedRouteWithParams> GetCompletedRoutes()
        {
            foreach (var route in Instance._completedRoutes)
            {
                yield return new CompletedRouteWithParams
                {
                    Parameters = new CaravanParameters(route.FromBody, route.FromSituation, route.ToBody, route.ToSituation),
                    Points = route.Points
                };
            }
            yield break;
        }

        public static void AddCompletedRoute(CaravanParameters routeParameters, Vessel vessel)
        {
            var points = ExtractPoints(vessel);
            Instance._completedRoutes.Add(new CompletedRoute {
                FromBody = routeParameters._fromBody,
                FromSituation = routeParameters._fromSituation,
                ToBody = routeParameters._toBody,
                ToSituation = routeParameters._toSituation,
                Points = points,
            });
        }

        private static int ExtractPoints(Vessel vessel)
        {
            return vessel.FindPartModulesImplementing<WolfCargoModule>().Sum(mod => mod.PointsAwardedOnRouteCompletion);
        }

    }


}