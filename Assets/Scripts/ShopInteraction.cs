using System.Collections;
using TMPro;
using UnityEngine;

public class ShopInteraction : MonoBehaviour
{

    [Header("UI Prompts")]
    [SerializeField] public GameObject interactionPrompt; // Icon "F"

    [Header("Dialogue")]
    [SerializeField] public GameObject dialoguePanel; // Panel chứa hộp thoại
    [SerializeField] public TextMeshProUGUI speakerNameLeftText;  // Tên người nói bên trái
    [SerializeField] public TextMeshProUGUI speakerNameRightText; // Tên người nói bên phải
    [SerializeField] public TextMeshProUGUI dialogueContentText;
    [SerializeField] public DialogueLine[] dialogueLines; // Mảng các câu thoại

    [Header("Shop")]
    [SerializeField] public GameObject shopPanel; // Panel cửa hàng

    private bool playerInRange = false;
    private bool isInteracting = false; // Cờ hiệu để biết shop/hộp thoại đang mở
    private PlayerController playerController; // Tham chiếu đến script Player// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && !isInteracting && Input.GetKeyDown(KeyCode.F))
        {
            // Bắt đầu chuỗi: Đối thoại -> Mở Shop
            StartCoroutine(StartInteractionSequence());
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isInteracting)
        {
            playerInRange = true;
            interactionPrompt.SetActive(true); // Hiển thị icon "F"

            // Lấy script PlayerController để tạm dừng
            playerController = other.GetComponent<PlayerController>();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            interactionPrompt.SetActive(false); // Ẩn icon "F"
            playerController = null;
        }
    }
    private IEnumerator StartInteractionSequence()
    {
        isInteracting = true;
        interactionPrompt.SetActive(false); // Ẩn icon "F"

        // Tạm dừng người chơi
        if (playerController != null)
        {
            playerController.StopAllMovement();
            playerController.enabled = false; // Tắt script PlayerController
        }

        // --- Bắt đầu Đối Thoại ---
        dialoguePanel.SetActive(true);

        foreach (DialogueLine line in dialogueLines)
        {
            speakerNameLeftText.gameObject.SetActive(false);
            speakerNameRightText.gameObject.SetActive(false);

            // 2. Kiểm tra xem ai là người nói (dựa vào struct)
            if (line.isSpeakerA)
            {
                // Nếu là người A, hiện tên bên trái
                speakerNameLeftText.text = line.speakerName;
                speakerNameLeftText.gameObject.SetActive(true);
            }
            else
            {
                // Nếu là người B, hiện tên bên phải
                speakerNameRightText.text = line.speakerName;
                speakerNameRightText.gameObject.SetActive(true);
            }

            // 3. Hiển thị nội dung câu thoại
            dialogueContentText.text = line.lineText;
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return new WaitForSeconds(0.1f); // Đợi 1 chút để tránh spam nút
        }

        // --- Mở Shop ---
        dialoguePanel.SetActive(false); // Ẩn hộp thoại
        shopPanel.SetActive(true);      // Hiển thị cửa hàng
    }
    public void CloseShop()
    {
        shopPanel.SetActive(false);
        isInteracting = false;

        // Kích hoạt lại người chơi
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Kiểm tra xem người chơi còn trong vùng không, nếu còn thì hiện lại icon "F"
        if (playerInRange)
        {
            interactionPrompt.SetActive(true);
        }
    }
}
