using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxEnergy = 50;
    [SerializeField] private int dashEnergyCost = 10;
    [SerializeField] private int PunchEnergyCost = 2;

    private int currentHealth;
    private int currentEnergy;

    [Header("UI Elements")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider energySlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f; 
    [SerializeField] private float runSpeed = 8.0f;
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10.0f;
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [Header("Attacking")]
    [SerializeField] private float attackDuration = 0.5f;
    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private bool isAttacking = false;
    private bool isSitting = false;
    private bool isDead = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;

        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;

        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing || isAttacking)
        {
            return;
        }
        HandleSit();
        if (isSitting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        if (isDead) return;
        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        HandleMovement();
        HandleJump();
        HandleDashInput();
        HandlePunchAttackInput();
        UpdateAnimation(); 
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
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
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
    public void UseEnergy(int amount)
    {
        currentEnergy -= amount;
        if (currentEnergy < 0) currentEnergy = 0;
        energySlider.value = currentEnergy; // Cập nhật UI
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Đảm bảo máu không tụt xuống dưới 0
        if (currentHealth < 0) currentHealth = 0;

        hpSlider.value = currentHealth; // Cập nhật UI

        if (currentHealth <= 0)
        {
            HandleDie();
        }
    }
    private void HandleDie()
    {
        isDead = true;
        animator.SetTrigger("isDead"); // Kích hoạt Trigger chết

        // Tắt vật lý để nhân vật không bị trôi đi
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        // Tắt collider để kẻ địch có thể đi xuyên qua
        GetComponent<Collider2D>().enabled = false;

        // Tắt script này đi để người chơi không thể điều khiển được nữa
        this.enabled = false;
    }
    private void HandleSit()
    {
        bool isHoldingSitKey = Input.GetKey(KeyCode.S);
        animator.SetBool("isSitting", isHoldingSitKey);
        isSitting = isHoldingSitKey;
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
