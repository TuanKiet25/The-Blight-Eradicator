using UnityEngine;
using System.Collections;
using System.Collections.Generic;

<<<<<<< Updated upstream
// public class ThanDichBenh : MonoBehaviour
// {
//     [Header("Stats")]
//     public float moveSpeed = 2f;
//     public float chargeSpeed = 6f;
//     public float chargeDelay = 1.2f;
//     public float detectRange = 8f;
//     public float summonRange = 3f; // Tăng range để summon an toàn hơn
//     public float stopDistance = 0.4f;

//     [Header("Summon Settings")]
//     [Tooltip("Prefab của Dai hoặc enemy khác cần triệu hồi")]
//     public GameObject summonPrefab; // Kéo prefab Dai vào đây

//     [Tooltip("Vị trí spawn tương đối so với ThanDichBenh (nếu null sẽ spawn tại chỗ)")]
//     public Transform summonSpawnPoint;

//     [Tooltip("Offset vị trí spawn nếu không dùng Transform (x, y, z)")]
//     public Vector3 summonOffset = new Vector3(1f, 0f, 0f); // Spawn cách 1 đơn vị về bên phải

//     [Tooltip("Thời gian chờ giữa các lần triệu hồi")]
//     public float summonCooldown = 5f; // Tăng cooldown vì summon mạnh hơn attack

//     [Tooltip("Số lượng Dai tối đa có thể triệu hồi cùng lúc")]
//     public int maxSummons = 2; // Giới hạn 2 Dai

//     [Tooltip("Thời gian animation triệu hồi (để đồng bộ với animation)")]
//     public float summonAnimationDuration = 1f;

//     private List<GameObject> activeSummons = new List<GameObject>(); // Danh sách Dai đang sống

//     [Header("Health")]
//     [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
//     [SerializeField] private float playerPunchDamage = 2f;
//     [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 5)")]
//     [SerializeField] private int requiredPunchesToKill = 5;
//     private float maxHealth;
//     private float currentHealth;

//     [Header("References")]
//     public LayerMask playerLayer;

//     private Transform player;
//     private Rigidbody2D rb;
//     private Animator animator;

//     private bool isDead = false;
//     private bool isChargingDelay = false;
//     private bool isChargeMoving = false;
//     private bool isSummoning = false; // Đang trong quá trình triệu hồi
//     private Coroutine chargeCoroutine;

//     private float lastSummonTime = -999f; // Cho phép summon ngay từ đầu

//     void Start()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         animator = GetComponent<Animator>();
//         player = GameObject.FindGameObjectWithTag("Player")?.transform;

//         maxHealth = requiredPunchesToKill * playerPunchDamage;
//         currentHealth = maxHealth;
//     }

//     void Update()
//     {
//         if (isDead || player == null) return;

//         // Làm sạch danh sách summon (loại bỏ những con đã chết)
//         activeSummons.RemoveAll(summon => summon == null);

//         float distance = Vector2.Distance(transform.position, player.position);

//         // Nếu đang triệu hồi thì dừng lại
//         if (isSummoning)
//         {
//             rb.linearVelocity = Vector2.zero;
//             return;
//         }

//         // Trong tầm triệu hồi và đủ điều kiện
//         if (distance <= summonRange)
//         {
//             StopChargeMovement();

//             if (CanSummon())
//             {
//                 rb.linearVelocity = Vector2.zero;
//                 animator.SetBool("isWalking", false);
//                 StartCoroutine(PerformSummon());
//             }
//             return;
//         }

//         // Trong tầm phát hiện -> di chuyển lại gần
//         if (distance <= detectRange)
//         {
//             if (!isChargingDelay && !isChargeMoving)
//             {
//                 if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
//                 chargeCoroutine = StartCoroutine(ChargeDelayThenMove());
//             }
//         }
//         else
//         {
//             StopChargeMovement();
//             animator.SetBool("isWalking", false);
//             animator.SetBool("isPreparing", false);
//         }

//         HandleFacing();
//     }

//     private bool CanSummon()
//     {
//         // Kiểm tra điều kiện có thể triệu hồi
//         return !isSummoning &&
//                Time.time - lastSummonTime >= summonCooldown &&
//                activeSummons.Count < maxSummons &&
//                summonPrefab != null;
//     }

//     private IEnumerator PerformSummon()
//     {
//         isSummoning = true;
//         rb.linearVelocity = Vector2.zero;

//         // Trigger animation triệu hồi
//         animator.SetTrigger("isSummoning");
//         animator.SetBool("isWalking", false);

