using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 5.0f; 
    [SerializeField] private float runSpeed = 8.0f;
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 10.0f;
    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [Header("Dashing")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.5f;
    private Animator animator;
    private bool isGrounded;
    private Rigidbody2D rb;
    private bool isRunning;
    private float moveInput;
    private bool isDashing;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            return;
        }
        moveInput = Input.GetAxis("Horizontal");
        isRunning = Input.GetKey(KeyCode.LeftShift);
        HandleMovement();
        HandleJump();
        HandleDashInput();
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
        
        if (Input.GetKeyDown(KeyCode.W))
        {
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
