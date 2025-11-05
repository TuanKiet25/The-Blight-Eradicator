using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 🔥 QUAN TRỌNG: Cần thêm thư viện này để dùng TextMeshPro

public class PlayerController : MonoBehaviour
{
    // --- STATS ---
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxEnergy = 50;
    [SerializeField] private float dashEnergyCost = 10;
    [SerializeField] private float PunchEnergyCost = 2;
    [SerializeField] private float DoubleJumpEnergyCost = 5;
    [SerializeField] private float energyRegenRate = 5f;
    [SerializeField] private int maxLives = 4;

    private int currentLives;
    private float currentHealth;
    private float currentEnergy;

    // 🔥 LOGIC GOLD MỚI
    [Header("Gold/Score")]
    private int currentGold = 0;
    // Kéo thả TextMeshProUGUI từ Inspector vào đây
    [SerializeField] private TextMeshProUGUI goldText;
    // -----------------

    [Header("UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image heartLivesImage;
    [SerializeField] private Sprite[] heartSprites;

    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float runSpeed = 8.0f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 20.0f;
    [SerializeField] private int maxJumps = 1;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Attacking")]
    [SerializeField] private float attackDuration = 0.5f;

    [Header("Attack Properties")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    [Header("Colliders")]
    [SerializeField] private Collider2D standingCollider;
    [SerializeField] private Collider2D deathCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip punchSound;
    public AudioClip runSound;
    public AudioClip dieSound;

    [Header("Respawn")] 
    [SerializeField] private Transform spawnPoint;

    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private bool isAttacking = false;
    public bool isDead = false;
    private int jumpCount;
    private bool hasPlayedRunSound = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        standingCollider.enabled = true;
        deathCollider.enabled = false;

        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        // 🔥 KHỞI TẠO VÀNG
        currentGold = 0;
        UpdateGoldUI(); // Cập nhật UI ngay lập tức
        // ---------------

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;

        jumpCount = maxJumps;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        if (isDashing || isAttacking) return;

        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);

        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandlePunchAttackInput();
        RegenEnergy(energyRegenRate * Time.deltaTime);

        // Test damage
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
            Debug.Log("Đã nhận 10 sát thương! Máu còn: " + currentHealth);
        }

        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

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

    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpCount > 0)
            {
                jumpCount--;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

                if (jumpSound != null) audioSource.PlayOneShot(jumpSound);

                if (!isGrounded)
                {
                    animator.SetTrigger("isDoubleJumping");
                    UseEnergy(DoubleJumpEnergyCost);
                }
            }
        }
    }
    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.W) && currentEnergy >= dashEnergyCost)
        {
            UseEnergy(dashEnergyCost);
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;

        if (dashSound != null) audioSource.PlayOneShot(dashSound);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    private void HandlePunchAttackInput()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking && currentEnergy >= PunchEnergyCost)
        {
            UseEnergy(PunchEnergyCost);
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

        if (punchSound != null) audioSource.PlayOneShot(punchSound);

        yield return new WaitForSeconds(0.1f);
        PunchDamage();

        float waitTimeRemaining = attackDuration - 0.1f;
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        isAttacking = false;
    }

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

            var mimic = enemy.GetComponent<MimicController>();
            if (mimic != null)
            {
                // Player gây sát thương là PunchEnergyCost
                mimic.TakeDamage(PunchEnergyCost);
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

    // 🔥 HÀM MỚI: CỘNG VÀNG (Được gọi từ ChestController)
    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateGoldUI();
        // Bạn có thể thêm hiệu ứng âm thanh/pop-up UI ở đây
        Debug.Log("Player đã nhận " + amount + " vàng. Tổng: " + currentGold);
    }

    // 🔥 HÀM MỚI: Cập nhật hiển thị UI Vàng
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            // Hiển thị số vàng (bạn có thể tùy chỉnh định dạng)
            goldText.text = currentGold.ToString();
        }
    }
    // ----------------------------------------

    public void UseEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        energySlider.value = currentEnergy;
    }
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
                UpdateHealthUI();
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
    private void HandleDie()
    {
        isDead = true;
        animator.SetTrigger("isDeath");
        standingCollider.enabled = false;
        deathCollider.enabled = true;

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

    private void RegenEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        energySlider.value = currentEnergy;
    }
    private void UpdateAnimation()
    {
        bool isMoving = moveInput != 0;
        bool isJumping = !isGrounded;
        animator.SetBool("isWalking", isMoving && !isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isRunning", isMoving && isRunning);
        animator.SetBool("isDashing", isDashing);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    /// <summary>
    /// Dừng toàn bộ chuyển động, vật lý và animation.
    /// Dùng khi player bị stun, chết hoặc reset scene.
    /// </summary>
    public void StopAllMovement()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();

        // 1️⃣ Dừng vật lý ngay lập tức
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 2️⃣ Reset lại input (để phòng lỗi di chuyển sau khi dừng)
        moveInput = 0f;
        isRunning = false;
        isDashing = false;
        isAttacking = false;

        // 3️⃣ Reset lại animation (ngừng mọi hoạt ảnh di chuyển)
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isDashing", false);
    }
    public void RespawnFromFall()
    {
        // Nếu nhân vật đã chết (hết mạng) thì không làm gì cả
        if (isDead) return;

        // 1. Mất một mạng
        currentLives--;
        UpdateHealthUI(); // Cập nhật lại UI trái tim

        // 2. Kiểm tra xem đã hết mạng chưa
        if (currentLives <= 0)
        {
            // Nếu hết mạng, gọi hàm chết thật
            HandleDie();
        }
        else
        {
            // 3. Nếu còn mạng, hồi sinh
            currentHealth = maxHealth;        // Hồi lại 100 HP
            hpSlider.value = currentHealth; // Cập nhật lại thanh máu

            // 4. Dịch chuyển nhân vật về Spawn Point
            transform.position = spawnPoint.position;

            // 5. Reset lại vật lý để nhân vật không bị trôi
            rb.linearVelocity = Vector2.zero;
        }
    } 
}
