using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel; // Panel chứa màn hình Game Over
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    [SerializeField] private NGUTransition nguTransitionEffect; // Kéo script NGUTransition vào đây

    [Header("Game Over Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Tên scene Main Menu

    private string currentSceneName; // Biến để lưu tên scene hiện tại

    private void Awake()
    {
        // Lưu tên scene hiện tại khi Game Over Manager được tạo
        currentSceneName = SceneManager.GetActiveScene().name;

        // Gán listener cho các nút
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }

        // Gán tên scene để NGUTransition tải lại
        if (nguTransitionEffect != null)
        {
            nguTransitionEffect.sceneToReload = currentSceneName;
        }
    }

    // Hàm public để gọi khi người chơi thua
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Tạm dừng thời gian game
            Time.timeScale = 0f;

            // (Tùy chọn) Ẩn các nút của Game Over Panel trong lúc NGU Transition đang chạy
            // để chỉ hiển thị Text Effect. Sau khi hiệu ứng xong, sẽ load scene mới.
            if (retryButton != null) retryButton.gameObject.SetActive(false);
            if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);

            // Đảm bảo NGU Transition ban đầu ẩn, sẽ được hiển thị khi nhấn Retry
            if (nguTransitionEffect != null)
            {
                nguTransitionEffect.gameObject.SetActive(true); // Bật GameObject chứa hiệu ứng
                nguTransitionEffect.enabled = false; // Vô hiệu hóa script NGUTransition để nó không chạy tự động
                                                     // Sẽ được kích hoạt bởi OnRetryButtonClicked
            }
        }
    }

    private void OnRetryButtonClicked()
    {
        // Khi nhấn Retry:
        // 1. Tắt các nút Game Over để tránh click lại
        if (retryButton != null) retryButton.interactable = false;
        if (mainMenuButton != null) mainMenuButton.interactable = false;

        // 2. Kích hoạt hiệu ứng NGU
        if (nguTransitionEffect != null)
        {
            nguTransitionEffect.enabled = true; // Bật script để nó tự StartCoroutine
            nguTransitionEffect.StartNGUTransitionAndReload(); // Gọi hàm bắt đầu hiệu ứng và tải lại scene
        }

        // (Quan trọng) Khôi phục Time.timeScale để game có thể chạy tiếp sau khi load scene mới
        Time.timeScale = 1f;
    }

    private void OnMainMenuButtonClicked()
    {
        // Khi nhấn Main Menu
        Time.timeScale = 1f; // Đảm bảo thời gian game được khôi phục
        SceneManager.LoadScene("MainMenu");
    }
}
