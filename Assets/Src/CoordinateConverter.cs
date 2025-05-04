using UnityEngine;

public static class CoordinateConverter2
{
    private const float EarthRadius = 6371f; // en km, à adapter selon l'échelle de votre jeu

    // Convertit des coordonnées GPS en position 3D
    public static Vector3 GpsToUnityPosition(float latitude, float longitude, float altitude = 0)
    {
        // Conversion en radians
        float latRad = latitude * Mathf.Deg2Rad;
        float lonRad = longitude * Mathf.Deg2Rad;

        // Calcul de la position sur la sphère
        float x = (EarthRadius + altitude) * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float z = (EarthRadius + altitude) * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        float y = (EarthRadius + altitude) * Mathf.Sin(latRad);

        return new Vector3(x, y, z);
    }

    // Convertit une position 3D en coordonnées GPS
    public static (float latitude, float longitude, float altitude) UnityPositionToGps(Vector3 position)
    {
        float altitude = position.magnitude - EarthRadius;

        // Calcul de la latitude et longitude
        float latitude = Mathf.Asin(position.y / position.magnitude) * Mathf.Rad2Deg;
        float longitude = Mathf.Atan2(position.z, position.x) * Mathf.Rad2Deg;

        return (latitude, longitude, altitude);
    }
}

// Classe utilitaire pour convertir entre coordonnées GPS et coordonnées Unity
public static class CoordinateConverter
{
    // Rayon de la Terre en Unity units (ajuster selon l'échelle de votre jeu)
    private static float earthRadius = 10f; // Par défaut à 10 unités

    // Permet de changer le rayon de référence
    public static void SetEarthRadius(float radius)
    {
        earthRadius = radius;
    }

    // Convertit des coordonnées GPS (latitude, longitude) en position 3D dans Unity
    public static Vector3 GpsToUnityPosition(float latitude, float longitude, float altitude = 0f)
    {
        // Conversion en radians
        float latRad = latitude * Mathf.Deg2Rad;
        float lonRad = longitude * Mathf.Deg2Rad;

        // Calcul de la position sur la sphère
        float radius = earthRadius + altitude / 1000f; // Conversion en km ou l'échelle utilisée

        // Les coordonnées sphériques vers cartésiennes
        float x = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        float y = radius * Mathf.Sin(latRad);

        return new Vector3(x, y, z);
    }

    // Convertit une position 3D Unity en coordonnées GPS (latitude, longitude, altitude)
    public static (float latitude, float longitude, float altitude) UnityPositionToGps(Vector3 position)
    {
        // Calcul du rayon (distance depuis le centre)
        float radius = position.magnitude;
        float altitude = (radius - earthRadius) * 1000f; // Conversion en mètres

        // Calcul de la latitude et longitude
        float latitude = Mathf.Asin(position.y / radius) * Mathf.Rad2Deg;
        float longitude = Mathf.Atan2(position.z, position.x) * Mathf.Rad2Deg;

        return (latitude, longitude, altitude);
    }

    // Calcule la distance en kilomètres entre deux points GPS
    public static float CalculateGpsDistance(float lat1, float lon1, float lat2, float lon2)
    {
        // Formule de la haversine pour la distance sur une sphère
        float latRad1 = lat1 * Mathf.Deg2Rad;
        float lonRad1 = lon1 * Mathf.Deg2Rad;
        float latRad2 = lat2 * Mathf.Deg2Rad;
        float lonRad2 = lon2 * Mathf.Deg2Rad;

        float dLat = latRad2 - latRad1;
        float dLon = lonRad2 - lonRad1;

        float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                  Mathf.Cos(latRad1) * Mathf.Cos(latRad2) *
                  Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        // Distance en kilomètres (rayon terrestre * angle central)
        float distance = 6371f * c; // 6371 km est le rayon moyen de la Terre

        return distance;
    }

    // Calcule un point à une distance et un azimut donnés depuis un point de départ
    public static (float latitude, float longitude) CalculateDestinationPoint(float startLat, float startLon, float distance, float bearing)
    {
        // Distance en radians (distance / rayon terrestre)
        float distanceRad = distance / 6371f;
        float bearingRad = bearing * Mathf.Deg2Rad;
        float startLatRad = startLat * Mathf.Deg2Rad;
        float startLonRad = startLon * Mathf.Deg2Rad;

        // Calcul du point de destination
        float destLatRad = Mathf.Asin(Mathf.Sin(startLatRad) * Mathf.Cos(distanceRad) +
                                      Mathf.Cos(startLatRad) * Mathf.Sin(distanceRad) * Mathf.Cos(bearingRad));

        float destLonRad = startLonRad + Mathf.Atan2(Mathf.Sin(bearingRad) * Mathf.Sin(distanceRad) * Mathf.Cos(startLatRad),
                                                   Mathf.Cos(distanceRad) - Mathf.Sin(startLatRad) * Mathf.Sin(destLatRad));

        // Convertir en degrés
        float destLat = destLatRad * Mathf.Rad2Deg;
        float destLon = destLonRad * Mathf.Rad2Deg;

        // Normaliser la longitude à [-180,180]
        destLon = (destLon + 540) % 360 - 180;

        return (destLat, destLon);
    }

    // Calcule l'azimut (cap) entre deux points GPS
    public static float CalculateBearing(float lat1, float lon1, float lat2, float lon2)
    {
        float latRad1 = lat1 * Mathf.Deg2Rad;
        float latRad2 = lat2 * Mathf.Deg2Rad;
        float lonDiffRad = (lon2 - lon1) * Mathf.Deg2Rad;

        float y = Mathf.Sin(lonDiffRad) * Mathf.Cos(latRad2);
        float x = Mathf.Cos(latRad1) * Mathf.Sin(latRad2) -
                  Mathf.Sin(latRad1) * Mathf.Cos(latRad2) * Mathf.Cos(lonDiffRad);

        float bearingRad = Mathf.Atan2(y, x);
        float bearingDeg = bearingRad * Mathf.Rad2Deg;

        // Normaliser à [0,360]
        bearingDeg = (bearingDeg + 360) % 360;

        return bearingDeg;
    }

    // Génère des points le long d'un grand cercle entre deux points GPS
    public static Vector3[] GenerateGreatCirclePath(float startLat, float startLon, float endLat, float endLon, int segments)
    {
        Vector3[] path = new Vector3[segments + 1];

        // Convertir en vecteurs normalisés
        Vector3 startPos = GpsToUnityPosition(startLat, startLon).normalized;
        Vector3 endPos = GpsToUnityPosition(endLat, endLon).normalized;

        // Générer les points intermédiaires avec une interpolation sphérique
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 pos = Vector3.Slerp(startPos, endPos, t);

            // Remettre à l'échelle correcte
            pos = pos.normalized * earthRadius;
            path[i] = pos;
        }

        return path;
    }
}