//         // Chờ animation chạy
//         yield return new WaitForSeconds(summonAnimationDuration);

//         // Xác định vị trí spawn
//         Vector3 spawnPosition;
//         if (summonSpawnPoint != null)
//         {
//             spawnPosition = summonSpawnPoint.position;
//         }
//         else
//         {
//             // Spawn với offset, flip theo hướng nhìn
//             Vector3 offset = summonOffset;
//             if (transform.localScale.x < 0) // Nếu đang nhìn trái
//             {
//                 offset.x = -offset.x;
//             }
//             spawnPosition = transform.position + offset;
//         }

//         // Spawn Dai
//         GameObject summonedDai = Instantiate(summonPrefab, spawnPosition, Quaternion.identity);

//         // Thêm vào danh sách
//         activeSummons.Add(summonedDai);

//         // Đặt scale phù hợp (cùng hướng với ThanDichBenh)
//         if (summonedDai != null)
//         {
//             Vector3 daiScale = summonedDai.transform.localScale;
//             daiScale.x = Mathf.Abs(daiScale.x) * Mathf.Sign(transform.localScale.x);
//             summonedDai.transform.localScale = daiScale;
//         }

//         lastSummonTime = Time.time;
//         isSummoning = false;

//         Debug.Log($"ThanDichBenh đã triệu hồi Dai! Tổng số: {activeSummons.Count}/{maxSummons}");
//     }

//     private IEnumerator ChargeDelayThenMove()
//     {
//         isChargingDelay = true;
//         rb.linearVelocity = Vector2.zero;

//         animator.SetBool("isPreparing", true);
//         animator.SetBool("isWalking", false);

//         float elapsed = 0f;
//         while (elapsed < chargeDelay)
//         {
//             if (player == null) yield break;
//             float dist = Vector2.Distance(transform.position, player.position);
//             if (dist <= summonRange || dist > detectRange)
//             {
//                 isChargingDelay = false;
//                 animator.SetBool("isPreparing", false);
//                 yield break;
//             }
//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         isChargingDelay = false;
//         isChargeMoving = true;

//         animator.SetBool("isPreparing", false);
//         animator.SetBool("isWalking", true);

//         while (isChargeMoving && player != null)
//         {
//             float dist = Vector2.Distance(transform.position, player.position);

//             if (dist > detectRange || dist <= summonRange)
//                 break;

//             Vector2 dir = (player.position - transform.position).normalized;
//             rb.linearVelocity = dir * chargeSpeed;

//             if (rb.linearVelocity.magnitude < 0.1f)
//             {
//                 animator.SetBool("isWalking", false);
//                 yield return new WaitForSeconds(0.2f);
//                 break;
//             }

//             yield return null;
//         }

//         StopChargeMovement();
//     }

//     private void StopChargeMovement()
//     {
//         isChargeMoving = false;
//         isChargingDelay = false;

//         if (rb != null) rb.linearVelocity = Vector2.zero;

//         animator.SetBool("isWalking", false);
//         animator.SetBool("isPreparing", false);

//         if (chargeCoroutine != null)
//         {
//             StopCoroutine(chargeCoroutine);
//             chargeCoroutine = null;
//         }
//     }

//     private void HandleFacing()
//     {
//         if (player == null) return;
//         float dx = player.position.x - transform.position.x;
//         Vector3 scale = transform.localScale;
//         scale.x = Mathf.Abs(scale.x) * (dx > 0 ? 1 : -1);
//         transform.localScale = scale;
//     }

//     public void TakeDamage(float dmg)
//     {
//         if (isDead) return;

//         StopChargeMovement();
//         isSummoning = false; // Ngừng triệu hồi nếu bị đánh

//         currentHealth -= dmg;
//         Debug.Log(gameObject.name + " bị nhận " + dmg + " sát thương. Máu còn: " + currentHealth);

//         if (currentHealth <= 0)
//         {
//             Die();
//         }
//         else
//         {
//             animator.SetTrigger("isHurt");
//         }
//     }

//     public void Die()
//     {
//         if (isDead) return;
//         isDead = true;

//         StopChargeMovement();
//         rb.linearVelocity = Vector2.zero;
//         rb.simulated = false;

//         animator.SetTrigger("isDeath");

//         GetComponent<Collider2D>().enabled = false;

//         // Có thể tùy chọn: Hủy tất cả Dai đã triệu hồi khi ThanDichBenh chết
//         // foreach (var summon in activeSummons)
//         // {
//         //     if (summon != null) Destroy(summon);
//         // }

