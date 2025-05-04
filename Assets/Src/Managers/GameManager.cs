using Assets.Src.Locations;
using Assets.Src.Managers;
using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Classe principale de gestion du jeu et du syst�me de tours
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Turn Info")]
    public int currentTurn = 1;
    public GamePhase currentPhase = GamePhase.Planning;
    public DateTime currentDate = new DateTime(1940, 7, 10); // D�but de la bataille d'Angleterre
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
        // Rechercher les managers si non assign�s
        if (locationManager == null)
            locationManager = FindObjectOfType<LocationManager>();

        if (aircraftManager == null)
            aircraftManager = FindObjectOfType<AircraftManager>();

        // D�marrer le premier tour
        StartNewTurn();
    }

    public void NextPhase()
    {
        // Ex�cuter la fin de phase actuelle
        CompleteCurrentPhase();

        // Avancer � la phase suivante
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

                // Si les deux camps ont jou�, passer au tour suivant
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

        // D�clencher l'�v�nement de changement de phase
        onPhaseChanged.Invoke();
        Debug.Log($"Tour {currentTurn}, Date {currentDate.ToShortDateString()}, Phase {currentPhase}, Camp {currentSide}");

        // Ex�cuter le d�but de la nouvelle phase
        StartCurrentPhase();
    }

    private void StartNewTurn()
    {
        Debug.Log($"D�but du tour {currentTurn} - {currentDate.ToShortDateString()}");

        // D�clencher l'�v�nement de d�but de tour
        onTurnStart.Invoke();

        // Mettre � jour la m�t�o
        UpdateWeather();

        // D�marrer la premi�re phase
        currentPhase = GamePhase.Planning;
        onPhaseChanged.Invoke();

        // Ex�cuter le d�but de la phase
        StartCurrentPhase();
    }

    private void EndTurn()
    {
        // D�clencher l'�v�nement de fin de tour
        onTurnEnd.Invoke();

        // Mettre � jour la date
        if (currentTurn % turnsPerDay == 0)
        {
            currentDate = currentDate.AddDays(1);
        }

        // Incr�menter le compteur de tours
        currentTurn++;
    }

    private void StartCurrentPhase()
    {
        // Logique sp�cifique au d�but de chaque phase
        switch (currentPhase)
        {
            case GamePhase.Planning:
                // Mise � jour de la disponibilit� des unit�s, etc.
                RefreshUnitStatus();
                break;

            case GamePhase.Movement:
                // Ex�cuter tous les mouvements planifi�s
                StartCoroutine(ExecuteMovementPhase());
                break;

            case GamePhase.Combat:
                // R�soudre tous les combats
                StartCoroutine(ExecuteCombatPhase());
                break;

            case GamePhase.Resolution:
                // Appliquer les r�sultats, r�parer, resupply, etc.
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
                // S'assurer que tous les mouvements sont termin�s
                StopCoroutine(ExecuteMovementPhase());
                break;

            case GamePhase.Combat:
                // S'assurer que tous les combats sont r�solus
                StopCoroutine(ExecuteCombatPhase());
                break;

            case GamePhase.Resolution:
                // Finaliser les changements d'�tat
                break;
        }
    }

    private void UpdateWeather()
    {
        // Simulation simple de m�t�o
        int weatherRoll = UnityEngine.Random.Range(0, 100);

        // Ajuster les probabilit�s en fonction de la saison
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
            // Plus de beau temps en �t�
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

        Debug.Log($"M�t�o actuelle: {currentWeather}");
    }

    private void RefreshUnitStatus()
    {
        // Mettre � jour l'�tat des unit�s au d�but du tour
        // Par exemple, augmenter l�g�rement la pr�paration des escadrilles non utilis�es

        string currentSideStr = (currentSide == Side.Allies) ? "Allied" : "Axis";
        List<AircraftSquadron> sideSquadrons = aircraftManager.GetSquadronsBySide(currentSideStr);

        foreach (AircraftSquadron squadron in sideSquadrons)
        {
            // Augmenter la pr�paration pour les escadrilles qui ne sont pas en mission
            if (!squadron.isOnMission)
            {
                squadron.readiness = Mathf.Min(squadron.readiness + 5f, 100f);
            }
        }
    }

    private IEnumerator ExecuteMovementPhase()
    {
        Debug.Log("Ex�cution de la phase de mouvement");

        // En principe, toutes les escadrilles en mouvement se d�placent pendant cette phase
        // Pour le prototype, on attend simplement 2 secondes
        yield return new WaitForSeconds(2f);

        // Passer automatiquement � la phase suivante
        NextPhase();
    }

    private IEnumerator ExecuteCombatPhase()
    {
        Debug.Log("Ex�cution de la phase de combat");

        // En principe, tous les combats sont r�solus pendant cette phase
        // Pour le prototype, on attend simplement 2 secondes
        yield return new WaitForSeconds(2f);

        // Passer automatiquement � la phase suivante
        NextPhase();
    }

    private void ApplyTurnResults()
    {
        Debug.Log("Application des r�sultats du tour");

        // Appliquer les r�parations aux bases a�riennes
        foreach (MapLocation location in locationManager.GetAllLocations())
        {
            if (location.controlledBy == currentSide.ToString() && location.currentHealth < location.maxHealth)
            {
                int repairAmount = 5; // Valeur arbitraire pour le prototype
                location.Repair(repairAmount);
            }
        }

        // Passer automatiquement � la phase suivante apr�s un court d�lai
        Invoke("NextPhase", 1f);
    }

    // M�thode publique pour acc�der au texte d'information du tour
    public string GetTurnInfoText()
    {
        string phaseText = currentPhase.ToString();
        string sideText = currentSide.ToString();
        string dateText = currentDate.ToShortDateString();
        string weatherText = currentWeather.ToString();

        return $"Tour {currentTurn} - {dateText}\nPhase: {phaseText}\nCamp: {sideText}\nM�t�o: {weatherText}";
    }
}
