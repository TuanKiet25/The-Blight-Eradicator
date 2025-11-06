using UnityEngine;
using System.Collections;
public class MapTitle : MonoBehaviour
{
    [SerializeField] private GameObject titleImageObject;

    // 2. Đặt thời gian bạn muốn ảnh hiển thị (ví dụ: 3 giây)
    [SerializeField] private float displayDuration = 3.0f;

    // Hàm Start() được gọi 1 lần duy nhất khi scene bắt đầu
    void Start()
    {
        // Bắt đầu Coroutine để xử lý việc hiển thị
        StartCoroutine(ShowTitleRoutine());
    }

    private IEnumerator ShowTitleRoutine()
    {
        // Bước 1: Bật ảnh lên (làm cho nó hiển thị)
        titleImageObject.SetActive(true);

        // Bước 2: Tạm dừng Coroutine này trong 'displayDuration' giây
        // (Trò chơi vẫn chạy bình thường)
        yield return new WaitForSeconds(displayDuration);

        // Bước 3: Sau khi chờ xong, tắt ảnh đi (làm nó biến mất)
        titleImageObject.SetActive(false);
    }
}
