using UnityEngine;
using Common;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal;
using TMPro;
using Signals;
using System;
using UnityEngine.Rendering;


#if UNITY_EDITOR

using UnityEditor.Animations;
#endif

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
    [SerializeField] private AnimationClip AttackClip;



    // ------------------------------------------------------------------------------------------------------------------
    // Animatoins Additions
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
    // ------------------------------------------------------------------------------------------------------------------



    private float rollDuration;
    private float rollTimer;
    public float perfectDodgeWindow = 0.2f;
    private float LastDodgeTime = -Mathf.Infinity;
    public bool IsInPerfectDodgeWindow => Time.unscaledTime - LastDodgeTime <= perfectDodgeWindow;

    [Header("Perfect Dodge Settings")]
    [SerializeField] private float extraIFamesAfterPerfect = 0.35f;
    [SerializeField] private float timeScaleOnPerfect = 0.25f;
    [SerializeField] private float timeScaleDuration;
    [SerializeField] private float counterWindowDuration = 0.6f;

    public Material _materialBulletTime;
    public float BulletTimeTimer;

    private static int _WaveDistanceFromCenter = Shader.PropertyToID("_WaveDistanceFromCenter");

    private float invulnUntil = -Mathf.Infinity;
    private float counterUntil = -Mathf.Infinity;

    public bool IsInvulnerable => Time.unscaledTime < invulnUntil || IsRolling;
    public bool CanCounter => Time.unscaledTime < counterUntil;

    public event Action OnPerfectDodge;

    private BulletTimePostProcessingScript bulletTimePostProcessingScript;

    private float AttackDuration;
    private float AttackTimer;
    private float[] AttackDamage = new float[] { 12, 31 };

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
    MagicManager magic;
    PlayerAttack PlayerAttacks;
    Sender<int> DamageOutput;

    private float targetTimeScale = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        castingSlowMoColor = new CastingSlowMoColor();
        CurrentHealth = GetComponent<Health>();
        castingSlowMoColor.R = 150;
        castingSlowMoColor.G = 141;
        castingSlowMoColor.B = 255;

        StartCoroutine(GetPlayerAtkRef());

        float width = HealthBar.gameObject.GetComponent<RectTransform>().rect.width;
        Mathf.Ceil(width);
        CurrentHealth.health = width;
        HealthFloatObj.text = $"{CurrentHealth.health.ToString("F0")}HP";

        float mWidth = ManaBar.gameObject.GetComponent<RectTransform>().rect.width;
        CurrentMana = mWidth;
        Mathf.Ceil(CurrentMana);
        ManaFloat.text = $"{CurrentMana.ToString("F0")}MP";

        rollDuration = rollingClip.length;
        runAttackDuration = runAttackClip.length;
        heavyAttackDuration = heavyAttackClip.length;
        castingDuration = castingClip.length;
        AttackDuration = AttackClip.length;

        if (MagicPanel.isActiveAndEnabled)
        {
            MagicPanel.gameObject.SetActive(false);
        }
        magic = FindFirstObjectByType<MagicManager>();

        DarkBoltBtn.onClick.AddListener(OnDarkBoltCast);
        BoltBtn.onClick.AddListener(OnBoltCast);

        animHashGenerator.GenerateAnimHash(AnimationClipHashes, playerAnimatorController);

        sFXManager = FindFirstObjectByType<SFXManager>();
        PlayerAttacks = GetComponentInChildren<PlayerAttack>();

        _materialBulletTime = transform.GetChild(0).GetComponent<SpriteRenderer>().material;

        bulletTimePostProcessingScript = GetComponent<BulletTimePostProcessingScript>();
    }

    private void Update()
    {

        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * 5f);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        HandleInput();
        IsLockedOn = Input.GetKey(KeyCode.LeftShift);
        UpdateHealthBar();
        UpdateHitBoxOffset(PlayerAttackHitbox);
    }

    private void HandleInput()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && Time.unscaledTime - LastDodgeTime >= rollDuration && movementInput != Vector2.zero && !IsRolling)
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
        else if (Input.GetMouseButtonDown(0) && movementInput == Vector2.zero && !IsRolling && !IsAttacking && IsLockedOn)
        {
            StartAttack();
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
        else if (IsAttacking && IsLockedOn)
        {
            UpdateAttack();
        }
        else
        {
            UpdateMovement();
        }
    }

    private void StartRoll()
    {
        LastDodgeTime = Time.unscaledTime;
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

    private void StartAttack()
    {
        PlayerAttackHitbox.gameObject.SetActive(true);
        IsAttacking = true;
        IsLockedOn = true;
        AttackTimer = AttackDuration;
        float Pitch = (float)UnityEngine.Random.Range(1f, 3f);
        sFXManager.PlaySFX(sFXManager.Attack, Pitch);
    }

    private void UpdateAttack()
    {
        AttackTimer -= Time.fixedDeltaTime;
        if (AttackTimer <= 0)
        {
            IsAttacking = false;
            PlayerAttackHitbox.gameObject.SetActive(false);
        }

        int dmg = (int)UnityEngine.Random.Range(AttackDamage[0], AttackDamage[1]);
        DamageOutput = new Sender<int>(PlayerAttacks.DamageAmount, dmg);
        DamageOutput.TransferData();
    }

    private void StartHeavyAttack()
    {
        PlayerAttackHitbox.gameObject.SetActive(true);
        IsHeavyAttacking = true;
        IsRunning = true;
        heavyAttackTimer = heavyAttackDuration;
        lockedHeavyAttackDirection = movementData.GetDirectionFromInput(movementInput.x, movementInput.y);
        float Pitch = (float)UnityEngine.Random.Range(1f, 3f);
        sFXManager.PlaySFX(sFXManager.Kick, Pitch);
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

        float Pitch = UnityEngine.Random.Range(1f, 3f);
        sFXManager.PlaySFX(sFXManager.SpinAttack, Pitch);
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
    }

    private void UpdateCasting()
    {
        if (!waitingForSpellSelection)
        {
            castingTimer -= Time.fixedDeltaTime;

            if (castingTimer <= 0f)
            {
                waitingForSpellSelection = true;
                targetTimeScale = 0.0f;
                sFXManager.PlaySFX(sFXManager.Open, 1);
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
        sFXManager.PlaySFX(sFXManager.Select, 1);
        StartCoroutine(WaitForClickAndCast(pfBolt, magic.bolt.Cost, sFXManager.BoltSFX));
    }

    private void OnDarkBoltCast()
    {
        sFXManager.PlaySFX(sFXManager.Select, 1);
        StartCoroutine(WaitForClickAndCast(pfDarkBolt, magic.darkBolt.Cost, sFXManager.DarkBoltSFX));
    }

    private IEnumerator WaitForClickAndCast(GameObject spellPrefab, int SpellManaCost, AudioClip SpellPfSFX)
    {
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
            sFXManager.PlaySFX(SpellPfSFX, 1);
            Destroy(spell, 0.8f);
        }
        else
        {
            sFXManager.PlaySFX(sFXManager.Denied, 1);
            yield return null;
        }

        EndCasting();
    }

    private void EndCasting()
    {
        MagicPanel.gameObject.SetActive(false);
        targetTimeScale = 1f;
        IsCasting = false;
        light2D.color = Color.white;
        waitingForSpellSelection = false;
    }

    public void TriggerPerfectDodge(Vector2 hitPoint)
    {
        invulnUntil = Time.unscaledTime + extraIFamesAfterPerfect;
        counterUntil = Time.unscaledTime + counterWindowDuration;
        OnPerfectDodge?.Invoke();
        StartCoroutine(PerfectDodgeSlowMo());
    }

    private IEnumerator PerfectDodgeSlowMo()
    {
        StartCoroutine(BulletTimeVFX(-0.1f, 1f));
        StartCoroutine(bulletTimePostProcessingScript.StartPostProcessingEffect(0.43f, timeScaleDuration));

        targetTimeScale = timeScaleOnPerfect;
        yield return new WaitForSecondsRealtime(timeScaleDuration);
        targetTimeScale = 1f;
    }

    private IEnumerator BulletTimeVFX(float start, float end)
    {
        _materialBulletTime.SetFloat("_WaveDistanceFromCenter", start);

        float lerpedAmount = -0.1f;
        float elapsedTime = 0f;

        while (elapsedTime <= BulletTimeTimer)
        {
            elapsedTime += Time.unscaledDeltaTime;
            lerpedAmount = Mathf.Lerp(start, end, (elapsedTime / BulletTimeTimer));
            _materialBulletTime.SetFloat("_WaveDistanceFromCenter", lerpedAmount);
            yield return null;
        }
        _materialBulletTime.SetFloat("_WaveDistanceFromCenter", -0.1f);
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

    private void MoveCharacter(Vector2 direction, float speed, bool ignoreBulletTime = true)
    {
        float effectiveSpeed = ignoreBulletTime
            ? speed / Mathf.Max(Time.timeScale, 0.0001f)   // unaffected by slowdown
            : speed;                                       // respect slowdown

        Vector2 newPosition = rb.position + direction * effectiveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
        CurrentVelocity = direction * effectiveSpeed;
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
        yield return new WaitForSeconds(0.5f);
        PlayerAttackHitbox.gameObject.SetActive(false);
    }
}