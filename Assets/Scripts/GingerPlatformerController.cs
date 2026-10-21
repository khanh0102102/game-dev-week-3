using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Small 2D controller for the Ginger practice prefab.
/// Move with A/D or arrow keys; jump with Space, W, or Up Arrow.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class GingerPlatformerController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField, Min(0f)] private float jumpImpulse = 8f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private bool jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            horizontalInput = 0f;
            return;
        }

        horizontalInput = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            horizontalInput -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            horizontalInput += 1f;

        if (keyboard.spaceKey.wasPressedThisFrame ||
            keyboard.wKey.wasPressedThisFrame ||
            keyboard.upArrowKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = body.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        body.linearVelocity = velocity;

        if (!jumpRequested)
            return;

        if (IsGrounded())
        {
            body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
        }

        jumpRequested = false;
    }

    private bool IsGrounded()
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;
        Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - 0.04f);
        Vector2 checkSize = new Vector2(Mathf.Max(0.1f, bounds.size.x * 0.8f), 0.06f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(checkCenter, checkSize, 0f, groundLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit != bodyCollider && !hit.isTrigger)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            return;

        Bounds bounds = col.bounds;
        Vector2 center = new Vector2(bounds.center.x, bounds.min.y - 0.04f);
        Vector2 size = new Vector2(Mathf.Max(0.1f, bounds.size.x * 0.8f), 0.06f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
