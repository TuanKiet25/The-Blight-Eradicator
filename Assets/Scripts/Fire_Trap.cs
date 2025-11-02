using UnityEngine;

public class Fire_Trap : MonoBehaviour
{
    public int damageAmount = 20; // Số máu bị trừ
    private bool canDealDamage = true; // Biến kiểm soát để tránh trừ máu liên tục mỗi frame

    private Animator anim; // Biến để tham chiếu đến Animator

    void Start()
    {
        // Lấy component Animator
        anim = GetComponent<Animator>();
        // Nếu bẫy của bạn có chu kỳ kích hoạt/nghỉ, bạn cần dùng Animation Events
        // để bật/tắt collider hoặc biến canDealDamage.
        
        // Ví dụ, nếu bẫy luôn hoạt động, không cần anim ở đây.
    }

    // Được gọi khi một Collider khác chạm vào Trigger Collider của Trap
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     // Kiểm tra xem đối tượng va chạm có phải là Player không (Dựa vào Tag)
    //     if (other.CompareTag("Player") && canDealDamage)
    //     {
    //         // Lấy script PlayerHealth từ đối tượng Player
    //         PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

    //         // Nếu tìm thấy script PlayerHealth
    //         if (playerHealth != null)
    //         {
    //             playerHealth.TakeDamage(damageAmount);
    //             // Sau khi gây sát thương, bạn có thể thiết lập canDealDamage = false
    //             // và dùng Coroutine hoặc Timer để bật lại sau một thời gian (Cooldown)
    //             // để tránh Player bị trừ máu quá nhanh.
                
    //             // Bắt đầu Coroutine cho cooldown sát thương (ví dụ)
    //             // StartCoroutine(DamageCooldown()); 
    //         }
    //     }
    // }
    
    // Ví dụ về việc sử dụng Animation Event để điều khiển sát thương (Nâng cao)
    // Nếu bạn muốn trap chỉ gây sát thương KHI hoạt ảnh đang ở đỉnh điểm (lửa cháy to nhất).
    
    /* public void EnableDamage()
    {
        canDealDamage = true;
    }

    public void DisableDamage()
    {
        canDealDamage = false;
    }
    */
    // (Bạn sẽ thêm các sự kiện này vào timeline Animation)
    public void EnableDamage()
    {
        canDealDamage = true;
        Debug.Log("Trap: Khả năng gây sát thương ĐƯỢC BẬT.");
    }

    // Hàm được gọi từ Animation Event để TẮT khả năng gây sát thương
    public void DisableDamage()
    {
        canDealDamage = false;
        Debug.Log("Trap: Khả năng gây sát thương ĐƯỢC TẮT.");
    }

    // OnTriggerEnter2D sẽ chỉ gây sát thương khi canDealDamage là TRUE
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canDealDamage)
        {
            PlayerController playerHealth = other.GetComponent<PlayerController>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log("Player bị trừ " + damageAmount + " máu từ Fire Trap.");
                // Tùy chọn: Sau khi gây sát thương, bạn có thể TẮT ngay
                // canDealDamage = false;
                // Nếu bạn muốn mỗi lần hoạt ảnh kích hoạt chỉ gây sát thương một lần duy nhất
                // cho mỗi Player đi qua, hoặc đợi Animation Event DisableDamage.
            }
        }
    }
}