using UnityEngine;
using Common;
using Pathfinding;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections;

public class EnemyAIBehaviour : MonoBehaviour, IHasBooleans, IHasDirection, IHasVelocity
{
    public Direction CurrentDirection { get; private set; }
    public Direction LockedInAttackDirection;
    public Vector2 CurrentVelocity { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsRolling { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsLockedOn { get; private set; }
    public bool IsCasting { get; private set; }
    public bool IsHeavyAttacking { get; private set; }

    public bool IsShooter = false;
    public bool IsFighter = false;
    public bool IsAssassin = false;

    [SerializeField] private AIPath EnemyAIPathComponent;
    [SerializeField] private AIDestinationSetter EnemyAIDestinationSetter;

    [SerializeField] private float defaultSpeed = 6f;
    [SerializeField] private float ridingSpeed = 14f;

    [SerializeField] private AnimationClip Attack1Clip;
    private float AttackTimer;
    private float AttackDuration;

    private EntityMovement EnemyAIMovement;
    private Seeker seeker;

    private Vector3? firstPathNode = null;

    private bool IsInAttackingRange => EnemyAIPathComponent.remainingDistance <= 5f;
    private Transform playerTarget;

    [SerializeField] private GameObject _pfBullet; //Bullet Prefab
    [SerializeField] private Vector2 _velocityBullet;

    private ObjectPooler _objectPooler;

    void Start()
    {
        EnemyAIPathComponent = GetComponent<AIPath>();
        EnemyAIDestinationSetter = GetComponent<AIDestinationSetter>();
        EnemyAIMovement = new EntityMovement();

        seeker = GetComponent<Seeker>();
        if (seeker != null)
        {
            seeker.pathCallback += OnPathCalculated;
        }

        playerTarget = EnemyAIDestinationSetter.target;
        AttackDuration = Attack1Clip.length;

        if (_pfBullet == null)
        {
            Debug.LogError("Bullet Object Missing");
        }

        _objectPooler = FindFirstObjectByType<ObjectPooler>();
    }

    void OnDestroy()
    {
        if (seeker != null)
        {
            seeker.pathCallback -= OnPathCalculated;
        }
    }

    private void OnPathCalculated(Path p)
    {
        if (!p.error && p.vectorPath != null && p.vectorPath.Count > 1)
        {
            firstPathNode = p.vectorPath[1];
        }
        else
        {
            firstPathNode = null;
        }
    }

    void Update()
    {
        UpdateMovementState();

        if (IsInAttackingRange)
        {
            HandleAttackFacing();
        }
        else
        {
            UpdateDirectionAndVelocity();
        }

        HandleAttacks();
    }

    void FixedUpdate()
    {
        if (IsAttacking)
        {
            UpdateAttackState();
        }
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
        CurrentVelocity = EnemyAIPathComponent.velocity;

        if (CurrentVelocity.sqrMagnitude > 0.001f)
        {
            Direction newDirection = EnemyAIMovement.GetDirectionFromInput(CurrentVelocity.x, CurrentVelocity.y);
            if (newDirection != Direction.None)
                CurrentDirection = newDirection;
        }
        else
        {
            if (firstPathNode.HasValue)
            {
                Vector2 toNode = (Vector2)firstPathNode.Value - (Vector2)transform.position;
                if (toNode.sqrMagnitude > 0.001f)
                {
                    Direction newDirection = EnemyAIMovement.GetDirectionFromInput(toNode.x, toNode.y);
                    if (newDirection != Direction.None)
                        CurrentDirection = newDirection;
                }
                else
                {
                    CurrentDirection = Direction.None;
                }
            }
            else
            {
                CurrentDirection = Direction.None;
            }
        }
    }

    private bool hasFiredBullet = false;

    void InitiateAttack()
    {
        IsAttacking = true;
        IsLockedOn = true;
        EnemyAIPathComponent.canMove = false;
        AttackTimer = AttackDuration;
        hasFiredBullet = false; // reset for new attack
    }


    void HandleAttacks()
    {
        if (IsInAttackingRange && !IsAttacking)
        {
            InitiateAttack();
        }
    }

    void UpdateAttackState()
    {
        AttackTimer -= Time.fixedUnscaledDeltaTime;

        // Fire once when attack starts
        if (!hasFiredBullet)
        {
            StartCoroutine(FireBullet());
            hasFiredBullet = true;
        }

        if (AttackTimer <= 0)
        {
            IsAttacking = false;
            IsLockedOn = false;
            EnemyAIPathComponent.canMove = true;
        }
    }

    IEnumerator FireBullet()
    {
        if (_pfBullet == null || playerTarget == null) yield return null;

        int bulletCount = 4;
        float bulletSpacing = 0.5f;


        for (int i = 0; i < bulletCount; i++)
        {
            Quaternion bulletRotation = EnemyAIMovement.GetRotation(CurrentDirection);
            Vector2 bulletVelocity = EnemyAIMovement.DirectionToVector(CurrentDirection) * 20f;
            Vector3 spawnPos = transform.position + (Vector3)(EnemyAIMovement.DirectionToVector(CurrentDirection) * 0.5f);
            GameObject bullet = _objectPooler.GetObject(_pfBullet);
            bullet.transform.rotation = bulletRotation;
            bullet.transform.position = spawnPos;
        
            Physics2D.IgnoreCollision(
                bullet.GetComponent<Collider2D>(),
                GetComponent<Collider2D>()
            );

            Thug_Bullet bulletScript = bullet.GetComponent<Thug_Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Init(bulletVelocity , _pfBullet);
            }

            yield return new WaitForSeconds(bulletSpacing);
        }

    }

    void HandleAttackFacing()
    {
        if (playerTarget != null)
        {
            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Direction newDirection = EnemyAIMovement.GetDirectionFromInput(toPlayer.x, toPlayer.y);
                if (newDirection != Direction.None)
                {
                    CurrentDirection = newDirection;
                }
            }
        }
    }

}
