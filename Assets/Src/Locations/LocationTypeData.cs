using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Locations
{
    // ScriptableObject pour créer facilement des types de lieux dans l'éditeur
    [CreateAssetMenu(fileName = "New Location Type", menuName = "War Game/Location Type")]
    public class LocationTypeData : ScriptableObject
    {
        public LocationType type;
        public string displayName;
        public Sprite icon;
        public GameObject prefab;
        [TextArea] public string description;

        // Propriétés spécifiques au type
        public bool canLaunchAircraft;
        public bool canRepairAircraft;
        public int defenseValue;
        public int strategicValue;
    }
}
