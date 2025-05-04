# Guide d'implémentation du wargame "Eagle Day Remake"

Ce guide vous explique comment mettre en place les systèmes pour votre jeu de stratégie aérienne basé sur une carte sphérique.

## 1. Configuration initiale

### Hiérarchie de base du projet
```
└── Earth (GameObject avec une sphère)
    ├── Locations
    │   ├── Airfields
    │   ├── Cities
    │   ├── Factories
    │   └── ...
    ├── Units
    │   ├── Allied
    │   └── Axis
    └── Managers
        ├── GameManager
        ├── LocationManager
        ├── AircraftManager
        └── UIManager
```

### Configuration de la Terre
1. Créez un GameObject avec une sphère pour représenter la Terre
2. Appliquez une texture de carte appropriée
3. Définissez sa taille (recommandé: rayon de 10 unités)

## 2. Système de coordonnées

1. **Ajoutez le script CoordinateConverter** dans votre projet
2. Si nécessaire, ajustez le rayon de la Terre dans le script:
   ```csharp
   CoordinateConverter.SetEarthRadius(10f); // À adapter selon votre échelle
   ```

## 3. Système de caméra

1. **Ajoutez le script SphereCameraController** à votre caméra principale
2. Configurez le script dans l'inspecteur:
   - Assignez le GameObject de la Terre à `sphereTransform`
   - Ajustez les paramètres `minDistance`, `maxDistance`, `zoomSpeed` et `rotationSpeed`

## 4. Système de lieux stratégiques

### A. Configuration des types de lieux
1. Créez un dossier "ScriptableObjects" dans votre projet
2. Dans Unity, créez des types de lieux (Assets > Create > War Game > Location Type):
   
   Pour un aérodrome:
   - Type: Airfield
   - DisplayName: "Aérodrome"
   - Cochez canLaunchAircraft et canRepairAircraft
   - DefenseValue: 30
   
   Pour une ville:
   - Type: City
   - DisplayName: "Ville"
   - DefenseValue: 10
   - StrategicValue: 50

### B. Création du gestionnaire de lieux
1. Créez un GameObject vide nommé "LocationManager" dans la hiérarchie
2. Ajoutez-y le script `LocationManager`
3. Assignez le transform de la Terre à `earthTransform`
4. Ajoutez les types de lieux créés à la liste `locationTypes`

### C. Ajout de lieux sur la carte
Dans un script d'initialisation ou via l'éditeur, ajoutez des lieux:

```csharp
// Exemple pour ajouter Londres
LocationTypeData cityType = locationManager.locationTypes.Find(t => t.type == LocationType.City);
locationManager.CreateLocation(cityType, "London", 51.5074f, -0.1278f, 0f, "Allied");

// Exemple pour ajouter un aérodrome allemand
LocationTypeData airfieldType = locationManager.locationTypes.Find(t => t.type == LocationType.Airfield);
locationManager.CreateLocation(airfieldType, "Luftwaffe Airfield", 50.8796f, 6.9036f, 0f, "Axis");
```

## 5. Système d'unités aériennes

### A. Configuration des types d'avions
1. Créez des types d'avions avec ScriptableObjects (Assets > Create > War Game > Aircraft Type):
   
   Pour un chasseur:
   - Type: Fighter
   - AircraftName: "Spitfire Mk.I"
   - MaxSpeed: 580
   - MaxAltitude: 9000
   - Range: 500
   - AttackPower: 80
   - DefensePower: 70
   
   Pour un bombardier:
   - Type: Bomber
   - AircraftName: "Heinkel He 111"
   - MaxSpeed: 415
   - MaxAltitude: 6500
   - Range: 1200
   - BombLoad: 2.0
   - Accuracy: 60

### B. Création du gestionnaire d'unités
1. Créez un GameObject vide nommé "AircraftManager"
2. Ajoutez-y le script `AircraftManager`
3. Assignez le transform de la Terre à `earthTransform`
4. Ajoutez les types d'avions créés à la liste `aircraftTypes`

