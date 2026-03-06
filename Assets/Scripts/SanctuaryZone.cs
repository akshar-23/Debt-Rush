using UnityEngine;

public class SanctuaryZone : MonoBehaviour
{
    public static bool IsPlayerInSanctuary(Transform player)
    {
        foreach (var zone in FindObjectsOfType<SanctuaryZone>())
        {
            BoxCollider col = zone.GetComponent<BoxCollider>();
            if (col == null) continue;

            // Convert player position to zone's local space
            Vector3 localPos = zone.transform.InverseTransformPoint(player.position);
            Vector3 half = col.size * 0.5f;
            Vector3 center = col.center;

            if (localPos.x >= center.x - half.x && localPos.x <= center.x + half.x &&
                localPos.z >= center.z - half.z && localPos.z <= center.z + half.z)
                return true;
        }
        return false;
    }
}

