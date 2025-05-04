using Assets.Src.Locations;
using Assets.Src.Managers;
using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Classe principale de gestion du jeu et du système de tours
public class InitScene : MonoBehaviour
{
    [Header("References")]
    public LocationManager locationManager;
    public AircraftManager aircraftManager;

    void Awake()
    {
        // Définit l'objet comme non destructible entre les scènes
        DontDestroyOnLoad(gameObject);

        // Appelle une fonction d'initialisation du jeu (exemple)
        InitGame();
    }

    void InitGame()
    {
        // Code d'initialisation du jeu




        // Exemple pour ajouter Londres
        LocationTypeData cityType = locationManager.locationTypes.Find(t => t.type == LocationType.City);
        locationManager.CreateLocation(cityType, "London", 51.5074f, -0.1278f, 0f, "Allied");

        // Exemple pour ajouter un aérodrome allemand
        LocationTypeData airfieldType = locationManager.locationTypes.Find(t => t.type == LocationType.Airfield);
        locationManager.CreateLocation(airfieldType, "Luftwaffe Airfield", 50.8796f, 6.9036f, 0f, "Axis");




        // Récupérer la base aérienne
        MapLocation londonAirfield = locationManager.GetLocationByName("RAF Northolt");

        // Créer une escadrille de Spitfires
        AircraftTypeData spitfireType = aircraftManager.aircraftTypes.Find(t => t.aircraftName == "Spitfire Mk.I");
        aircraftManager.CreateSquadron(spitfireType, "No. 303 Squadron", londonAirfield, 12, "Allied");
    }
}
