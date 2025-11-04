using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxEnergy = 50;
    [SerializeField] private float dashEnergyCost = 10;
<<<<<<< HEAD
    [SerializeField] private float PunchEnergyCost = 2;
=======
    [SerializeField] private float PunchEnergyCost = 2; // Sát thương 1 cú đấm = 2f
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    [SerializeField] private float DoubleJumpEnergyCost = 5;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private int maxLives = 4;

    private int currentLives;
    private float currentHealth;
    private float currentEnergy;

    [Header("UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image heartLivesImage;
    [SerializeField] private Sprite[] heartSprites;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 8.0f;
<<<<<<< HEAD

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 20.0f;
    [SerializeField] private int maxJumps = 1;

=======
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 20.0f;
    [SerializeField] private int maxJumps = 1;
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Attacking")]
    [SerializeField] private float attackDuration = 0.5f;

<<<<<<< HEAD
    [Header("Attack Properties")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;
=======
    // ⚔️ THÊM: Biến kiểm tra va chạm tấn công
    [Header("Attack Properties")]
    [SerializeField] private Transform attackPoint; // Vị trí điểm tấn công
    [SerializeField] private float attackRange = 0.5f; // Bán kính tấn công
    [SerializeField] private LayerMask enemyLayer; // Layer của Enemy
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    [Header("Colliders")]
    [SerializeField] private Collider2D standingCollider;
    [SerializeField] private Collider2D deathCollider;

<<<<<<< HEAD
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip punchSound;
    public AudioClip runSound;
    public AudioClip dieSound;

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private bool isAttacking = false;
    private bool isDead = false;
    private int jumpCount;
<<<<<<< HEAD
    private bool hasPlayedRunSound = false;
=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        standingCollider.enabled = true;
        deathCollider.enabled = false;

        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
<<<<<<< HEAD
=======
        UpdateHealthUI();
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
<<<<<<< HEAD

        jumpCount = maxJumps;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
=======
        jumpCount = maxJumps;
    }
    void Start()
    {
        
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    }

    void Update()
    {
        if (isDead) return;
<<<<<<< HEAD

        if (isDashing || isAttacking) return;

        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

=======
        if (isDashing || isAttacking)
        {
            return;
        }

        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandlePunchAttackInput();
<<<<<<< HEAD
        RegenEnergy(energyRegenRate * Time.deltaTime);

        // Test damage
=======

        RegenEnergy(energyRegenRate * Time.deltaTime);

        //testdamage
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
            Debug.Log("Đã nhận 10 sát thương! Máu còn: " + currentHealth);
        }

        UpdateAnimation();
    }

<<<<<<< HEAD
=======
    private void UpdateHealthUI()
    {
        int spriteIndex = maxLives - currentLives;

        if (spriteIndex >= 0 && spriteIndex < heartSprites.Length)
        {
            heartLivesImage.sprite = heartSprites[spriteIndex];
        }
    }
    

>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
<<<<<<< HEAD

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            PlayRunSoundOnce();
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            PlayRunSoundOnce();
        }
        else
        {
            hasPlayedRunSound = false;
        }
    }

    private void PlayRunSoundOnce()
    {
        if (!hasPlayedRunSound && runSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(runSound);
            hasPlayedRunSound = true;
        }
    }

=======
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

    }
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
        }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpCount > 0)
            {
                jumpCount--;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

<<<<<<< HEAD
                if (jumpSound != null) audioSource.PlayOneShot(jumpSound);

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
                if (!isGrounded)
                {
                    animator.SetTrigger("isDoubleJumping");
                    UseEnergy(DoubleJumpEnergyCost);
                }
            }
        }
    }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && currentEnergy >= dashEnergyCost)
        {
            UseEnergy(dashEnergyCost);
            StartCoroutine(Dash());
        }
    }
<<<<<<< HEAD

    private IEnumerator Dash()
    {
        isDashing = true;

        if (dashSound != null) audioSource.PlayOneShot(dashSound);

=======
    private IEnumerator Dash()
    {
        isDashing = true;
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void HandlePunchAttackInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking && currentEnergy >= PunchEnergyCost)
        {
            UseEnergy(PunchEnergyCost);
            StartCoroutine(Attack());
        }
    }

