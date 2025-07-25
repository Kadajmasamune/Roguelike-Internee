using UnityEngine;
using Common;
using MagicSpells;
using System.Collections.Generic;
using UnityEditor.Animations;
using System.Collections;
using UnityEngine.UI;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal;
using TMPro;
using Signals;
using System;

public class PlayerController : MonoBehaviour, IHasDirection, IHasVelocity, IHasBooleans
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rollSpeedBoost;
    [SerializeField] private float attackSpeedBoost;

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

    [SerializeField] BoxCollider2D PlayerAttackHitbox;


    private float rollDuration;
    private float rollTimer;

    private float runAttackDuration;
    private float runAttackTimer;
    private float[] runAttackDamage = new float[] { 32, 63 };

    private float heavyAttackDuration;
    private float heavyAttackTimer;
    private float[] heavyAttackDamage = new float[] { 46, 83 };


    [SerializeField] GameObject pfDarkBolt;
    [SerializeField] GameObject pfBolt;
    [SerializeField] Canvas MagicPanel;
    [SerializeField] Button BoltBtn;
    [SerializeField] Button DarkBoltBtn;
    [SerializeField] Light2D light2D;

    [SerializeField] TextMeshProUGUI HealthFloatObj;
    private Health CurrentHealth;
    [SerializeField] GameObject HealthBar;

    [SerializeField] TextMeshProUGUI ManaFloat;
    private float CurrentMana;
    [SerializeField] GameObject ManaBar;


    struct CastingSlowMoColor
    {
        public int R;
        public int G;
        public int B;
        public int Alpha;
    }

    public float castingDuration;
    public float castingTimer;
    public bool waitingForSpellSelection;


    CastingSlowMoColor castingSlowMoColor;

    [Header("State Booleans")]
    public bool IsRolling { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsAttacking { get; private set; }
    public bool IsLockedOn { get; private set; }
    public bool IsCasting { get; private set; }
    public bool IsHeavyAttacking { get; private set; }

    SFXManager sFXManager;
    SpellBook magic;

    PlayerAttack PlayerAttacks;

    Sender<int> DamageOutput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        castingSlowMoColor = new CastingSlowMoColor();
        CurrentHealth = GetComponent<Health>();
        castingSlowMoColor.R = 150;
        castingSlowMoColor.G = 141;
        castingSlowMoColor.B = 255;



        StartCoroutine(GetPlayerAtkRef());

        float width = HealthBar.gameObject.GetComponent<RectTransform>().rect.width;
        Mathf.Ceil(width);
        CurrentHealth.health = width;
        HealthFloatObj.text = $"{CurrentHealth.health.ToString("F0")}" + "HP";

        float mWidth = ManaBar.gameObject.GetComponent<RectTransform>().rect.width;
        CurrentMana = mWidth;
        Mathf.Ceil(CurrentMana);
        ManaFloat.text = $"{CurrentMana.ToString("F0")}" + "MP";


        rollDuration = rollingClip.length;
        runAttackDuration = runAttackClip.length;
        heavyAttackDuration = heavyAttackClip.length;
        castingDuration = castingClip.length;

        if (MagicPanel.isActiveAndEnabled)
        {
            MagicPanel.gameObject.SetActive(false);
        }
        magic = new SpellBook();

        DarkBoltBtn.onClick.AddListener(OnDarkBoltCast);
        BoltBtn.onClick.AddListener(OnBoltCast);
        animHashGenerator.GenerateAnimHash(AnimationClipHashes, playerAnimatorController);

        sFXManager = FindFirstObjectByType<SFXManager>();
        PlayerAttacks = GetComponentInChildren<PlayerAttack>();
    }

    private void Update()
    {
        HandleInput();
        IsLockedOn = Input.GetKey(KeyCode.LeftShift);
        UpdateHealthBar();
        UpdateHitBoxOffset(PlayerAttackHitbox);
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
        PlayerAttackHitbox.gameObject.SetActive(true);
        IsHeavyAttacking = true;
        IsRunning = true;
        heavyAttackTimer = heavyAttackDuration;
        lockedHeavyAttackDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
    }

    private void UpdateHeavyAttack()
    {
        heavyAttackTimer -= Time.fixedDeltaTime;
        if (heavyAttackTimer <= 0f)
        {
            IsHeavyAttacking = false;
            PlayerAttackHitbox.gameObject.SetActive(false);
        }

        MoveCharacter(movementInput, moveSpeed + attackSpeedBoost);
        CurrentDirection = lockedHeavyAttackDirection;

        int dmg = (int)UnityEngine.Random.Range(heavyAttackDamage[0], heavyAttackDamage[1]);
        DamageOutput = new Sender<int>(PlayerAttacks.DamageAmount, dmg);
        DamageOutput.TransferData();
    }




    private void StartRunAttack()
    {
        IsAttacking = true;
        IsRunning = true;
        PlayerAttackHitbox.gameObject.SetActive(true);
        runAttackTimer = runAttackDuration;
        lockedAttackDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
    }

    private void UpdateRunAttack()
    {
        runAttackTimer -= Time.fixedDeltaTime;
        if (runAttackTimer <= 0f)
        {
            IsAttacking = false;
            PlayerAttackHitbox.gameObject.SetActive(false);
        }

        MoveCharacter(movementInput, moveSpeed + attackSpeedBoost);
        CurrentDirection = lockedAttackDirection;
        int dmg = (int)UnityEngine.Random.Range(runAttackDamage[0], runAttackDamage[1]);
        DamageOutput = new Sender<int>(PlayerAttacks.DamageAmount, dmg);
        DamageOutput.TransferData();
    }






    private void StartCasting()
    {
        IsCasting = true;
        castingTimer = castingDuration;
        waitingForSpellSelection = false;
        // light2D.color = new Color(castingSlowMoColor.R , castingSlowMoColor.G , castingSlowMoColor.B , 255);
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
                sFXManager.PlaySFX(sFXManager.Open);
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
        sFXManager.PlaySFX(sFXManager.Select);
        StartCoroutine(WaitForClickAndCast(pfBolt, magic.Bolt.Cost, sFXManager.BoltSFX));
    }

    private void OnDarkBoltCast()
    {
        sFXManager.PlaySFX(sFXManager.Select);
        StartCoroutine(WaitForClickAndCast(pfDarkBolt, magic.DarkBolt.Cost, sFXManager.DarkBoltSFX));
    }

    private IEnumerator WaitForClickAndCast(GameObject spellPrefab, int SpellManaCost, AudioClip SpellPfSFX)
    {
        Debug.Log("Waiting for target click...");

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        clickPos.z = 0f;

        if (spellPrefab != null && CurrentMana > 0f && CurrentMana >= SpellManaCost)
        {
            CurrentMana -= SpellManaCost;

            RectTransform manaRect = ManaBar.GetComponent<RectTransform>();
            manaRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentMana);

            ManaFloat.text = $"{CurrentMana.ToString("F0")}MP";

            GameObject spell = Instantiate(spellPrefab, clickPos, quaternion.identity);
            sFXManager.PlaySFX(SpellPfSFX);
            Destroy(spell, 0.8f);
        }
        else
        {
            sFXManager.PlaySFX(sFXManager.Denied);
            Debug.Log("Not enough Mana!");
            yield return null;
        }

        EndCasting();
    }

    private void EndCasting()
    {
        MagicPanel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        IsCasting = false;
        light2D.color = Color.white;
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


    public void UpdateHealthBar()
    {
        CurrentHealth.health = Mathf.Max(0, CurrentHealth.health);

        RectTransform healthRect = HealthBar.GetComponent<RectTransform>();
        healthRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentHealth.health);

        HealthFloatObj.text = $"{CurrentHealth.health.ToString("F0")}HP";
    }

    public void UpdateHitBoxOffset(BoxCollider2D Hitbox)
    {
        switch (CurrentDirection)
        {
            case Direction.Right:
                Hitbox.offset = new Vector2(0.36f, 0.06f);
                break;

            case Direction.BottomRight:
                Hitbox.offset = new Vector2(0.26f, -0.12f);
                break;

            case Direction.Down:
                Hitbox.offset = new Vector2(0.03f, -0.27f);
                break;

            case Direction.BottomLeft:
                Hitbox.offset = new Vector2(-0.11f, -0.18f);
                break;

            case Direction.Left:
                Hitbox.offset = new Vector2(-0.36f, 0.06f);
                break;

            case Direction.UpLeft:
                Hitbox.offset = new Vector2(-0.16f, 0.19f);
                break;

            case Direction.Up:
                Hitbox.offset = new Vector2(-0.04f, 0.34f);
                break;

            case Direction.UpRight:
                Hitbox.offset = new Vector2(0.1f, 0.19f);
                break;

            default:
                break;
        }
    }

    private IEnumerator GetPlayerAtkRef()
    {
        PlayerAttackHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        PlayerAttackHitbox.gameObject.SetActive(false);
    }
}
