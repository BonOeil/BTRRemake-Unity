using UnityEngine;

public class SphereCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform sphereTransform; // La référence à la sphère

    [Header("Camera Settings")]
    public float minDistance = 10f;   // Distance minimale de la caméra à la sphère
    public float maxDistance = 50f;   // Distance maximale de la caméra à la sphère
    public float zoomSpeed = 10f;     // Vitesse de zoom
    public float rotationSpeed = 100f; // Vitesse de rotation
    public float smoothTime = 0.2f;   // Temps de lissage pour les mouvements de la caméra

    [Header("Current State")]
    public float currentDistance = 20f; // Distance actuelle de la caméra
    public float currentLatitude = 0f;  // Latitude actuelle (élévation)
    public float currentLongitude = 0f; // Longitude actuelle (rotation horizontale)

    // Variables privées pour le lissage des mouvements
    private Vector3 currentVelocity = Vector3.zero;
    private float zoomVelocity = 0f;
    private float latitudeVelocity = 0f;
    private float longitudeVelocity = 0f;

    // Variables pour le contrôle du drag avec la souris
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    void Start()
    {
        if (sphereTransform == null)
        {
            Debug.LogError("Sphere Transform not assigned to the SphereCameraController!");

            // Tentative de trouver automatiquement la sphère
            GameObject sphere = GameObject.FindGameObjectWithTag("Earth");
            if (sphere != null)
                sphereTransform = sphere.transform;
            else
                Debug.LogError("Could not find sphere with tag 'Earth'");
        }

        // Positionner la caméra initialement
        UpdateCameraPosition();
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
        UpdateCameraPosition();
    }

    void HandleMouseInput()
    {
        // Gérer le zoom avec la molette de la souris
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            float targetDistance = currentDistance - scrollInput * zoomSpeed;
            currentDistance = Mathf.SmoothDamp(currentDistance, Mathf.Clamp(targetDistance, minDistance, maxDistance), ref zoomVelocity, smoothTime);
        }

        // Gérer la rotation par drag avec la souris
        if (Input.GetMouseButtonDown(1)) // Bouton droit de la souris
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            float targetLongitude = currentLongitude - delta.x * rotationSpeed * Time.deltaTime * 0.1f;
            float targetLatitude = currentLatitude + delta.y * rotationSpeed * Time.deltaTime * 0.1f;

            // Limiter la latitude (pour éviter les rotations extrêmes aux pôles)
            targetLatitude = Mathf.Clamp(targetLatitude, -80f, 80f);

            // Appliquer les rotations avec lissage
            currentLongitude = Mathf.SmoothDampAngle(currentLongitude, targetLongitude, ref longitudeVelocity, smoothTime);
            currentLatitude = Mathf.SmoothDamp(currentLatitude, targetLatitude, ref latitudeVelocity, smoothTime);

            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleKeyboardInput()
    {
        // Rotation avec les touches WASD
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput != 0)
        {
            float targetLongitude = currentLongitude - horizontalInput * rotationSpeed * Time.deltaTime;
            currentLongitude = Mathf.SmoothDampAngle(currentLongitude, targetLongitude, ref longitudeVelocity, smoothTime);
        }

        if (verticalInput != 0)
        {
            float targetLatitude = currentLatitude + verticalInput * rotationSpeed * Time.deltaTime;
            targetLatitude = Mathf.Clamp(targetLatitude, -80f, 80f);
            currentLatitude = Mathf.SmoothDamp(currentLatitude, targetLatitude, ref latitudeVelocity, smoothTime);
        }

        // Zoom avec les touches Q et E
        if (Input.GetKey(KeyCode.E))
        {
            float targetDistance = currentDistance - zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.SmoothDamp(currentDistance, Mathf.Clamp(targetDistance, minDistance, maxDistance), ref zoomVelocity, smoothTime);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            float targetDistance = currentDistance + zoomSpeed * Time.deltaTime;
            currentDistance = Mathf.SmoothDamp(currentDistance, Mathf.Clamp(targetDistance, minDistance, maxDistance), ref zoomVelocity, smoothTime);
        }

        // Centrer la caméra avec la touche C
        if (Input.GetKeyDown(KeyCode.C))
        {
            CenterCamera();
        }
    }

    void UpdateCameraPosition()
    {
        if (sphereTransform == null) return;

        // Calculer la position cible de la caméra en fonction de la latitude et longitude
        Quaternion rotation = Quaternion.Euler(currentLatitude, currentLongitude, 0);
        Vector3 targetPosition = sphereTransform.position + rotation * Vector3.back * currentDistance;

        // Appliquer la position avec lissage
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // Orienter la caméra vers le centre de la sphère
        transform.LookAt(sphereTransform);
    }

    public void CenterCamera()
    {
        // Réinitialiser la position et la rotation
        currentLatitude = 0f;
        currentLongitude = 0f;
        currentDistance = (minDistance + maxDistance) / 2f; // Distance moyenne
    }

    public void LookAtCoordinates(float latitude, float longitude)
    {
        // Positionner la caméra pour regarder des coordonnées spécifiques
        currentLatitude = latitude;
        currentLongitude = -longitude; // Inversion car la logique de la caméra vs celle des coordonnées est inversée

        // Actualiser immédiatement
        UpdateCameraPosition();
    }

    public void SetAltitude(float distance)
    {
        // Définir l'altitude (distance) de la caméra
        currentDistance = Mathf.Clamp(distance, minDistance, maxDistance);
    }
}