<<<<<<< HEAD
=======
    // ⚔️ Hàm gây sát thương cho Enemy
    private void PunchDamage()
    {
        // Quét tất cả collider trong phạm vi tấn công
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // Thử gọi TakeDamage trên EnemyController
            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(PunchEnergyCost);
                continue;
            }

            // Thử gọi TakeDamage trên Enemy1Controller
            var enemy1Controller = enemy.GetComponent<Enemy1Controller>();
            if (enemy1Controller != null)
            {
                enemy1Controller.TakeDamage(PunchEnergyCost);
            }
        }
    }

>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

<<<<<<< HEAD
        if (punchSound != null) audioSource.PlayOneShot(punchSound);

        yield return new WaitForSeconds(0.1f);
        PunchDamage();

=======
        // 1. Chờ 0.1s để khớp với animation frame gây sát thương
        yield return new WaitForSeconds(0.1f);

        // 2. Gây Sát Thương
        PunchDamage();

        // 3. Chờ phần còn lại của hoạt ảnh
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
        float waitTimeRemaining = attackDuration - 0.1f;
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        isAttacking = false;
    }

<<<<<<< HEAD
    private void PunchDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // ✅ Đổi sang Boss1
            var boss1 = enemy.GetComponent<BossController>();
            if (boss1 != null)
            {
                boss1.TakeDamage(PunchEnergyCost);
                continue;
            }

            var enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.TakeDamage(PunchEnergyCost);
                continue;
            }

            var rangedEnemyController = enemy.GetComponent<RangedEnemyController>();
            if (rangedEnemyController != null)
            {
                rangedEnemyController.TakeDamage(PunchEnergyCost);
                continue;
            }

            var enemy1Controller = enemy.GetComponent<Enemy1Controller>();
            if (enemy1Controller != null)
            {
                enemy1Controller.TakeDamage(PunchEnergyCost);
            }
        }
    }
=======
    // ... (Các hàm UseEnergy, TakeDamage, HandleDie, RegenEnergy giữ nguyên logic của bạn) ...
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc

    public void UseEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        energySlider.value = currentEnergy;
    }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        hpSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            currentLives--;
            if (currentLives <= 0)
            {
<<<<<<< HEAD
=======
                UpdateHealthUI();
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
                HandleDie();
            }
            else
            {
                animator.SetTrigger("isLostHeart");
                currentHealth = maxHealth;
                hpSlider.value = currentHealth;
                UpdateHealthUI();
            }
        }
    }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void HandleDie()
    {
        isDead = true;
        animator.SetTrigger("isDeath");
        standingCollider.enabled = false;
        deathCollider.enabled = true;
<<<<<<< HEAD

        if (dieSound != null) audioSource.PlayOneShot(dieSound);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }

    private void UpdateHealthUI()
    {
        int spriteIndex = maxLives - currentLives;
        if (spriteIndex >= 0 && spriteIndex < heartSprites.Length)
        {
            heartLivesImage.sprite = heartSprites[spriteIndex];
        }
    }

=======
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void RegenEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        energySlider.value = currentEnergy;
    }
<<<<<<< HEAD

=======
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void UpdateAnimation()
    {
        bool isMoving = moveInput != 0;
        bool isJumping = !isGrounded;
        animator.SetBool("isWalking", isMoving && !isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isRunning", isMoving && isRunning);
        animator.SetBool("isDashing", isDashing);
    }

<<<<<<< HEAD
=======
    // ℹ️ Hỗ trợ Debug/Inspector: Vẽ Attack Range
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
<<<<<<< HEAD
}
=======
    public void StopAllMovement()
    {
        // 1. Dừng vật lý ngay lập tức
        rb.linearVelocity = Vector2.zero;

        // 2. Reset lại input (để phòng lỗi)
        moveInput = 0f;
        isRunning = false;

        // 3. Reset lại animation (quan trọng)
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
    }
}
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