//         Destroy(gameObject, 5f);

//         this.enabled = false;
//     }

//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, detectRange);

//         Gizmos.color = Color.magenta;
//         Gizmos.DrawWireSphere(transform.position, summonRange);

//         // Hiển thị vị trí spawn
//         if (summonSpawnPoint != null)
//         {
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawWireSphere(summonSpawnPoint.position, 0.5f);
//             Gizmos.DrawLine(transform.position, summonSpawnPoint.position);
//         }
//         else
//         {
//             // Hiển thị offset spawn
//             Vector3 offset = summonOffset;
//             if (transform.localScale.x < 0) offset.x = -offset.x;
//             Vector3 spawnPos = transform.position + offset;

//             Gizmos.color = Color.cyan;
//             Gizmos.DrawWireSphere(spawnPos, 0.3f);
//             Gizmos.DrawLine(transform.position, spawnPos);
//         }
//     }
// }


public class ThanDichBenh : MonoBehaviour
{
=======
public class ThanDichBenh : MonoBehaviour
{
    // ... (Giữ nguyên các khai báo Components, Stats, References, Health & Combat) ...
>>>>>>> Stashed changes
    [Header("Components")]
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float moveSpeed = 2f;
<<<<<<< Updated upstream
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public bool isDead = false;

    private Transform player;
    private bool isFacingRight = true;
    private float lastAttackTime = 0f;

    [Header("Summon Settings")]
    [Tooltip("Prefab của Dai hoặc enemy khác cần triệu hồi")]
    public GameObject summonPrefab; // Kéo prefab Dai vào đây
=======
    public float chargeSpeed = 6f;
    public float chargeDelay = 1.2f;
    public float detectRange = 8f; // Phạm vi phát hiện/chạy
    public float summonRange = 3f; // Phạm vi ưu tiên triệu hồi/tấn công
    public float stopDistance = 0.4f; // Khoảng cách dừng khi di chuyển thường (không dùng trong charge)

    [Header("References")]
    public BossHealthBar healthBar; // Giả định có

    [Header("Summon Settings")]
    [Tooltip("Prefab của Dai hoặc enemy khác cần triệu hồi")]
    public GameObject summonPrefab;
>>>>>>> Stashed changes

    [Tooltip("Vị trí spawn tương đối so với ThanDichBenh (nếu null sẽ spawn tại chỗ)")]
    public Transform summonSpawnPoint;

    [Tooltip("Offset vị trí spawn nếu không dùng Transform (x, y, z)")]
<<<<<<< Updated upstream
    public Vector3 summonOffset = new Vector3(1f, 0f, 0f); // Spawn cách 1 đơn vị về bên phải

    [Tooltip("Thời gian chờ giữa các lần triệu hồi")]
    public float summonCooldown = 5f; // Tăng cooldown vì summon mạnh hơn attack

    [Tooltip("Số lượng Dai tối đa có thể triệu hồi cùng lúc")]
    public int maxSummons = 2; // Giới hạn 2 Dai
=======
    public Vector3 summonOffset = new Vector3(1f, 0f, 0f);

    [Tooltip("Thời gian chờ giữa các lần triệu hồi")]
    public float summonCooldown = 5f;

    [Tooltip("Số lượng Dai tối đa có thể triệu hồi cùng lúc")]
    public int maxSummons = 2;
>>>>>>> Stashed changes

    [Tooltip("Thời gian animation triệu hồi (để đồng bộ với animation)")]
    public float summonAnimationDuration = 1f;

<<<<<<< Updated upstream
    private List<GameObject> activeSummons = new List<GameObject>(); // Danh sách Dai đang sống
=======
    private List<GameObject> activeSummons = new List<GameObject>();
    private float lastSummonTime = -999f;

    // <--- 🔑 BIẾN MỚI: LƯU VỊ TRÍ START SUMMON
    private Vector3 initialSummonPosition;
    // <--- 🔑 LƯU HƯỚNG NHÌN KHI BẮT ĐẦU SUMMON
    private bool initialFacingRight;


    [Header("Health & Combat")]
    [Tooltip("Sát thương Player gây ra trong 1 cú đấm. (Nên là 2f)")]
    [SerializeField] private float playerPunchDamage = 2f;
    [Tooltip("Số lần Player phải đấm để Enemy chết. (Cần là 5)")]
    [SerializeField] private int requiredPunchesToKill = 5;
    private float maxHealth;
    private float currentHealth;

