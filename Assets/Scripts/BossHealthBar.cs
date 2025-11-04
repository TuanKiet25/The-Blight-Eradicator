using UnityEngine;
using UnityEngine.UI; // Rất quan trọng để sử dụng Slider

public class BossHealthBar : MonoBehaviour
{
    // ⭐ Kéo thả thành phần Slider (UI) vào đây trong Inspector ⭐
    public Slider slider;

    // Hàm được gọi khi khởi tạo để đặt MaxValue
    public void SetMaxHealth(float health)
    {
        if (slider != null)
        {
            slider.maxValue = health;
            slider.value = health;
        }

        // Tùy chọn: Đảm bảo thanh máu ẩn nếu boss đầy máu khi bắt đầu
        // gameObject.SetActive(false); 
    }

    // Hàm được gọi từ BossController mỗi khi máu thay đổi
    public void SetHealth(float health)
    {
        if (slider != null)
        {
            slider.value = health;

            // Tùy chọn: Hiển thị thanh máu ngay khi máu bắt đầu giảm
            // if (!gameObject.activeSelf) 
            // {
            //     gameObject.SetActive(true);
            // }
        }
    }
}