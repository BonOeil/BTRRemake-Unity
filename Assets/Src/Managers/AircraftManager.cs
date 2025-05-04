using Assets.Src.Locations;
using Assets.Src.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Managers
{

    // Gestionnaire des unités aériennes
    public class AircraftManager : MonoBehaviour
    {
        public static AircraftManager Instance;

        [Header("References")]
        public Transform earthTransform;

        [Header("Squadron Collections")]
        public List<AircraftTypeData> aircraftTypes = new List<AircraftTypeData>();
        private List<AircraftSquadron> allSquadrons = new List<AircraftSquadron>();

        [Header("Movement Settings")]
        public float waypointSpacing = 10f; // Distance entre les points de passage en degrés

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public AircraftSquadron CreateSquadron(AircraftTypeData aircraftType, string name, MapLocation homeBase,
                                              int count, string side)
        {
            // Créer un GameObject parent pour cette escadrille
            GameObject squadronObject = new GameObject(name);
            squadronObject.transform.SetParent(earthTransform);

            // Ajouter le composant AircraftSquadron
            AircraftSquadron squadron = squadronObject.AddComponent<AircraftSquadron>();
            squadron.aircraftType = aircraftType;
            squadron.squadronName = name;
            squadron.homeBase = homeBase;
            squadron.aircraftCount = count;
            squadron.side = side;

            // Positionner à la base
            if (homeBase != null)
            {
                squadronObject.transform.position = homeBase.transform.position + Vector3.up * 0.5f;
            }

            // Instancier le prefab visuel si disponible
            if (aircraftType.prefab != null)
            {
                GameObject visual = Instantiate(aircraftType.prefab, squadronObject.transform);
                squadron.squadronVisual = visual.transform;
            }
            else
            {
                // Créer un visuel par défaut
                GameObject defaultVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                defaultVisual.transform.SetParent(squadronObject.transform);
                defaultVisual.transform.localPosition = Vector3.zero;
                defaultVisual.transform.localScale = new Vector3(0.3f, 0.1f, 0.5f);

                // Configurer le matériau
                Renderer renderer = defaultVisual.GetComponent<Renderer>();
                Material material = new Material(Shader.Find("Standard"));
                material.color = side == "Allied" ? Color.blue : Color.red;
                renderer.material = material;

                squadron.squadronVisual = defaultVisual.transform;
            }

            // Créer un indicateur de sélection
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.SetParent(squadronObject.transform);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Configurer le matériau pour l'indicateur
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            Material indicatorMaterial = new Material(Shader.Find("Standard"));
            indicatorMaterial.color = new Color(0f, 1f, 0f, 0.5f);
            indicatorMaterial.SetFloat("_Mode", 3); // Mode transparent
            indicatorRenderer.material = indicatorMaterial;

            squadron.selectionIndicator = indicator;
            indicator.SetActive(false);

            // Enregistrer l'escadrille dans notre liste
            allSquadrons.Add(squadron);

            return squadron;
        }

        public void SendSquadronToLocation(AircraftSquadron squadron, MapLocation target, float altitude)
        {
            if (!squadron.CanFly)
            {
                Debug.LogWarning($"Squadron {squadron.squadronName} cannot fly: insufficient readiness or fuel");
                return;
            }

            // Calculer un chemin entre la position actuelle et la cible
            List<Vector3> path = CalculatePath(squadron.transform.position, target.transform.position);

            // Définir la mission
            squadron.SetMission(path, target);

            Debug.Log($"Squadron {squadron.squadronName} sent from {squadron.homeBase.locationName} to {target.locationName}");
        }

        private List<Vector3> CalculatePath(Vector3 start, Vector3 end)
        {
            List<Vector3> path = new List<Vector3>();

            // Ajouter le point de départ
            path.Add(start);

            // Calculer le grand cercle entre le début et la fin
            Vector3 startDir = start.normalized;
            Vector3 endDir = end.normalized;
            float angle = Vector3.Angle(startDir, endDir);

            // Calculer le nombre de waypoints basé sur la distance
            int segments = Mathf.Max(1, Mathf.FloorToInt(angle / waypointSpacing));

            for (int i = 1; i < segments; i++)
            {
                // Interpolation sphérique pour suivre le grand cercle
                float t = (float)i / segments;
                Vector3 direction = Vector3.Slerp(startDir, endDir, t);
                // Utiliser le même rayon que les points de départ/arrivée
                float radius = (start.magnitude + end.magnitude) * 0.5f;
                Vector3 waypoint = direction * radius;
                path.Add(waypoint);
            }

            // Ajouter le point d'arrivée
            path.Add(end);

            return path;
        }

        public List<AircraftSquadron> GetSquadronsBySide(string side)
        {
            return allSquadrons.FindAll(squad => squad.side == side);
        }

        public List<AircraftSquadron> GetSquadronsByType(AircraftType type)
        {
            return allSquadrons.FindAll(squad => squad.aircraftType.type == type);
        }

        public AircraftSquadron GetSquadronByName(string name)
        {
            return allSquadrons.Find(squad => squad.squadronName == name);
        }
    }
}
