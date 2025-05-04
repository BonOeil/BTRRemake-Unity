using Assets.Src.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Units
{

    // Classe pour une escadrille d'avions
    public class AircraftSquadron : MonoBehaviour
    {
        [Header("Squadron Data")]
        public AircraftTypeData aircraftType;
        public string squadronName;
        public string side; // "Allied" ou "Axis"
        public int aircraftCount; // Nombre d'avions dans l'escadrille

        [Header("Status")]
        public float currentFuel;
        public float maxFuel;
        public int ammunition;
        public int bombLoad;
        public float readiness; // Pourcentage de préparation (0-100)
        public int experience; // Niveau d'expérience des pilotes

        [Header("Current Mission")]
        public MapLocation homeBase;
        public MapLocation targetLocation;
        public Vector3 currentPosition;
        public float altitude; // en mètres
        public bool isOnMission = false;
        public List<Vector3> waypoints = new List<Vector3>();
        public int currentWaypointIndex = 0;

        [Header("Visual")]
        public Transform squadronVisual;
        public GameObject selectionIndicator;

        // Propriétés calculées
        public float CurrentSpeed { get; private set; }
        public bool CanFly => readiness > 20f && currentFuel > 0 && aircraftCount > 0;

        private void Start()
        {
            // Initialiser les valeurs par défaut
            maxFuel = aircraftType.range * aircraftCount;
            currentFuel = maxFuel;

            // Positionner à la base de départ si définie
            if (homeBase != null)
            {
                transform.position = homeBase.transform.position + Vector3.up * 0.5f; // Légèrement au-dessus de la base
                UpdatePositionOnSphere();
            }

            // Désactiver l'indicateur de sélection par défaut
            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);
        }

        private void Update()
        {
            if (isOnMission)
            {
                MoveAlongWaypoints();
                ConsumeFuel();
            }
        }

        public void Select()
        {
            if (selectionIndicator != null)
                selectionIndicator.SetActive(true);
        }

        public void Deselect()
        {
            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);
        }

        public void UpdatePositionOnSphere()
        {
            // Orienter correctement l'unité par rapport à la surface de la sphère
            Transform earthTransform = transform.parent;
            if (earthTransform != null)
            {
                Vector3 up = (transform.position - earthTransform.position).normalized;
                transform.up = up;

                // Orienter l'avant de l'unité dans la direction du mouvement si en mission
                if (isOnMission && currentWaypointIndex < waypoints.Count)
                {
                    Vector3 direction = (waypoints[currentWaypointIndex] - transform.position).normalized;
                    Vector3 right = Vector3.Cross(up, direction).normalized;
                    Vector3 forward = Vector3.Cross(right, up).normalized;
                    transform.forward = forward;
                }
            }
        }

        public void SetMission(List<Vector3> newWaypoints, MapLocation target)
        {
            waypoints = newWaypoints;
            targetLocation = target;
            currentWaypointIndex = 0;
            isOnMission = true;

            // Calculer la vitesse actuelle en fonction de la charge et de l'état
            CurrentSpeed = CalculateCurrentSpeed();
        }

        private float CalculateCurrentSpeed()
        {
            // La vitesse est affectée par la charge de bombes et l'expérience des pilotes
            float speedModifier = 1f;

            // Plus de bombes = vitesse réduite
            if (bombLoad > 0)
            {
                float loadRatio = (float)bombLoad / (aircraftType.bombLoad * aircraftCount);
                speedModifier *= (1f - (loadRatio * 0.2f)); // jusqu'à 20% de réduction
            }

            // Plus d'expérience = vitesse légèrement améliorée
            float expModifier = 1f + (experience / 100f) * 0.1f; // jusqu'à 10% d'amélioration

            return aircraftType.cruisingSpeed * speedModifier * expModifier;
        }

        private void MoveAlongWaypoints()
        {
            if (currentWaypointIndex >= waypoints.Count)
            {
                // Mission terminée
                CompleteMission();
                return;
            }

            // Calculer la direction vers le prochain waypoint
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Calculer la distance à parcourir ce frame
            float distanceToTravel = CurrentSpeed * Time.deltaTime * 0.001f; // Conversion km/h en unités par frame

            // Calculer la distance jusqu'au waypoint
            float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTravel >= distanceToWaypoint)
            {
                // On atteint le waypoint, passer au suivant
                transform.position = targetPosition;
                currentWaypointIndex++;
            }
            else
            {
                // Se déplacer vers le waypoint
                transform.position += direction * distanceToTravel;
            }

            // Mettre à jour l'orientation sur la sphère
            UpdatePositionOnSphere();
        }

        private void ConsumeFuel()
        {
            // Consommer du carburant en fonction du temps et du type d'avion
            float fuelConsumptionRate = aircraftType.fuelConsumption * aircraftCount;
            currentFuel -= fuelConsumptionRate * Time.deltaTime / 3600f; // Convertir en consommation par frame

            if (currentFuel <= 0)
            {
                // Plus de carburant, abandonner la mission
                currentFuel = 0;
                AbortMission("Out of fuel");
            }
        }

        public void AbortMission(string reason)
        {
            Debug.Log($"Squadron {squadronName} aborted mission: {reason}");

            // Définir un chemin de retour vers la base
            if (homeBase != null)
            {
                waypoints.Clear();
                waypoints.Add(homeBase.transform.position);
                currentWaypointIndex = 0;
            }
            else
            {
                isOnMission = false;
            }
        }

        private void CompleteMission()
        {
            isOnMission = false;

            // Si l'escadron est à sa base d'origine
            if (targetLocation == homeBase)
            {
                // Ravitaillement et réparations
                currentFuel = maxFuel;
                readiness = Mathf.Min(readiness + 20f, 100f);
                Debug.Log($"Squadron {squadronName} returned to base and refueled.");
            }
            // Si l'escadron a atteint sa cible (mission de bombardement)
            else if (targetLocation != null && Vector3.Distance(transform.position, targetLocation.transform.position) < 1f)
            {
                PerformAttack();

                // Définir un chemin de retour vers la base
                if (homeBase != null)
                {
                    waypoints.Clear();
                    waypoints.Add(homeBase.transform.position);
                    currentWaypointIndex = 0;
                    isOnMission = true;
                }
            }
        }

        private void PerformAttack()
        {
            if (targetLocation != null && bombLoad > 0)
            {
                // Calculer les dégâts en fonction de la précision et du type d'avion
                float accuracyModifier = aircraftType.accuracy * (1f + experience / 100f);
                int baseDamage = aircraftType.attackPower * bombLoad;
                int actualDamage = Mathf.RoundToInt(baseDamage * accuracyModifier);

                // Appliquer les dégâts à la cible
                targetLocation.TakeDamage(actualDamage);

                // Utiliser les bombes
                bombLoad = 0;

                Debug.Log($"Squadron {squadronName} attacked {targetLocation.locationName} and caused {actualDamage} damage.");

                // Gain d'expérience pour la mission réussie
                experience += UnityEngine.Random.Range(1, 3);
            }
        }
    }

}
