using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Managers
{

    // Interface utilisateur pour le système de tours
    public class TurnSystemUI : MonoBehaviour
    {
        [Header("UI References")]
        public UnityEngine.UI.Text turnInfoText;
        public UnityEngine.UI.Button nextPhaseButton;

        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;

            if (gameManager == null)
            {
                Debug.LogError("GameManager not found!");
                return;
            }

            // S'abonner aux événements
            gameManager.onTurnStart.AddListener(UpdateUI);
            gameManager.onPhaseChanged.AddListener(UpdateUI);

            // Configurer le bouton
            if (nextPhaseButton != null)
            {
                nextPhaseButton.onClick.AddListener(OnNextPhaseClicked);
            }

            // Mettre à jour l'UI initiale
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (turnInfoText != null)
            {
                turnInfoText.text = gameManager.GetTurnInfoText();
            }
        }

        private void OnNextPhaseClicked()
        {
            gameManager.NextPhase();
        }
    }
}
