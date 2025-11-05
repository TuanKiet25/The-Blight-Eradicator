using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossDialogue : MonoBehaviour
{
    // Hằng số SaveKey (phải giống hệt key ở script PlayerState)
    private const string SaveKey = "LastSavedScene";

    // --- ĐÃ LOẠI BỎ GameManager ---

    [Header("DIALOGUE UI")]
    [SerializeField] public GameObject dialoguePanel;
    [SerializeField] public TextMeshProUGUI speakerNameLeftText;
    [SerializeField] public TextMeshProUGUI speakerNameRightText;
    [SerializeField] public TextMeshProUGUI dialogueContentText;
    [SerializeField] public DialogueLine[] dialogueLines;

    [Header("CHOICE UI")]
    [SerializeField] public GameObject choicePanel;
    [SerializeField] public Button option1Button;
    [SerializeField] public Button option2Button;
    [SerializeField] public TextMeshProUGUI option1Text;
    [SerializeField] public TextMeshProUGUI option2Text;

    [Header("CHOICE OPTIONS")]
    public string option1Label = "Chiến đấu, ta không theo ngươi!";
    public string option2Label = "Đừng hòng ta làm theo lời người!!.";

    [Tooltip("Tên của Scene 'Bad Ending' để tải và lưu.")]
    [SerializeField] private string badEndingSceneName = "BadEnding";

    private bool playerInRange = false;
    private bool isInteracting = false;
    private PlayerController playerController;

    [Header("ONE-TIME INTERACTION")]
    [Tooltip("Nếu True, đối thoại sẽ không được kích hoạt lại.")]
    [SerializeField] private bool hasInteracted = false;

    void Start()
    {
        // Ẩn tất cả UI khi bắt đầu
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);

        // Gán sự kiện cho các nút
        option1Button.onClick.RemoveAllListeners();
        option2Button.onClick.RemoveAllListeners();
        option1Button.onClick.AddListener(SelectOption1_FightBoss);
        option2Button.onClick.AddListener(SelectOption2_BadEnding);

        // --- ĐÃ LOẠI BỎ logic kiểm tra GameManager ---
    }

    void Update()
    {
        // Không cần logic ở đây
    }

    // --- Vùng Trigger (Va chạm) ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInteracting && !hasInteracted)
        {
            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();
            StartCoroutine(StartInteractionSequence());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerController = null;
        }
    }

    // --- Chuỗi Đối Thoại Chính ---
    private IEnumerator StartInteractionSequence()
    {
        isInteracting = true;

        // Tạm dừng người chơi
        if (playerController != null)
        {
            playerController.StopAllMovement();
            playerController.enabled = false;
        }

        // Bắt đầu Đối Thoại
        dialoguePanel.SetActive(true);

        foreach (DialogueLine line in dialogueLines)
        {
            speakerNameLeftText.gameObject.SetActive(false);
            speakerNameRightText.gameObject.SetActive(false);

            if (line.isSpeakerA)
            {
                speakerNameLeftText.text = line.speakerName;
                speakerNameLeftText.gameObject.SetActive(true);
            }
            else
            {
                speakerNameRightText.text = line.speakerName;
                speakerNameRightText.gameObject.SetActive(true);
            }
            dialogueContentText.text = line.lineText;

            // Đợi người dùng nhấn phím bất kỳ
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return new WaitForSeconds(0.1f);
        }

        // --- Hiển thị Lựa Chọn ---
        dialoguePanel.SetActive(false);
        ShowChoices();
    }

    // --- Xử lý Lựa Chọn ---
    private void ShowChoices()
    {
        choicePanel.SetActive(true);
        option1Text.text = option1Label;
        option2Text.text = option2Label;
    }

    // Lựa chọn 1: Chiến đấu (Player tiếp tục di chuyển)
    private void SelectOption1_FightBoss()
    {
        Debug.Log("Chọn Option 1: Tiếp tục di chuyển và đối mặt Boss!");
        EndInteraction();
    }

    // Lựa chọn 2: Bad Ending (Chuyển Scene bằng SaveKey)
    private void SelectOption2_BadEnding()
    {
        Debug.Log("Chọn Option 2: Bad Ending!");
        EndInteraction(); // Ẩn UI lựa chọn

        // --- SỬ DỤNG KỸ THUẬT SAVEKEY ---

        // 1. Cập nhật SaveKey thành tên scene "Bad Ending"
        PlayerPrefs.SetString(SaveKey, "BadEnding");
        PlayerPrefs.Save(); // Lưu thay đổi

        // 2. Tải scene "Bad Ending"
        SceneManager.LoadScene("BadEnding");
    }


    // --- Kết thúc tương tác và phục hồi Player ---
    private void EndInteraction()
    {
        choicePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        isInteracting = false;
        //hasInteracted = true;

        // Kích hoạt lại người chơi
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}