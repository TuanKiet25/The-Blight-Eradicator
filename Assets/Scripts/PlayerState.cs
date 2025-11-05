using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    [SerializeField] public string nextSceneName;

    // 2. Sử dụng cùng một key "LastSavedScene"
    // Điều này RẤT QUAN TRỌNG để nó ghi đè lên save cũ
    private const string SaveKey = "LastSavedScene";
    private const string GoldKey = "PlayerGold";

    // 3. Hàm này được gọi khi có vật thể khác đi vào trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 4. Chỉ kích hoạt nếu đối tượng va chạm là "Player"
        // (Hãy đảm bảo Player của bạn có tag là "Player")
        if (other.CompareTag("Player"))
        {
            // 5. CẬP NHẬT SaveKey thành tên scene TIẾP THEO
            PlayerPrefs.SetString(SaveKey, nextSceneName);
            PlayerController player = other.GetComponent<PlayerController>();
            PlayerPrefs.Save();

            // 6. Tải scene tiếp theo
            SceneManager.LoadScene(nextSceneName);
        }//helol
    }
}
