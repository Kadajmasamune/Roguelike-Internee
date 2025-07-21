using UnityEngine;
using Common;
using System.Collections.Generic;
using UnityEditor.Animations;
using System;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using UnityEngine.SocialPlatforms;

public class PlayerController : MonoBehaviour, IHasDirection, IHasVelocity, IHasBooleans
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollSpeedBoost = 2f;
    [SerializeField] private float attackSpeedBoost = 1.5f;

    [Header("Animation Clips")]
    [SerializeField] private AnimationClip rollingClip;
    [SerializeField] private AnimationClip runAttackClip;
    [SerializeField] private AnimationClip heavyAttackClip;
    [SerializeField] private AnimationClip castingClip;

    [Header("Animation")]
    [SerializeField] private AnimatorController playerAnimatorController;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 rollDirection;

    private AnimHashGenerator animHashGenerator = new AnimHashGenerator();
    public Dictionary<string, int> AnimationClipHashes { get; private set; } = new Dictionary<string, int>();

    private EntityMovement movementData = new EntityMovement();

    public Direction CurrentDirection { get; private set; }
    public Vector2 CurrentVelocity { get; private set; }

    private Direction lockedAttackDirection;
    private Direction lockedHeavyAttackDirection;

    private float rollDuration;
    private float rollTimer;

    private float runAttackDuration;
    private float runAttackTimer;

    private float heavyAttackDuration;
    private float heavyAttackTimer;

    [Header("Magic")]
    [SerializeField] GameObject DarkBolt;
    [SerializeField] GameObject Bolt;
    [SerializeField] Canvas MagicPanel;
    [SerializeField] Button DarkBoltBtn;
    [SerializeField] Button BoltBtn;
    private float castingDuration;
    private float castingTimer;
    private bool waitingForSpellSelection = false;

    [Header("State Booleans")]
    public bool IsRolling { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsLockedOn { get; private set; }
    public bool IsCasting { get; private set; }
    public bool IsHeavyAttacking { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rollDuration = rollingClip.length;
        runAttackDuration = runAttackClip.length;
        heavyAttackDuration = heavyAttackClip.length;
        castingDuration = castingClip.length;

        if (MagicPanel.isActiveAndEnabled)
        {
            MagicPanel.gameObject.SetActive(false);
        }

        DarkBoltBtn.onClick.AddListener(OnDarkBoltCast);
        BoltBtn.onClick.AddListener(OnBoltCast);

        animHashGenerator.GenerateAnimHash(AnimationClipHashes, playerAnimatorController);
    }

    private void Update()
    {
        HandleInput();
        IsLockedOn = Input.GetKey(KeyCode.LeftShift);
    }

    private void HandleInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && movementInput != Vector2.zero && !IsRolling)
        {
            StartRoll();
        }
        else if (Input.GetMouseButtonDown(1) && movementInput != Vector2.zero && !IsRolling && !IsAttacking && !IsHeavyAttacking)
        {
            StartHeavyAttack();
        }
        else if (Input.GetMouseButtonDown(0) && movementInput != Vector2.zero && !IsRolling && !IsAttacking)
        {
            StartRunAttack();
        }
        else if (Input.GetKeyDown(KeyCode.Q) && !IsRolling && !IsAttacking && !IsCasting)
        {
            StartCasting();
        }
    }

    private void FixedUpdate()
    {
        if (IsRolling)
        {
            UpdateRoll();
        }
        else if (IsCasting)
        {
            UpdateCasting();
        }
        else if (IsHeavyAttacking)
        {
            UpdateHeavyAttack();
        }
        else if (IsAttacking)
        {
            UpdateRunAttack();
        }
        else
        {
            UpdateMovement();
        }
    }

    private void StartRoll()
    {
        IsRolling = true;
        rollTimer = rollDuration;
        rollDirection = movementInput;
    }

    private void UpdateRoll()
    {
        rollTimer -= Time.fixedDeltaTime;
        if (rollTimer <= 0f) IsRolling = false;

        MoveCharacter(rollDirection, moveSpeed + rollSpeedBoost);
        CurrentDirection = movementData.GetDirectionFromInput(rollDirection.x, rollDirection.y);
    }

    private void StartHeavyAttack()
    {
        IsHeavyAttacking = true;
        IsRunning = true;
        heavyAttackTimer = heavyAttackDuration;
        lockedHeavyAttackDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
    }

    private void UpdateHeavyAttack()
    {
        heavyAttackTimer -= Time.fixedDeltaTime;
        if (heavyAttackTimer <= 0f) IsHeavyAttacking = false;

        MoveCharacter(movementInput, moveSpeed + attackSpeedBoost);
        CurrentDirection = lockedHeavyAttackDirection;
    }

    private void StartRunAttack()
    {
        IsAttacking = true;
        IsRunning = true;
        runAttackTimer = runAttackDuration;
        lockedAttackDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
    }

    private void UpdateRunAttack()
    {
        runAttackTimer -= Time.fixedDeltaTime;
        if (runAttackTimer <= 0f) IsAttacking = false;

        MoveCharacter(movementInput, moveSpeed + attackSpeedBoost);
        CurrentDirection = lockedAttackDirection;
    }

    private void StartCasting()
    {
        IsCasting = true;
        castingTimer = castingDuration;
        waitingForSpellSelection = false;
    }

    private void UpdateCasting()
    {
        if (!waitingForSpellSelection)
        {
            castingTimer -= Time.fixedDeltaTime;

            if (castingTimer <= 0f)
            {
                waitingForSpellSelection = true;
                Time.timeScale = 0.0f;
                MagicPanel.gameObject.SetActive(true);
                return;
            }

            CurrentVelocity = Vector2.zero;
        }
        else
        {
            CurrentVelocity = Vector2.zero;
        }
    }

    private void OnBoltCast()
    {
        StartCoroutine(WaitForClickAndCast(Bolt));
    }

    private void OnDarkBoltCast()
    {
        StartCoroutine(WaitForClickAndCast(DarkBolt));
    }

    private IEnumerator WaitForClickAndCast(GameObject spellPrefab)
    {
        Debug.Log("Waiting for target click...");

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos.z = 0f;

        if (spellPrefab != null)
        {
            GameObject spell = Instantiate(spellPrefab, clickPos, quaternion.identity);
            Destroy(spell, 1f); 
        }

        EndCasting();
    }

    private void EndCasting()
    {
        MagicPanel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        IsCasting = false;
        waitingForSpellSelection = false;
    }

    private void UpdateMovement()
    {
        if (movementInput == Vector2.zero)
        {
            IsRunning = false;
            CurrentVelocity = Vector2.zero;
            return;
        }

        MoveCharacter(movementInput, moveSpeed);

        if (!IsLockedOn)
        {
            CurrentDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
        }

        IsRunning = true;
    }

    private void MoveCharacter(Vector2 direction, float speed)
    {
        Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        CurrentVelocity = direction * speed;
    }
}



