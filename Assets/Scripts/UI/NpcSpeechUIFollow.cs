using UnityEngine;

public class NpcSpeechUIFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);

    [Header("References")]
    [SerializeField] private RectTransform speechPanel;
    [SerializeField] private Canvas parentCanvas;
    [SerializeField] private Camera worldCamera;

    [Header("Follow")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothSpeed = 10f;

    private void Awake()
    {
        if (speechPanel == null)
        {
            speechPanel = GetComponent<RectTransform>();
        }

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (target == null || speechPanel == null || parentCanvas == null || worldCamera == null)
            return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);

        // Αν ο στόχος είναι πίσω από την κάμερα, κρύψε το panel
        bool isVisible = screenPos.z > 0f;
        speechPanel.gameObject.SetActive(isVisible);

        if (!isVisible)
            return;

        Vector2 anchoredPos;
        RectTransform canvasRect = parentCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera,
            out anchoredPos))
        {
            if (smoothFollow)
            {
                speechPanel.anchoredPosition = Vector2.Lerp(
                    speechPanel.anchoredPosition,
                    anchoredPos,
                    Time.deltaTime * smoothSpeed
                );
            }
            else
            {
                speechPanel.anchoredPosition = anchoredPos;
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}