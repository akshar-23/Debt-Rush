using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    [Header("Portrait")]
    [SerializeField] private Sprite portrait;

    [Header("Respawn Settings")]
    [SerializeField] private bool updateRespawnPoints = false;
    [SerializeField] private float respawnOffset = 1f;
    [SerializeField] private float respawnYOffset = 0f;

    private bool hasTriggered = false;
    private BoxCollider boxCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (hasTriggered || boxCollider == null) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerGO in players)
        {
            if (IsInsideBox(playerGO.transform.position))
            {
                PlayerController player = playerGO.GetComponent<PlayerController>();
                if (player != null)
                {
                    hasTriggered = true;

                    if (updateRespawnPoints)
                    {
                        GameplaySceneManager gsm = FindObjectOfType<GameplaySceneManager>();
                        if (gsm != null)
                        {
                            Vector3 respawnPos = transform.position;
                            respawnPos.y += respawnYOffset;
                            gsm.SetRespawnPoints(respawnPos, respawnOffset);
                        }
                    }

                    DialogueManager.GetInstance().EnterDialogueMode(inkJSON, player, this.gameObject, portrait);
                    return;
                }
            }
        }
    }

    private bool IsInsideBox(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector3 half = boxCollider.size * 0.5f;
        Vector3 center = boxCollider.center;

        return localPos.x >= center.x - half.x && localPos.x <= center.x + half.x &&
               localPos.z >= center.z - half.z && localPos.z <= center.z + half.z;
    }
}
