using UnityEngine;
using System.Collections; // Cần thiết cho Coroutine nếu bạn dùng

public class ChestController : MonoBehaviour
{
    // --- REFERENCES ---
    [Header("References")]
    private Animator animator;
    [SerializeField] private Collider2D triggerCollider;

    // --- ANIMATION & STATE ---
    [Header("Animation & State")]
    // Tên Trigger bạn đã đặt trong Animator (VD: OpenChest)
    [SerializeField] private string openAnimationTrigger = "OpenChest";
    // Biến để tránh mở rương nhiều lần
    private bool isOpened = false;

    // --- AUDIO ---
    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    private AudioSource audioSource;

    // --- GACHA LOGIC (CẦN ĐIỀN TRONG INSPECTOR) ---
    [Header("Gacha Settings (Total must be 1.0)")]
    [Tooltip("Xác suất (0 đến 1) nhận 999 vàng. (0.000999)")]
    [SerializeField] private float chanceFor999 = 0.000999f;
    [Tooltip("Xác suất (0 đến 1) nhận 100 vàng. (0.099001)")]
    [SerializeField] private float chanceFor100 = 0.099001f;
    [Tooltip("Xác suất (0 đến 1) nhận 20 vàng. (0.30)")]
    [SerializeField] private float chanceFor20 = 0.30f;
    // 10 Vàng sẽ là phần còn lại (0.60f)

    void Start()
    {
        // Lấy Animator Component
        animator = GetComponent<Animator>();

        // Lấy hoặc thêm AudioSource Component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Tự động tìm Collider 2D và thiết lập là Trigger
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        // (Kiểm tra lỗi: rương ban đầu phải đóng)
        isOpened = false;
    }

    // Phương thức này được gọi khi một Collider khác chạm vào Trigger (Is Trigger = true)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đó có phải là Player không
        // Giả sử Player của bạn có Tag là "Player"
        if (other.CompareTag("Player") && !isOpened)
        {
            OpenAndLootChest();
        }
    }

    private void OpenAndLootChest()
    {
        // 1. Đặt trạng thái là đã mở
        isOpened = true;

        // 2. Kích hoạt hoạt hình mở rương và âm thanh
        if (animator != null)
        {
            animator.SetTrigger(openAnimationTrigger);
        }
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        // 3. Trao thưởng ngẫu nhiên cho người chơi
        int goldReceived = GetRandomGoldAmount();
        LootGold(goldReceived);

        // 4. Vô hiệu hóa Collider để Player không thể mở rương lần nữa
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        // 5. Hủy rương sau 1 giây (hoặc bằng độ dài animation)
        Destroy(gameObject, 1.0f);
    }

    // 🔥 HÀM TÍNH TOÁN VÀNG NGẪU NHIÊN DỰA TRÊN XÁC SUẤT
    private int GetRandomGoldAmount()
    {
        // Lấy một số ngẫu nhiên từ 0.0 đến 1.0
        float roll = Random.Range(0f, 1f);
        float cumulativeProbability = 0f;

        // 1. Mức Gacha Siêu Hiếm: 999 Vàng (0.0999%)
        cumulativeProbability += chanceFor999;
        if (roll < cumulativeProbability)
        {
            Debug.Log("🎉 [Gacha Rare] Rương trúng 999 Vàng!");
            return 999;
        }

        // 2. Mức Hiếm: 100 Vàng (9.9001%)
        cumulativeProbability += chanceFor100;
        if (roll < cumulativeProbability)
        {
            Debug.Log("⭐ [Gacha Uncommon] Rương trúng 100 Vàng!");
            return 100;
        }

        // 3. Mức Thường: 20 Vàng (30%)
        cumulativeProbability += chanceFor20;
        if (roll < cumulativeProbability)
        {
            Debug.Log("[Gacha Common] Rương trúng 20 Vàng.");
            return 20;
        }

        // 4. Mức Phổ biến nhất: 10 Vàng (Phần còn lại ~ 60%)
        Debug.Log("[Gacha Basic] Rương trúng 10 Vàng.");
        return 10;
    }

    // Cần sửa lại LootGold để nhận tham số Vàng
    private void LootGold(int amount)
    {
        // 1. Tìm đối tượng Player (Giả định Player có tag "Player")
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            if (playerController != null)
            {
                // 2. Gọi hàm AddGold() của Player
                playerController.AddGold(amount);
            }
            else
            {
                Debug.LogError("Lỗi: Player object thiếu script PlayerController!");
            }
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy Player trong Scene (Kiểm tra Tag 'Player')!");
        }
    }
}