using UnityEngine;
using UnityEngine.InputSystem;

public class Cursor : MonoBehaviour
{
    public Sprite sprite;
    public GameObject BombPrefab;

    public int playerNumber = 2;

    public float moveSpeed = 10f;
    public float height = 10f;

    public bool canCursorMove;
    public PlayerController player;

    private bool ignoreFirstInput = true;

    [SerializeField] private AudioClip bombFallingSFX;

    void Start()
    {
        canCursorMove = true;
    }

    void Update()
    {
        if (ignoreFirstInput)
        {
            ignoreFirstInput = false;
        }
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        if (!ignoreFirstInput && canCursorMove)
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            Vector3 step = new Vector3(input.x, 0, input.y);
            transform.position += step * moveSpeed * Time.deltaTime;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !ignoreFirstInput && canCursorMove)
        {
            DropBomb();
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !ignoreFirstInput && canCursorMove)
        {
            CancelBomb();
        }
    }

    private void DropBomb()
    {
        canCursorMove = false;

        // Decrease bomb count only when actually dropped
        if (player != null && player.itemEquipped != null)
        {
            player.itemEquipped.currentCount--;
        }

        if (BombPrefab != null)
        {
            GameObject bomb = Instantiate(BombPrefab, new Vector3(transform.position.x, transform.position.y + height, transform.position.z), transform.rotation);
            bomb.GetComponent<Bomb>().playerId = player.playerNumber;

            if (bombFallingSFX != null)
            {
                AudioManager.Instance.PlaySFX(bombFallingSFX);
            }
        }

        Destroy(gameObject);

        player.hudref.UpdateItemCount(player.id, player.itemEquipped);

    }

    private void CancelBomb()
    {
        canCursorMove = false;
        // Don't decrease count when canceling
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnSpawnedObjectDestroyed();
        }
    }
}