### C. Ajout d'escadrilles
Dans un script d'initialisation ou via l'éditeur, ajoutez des escadrilles:

```csharp
// Récupérer la base aérienne
MapLocation londonAirfield = locationManager.GetLocationByName("RAF Northolt");

// Créer une escadrille de Spitfires
AircraftTypeData spitfireType = aircraftManager.aircraftTypes.Find(t => t.aircraftName == "Spitfire Mk.I");
aircraftManager.CreateSquadron(spitfireType, "No. 303 Squadron", londonAirfield, 12, "Allied");
```

## 6. Système de tours de jeu

1. Créez un GameObject vide nommé "GameManager"
2. Ajoutez-y le script `GameManager`
3. Configurez les références vers `LocationManager` et `AircraftManager`
4. Paramétrez:
   - Nombre de tours par jour (`turnsPerDay`)
   - Date de départ (`currentDate`)

## 7. Interface utilisateur de base

1. Créez un Canvas pour l'UI
2. Ajoutez des éléments UI:
   - Texte pour les informations du tour
   - Bouton pour passer à la phase suivante
   - Panneau d'information sur les lieux sélectionnés
   - Panneau d'information sur les escadrilles sélectionnées
3. Ajoutez le script `TurnSystemUI` au Canvas et configurez les références

## 8. Interactions de base

### A. Sélection des lieux et unités
Créez un script de sélection:

```csharp
public class SelectionManager : MonoBehaviour
{
    private Camera mainCamera;
    private MapLocation selectedLocation;
    private AircraftSquadron selectedSquadron;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Vérifier si on a cliqué sur un lieu
                MapLocation location = hit.transform.GetComponentInParent<MapLocation>();
                if (location != null)
                {
                    SelectLocation(location);
                    return;
                }
                
                // Vérifier si on a cliqué sur une escadrille
                AircraftSquadron squadron = hit.transform.GetComponentInParent<AircraftSquadron>();
                if (squadron != null)
                {
                    SelectSquadron(squadron);
                    return;
                }
            }
        }
    }
    
    private void SelectLocation(MapLocation location)
    {
        // Désélectionner l'ancien lieu
        if (selectedLocation != null)
            selectedLocation.Deselect();
            
        // Sélectionner le nouveau
        selectedLocation = location;
        selectedLocation.Select();
        
        // Mettre à jour l'UI
        // ...
    }
    
    private void SelectSquadron(AircraftSquadron squadron)
    {
        // Désélectionner l'ancienne escadrille
        if (selectedSquadron != null)
            selectedSquadron.Deselect();
            
        // Sélectionner la nouvelle
        selectedSquadron = squadron;
        selectedSquadron.Select();
        
        // Mettre à jour l'UI
        // ...
    }
}
```

### B. Envoi de missions

Pour envoyer une escadrille en mission, ajoutez une méthode comme celle-ci dans votre script de gestion des ordres:

```csharp
public void SendSelectedSquadronToTarget(MapLocation targetLocation)
{
    if (selectedSquadron != null && targetLocation != null)
    {
        // Définir l'altitude de mission (en mètres)
        float missionAltitude = 3000f;
        
        // Envoyer l'escadrille vers la cible
        AircraftManager.Instance.SendSquadronToLocation(selectedSquadron, targetLocation, missionAltitude);
    }
}
```

## 9. Prochaines étapes

Une fois que ces systèmes de base sont en place et fonctionnels, vous pourrez vous concentrer sur:

1. **Système de combat aérien**
   - Intercepteurs vs bombardiers
   - Combat air-air 
   - Défense anti-aérienne

2. **Système de logistique**
   - Production d'avions
   - Ravitaillement en carburant et munitions
   - Réparation des bases endommagées

3. **Intelligence et reconnaissance**
   - Visibilité limitée des unités ennemies
   - Avions de reconnaissance
   - Stations radar

4. **Conditions météorologiques**
   - Impact sur les opérations aériennes
   - Visibilité réduite
   - Conditions de vol dégradées

5. **Interface utilisateur complète**
   - Rapports de mission
   - Statistiques de campagne
   - Menus de planification avancés