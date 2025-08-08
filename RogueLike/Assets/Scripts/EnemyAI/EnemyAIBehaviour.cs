using UnityEngine;
using Common;
using Pathfinding;



public class EnemyAIBehaviour : MonoBehaviour, IHasBooleans, IHasDirection, IHasVelocity
{
    public Direction CurrentDirection { get; private set; }
    public Vector2 CurrentVelocity { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsRolling { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsLockedOn { get; private set; }
    public bool IsCasting { get; private set; }
    public bool IsHeavyAttacking { get; private set; }

    [SerializeField] private AIPath EnemyAIPathComponent;
    [SerializeField] private AIDestinationSetter EnemyAIDestinationSetter;



    [SerializeField] private float defaultSpeed = 6f;
    [SerializeField] private float ridingSpeed = 14f;


    private EntityMovement EnemyAIMovement;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnemyAIPathComponent = GetComponent<AIPath>();
        EnemyAIDestinationSetter = GetComponent<AIDestinationSetter>();
        EnemyAIMovement = new EntityMovement();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovementState();
        UpdateDirectionAndVelocity();
        Debug.Log(EnemyAIPathComponent.velocity);
    }


    void UpdateMovementState()
    {
        float minDistanceFromTarget = 6f;

        if (EnemyAIPathComponent.remainingDistance > minDistanceFromTarget)
        {
            IsRolling = EnemyAIPathComponent.desiredVelocity.sqrMagnitude > 0.01f;
            EnemyAIPathComponent.maxSpeed = IsRolling ? ridingSpeed : defaultSpeed;
            IsRunning = false;
        }
        else
        {
            IsRolling = false;
            IsRunning = !EnemyAIPathComponent.reachedEndOfPath && !EnemyAIPathComponent.pathPending &&
                        EnemyAIPathComponent.velocity.magnitude > 0.05f;
            EnemyAIPathComponent.maxSpeed = defaultSpeed;
        }
    }

    void UpdateDirectionAndVelocity()
    {
        Vector2 velocity = EnemyAIPathComponent.desiredVelocity;

        if (velocity.sqrMagnitude > 0.01f)
        {
            CurrentVelocity = velocity;

            Direction newDirection = EnemyAIMovement.GetDirectionFromInput(velocity.x, velocity.y);
            if (newDirection != Direction.None && newDirection != CurrentDirection)
            {
                CurrentDirection = newDirection;
            }
        }
        else
        {
            CurrentVelocity = Vector2.zero;
            CurrentDirection = Direction.None;
        }
    }

}
