using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public Image portraitA; // Ảnh nhân vật A (trái)
    [SerializeField] public Image portraitB; // Ảnh nhân vật B (phải)
    [SerializeField] public TextMeshProUGUI speakerNameText;
    [SerializeField] public TextMeshProUGUI dialogueContentText;
    [SerializeField] public GameObject continueIcon; // Icon "nhấn để tiếp tục"

    [Header("Dialogue Content")]
    [SerializeField] public DialogueLine[] allDialogueLines; // Mảng chứa tất cả câu thoại

    [Header("Settings")]
    [SerializeField] public string mainGameSceneName = "Level_1"; // Cảnh sẽ vào sau khi hết intro
    [SerializeField] public Color activeSpeakerColor = Color.white; // Màu khi đang nói
    [SerializeField] public Color inactiveSpeakerColor = new Color(0.5f, 0.5f, 0.5f); // Màu khi mờ đi

    private int currentLineIndex = 0; // Đếm xem đang ở câu thoại thứ mấy
    private bool isWaitingForInput = false; // Cờ hiệu, tránh nhấn quá nhanh
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Bắt đầu bằng câu thoại đầu tiên
        StartCoroutine(RunDialogue());
    }

    void Update()
    {
        // Nếu đang chờ, và người chơi nhấn nút
        if (isWaitingForInput && Input.anyKeyDown)
        {
            currentLineIndex++; // Chuyển sang câu tiếp theo
            isWaitingForInput = false; // Dừng chờ
            StartCoroutine(RunDialogue()); // Chạy câu thoại tiếp
        }
    }

    private IEnumerator RunDialogue()
    {
        // Tắt icon "tiếp tục" đi
        if (continueIcon != null) continueIcon.SetActive(false);

        // Kiểm tra xem đã hết thoại chưa
        if (currentLineIndex >= allDialogueLines.Length)
        {
            // Hết thoại -> Chuyển cảnh
            SceneManager.LoadScene(mainGameSceneName);
            yield break; // Dừng Coroutine
        }

        // Lấy câu thoại hiện tại
        DialogueLine line = allDialogueLines[currentLineIndex];

        // Cập nhật tên và nội dung
        speakerNameText.text = line.speakerName;
        dialogueContentText.text = line.lineText; // (Bạn có thể làm hiệu ứng gõ chữ ở đây nếu muốn)

        // Cập nhật hình ảnh (Làm mờ/rõ)
        if (line.isSpeakerA)
        {
            portraitA.color = activeSpeakerColor;
            portraitB.color = inactiveSpeakerColor;
        }
        else
        {
            portraitA.color = inactiveSpeakerColor;
            portraitB.color = activeSpeakerColor;
        }

        // (Chờ 1 chút để tránh người chơi spam nút)
        yield return new WaitForSeconds(0.2f);

        // Bật icon "tiếp tục" và chờ người chơi nhấn nút
        if (continueIcon != null) continueIcon.SetActive(true);
        isWaitingForInput = true;
    }
}
