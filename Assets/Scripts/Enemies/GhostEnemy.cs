using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GhostEnemy : EnemyStateMachine
{
    [Header("Ghost Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Flashlight Settings")]
    [SerializeField] private Transform flashlight;
    [SerializeField] private float lightAngle = 60f;
    [SerializeField] private float lightRange = 4f;
    [SerializeField] private GameObject lightVisual;
    [SerializeField] private GameObject lightConeVisual;

    private Vector2 startPosition;
    private int direction = 1;
    private Transform shadowTarget;
    private Transform playerTarget;
    private bool shadowFoundWarningShown = false;
    private float debugTimer = 0f;

    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        SetState(EnemyState.Patrol);

        if (flashlight != null)
        {
            Light2D light = flashlight.GetComponent<Light2D>();
            if (light != null)
            {
                light.pointLightOuterAngle = lightAngle;
                light.pointLightInnerAngle = lightAngle * 0.5f;
                light.pointLightOuterRadius = lightRange;
            }
            else
            {
                Debug.LogError("Light2D component not found on flashlight!");
            }
        }
        else
        {
            Debug.LogError("Flashlight Transform is not assigned in Inspector!");
        }

        if (lightConeVisual != null)
            lightConeVisual.SetActive(false);
        if (lightVisual != null)
            lightVisual.SetActive(true);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTarget = player.transform;
        else
            Debug.LogError("Player not found with tag 'Player'!");
    }

    protected override void Update()
    {
        try
        {
            if (shadowTarget == null || Time.frameCount % 30 == 0)
            {
                GameObject shadow = GameObject.FindGameObjectWithTag("Shadow");
                if (shadow != null)
                {
                    shadowTarget = shadow.transform;
                    if (!shadowFoundWarningShown)
                    {
                        Debug.Log("Shadow found and assigned!");
                        shadowFoundWarningShown = true;
                    }
                }
                else if (!shadowFoundWarningShown)
                {
                    Debug.LogWarning("Shadow not found! Make sure tag is 'Shadow'");
                    shadowFoundWarningShown = true;
                }
            }

            if (playerTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
            }

            debugTimer += Time.deltaTime;
            if (debugTimer >= 2f && shadowTarget != null)
            {
                debugTimer = 0f;
                float dist = Vector2.Distance(transform.position, shadowTarget.position);
                Debug.Log("Distance to shadow: " + dist + ", State: " + currentState);
            }

            base.Update();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Update: " + e.Message + "\n" + e.StackTrace);
        }
    }

    protected override void Patrol()
    {
        try
        {
            float newX = transform.position.x + direction * moveSpeed * Time.deltaTime;
            transform.position = new Vector2(newX, transform.position.y);

            UpdateFlashlightDirection();

            if (shadowTarget != null && IsShadowInRange(detectionRadius))
            {
                Debug.Log("Shadow detected! Switching to Chase state!");
                SetState(EnemyState.Chase);
                return;
            }

            if (Mathf.Abs(transform.position.x - startPosition.x) > patrolRange)
            {
                direction *= -1;
            }

            transform.localScale = new Vector3(
                direction > 0 ? 1 : -1,
                transform.localScale.y,
                transform.localScale.z
            );

            if (lightConeVisual != null)
                lightConeVisual.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Patrol: " + e.Message);
        }
    }

    protected override void Chase()
    {
        try
        {
            Debug.Log("In Chase state!");

            if (shadowTarget == null)
            {
                Debug.Log("Shadow target is null in Chase!");
                SetState(EnemyState.Patrol);
                return;
            }

            Vector2 directionToShadow = (shadowTarget.position - transform.position).normalized;
            transform.position += (Vector3)directionToShadow * chaseSpeed * Time.deltaTime;

            UpdateFlashlightDirection();
            CheckLightCollisionWithShadow();

            if (lightConeVisual != null)
                lightConeVisual.SetActive(true);

            if (!IsShadowInRange(detectionRadius))
            {
                Debug.Log("Shadow out of range! Returning to Patrol!");
                SetState(EnemyState.Patrol);
                return;
            }

            if (IsShadowInRange(attackRange))
            {
                Debug.Log("Shadow in attack range! Attacking!");
                SetState(EnemyState.Attack);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Chase: " + e.Message + "\n" + e.StackTrace);
        }
    }

    protected override void Attack()
    {
        try
        {
            Debug.Log("In Attack state!");

            if (shadowTarget != null && IsShadowInRange(attackRange))
            {
                PlayerState ps = shadowTarget.GetComponent<PlayerState>();
                if (ps != null && ps.IsInShadowMode())
                {
                    Debug.Log("Shadow touched Ghost! Game Over!");
                    GameManager.Instance?.GameOver();
                }
            }
            SetState(EnemyState.Patrol);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Attack: " + e.Message);
        }
    }

    private void UpdateFlashlightDirection()
    {
        try
        {
            if (flashlight == null) return;

            float targetAngle = 0f;

            if (transform.localScale.x < 0)
            {
                targetAngle = 180f;
            }
            else
            {
                targetAngle = 0f;
            }

            flashlight.rotation = Quaternion.Euler(0, 0, targetAngle);

            if (lightVisual != null)
            {
                lightVisual.transform.rotation = flashlight.rotation;
            }

            if (lightConeVisual != null)
            {
                lightConeVisual.transform.rotation = flashlight.rotation;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in UpdateFlashlightDirection: " + e.Message);
        }
    }

    private bool IsShadowInRange(float range)
    {
        if (shadowTarget == null) return false;
        float distance = Vector2.Distance(transform.position, shadowTarget.position);
        return distance <= range;
    }

    private void CheckLightCollisionWithShadow()
    {
        try
        {
            Debug.Log("Checking light collision...");

            if (flashlight == null)
            {
                Debug.LogError("Flashlight is null!");
                return;
            }

            if (shadowTarget == null)
            {
                Debug.Log("Shadow target is null, can't check collision.");
                return;
            }

            // IMPORTANT: Get PlayerState from the Player, not from Shadow
            if (playerTarget == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTarget = player.transform;
                else
                {
                    Debug.LogError("Player not found!");
                    return;
                }
            }

            PlayerState ps = playerTarget.GetComponent<PlayerState>();
            if (ps == null)
            {
                Debug.LogError("PlayerState component not found on Player!");
                return;
            }

            if (!ps.IsInShadowMode())
            {
                Debug.Log("Player is in Light mode, ignoring light collision.");
                return;
            }

            float distance = Vector2.Distance(transform.position, shadowTarget.position);
            Debug.Log("Distance to shadow: " + distance + ", Light Range: " + lightRange);

            if (distance > lightRange)
            {
                Debug.Log("Shadow is too far: " + distance + " > " + lightRange);
                return;
            }

            Vector2 directionToShadow = (shadowTarget.position - transform.position).normalized;
            Vector2 flashlightDirection = flashlight.right;

            float angleToShadow = Vector2.Angle(flashlightDirection, directionToShadow);

            Debug.Log("Angle to shadow: " + angleToShadow + ", Max Angle: " + lightAngle / 2f);

            if (angleToShadow <= lightAngle / 2f)
            {
                Debug.Log("LIGHT HIT THE SHADOW! GAME OVER!");
                GameManager.Instance?.GameOver();
            }
            else
            {
                Debug.Log("Shadow is out of light cone. Angle: " + angleToShadow);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CRITICAL ERROR in CheckLightCollisionWithShadow: " + e.Message + "\n" + e.StackTrace);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        try
        {
            Debug.Log("Trigger entered with: " + other.gameObject.name);

            if (other.CompareTag("Shadow"))
            {
                PlayerState ps = other.GetComponent<PlayerState>();
                if (ps != null && ps.IsInShadowMode())
                {
                    Debug.Log("Shadow touched Ghost (Trigger)! Game Over!");
                    GameManager.Instance?.GameOver();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnTriggerEnter2D: " + e.Message);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        try
        {
            Debug.Log("Collision entered with: " + collision.gameObject.name);

            if (collision.gameObject.CompareTag("Shadow"))
            {
                PlayerState ps = collision.gameObject.GetComponent<PlayerState>();
                if (ps != null && ps.IsInShadowMode())
                {
                    Debug.Log("Shadow touched Ghost (Collision)! Game Over!");
                    GameManager.Instance?.GameOver();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnCollisionEnter2D: " + e.Message);
        }
    }

    private void OnDrawGizmos()
    {
        if (flashlight == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(flashlight.position, lightRange);

        Vector3 coneDirection = flashlight.right;
        float halfAngle = lightAngle / 2f;

        Vector3 leftEdge = Quaternion.Euler(0, 0, -halfAngle) * coneDirection;
        Vector3 rightEdge = Quaternion.Euler(0, 0, halfAngle) * coneDirection;

        Gizmos.DrawLine(flashlight.position, flashlight.position + leftEdge * lightRange);
        Gizmos.DrawLine(flashlight.position, flashlight.position + rightEdge * lightRange);
    }
}