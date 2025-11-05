using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật thể có phải là "Player" không
        if (other.CompareTag("Player"))
        {
            // Lấy script PlayerController từ vật thể đó
            PlayerController player = other.GetComponent<PlayerController>();

            // Nếu tìm thấy script và nhân vật chưa chết
            if (player != null && !player.isDead)
            {
                // Gọi hàm RespawnFromFall() mà bạn vừa tạo
                player.RespawnFromFall();
            }
        }
    }
}
