using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Units
{

    // Types d'unités aériennes
    [System.Serializable]
    public enum AircraftType
    {
        Fighter,
        Bomber,
        ReconAircraft,
        Transport,
        NightFighter,
        Escort
    }
}
