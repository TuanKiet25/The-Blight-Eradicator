using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    // --- Cần Gán trong Inspector ---
    public float speed = 10f;             // Tốc độ bay của đạn
    public float lifetime = 3f;           // Thời gian đạn tồn tại trước khi tự hủy
    public float damage = 5f;             // Sát thương gây ra

    // --- Biến nội bộ ---
    private int direction = 1;            // 1: Bay sang phải, -1: Bay sang trái

    // Hàm Khởi tạo (Sẽ được gọi ngay sau khi đạn được sinh ra)
    public void Initialize(float dmg, bool isFacingRight)
    {
        damage = dmg;
        direction = isFacingRight ? 1 : -1;

        // Đặt hướng quay của Sprite nếu cần (tùy thuộc vào hình ảnh của bạn)
        // transform.localScale = new Vector3(direction, 1, 1);

        // Bắt đầu đếm ngược thời gian tự hủy
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Đạn di chuyển theo hướng (direction) với tốc độ (speed)
        transform.Translate(Vector2.right * speed * Time.deltaTime * direction);
    }

    // Xử lý va chạm
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ va chạm với Player
        if (other.CompareTag("Player"))
        {
            // 1. Gây sát thương
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }

            // 2. Hủy viên đạn sau khi gây sát thương
            Destroy(gameObject);
        }

        // Tùy chọn: Hủy nếu va chạm với tường/địa hình
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}