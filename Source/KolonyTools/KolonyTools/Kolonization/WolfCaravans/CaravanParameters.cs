using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace KolonyTools
{

    public class CaravanParameters
    {
        public enum Situation { Surface, Orbit }

        public static List<Vessel.Situations> GetSituationList(Situation s)
        {
            if (s == Situation.Orbit)
                return new List<Vessel.Situations>{ Vessel.Situations.ORBITING };
            else if (s == Situation.Surface)
                return new List<Vessel.Situations>{ Vessel.Situations.LANDED, Vessel.Situations.SPLASHED, Vessel.Situations.PRELAUNCH };
            else
                throw new Exception("Unknown situation");
        }

        public CelestialBody _fromBody;
        public Situation _fromSituation;
        public CelestialBody _toBody;
        public Situation _toSituation;

        public CaravanParameters(CelestialBody f, Situation fs, CelestialBody t, Situation ts)
        {
            _fromBody = f;
            _fromSituation = fs;
            _toBody = t;
            _toSituation = ts;
        }

        public override string ToString()
        {
            return String.Format("{0}/{1} → {2}/{3}", _fromBody.name, _fromSituation, _toBody.name, _toSituation);
        }

    }

}
