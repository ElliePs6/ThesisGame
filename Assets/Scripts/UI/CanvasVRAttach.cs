using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class CanvasVRAttach : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private float distance = 1f;

    [Tooltip("1 = γεμίζει περίπου όλο το κάθετο οπτικό πεδίο. Πάνω από 1 καλύπτει ακόμα περισσότερο.")]
    [Range(0.5f, 2f)]
    [SerializeField] private float fieldOfViewCoverage = 1.2f;

    [Header("Canvas Resolution")]
    [SerializeField] private Vector2 canvasResolution =
        new Vector2(1920f, 1080f);

    [Header("Vertical Offset")]
    [Tooltip("Θετικό μετακινεί το panel ψηλότερα σε σχέση με το spawn point (χρήσιμο αν το spawn point σημαδεύει το ύψος της καρέκλας, όχι το ύψος των ματιών).")]
    [SerializeField] private float verticalOffset = 0f;

    private Canvas canvas;
    private RectTransform rectTransform;
    private Camera xrCamera;
    private bool positioned;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        ConfigureCanvas();
    }

    private void OnEnable()
    {
        positioned = false;
    }

    private void LateUpdate()
    {
        // Θέλουμε το panel να τοποθετηθεί ΜΙΑ φορά και μετά να μείνει
        // ακίνητο στον χώρο -- δεν ακολουθεί πλέον το κεφάλι σε κάθε frame.
        if (positioned)
            return;

        if (xrCamera == null)
        {
            ResolveXRCamera();
        }

        if (xrCamera == null)
        {
            return;
        }

        PositionOnceInFrontOfSpawnPoint();
    }

    private void ConfigureCanvas()
    {
        canvas.renderMode = RenderMode.WorldSpace;

        if (GetComponent<TrackedDeviceGraphicRaycaster>() == null)
        {
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();

        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        rectTransform.sizeDelta = canvasResolution;
    }

    private void ResolveXRCamera()
    {
        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null && xrOrigin.Camera != null)
        {
            xrCamera = xrOrigin.Camera;
            canvas.worldCamera = xrCamera;
            return;
        }

        xrCamera = Camera.main;

        if (xrCamera != null)
        {
            canvas.worldCamera = xrCamera;
        }
    }

    private void PositionOnceInFrontOfSpawnPoint()
    {
        // Προτιμάμε το PlayerSpawnPoint της σκηνής -- αυτό είναι η σταθερή
        // "θέση καθίσματος" στην οποία πρέπει να είναι κολλημένο το panel,
        // όχι εκεί που τυχαίνει να κοιτάει το κεφάλι τη στιγμή του Awake.
        // Αν η σκηνή δεν έχει PlayerSpawnPoint, πέφτουμε πίσω στην τρέχουσα
        // θέση/κατεύθυνση της κάμερας, ώστε να συνεχίσει να δουλεύει.
        PlayerSpawnPoint spawnPoint = FindFirstObjectByType<PlayerSpawnPoint>();

        Vector3 originPosition;
        Quaternion originRotation;

        if (spawnPoint != null)
        {
            originPosition = spawnPoint.transform.position;
            originRotation = spawnPoint.transform.rotation;
        }
        else
        {
            originPosition = xrCamera.transform.position;
            originRotation = xrCamera.transform.rotation;
        }

        Vector3 forward = originRotation * Vector3.forward;

        rectTransform.position = originPosition + forward * distance + Vector3.up * verticalOffset;
        rectTransform.rotation = originRotation;

        float verticalFovRadians = xrCamera.fieldOfView * Mathf.Deg2Rad;

        float visibleWorldHeight =
            2f * distance *
            Mathf.Tan(verticalFovRadians * 0.5f);

        float requiredScale =
            visibleWorldHeight /
            canvasResolution.y *
            fieldOfViewCoverage;

        rectTransform.localScale = Vector3.one * requiredScale;

        canvas.worldCamera = xrCamera;

        positioned = true;
    }
}