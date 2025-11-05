using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopInteraction : MonoBehaviour
{

    [Header("UI Prompts")]
    [SerializeField] public GameObject interactionPrompt; // Icon "F"

    [Header("Dialogue")]
    [SerializeField] public GameObject dialoguePanel; // Panel chứa hộp thoại
    [SerializeField] public TextMeshProUGUI speakerNameLeftText;  // Tên người nói bên trái
    [SerializeField] public TextMeshProUGUI speakerNameRightText; // Tên người nói bên phải
    [SerializeField] public TextMeshProUGUI dialogueContentText;
    [SerializeField] public DialogueLine[] dialogueLines;
    [SerializeField] public DialogueLine[] closingDialogueLines;// Mảng các câu thoại
    [Header("Dialogue Portraits")] // <-- MỚI: Thêm phần này
    [SerializeField] public Image playerPortrait; // Kéo Image chân dung người chơi vào
    [SerializeField] public Image shopkeeperPortrait; // Kéo Image chân dung chủ shop vào
    [SerializeField] private float fadedAlpha = 0.4f; // Độ mờ khi không nói (0.0 - 1.0)

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
        yield return null;
        foreach (DialogueLine line in dialogueLines)
        {
            speakerNameLeftText.gameObject.SetActive(false);
            speakerNameRightText.gameObject.SetActive(false);
            Color playerColor = playerPortrait.color;
            Color shopkeeperColor = shopkeeperPortrait.color;
            // 2. Kiểm tra xem ai là người nói (dựa vào struct)
            if (line.isSpeakerA)
            {
                // Nếu là người A, hiện tên bên trái
                speakerNameLeftText.text = line.speakerName;
                speakerNameLeftText.gameObject.SetActive(true);
                playerColor.a = 1f;
                shopkeeperColor.a = fadedAlpha;
            }
            else
            {
                // Nếu là người B, hiện tên bên phải
                speakerNameRightText.text = line.speakerName;
                speakerNameRightText.gameObject.SetActive(true);
                playerColor.a = fadedAlpha;
                shopkeeperColor.a = 1f;
            }
            playerPortrait.color = playerColor;
            shopkeeperPortrait.color = shopkeeperColor;
            // 3. Hiển thị nội dung câu thoại
            dialogueContentText.text = line.lineText;
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return new WaitForSeconds(0.1f); // Đợi 1 chút để tránh spam nút
        }

        // --- Mở Shop ---
        dialoguePanel.SetActive(false); // Ẩn hộp thoại
        shopPanel.SetActive(true);      // Hiển thị cửa hàng
    }
    //public void CloseShop()
    //{
    //    shopPanel.SetActive(false);
    //    isInteracting = false;

    //    // Kích hoạt lại người chơi
    //    if (playerController != null)
    //    {
    //        playerController.enabled = true;
    //    }

    //    // Kiểm tra xem người chơi còn trong vùng không, nếu còn thì hiện lại icon "F"
    //    if (playerInRange)
    //    {
    //        interactionPrompt.SetActive(true);
    //    }
    //}
    public void CloseShop()
    {
        shopPanel.SetActive(false); // 1. Ẩn shop đi

        // isInteracting VẪN LÀ TRUE (để người chơi không thể di chuyển)

        // 2. Bắt đầu chạy Coroutine thoại "Đóng"
        StartCoroutine(ClosingDialogueSequence());
    }
    private IEnumerator ClosingDialogueSequence()
    {
        // 1. Hiện UI đối thoại
        dialoguePanel.SetActive(true);

        // Chờ 1 frame để "tiêu thụ" cú click chuột vào nút Close
        // (Tránh bị skip câu thoại đầu tiên)
        yield return null;

        // 2. Chạy qua các câu thoại đóng
        foreach (DialogueLine line in closingDialogueLines)
        {
            // (Copy y hệt logic hiển thị tên/thoại từ hàm StartInteractionSequence)
            speakerNameLeftText.gameObject.SetActive(false);
            speakerNameRightText.gameObject.SetActive(false);
            Color playerColor = playerPortrait.color;
            Color shopkeeperColor = shopkeeperPortrait.color;

            if (line.isSpeakerA)
            {
                speakerNameLeftText.text = line.speakerName;
                speakerNameLeftText.gameObject.SetActive(true);
                playerColor.a = 1f;
                shopkeeperColor.a = fadedAlpha;
            }
            else
            {
                speakerNameRightText.text = line.speakerName;
                speakerNameRightText.gameObject.SetActive(true);
                playerColor.a = fadedAlpha;
                shopkeeperColor.a = 1f;
            }
            playerPortrait.color = playerColor;
            shopkeeperPortrait.color = shopkeeperColor;
            dialogueContentText.text = line.lineText;

            // Chờ người chơi nhấn nút
            yield return new WaitUntil(() => Input.anyKeyDown);
            yield return new WaitForSeconds(0.1f);
        }

        // 3. Kết thúc
        dialoguePanel.SetActive(false); // Ẩn hộp thoại
        isInteracting = false; // BÂY GIỜ mới kết thúc tương tác

        // 4. Kích hoạt lại người chơi
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // 5. Kiểm tra xem người chơi còn trong vùng không
        if (playerInRange)
        {
            interactionPrompt.SetActive(true);
        }
    }
}
