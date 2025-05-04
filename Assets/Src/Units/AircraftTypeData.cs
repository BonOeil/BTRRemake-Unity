using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Units
{

    // ScriptableObject pour les données des types d'avions
    [CreateAssetMenu(fileName = "New Aircraft Type", menuName = "War Game/Aircraft Type")]
    public class AircraftTypeData : ScriptableObject
    {
        [Header("Basic Info")]
        public AircraftType type;
        public string aircraftName;
        public Sprite icon;
        public GameObject prefab;
        [TextArea] public string description;

        [Header("Performance")]
        public float maxSpeed; // en km/h
        public float cruisingSpeed; // en km/h
        public float maxAltitude; // en mètres
        public float range; // en km
        public float fuelConsumption; // par heure de vol

        [Header("Combat")]
        public int attackPower;
        public int defensePower;
        public float bombLoad; // en tonnes
        public float accuracy; // pourcentage de précision des bombardements

        [Header("Logistics")]
        public int crewRequired;
        public int maintenanceCost;
        public int productionCost;
    }
}
