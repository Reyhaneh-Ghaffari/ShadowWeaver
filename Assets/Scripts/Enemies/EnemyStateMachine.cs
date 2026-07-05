using UnityEngine;

public abstract class EnemyStateMachine : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Attack, Dead }

    protected EnemyState currentState = EnemyState.Patrol;
    protected Transform target;
    protected float detectionRadius = 5f;

    public System.Action<EnemyState> OnStateChanged;

    protected virtual void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetState(EnemyState.Patrol);
    }

    protected virtual void Update()
    {
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }
    }

    protected abstract void Patrol();
    protected abstract void Chase();
    protected abstract void Attack();

    protected void SetState(EnemyState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    protected bool IsPlayerInRange(float range)
    {
        if (target == null) return false;
        float distance = Vector2.Distance(transform.position, target.position);
        return distance <= range;
    }

    protected bool IsPlayerInSight()
    {
        if (target == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            (target.position - transform.position).normalized,
            detectionRadius,
            LayerMask.GetMask("Player", "Obstacle")
        );

        return hit.collider != null && hit.collider.CompareTag("Player");
    }
}