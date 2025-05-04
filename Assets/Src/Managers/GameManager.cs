using Assets.Src.Locations;
using Assets.Src.Managers;
using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Classe principale de gestion du jeu et du système de tours
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Turn Info")]
    public int currentTurn = 1;
    public GamePhase currentPhase = GamePhase.Planning;
    public DateTime currentDate = new DateTime(1940, 7, 10); // Début de la bataille d'Angleterre
    public int turnsPerDay = 2; // Nombre de tours par jour (jour/nuit)

    [Header("Side Info")]
    public Side currentSide = Side.Allies;

    [Header("Weather")]
    public WeatherCondition currentWeather = WeatherCondition.Clear;

    [Header("Events")]
    public UnityEvent onTurnStart = new UnityEvent();
    public UnityEvent onTurnEnd = new UnityEvent();
    public UnityEvent onPhaseChanged = new UnityEvent();

    [Header("References")]
    public LocationManager locationManager;
    public AircraftManager aircraftManager;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Rechercher les managers si non assignés
        if (locationManager == null)
            locationManager = FindObjectOfType<LocationManager>();

        if (aircraftManager == null)
            aircraftManager = FindObjectOfType<AircraftManager>();

        // Démarrer le premier tour
        StartNewTurn();
    }

    public void NextPhase()
    {
        // Exécuter la fin de phase actuelle
        CompleteCurrentPhase();

        // Avancer à la phase suivante
        switch (currentPhase)
        {
            case GamePhase.Planning:
                currentPhase = GamePhase.Movement;
                break;

            case GamePhase.Movement:
                currentPhase = GamePhase.Combat;
                break;

            case GamePhase.Combat:
                currentPhase = GamePhase.Resolution;
                break;

            case GamePhase.Resolution:
                // Changer de camp
                currentSide = (currentSide == Side.Allies) ? Side.Axis : Side.Allies;

                // Si les deux camps ont joué, passer au tour suivant
                if (currentSide == Side.Allies)
                {
                    EndTurn();
                    StartNewTurn();
                }
                else
                {
                    // Commencer le tour du camp suivant
                    currentPhase = GamePhase.Planning;
                }
                break;
        }

        // Déclencher l'événement de changement de phase
        onPhaseChanged.Invoke();
        Debug.Log($"Tour {currentTurn}, Date {currentDate.ToShortDateString()}, Phase {currentPhase}, Camp {currentSide}");

        // Exécuter le début de la nouvelle phase
        StartCurrentPhase();
    }

    private void StartNewTurn()
    {
        Debug.Log($"Début du tour {currentTurn} - {currentDate.ToShortDateString()}");

        // Déclencher l'événement de début de tour
        onTurnStart.Invoke();

        // Mettre à jour la météo
        UpdateWeather();

        // Démarrer la première phase
        currentPhase = GamePhase.Planning;
        onPhaseChanged.Invoke();

        // Exécuter le début de la phase
        StartCurrentPhase();
    }

    private void EndTurn()
    {
        // Déclencher l'événement de fin de tour
        onTurnEnd.Invoke();

        // Mettre à jour la date
        if (currentTurn % turnsPerDay == 0)
        {
            currentDate = currentDate.AddDays(1);
        }

        // Incrémenter le compteur de tours
        currentTurn++;
    }

    private void StartCurrentPhase()
    {
        // Logique spécifique au début de chaque phase
        switch (currentPhase)
        {
            case GamePhase.Planning:
                // Mise à jour de la disponibilité des unités, etc.
                RefreshUnitStatus();
                break;

            case GamePhase.Movement:
                // Exécuter tous les mouvements planifiés
                StartCoroutine(ExecuteMovementPhase());
                break;

            case GamePhase.Combat:
                // Résoudre tous les combats
                StartCoroutine(ExecuteCombatPhase());
                break;

            case GamePhase.Resolution:
                // Appliquer les résultats, réparer, resupply, etc.
                ApplyTurnResults();
                break;
        }
    }

    private void CompleteCurrentPhase()
    {
        // Logique de nettoyage pour chaque phase
        switch (currentPhase)
        {
            case GamePhase.Planning:
                // Finaliser les ordres de mission
                break;

            case GamePhase.Movement:
                // S'assurer que tous les mouvements sont terminés
                StopCoroutine(ExecuteMovementPhase());
                break;

            case GamePhase.Combat:
                // S'assurer que tous les combats sont résolus
                StopCoroutine(ExecuteCombatPhase());
                break;

            case GamePhase.Resolution:
                // Finaliser les changements d'état
                break;
        }
    }

    private void UpdateWeather()
    {
        // Simulation simple de météo
        int weatherRoll = UnityEngine.Random.Range(0, 100);

        // Ajuster les probabilités en fonction de la saison
        int month = currentDate.Month;
        bool isWinter = (month == 12 || month <= 2);
        bool isSummer = (month >= 6 && month <= 8);

        if (isWinter)
        {
            // Plus de mauvais temps en hiver
            if (weatherRoll < 30) currentWeather = WeatherCondition.Clear;
            else if (weatherRoll < 50) currentWeather = WeatherCondition.Cloudy;
            else if (weatherRoll < 75) currentWeather = WeatherCondition.Rainy;
            else if (weatherRoll < 90) currentWeather = WeatherCondition.Stormy;
            else currentWeather = WeatherCondition.Foggy;
        }
        else if (isSummer)
        {
            // Plus de beau temps en été
            if (weatherRoll < 60) currentWeather = WeatherCondition.Clear;
            else if (weatherRoll < 85) currentWeather = WeatherCondition.Cloudy;
            else if (weatherRoll < 95) currentWeather = WeatherCondition.Rainy;
            else currentWeather = WeatherCondition.Stormy;
        }
        else
        {
            // Printemps / Automne
            if (weatherRoll < 45) currentWeather = WeatherCondition.Clear;
            else if (weatherRoll < 70) currentWeather = WeatherCondition.Cloudy;
            else if (weatherRoll < 85) currentWeather = WeatherCondition.Rainy;
            else if (weatherRoll < 95) currentWeather = WeatherCondition.Stormy;
            else currentWeather = WeatherCondition.Foggy;
        }

        Debug.Log($"Météo actuelle: {currentWeather}");
    }

    private void RefreshUnitStatus()
    {
        // Mettre à jour l'état des unités au début du tour
        // Par exemple, augmenter légèrement la préparation des escadrilles non utilisées

        string currentSideStr = (currentSide == Side.Allies) ? "Allied" : "Axis";
        List<AircraftSquadron> sideSquadrons = aircraftManager.GetSquadronsBySide(currentSideStr);

        foreach (AircraftSquadron squadron in sideSquadrons)
        {
            // Augmenter la préparation pour les escadrilles qui ne sont pas en mission
            if (!squadron.isOnMission)
            {
                squadron.readiness = Mathf.Min(squadron.readiness + 5f, 100f);
            }
        }
    }

    private IEnumerator ExecuteMovementPhase()
    {
        Debug.Log("Exécution de la phase de mouvement");

        // En principe, toutes les escadrilles en mouvement se déplacent pendant cette phase
        // Pour le prototype, on attend simplement 2 secondes
        yield return new WaitForSeconds(2f);

        // Passer automatiquement à la phase suivante
        NextPhase();
    }

    private IEnumerator ExecuteCombatPhase()
    {
        Debug.Log("Exécution de la phase de combat");

        // En principe, tous les combats sont résolus pendant cette phase
        // Pour le prototype, on attend simplement 2 secondes
        yield return new WaitForSeconds(2f);

        // Passer automatiquement à la phase suivante
        NextPhase();
    }

    private void ApplyTurnResults()
    {
        Debug.Log("Application des résultats du tour");

        // Appliquer les réparations aux bases aériennes
        foreach (MapLocation location in locationManager.GetAllLocations())
        {
            if (location.controlledBy == currentSide.ToString() && location.currentHealth < location.maxHealth)
            {
                int repairAmount = 5; // Valeur arbitraire pour le prototype
                location.Repair(repairAmount);
            }
        }

        // Passer automatiquement à la phase suivante après un court délai
        Invoke("NextPhase", 1f);
    }

    // Méthode publique pour accéder au texte d'information du tour
    public string GetTurnInfoText()
    {
        string phaseText = currentPhase.ToString();
        string sideText = currentSide.ToString();
        string dateText = currentDate.ToShortDateString();
        string weatherText = currentWeather.ToString();

        return $"Tour {currentTurn} - {dateText}\nPhase: {phaseText}\nCamp: {sideText}\nMétéo: {weatherText}";
    }
}