    [Header("State")]
    public bool isDead = false;
    private Transform player;
    private bool isFacingRight = true;
    private bool isChargingDelay = false;
    private bool isChargeMoving = false;
    private bool isSummoning = false;
    private Coroutine chargeCoroutine;

>>>>>>> Stashed changes

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
<<<<<<< Updated upstream
        
=======

>>>>>>> Stashed changes
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
<<<<<<< Updated upstream
=======

        maxHealth = requiredPunchesToKill * playerPunchDamage;
        currentHealth = maxHealth;
>>>>>>> Stashed changes
    }

    void Update()
    {
        if (isDead || player == null) return;

<<<<<<< Updated upstream
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // Di chuyển
            animator.SetBool("isWalk", true);
            MoveTowardPlayer();
        }
        else
        {
            // Dừng lại
            animator.SetBool("isWalk", false);
            StopMoving();
            
            // Tấn công
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                SummonAttack();
                lastAttackTime = Time.time;
            }
        }
    }

    void MoveTowardPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        if (direction.x > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void StopMoving()
    {
        animator.SetBool("isWalk", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FlipTowardsPlayer();
    }

    public void SummonAttack()
    {
        animator.SetTrigger("TgSummon");
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;
        animator.SetTrigger("TgHurt");
=======
        activeSummons.RemoveAll(summon => summon == null);

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Nếu đang triệu hồi thì dừng mọi thứ (Vẫn cho phép Boss di chuyển nếu logic di chuyển nằm ngoài Update)
        if (isSummoning)
        {
            // Vẫn cần dừng di chuyển dựa trên Rigidbody
            if (rb.linearVelocity != Vector2.zero) rb.linearVelocity = Vector2.zero;
            return;
        }

        // ... (Logic di chuyển, Charge, Detect giữ nguyên) ...

        // 2. Trong tầm triệu hồi (summonRange) và đủ điều kiện
        if (distance <= summonRange)
        {
            StopChargeMovement();

            if (CanSummon())
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalking", false);
                StartCoroutine(PerformSummon()); // BẮT ĐẦU TRIỆU HỒI
                FlipTowardsPlayer();
            }
            return;
        }

        // 3. Trong tầm phát hiện (detectRange) nhưng ngoài tầm triệu hồi
        if (distance <= detectRange)
        {
            if (!isChargingDelay && !isChargeMoving)
            {
                if (chargeCoroutine != null) StopCoroutine(chargeCoroutine);
                chargeCoroutine = StartCoroutine(ChargeDelayThenMove());
            }
        }
        // 4. Ngoài tầm phát hiện (Quay về trạng thái Idle)
        else
        {
            StopChargeMovement();
            animator.SetBool("isWalking", false);
        }

        HandleFacing();
    }

    // ================== LOGIC SUMMON (ĐÃ SỬA) ==================

    private bool CanSummon()
    {
        return !isSummoning &&
                Time.time - lastSummonTime >= summonCooldown &&
                activeSummons.Count < maxSummons &&
                summonPrefab != null;
    }

    private IEnumerator PerformSummon()
    {
        isSummoning = true;
        initialSummonPosition = transform.position;
        initialFacingRight = isFacingRight;

        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("TgSummon");
        animator.SetBool("isWalking", false);

        // Xóa dòng yield return này!
        // yield return new WaitForSeconds(summonAnimationDuration);

        yield break; // Kết thúc Coroutine ngay lập tức
    }

    public void SpawnSummonedEnemy()
    {
        if (summonPrefab == null || activeSummons.Count >= maxSummons) return;

        Vector3 spawnPosition;

        // 🔑 SỬ DỤNG VỊ TRÍ VÀ HƯỚNG LƯU TRỮ (initialSummonPosition, initialFacingRight)
        if (summonSpawnPoint != null)
        {
            // Nếu dùng Transform con, ta vẫn phải giả định Transform con KHÔNG bị dịch chuyển so với gốc
            spawnPosition = summonSpawnPoint.position;
        }
        else
        {
            // Tính toán vị trí spawn dựa trên VỊ TRÍ BAN ĐẦU (initialSummonPosition)
            Vector3 offset = summonOffset;
            if (!initialFacingRight) // Nếu hướng nhìn ban đầu là trái
            {
                offset.x = -offset.x;
            }
            // Vị trí spawn = VỊ TRÍ BAN ĐẦU + Offset
            spawnPosition = initialSummonPosition + offset;
        }

        // Spawn Dai
        GameObject summonedDai = Instantiate(summonPrefab, spawnPosition, Quaternion.identity);

        activeSummons.Add(summonedDai);

        // Đặt scale phù hợp (theo hướng ban đầu của ThanDichBenh)
        if (summonedDai != null)
        {
            Vector3 daiScale = summonedDai.transform.localScale;
            daiScale.x = Mathf.Abs(daiScale.x) * (initialFacingRight ? 1 : -1);
            summonedDai.transform.localScale = daiScale;
        }
    }

    // Đặt hàm này ở cuối animation (thông qua Animation Event) hoặc ngay sau khi SpawnSummonedEnemy()
    public void EndSummonAnimation()
    {
        lastSummonTime = Time.time;
        isSummoning = false;
        // Bỏ qua animator.SetBool("isWalking", false); nếu animation tự chuyển về Idle
    }

    
    // ... (Các logic ChargeDelayThenMove, StopChargeMovement, TakeDamage, Die, HandleFacing, Flip, Gizmos giữ nguyên) ...

    // ================== LOGIC MOVEMENT (CHARGE) GIỮ NGUYÊN ==================
    private IEnumerator ChargeDelayThenMove()
    {
        isChargingDelay = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalking", false);

        float elapsed = 0f;
        while (elapsed < chargeDelay)
        {
            // ... logic giữ nguyên ...
            if (player == null) yield break;
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= summonRange || dist > detectRange)
            {
                isChargingDelay = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        isChargingDelay = false;
        isChargeMoving = true;
        animator.SetBool("isWalking", true);

        while (isChargeMoving && player != null)
        {
            // ... logic giữ nguyên ...
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist > detectRange || dist <= summonRange)
                break;

            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * chargeSpeed;

            HandleFacing();

            yield return null;
        }

        StopChargeMovement();
    }

    private void StopChargeMovement()
    {
        isChargeMoving = false;
        isChargingDelay = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("isWalking", false);

        if (chargeCoroutine != null)
        {
            StopCoroutine(chargeCoroutine);
            chargeCoroutine = null;
        }
    }


    // ================== LOGIC HEALTH & FACING GIỮ NGUYÊN ==================
    public void TakeDamage(float dmg)
    {
        // ... logic giữ nguyên ...
        if (isDead) return;

        StopChargeMovement();
        isSummoning = false;

        currentHealth -= dmg;
        // ...

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("TgHurt");
        }
>>>>>>> Stashed changes
    }

    public void Die()
    {
<<<<<<< Updated upstream
        if (isDead) return;
        
        isDead = true;
        animator.SetTrigger("TgDeath");
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        Destroy(gameObject, 2f);
=======
        // ... logic giữ nguyên ...
        if (isDead) return;
        isDead = true;

        StopChargeMovement();
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        animator.SetTrigger("TgDeath");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 5f);

        this.enabled = false;
    }

    private void HandleFacing()
    {
        if (player == null) return;
        float dx = player.position.x - transform.position.x;

        bool shouldBeFacingRight = dx > 0;

        if (shouldBeFacingRight != isFacingRight)
        {
            Flip();
        }
>>>>>>> Stashed changes
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void FlipTowardsPlayer()
    {
        if (player == null) return;
<<<<<<< Updated upstream

        float directionX = player.position.x - transform.position.x;
        
        if (directionX > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (directionX < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
=======
        HandleFacing();
    }


    // ================== GIZMOS GIỮ NGUYÊN ==================

    private void OnDrawGizmosSelected()
    {
        // ... logic giữ nguyên ...
        // Vòng tròn Phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Vòng tròn Summon
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRange);

        // Hiển thị vị trí spawn (theo hướng nhìn)
        if (summonSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(summonSpawnPoint.position, 0.5f);
            Gizmos.DrawLine(transform.position, summonSpawnPoint.position);
        }
        else
        {
            // Hiển thị offset spawn (giả định đang nhìn phải)
            Vector3 offset = summonOffset;

            // Dùng initialFacingRight nếu đang chạy trong Editor
            bool currentFacing = Application.isPlaying ? initialFacingRight : (transform.localScale.x > 0);

            if (!currentFacing) offset.x = -offset.x;

            Vector3 spawnPos = Application.isPlaying ? initialSummonPosition + offset : transform.position + offset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPos, 0.3f);
            Gizmos.DrawLine(Application.isPlaying ? initialSummonPosition : transform.position, spawnPos);
        }
>>>>>>> Stashed changes
    }
}