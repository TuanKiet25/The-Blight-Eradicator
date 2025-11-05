using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float maxEnergy = 50;
    [SerializeField] private float dashEnergyCost = 10;
    [SerializeField] private float PunchEnergyCost = 2; // Sát thương 1 cú đấm = 2f
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
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 20.0f;
    [SerializeField] private int maxJumps = 1;
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Attacking")]
    [SerializeField] private float attackDuration = 0.5f;

    // ⚔️ THÊM: Biến kiểm tra va chạm tấn công
    [Header("Attack Properties")]
    [SerializeField] private Transform attackPoint; // Vị trí điểm tấn công
    [SerializeField] private float attackRange = 0.5f; // Bán kính tấn công
    [SerializeField] private LayerMask enemyLayer; // Layer của Enemy

    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    [Header("Colliders")]
    [SerializeField] private Collider2D standingCollider;
    [SerializeField] private Collider2D deathCollider;
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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        standingCollider.enabled = true;
        deathCollider.enabled = false;

        currentLives = maxLives;
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        UpdateHealthUI();

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
        jumpCount = maxJumps;
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (isDead) return;
        if (isDashing || isAttacking)
        {
            return;
        }

        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandlePunchAttackInput();

        RegenEnergy(energyRegenRate * Time.deltaTime);

        //testdamage
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10f);
            Debug.Log("Đã nhận 10 sát thương! Máu còn: " + currentHealth);
        }

        UpdateAnimation();
    }

    private void UpdateHealthUI()
    {
        int spriteIndex = maxLives - currentLives;

        if (spriteIndex >= 0 && spriteIndex < heartSprites.Length)
        {
            heartLivesImage.sprite = heartSprites[spriteIndex];
        }
    }
    

    private void HandleMovement()
    {
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

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

    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking");

        // 1. Chờ 0.1s để khớp với animation frame gây sát thương
        yield return new WaitForSeconds(0.1f);

        // 2. Gây Sát Thương
        PunchDamage();

        // 3. Chờ phần còn lại của hoạt ảnh
        float waitTimeRemaining = attackDuration - 0.1f;
        if (waitTimeRemaining > 0)
        {
            yield return new WaitForSeconds(waitTimeRemaining);
        }

        isAttacking = false;
    }

    // ... (Các hàm UseEnergy, TakeDamage, HandleDie, RegenEnergy giữ nguyên logic của bạn) ...

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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
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

    // ℹ️ Hỗ trợ Debug/Inspector: Vẽ Attack Range
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
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