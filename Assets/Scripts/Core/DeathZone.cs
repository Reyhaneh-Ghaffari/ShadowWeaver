using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"💀 Triggered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Player") || other.CompareTag("Shadow"))
        {
            Debug.Log($"💀 {other.gameObject.name} fell into the gap!");

            // ریست کردن Rigidbody
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // GameOver
            GameManager.Instance?.GameOver();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}