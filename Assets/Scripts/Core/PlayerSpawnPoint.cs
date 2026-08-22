using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Gizmo (editor only, for visualizing where the player will spawn)")]
    [SerializeField] private float gizmoRadius = 0.1f;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 1f, 0.8f);

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
        Gizmos.DrawSphere(transform.position + transform.forward * 0.5f, gizmoRadius * 0.5f);
    }
}