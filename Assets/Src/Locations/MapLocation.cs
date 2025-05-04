using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Locations
{
    // Classe qui représente un lieu concret sur la carte
    public class MapLocation : MonoBehaviour
    {
        [Header("Location Data")]
        public LocationTypeData typeData;
        public string locationName;
        public string countryCode; // Code du pays (ex: "UK", "DE", "FR")

        [Header("Position")]
        public float latitude;
        public float longitude;
        public float elevation; // En mètres au-dessus du niveau de la sphère

        [Header("Status")]
        public int currentHealth;
        public int maxHealth = 100;
        public bool isOperational = true;
        public bool isOccupied = false;
        public string controlledBy; // Faction qui contrôle ce lieu

        [Header("Resources")]
        public int fuelStorage;
        public int ammunitionStorage;
        public int suppliesStorage;

        [Header("References")]
        public Transform visualTransform; // Pour l'orientation du modèle 3D
        public GameObject selectionIndicator;

        // Méthodes
        private void Start()
        {
            // Positionner le lieu sur la sphère
            PositionOnSphere();

            // Initialiser la santé
            currentHealth = maxHealth;

            // Désactiver l'indicateur de sélection par défaut
            if (selectionIndicator != null)
                selectionIndicator.SetActive(false);
        }

        public void PositionOnSphere()
        {
            // Utilise le service de coordonnées pour placer ce lieu
            transform.position = CoordinateConverter.GpsToUnityPosition(latitude, longitude, elevation);

            // Orienter correctement le modèle
            if (visualTransform != null)
            {
                // Orienter le modèle pour qu'il soit "debout" par rapport à la sphère
                Vector3 up = (transform.position - transform.parent.position).normalized;
                visualTransform.up = up;

                // Rotation aléatoire autour de l'axe vertical (si nécessaire)
                visualTransform.Rotate(up, UnityEngine.Random.Range(0, 360f), Space.World);
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

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isOperational = false;
                // Déclencher événements liés à la destruction
            }

            // Mettre à jour l'apparence en fonction des dégâts
            UpdateVisuals();
        }

        public void Repair(int amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            if (currentHealth > 0 && !isOperational)
            {
                isOperational = true;
                // Déclencher événements liés à la réparation
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            // Mettre à jour l'apparence visuelle en fonction de l'état
            // Par exemple, ajouter des effets de fumée, changer la texture, etc.
        }

        // Pour le débogage
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                // Afficher un gizmo pour voir le lieu dans l'éditeur
                Gizmos.color = Color.yellow;
                Vector3 position = CoordinateConverter.GpsToUnityPosition(latitude, longitude, elevation);
                Gizmos.DrawSphere(position, 0.2f);
            }
        }
    }

}
