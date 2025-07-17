using UnityEngine;
using Common;
using System.Collections.Generic;
using UnityEditor.Animations;
using Unity.VisualScripting;
using System;

public class PlayerController : MonoBehaviour, IHasDirection, IHasVelocity, IHasBooleans
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float RollSpeed;
    [SerializeField] private float RunAttackSpeed;

    private EntityMovement PlayerMovementData = new EntityMovement();


    [Header("Roll")]
    [SerializeField] AnimationClip RollingClip;
    float rollDuration;
    float rollTimer = 0f;



    [Header("Attacks")]
    [SerializeField] AnimationClip RunAttackingClip;
    [SerializeField] AnimationClip KickAttackClip;

    float RunAttackDuration;
    float RunAttackTimer = 0f;

    float Attack1Duration;
    float Attack2Duration;
    float AttackTimer;

    float HeavyAttackDuration;
    float HeavyAttackTimer;



    [Header("Cast")]
    [SerializeField] AnimationClip castingClip;
    float CastingRange;
    float CastingDuration;
    float CastingTimer;
    GameObject Target;


    private Direction lockedAttackDirection;
    private Direction lockedKickDirection;


    [Header("Booleans")]
    public bool IsRolling { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsLockedOn { get; private set; }
    public bool IsCasting { get; private set; }
    public bool IsHeavyAttacking { get; private set; }

    [Header("Animation")]
    [SerializeField] AnimatorController PlayerAnimatorController;

    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private Vector2 rollDirection;

    private AnimHashGenerator animHashGenerator = new AnimHashGenerator();
    public Dictionary<string, int> AnimationClipHashes { get; set; } = new Dictionary<string, int>();

    public Direction CurrentDirection { get; private set; }
    public Vector2 CurrentVelocity { get; private set; }

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        RollSpeed = moveSpeed + 2f;
        RunAttackSpeed = moveSpeed + 1.5f;

        rollDuration = RollingClip.length;
        RunAttackDuration = RunAttackingClip.length;
        CastingDuration = castingClip.length;
        HeavyAttackDuration = KickAttackClip.length;



        animHashGenerator.GenerateAnimHash(AnimationClipHashes, PlayerAnimatorController);
    }

    private void Update()
    {
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (!IsRolling && Input.GetKeyDown(KeyCode.V) && movementInput != Vector2.zero)
        {
            IsRolling = true;
            rollTimer = rollDuration;
            rollDirection = movementInput;
        }
        else if (!IsRolling && !IsAttacking && !IsHeavyAttacking && Input.GetMouseButtonDown(1) && movementInput != Vector2.zero)
        {
            IsHeavyAttacking = true;
            IsRunning = true; 
            HeavyAttackTimer = KickAttackClip.length;
            lockedKickDirection = PlayerMovementData.GetDirectionFromInput(movementInput.x, movementInput.y);
        }

        else if (!IsRolling && !IsAttacking && Input.GetMouseButtonDown(0) && movementInput != Vector2.zero)
        {
            IsRunning = true;
            IsAttacking = true;
            RunAttackTimer = RunAttackDuration;
            lockedAttackDirection = PlayerMovementData.GetDirectionFromInput(movementInput.x, movementInput.y);
        }

        else if (!IsRolling && !IsAttacking && !IsCasting && Input.GetKeyDown(KeyCode.Q) && movementInput != Vector2.zero)
        {
            IsCasting = true;
            CastingTimer = CastingDuration;
        }

        IsLockedOn = Input.GetKey(KeyCode.LeftShift);

    }

    private void FixedUpdate()
    {
        if (IsRolling)
        {
            rollTimer -= Time.fixedDeltaTime;
            if (rollTimer <= 0f) IsRolling = false;

            Vector2 newPosition = playerRb.position + rollDirection * RollSpeed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            CurrentDirection = PlayerMovementData.GetDirectionFromInput(rollDirection.x, rollDirection.y);
            CurrentVelocity = rollDirection * RollSpeed;
        }
        else if (IsCasting)
        {
            CastingTimer -= Time.fixedDeltaTime;

            if (CastingTimer <= 0f)
            {
                IsCasting = false;
            }

            Debug.Log("Casting Magic!");
            CurrentVelocity = Vector2.zero;
        }
        else if (IsRunning && IsHeavyAttacking)
        {
            HeavyAttackTimer -= Time.fixedDeltaTime;
            if (HeavyAttackTimer <= 0f)
            {
                IsHeavyAttacking = false;
            }

            Vector2 newPosition = playerRb.position + movementInput * RunAttackSpeed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            CurrentDirection = lockedKickDirection; // Fixed direction for attack
            CurrentVelocity = movementInput * RunAttackSpeed;


        }
        else if (IsRunning && IsAttacking)
        {
            RunAttackTimer -= Time.fixedDeltaTime;
            if (RunAttackTimer <= 0f)
            {
                IsAttacking = false;
            }

            Vector2 newPosition = playerRb.position + movementInput * RunAttackSpeed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            CurrentDirection = lockedAttackDirection; // Fixed direction for attack
            CurrentVelocity = movementInput * RunAttackSpeed;

        }

        // if (IsLockedOn)
        // {
        //     Debug.Log("Locking In on Enemy");
        //     IsLockedOn = true;
        // }


        else if (movementInput != Vector2.zero)
        {
            Vector2 moveDir = movementInput;

            Vector2 newPosition = playerRb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
            playerRb.MovePosition(newPosition);
            CurrentDirection = PlayerMovementData.GetDirectionFromInput(movementInput.x, movementInput.y);
            CurrentVelocity = moveDir * moveSpeed;
            IsRunning = true;
        }
        else
        {
            CurrentVelocity = Vector2.zero;
            IsRunning = false;
        }
    }
}