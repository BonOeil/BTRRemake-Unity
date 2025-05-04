using Assets.Src.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Managers
{

    // Gestionnaire central des lieux sur la carte
    public class LocationManager : MonoBehaviour
    {
        public static LocationManager Instance;

        [Header("Location Settings")]
        public Transform earthTransform; // Référence à la sphère terrestre

        [Header("Location Collections")]
        public List<LocationTypeData> locationTypes = new List<LocationTypeData>();
        private List<MapLocation> allLocations = new List<MapLocation>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public MapLocation CreateLocation(LocationTypeData type, string name, float latitude, float longitude, float elevation = 0f, string controlledBy = "")
        {
            // Créer un GameObject parent pour ce lieu
            GameObject locationObject = new GameObject(name);
            locationObject.transform.SetParent(earthTransform);

            // Ajouter le composant MapLocation
            MapLocation location = locationObject.AddComponent<MapLocation>();
            location.typeData = type;
            location.locationName = name;
            location.latitude = latitude;
            location.longitude = longitude;
            location.elevation = elevation;
            location.controlledBy = controlledBy;

            // Instancier le prefab visuel si disponible
            if (type.prefab != null)
            {
                GameObject visual = Instantiate(type.prefab, locationObject.transform);
                location.visualTransform = visual.transform;
            }

            // Créer un indicateur de sélection simple
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.SetParent(locationObject.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Configurer le matériau pour l'indicateur
            Renderer renderer = indicator.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0f, 1f, 1f, 0.5f);
            material.SetFloat("_Mode", 3); // Mode transparent
            renderer.material = material;

            location.selectionIndicator = indicator;
            indicator.SetActive(false);

            // Enregistrer le lieu dans notre liste
            allLocations.Add(location);

            // Positionner le lieu sur la sphère
            location.PositionOnSphere();

            return location;
        }

        public List<MapLocation> GetLocationsInRadius(Vector3 center, float radiusDegrees)
        {
            List<MapLocation> locationsInRadius = new List<MapLocation>();

            foreach (MapLocation location in allLocations)
            {
                // Calculer la distance angulaire (approximative)
                Vector3 locationPos = location.transform.position;
                float distance = Vector3.Angle(center.normalized, locationPos.normalized);

                if (distance <= radiusDegrees)
                {
                    locationsInRadius.Add(location);
                }
            }

            return locationsInRadius;
        }

        public List<MapLocation> GetLocationsByType(LocationType type)
        {
            return allLocations.FindAll(loc => loc.typeData.type == type);
        }

        public List<MapLocation> GetLocationsByCountry(string countryCode)
        {
            return allLocations.FindAll(loc => loc.countryCode == countryCode);
        }

        public MapLocation GetLocationByName(string name)
        {
            return allLocations.Find(loc => loc.locationName == name);
        }

        internal IEnumerable<MapLocation> GetAllLocations()
        {
            return allLocations;
        }
    }
}
