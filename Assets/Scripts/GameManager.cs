using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // GIỮ GAME MANAGER KHI CHUYỂN SCENE
        }
        else
        {
            Destroy(gameObject); // Hủy bản sao nếu đã có một cái rồi
        }
    }
    // --- KẾT THÚC SINGLETON ---

    // --- DỮ LIỆU CẦN DUY TRÌ QUA CÁC SCENE (Đã chuyển từ PlayerController) ---
    [Header("Player Global Stats")]
    public int playerLives = 4; // Số mạng ban đầu
    public int playerGold = 0; // Số vàng ban đầu

    // Các biến trạng thái game khác
    [Header("Game State")]
    public bool isGamePaused = false;
    public bool isGameOver = false;

    // --- HÀM QUẢN LÝ GAME ---

    // Hàm chuyển Scene
    public void LoadScene(string sceneName)
    {
        Debug.Log("GameManager: Đang tải scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    // Hàm để tạm dừng/tiếp tục game
    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f; // Dừng/chạy thời gian game
        Debug.Log("GameManager: Game tạm dừng: " + isGamePaused);
        // Ở đây bạn có thể gọi một sự kiện để UI Manager hiển thị PausePanel
        // if (UIManager.Instance != null) UIManager.Instance.ShowPausePanel(isGamePaused);
    }

    // --- HÀM QUẢN LÝ DỮ LIỆU NGƯỜI CHƠI (được gọi từ PlayerController hoặc các script khác) ---

    public void AddGold(int amount)
    {
        playerGold += amount;
        Debug.Log("GameManager: Thêm " + amount + " vàng. Tổng: " + playerGold);
        // Có thể gọi UpdateGoldUI nếu bạn muốn GameManager quản lý trực tiếp Gold Text.
        // NHƯNG thường thì PlayerController (hoặc UI Manager) sẽ tự cập nhật UI của nó.
    }

    public bool TrySpendGold(int amountToSpend)
    {
        if (playerGold >= amountToSpend)
        {
            playerGold -= amountToSpend;
            Debug.Log("GameManager: Đã tiêu " + amountToSpend + " vàng. Còn lại: " + playerGold);
            return true;
        }
        Debug.Log("GameManager: Không đủ vàng để tiêu " + amountToSpend + ". Hiện có: " + playerGold);
        return false;
    }

    public void PlayerLostLife()
    {
        if (isGameOver) return; // Không làm gì nếu game đã kết thúc

        playerLives--;
        

        if (playerLives <= 0)
        {
            Debug.Log("GameManager: Game Over!");
            isGameOver = true;
            // Ở đây bạn có thể Load Game Over Scene hoặc hiển thị Game Over UI
            // LoadScene("GameOverScene"); 
        }
    }
}
