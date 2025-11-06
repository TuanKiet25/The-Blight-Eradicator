using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NGUTransition : MonoBehaviour
{
    [SerializeField] private TMP_Text nText;
    [SerializeField] private TMP_Text gText;
    [SerializeField] private TMP_Text uText;
    [SerializeField] private TMP_Text fullText; // Text cho "Never Give Up"

    [Header("Animation Settings")]
    [SerializeField] private float initialDelay = 0.5f; // Chờ trước khi bắt đầu
    [SerializeField] private float moveDuration = 1f; // Thời gian chữ NGU tách ra
    [SerializeField] private float moveDistance = 150f; // Khoảng cách NGU tách ra (pixels)
    [SerializeField] private float fadeInDuration = 1.0f; // Thời gian chữ Never Give Up hiện lên
    [SerializeField] private float finalDisplayDuration = 2.0f; // Thời gian hiển thị cuối cùng
    [SerializeField] private float characterTypeDelay = 0.05f; // Tốc độ hiện chữ Never Give Up

    // Tên scene để load lại (gán trong Inspector của GameObject Game Over)
    [SerializeField] public string sceneToReload;

    private Vector2 nInitialPos;
    private Vector2 gInitialPos;
    private Vector2 uInitialPos;

    // Biến để theo dõi Coroutine đang chạy
    private Coroutine currentTransitionCoroutine;

    private void Awake()
    {
        // Lưu vị trí ban đầu của N, G, U
        if (nText != null) nInitialPos = nText.rectTransform.anchoredPosition;
        if (gText != null) gInitialPos = gText.rectTransform.anchoredPosition;
        if (uText != null) uInitialPos = uText.rectTransform.anchoredPosition;

        // Đặt màu ban đầu của fullText là trong suốt (alpha = 0)
        if (fullText != null) fullText.color = new Color(fullText.color.r, fullText.color.g, fullText.color.b, 0f);

        // Ban đầu, ẩn tất cả các Text khi khởi tạo
        nText.gameObject.SetActive(false);
        gText.gameObject.SetActive(false);
        uText.gameObject.SetActive(false);
        fullText.gameObject.SetActive(false);
    }

    // Hàm public để Game Over Manager gọi khi nhấn Retry
    public void StartNGUTransitionAndReload()
    {
        // Nếu có Coroutine cũ đang chạy, dừng nó lại để tránh xung đột
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
        }
        currentTransitionCoroutine = StartCoroutine(DoNGUTransition());
    }

    private IEnumerator DoNGUTransition()
    {
        // Reset trạng thái ban đầu của các Text
        nText.rectTransform.anchoredPosition = nInitialPos;
        gText.rectTransform.anchoredPosition = gInitialPos;
        uText.rectTransform.anchoredPosition = uInitialPos;
        fullText.text = "";
        fullText.color = new Color(fullText.color.r, fullText.color.g, fullText.color.b, 0f);

        // Hiển thị chữ NGU
        nText.gameObject.SetActive(true);
        gText.gameObject.SetActive(true);
        uText.gameObject.SetActive(true);
        fullText.gameObject.SetActive(true); // Cần thiết để fade in

        yield return new WaitForSeconds(initialDelay);

        // --- Hiệu ứng tách chữ NGU ---
        float timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration; // Giá trị từ 0 đến 1

            // N di chuyển sang trái
            //nText.rectTransform.anchoredPosition = Vector2.Lerp(nInitialPos, nInitialPos + Vector2.left * moveDistance, t);
            //// U di chuyển sang phải
            //uText.rectTransform.anchoredPosition = Vector2.Lerp(uInitialPos, uInitialPos + Vector2.right * moveDistance, t);
            //// G có thể dịch xuống một chút hoặc giữ nguyên, tùy bạn
            //gText.rectTransform.anchoredPosition = Vector2.Lerp(gInitialPos, gInitialPos + Vector2.down * moveDistance * 0.5f, t);

            // Chữ "Never Give Up" hiện dần lên (Fade In)
            fullText.color = new Color(fullText.color.r, fullText.color.g, fullText.color.b, t);

            yield return null; // Chờ frame tiếp theo
        }

        // --- Hiệu ứng đánh máy chữ "Never Give Up" ---
        // Đặt alpha của fullText về 1 sau khi fade in xong
        fullText.color = new Color(fullText.color.r, fullText.color.g, fullText.color.b, 1f);

        // Sau khi tách ra, ẩn NGU để chỉ còn "Never Give Up"
        nText.gameObject.SetActive(false);
        gText.gameObject.SetActive(false);
        uText.gameObject.SetActive(false);

        string targetText = "Never Give Up";
        fullText.text = "";

        for (int i = 0; i < targetText.Length; i++)
        {
            fullText.text += targetText[i];
            yield return new WaitForSeconds(characterTypeDelay);
        }

        // --- Giữ hiển thị một thời gian ---
        yield return new WaitForSeconds(finalDisplayDuration);

        // --- Tải lại Scene ---
        if (!string.IsNullOrEmpty(sceneToReload))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneToReload);
        }
        else
        {
            Debug.LogError("Chưa gán 'Scene To Reload' trong Inspector của NGUTransition!");
        }
    }
}
