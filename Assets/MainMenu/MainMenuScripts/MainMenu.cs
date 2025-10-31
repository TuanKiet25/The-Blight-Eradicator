using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button ContinueButton;
    [SerializeField] private Button ExitButton;

    // Khóa (key) để lưu trữ. Phải giống hệt các script khác.
    private const string SaveKey = "LastSavedScene";

    void Start()
    {
        // 1. Kiểm tra xem có save data hay không khi game bắt đầu
        //    (Dùng Start() thay vì Awake() để đảm bảo các đối tượng đã được khởi tạo)
        if (PlayerPrefs.HasKey(SaveKey))
        {
            // Nếu có, cho phép nhấn
            ContinueButton.interactable = true;
        }
        else
        {
            // Nếu không có, vô hiệu hóa (làm mờ) nút
            ContinueButton.interactable = false;
        }
    }

    private void Awake()
    {
        // 2. Gán sự kiện cho nút "Continue"
        ContinueButton.onClick.AddListener(() =>
        {
            // Gọi hàm tải scene khi nhấn
            LoadSavedScene();
        });

        // 3. Gán sự kiện cho nút "Exit"
        ExitButton.onClick.AddListener(() =>
        {
            ExitGame();
        });
    }

    // Hàm này được gọi bởi nút Continue
    private void LoadSavedScene()
    {
        // Tải scene đã được lưu
        string sceneToLoad = PlayerPrefs.GetString(SaveKey);
        SceneManager.LoadScene(sceneToLoad);
    }

    // Hàm này được gọi bởi nút Exit
    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
