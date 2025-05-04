using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src.Locations
{
    // Classe pour définir les types de lieux
    [System.Serializable]
    public enum LocationType
    {
        Airfield,
        City,
        Factory,
        Port,
        Radar,
        AAPosition, // Position anti-aérienne
        SupplyDepot
    }
}
