using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Cường độ di chuyển (giá trị nhỏ như 0.05 là tốt)
    [SerializeField] private float moveStrength = 0.05f;

    // Tốc độ di chuyển mượt mà
    [SerializeField] private float smoothSpeed = 5f;

    private RectTransform uiContainer;
    private Vector2 initialPosition;
    private Vector2 screenCenter;

    void Start()
    {
        // Lấy chính RectTransform của "UI_Container" này
        uiContainer = GetComponent<RectTransform>();

        // Lưu lại vị trí ban đầu
        initialPosition = uiContainer.anchoredPosition;

        // Tìm tâm màn hình
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        // 1. Lấy vị trí chuột hiện tại
        Vector2 mousePos = Input.mousePosition;

        // 2. Tính toán khoảng cách từ chuột đến tâm màn hình
        // Chúng ta di chuyển "ngược" nên dùng dấu trừ (-)
        float moveX = -(mousePos.x - screenCenter.x);
        float moveY = -(mousePos.y - screenCenter.y);

        // 3. Tính toán vị trí mục tiêu
        Vector2 targetPosition = initialPosition + new Vector2(moveX * moveStrength, moveY * moveStrength);

        // 4. Di chuyển mượt mà (Lerp) đến vị trí mục tiêu
        uiContainer.anchoredPosition = Vector2.Lerp(
            uiContainer.anchoredPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}
