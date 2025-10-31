using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
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

    [Header("UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image heartLivesImage;
    [SerializeField] private Sprite[] heartSprites;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    [Header("Colliders")]
    [SerializeField] private Collider2D standingCollider; 
    [SerializeField] private Collider2D deathCollider;

    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private bool isAttacking = false;
    private bool isDead = false;
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

    // Update is called once per frame
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
        if (currentEnergy < maxEnergy)
        {
            RegenEnergy(energyRegenRate * Time.deltaTime);
        }
        RegenEnergy(energyRegenRate * Time.deltaTime);
//testdamage
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Gọi hàm TakeDamage với một lượng sát thương bất kỳ (ví dụ: 10)
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
        if(moveInput > 0)  transform.localScale = new Vector3(1, 1, 1);
        else if(moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

    }
    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded)
        {
            jumpCount = maxJumps;
        }
        if (Input.GetButtonDown("Jump") )
        {
            if (jumpCount > 0)
            {
                jumpCount--; // Trừ 1 lượt nhảy

                // Mẹo hay: Reset vận tốc Y để cú nhảy 2 mạnh như cú nhảy 1
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                // Thêm lực nhảy (giữ nguyên)
                rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);

                // 5. Nếu đây là cú nhảy trên không (Double Jump)
                if (!isGrounded)
                {
                    // Kích hoạt animation Double Jump
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
        rb.gravityScale = 0f; // Tạm thời bỏ trọng lực để lướt thẳng
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, 0f); // Dùng velocity để có cú lướt dứt khoát

        // --- GIAI ĐOẠN ĐANG LƯỚT ---
        yield return new WaitForSeconds(dashDuration); // Tạm dừng hàm trong một khoảng thời gian

        // --- GIAI ĐOẠN KẾT THÚC LƯỚT ---
        rb.gravityScale = originalGravity; // Khôi phục trọng lực
        isDashing = false;
        // Có thể thêm một lực đẩy nhẹ để nhân vật không dừng đột ngột
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    private void HandlePunchAttackInput()
    {
        // "Fire1" là nút chuột trái hoặc Ctrl trái (mặc định của Unity)
        if (Input.GetButtonDown("Fire1") && isGrounded && currentEnergy >= PunchEnergyCost)
        {
            UseEnergy(PunchEnergyCost);
            StartCoroutine(Attack());
        }
    }
    private IEnumerator Attack()
    {
        isAttacking = true;
        animator.SetTrigger("isAttacking"); // Kích hoạt Trigger

        // (Sau này bạn sẽ thêm code tạo hitbox ở đây)

        // Chờ animation tấn công kết thúc
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }
    public void UseEnergy(float amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        energySlider.value = currentEnergy; // Cập nhật UI
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Đảm bảo máu không tụt xuống dưới 0
        if (currentHealth < 0) currentHealth = 0;

        hpSlider.value = currentHealth; // Cập nhật UI

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
                currentHealth = maxHealth;      // Hồi lại 100 HP
                hpSlider.value = currentHealth; // Cập nhật lại thanh máu
                UpdateHealthUI();
            }     
        }
    }
    private void HandleDie()
    {
        isDead = true;
        animator.SetTrigger("isDeath"); // Kích hoạt Trigger chết
        standingCollider.enabled = false;
        deathCollider.enabled = true;
        // Tắt duy chuyển ngang
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Tắt collider để kẻ địch có thể đi xuyên qua
        GetComponent<Collider2D>().enabled = false;
        // Tắt script này đi để người chơi không thể điều khiển được nữa
        this.enabled = false;
    }
    private void RegenEnergy(float amount)
    {
        currentEnergy += amount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        energySlider.value = currentEnergy; // Cập nhật UI
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
}
