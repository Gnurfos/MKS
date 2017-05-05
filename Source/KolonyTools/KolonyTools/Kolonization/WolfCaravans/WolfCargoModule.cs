using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USITools;

namespace KolonyTools
{
    public class WolfCargoModule : PartModule
    {
        [KSPField]
        public int PointsAwardedOnRouteCompletion = 1;

        public override string GetInfo()
        {
            return String.Format("Allows establishing WOLF routes (points: {0})", PointsAwardedOnRouteCompletion);
        }
    }
}