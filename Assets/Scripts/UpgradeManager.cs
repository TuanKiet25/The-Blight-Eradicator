using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private PlayerController playerController;
    [Header("Upgrade Values")] // <-- MỚI: Chỉ số cộng thêm mỗi lần nâng cấp
    [SerializeField] private float healthIncreaseAmount = 20f;
    [SerializeField] private float energyIncreaseAmount = 10f;
    [SerializeField] private float damageIncreaseAmount = 5f;

    [Header("Player Stats (Tạm thời)")]
    private int strengthLevel = 0;
    private int healthLevel = 0;
    private int energyLevel = 0;
    private int maxLevel = 5;

    [Header("Sprites Bậc (Bậc trắng)")]
    [SerializeField] public Sprite tierLockedSprite; // Hình bậc bị khóa (màu xám)
    [SerializeField] public Sprite tierUnlockedSprite; // Hình bậc đã mở (màu trắng)

    // --- SỨC MẠNH (STRENGTH) ---
    [Header("Strength UI")]
    [SerializeField] public Image strengthIcon; // Kéo Icon Cánh tay vào đây
    [SerializeField] public Sprite[] strengthSprites; // Mảng 5 sprite màu (kéo vào)
    [SerializeField] public Image[] strengthTiers; // Mảng 5 bậc trắng (kéo vào)
    [SerializeField] public Button strengthButton; // Kéo nút Upgrade của Sức mạnh

    // --- MÁU (HEALTH) ---
    [Header("Health UI")]
    [SerializeField] public Image healthIcon; // Kéo Icon Trái tim
    [SerializeField] public Sprite[] healthSprites;
    [SerializeField] public Image[] healthTiers;
    [SerializeField] public Button healthButton;

    // --- NĂNG LƯỢNG (ENERGY) ---
    [Header("Energy UI")]
    [SerializeField] public Image energyIcon; // Kéo Icon Pin
    [SerializeField] public Sprite[] energySprites;
    [SerializeField] public Image[] energyTiers;
    [SerializeField] public Button energyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Upgrade Costs")]
    [SerializeField] private int[] strengthCosts; // Mảng 5 giá (vd: 50, 100, 150, 200, 250)
    [SerializeField] private int[] healthCosts;
    [SerializeField] private int[] energyCosts;

    [Header("Cost UI Texts")]
    [SerializeField] private TextMeshProUGUI strengthCostText;
    [SerializeField] private TextMeshProUGUI healthCostText;
    [SerializeField] private TextMeshProUGUI energyCostText;
    void Start()
    {
        UpdateAllUI();
    }

    public void OnUpgradeStrength()
    {
        // Kiểm tra xem có đủ tiền không (ví dụ)
        // if (PlayerData.money < 50) return; 
        int cost = strengthCosts[strengthLevel];
        if (playerController.TrySpendGold(cost))
        {
            if (strengthLevel < maxLevel)
            {
                strengthLevel++;
                playerController.UpgradeDamage(damageIncreaseAmount);
                UpdateAllUI();
            }
        }
        else
        {
            strengthCostText.text = "Nâng cấp thất bại, không đủ tiền!";
            Debug.Log("Nâng cấp thất bại, không đủ tiền!");
        }
    }

    public void OnUpgradeHealth()
    {
        int cost = strengthCosts[healthLevel];
        if (playerController.TrySpendGold(cost))
        {
            if (healthLevel < maxLevel)
            {
                healthLevel++;
                playerController.UpgradeMaxHealth(healthIncreaseAmount);
                UpdateAllUI();
                // ví dụ: playerController.IncreaseMaxHealth(20);
            }
        }
        else
        {
            healthCostText.text = "Nâng cấp thất bại, không đủ tiền!";
            Debug.Log("Nâng cấp thất bại, không đủ tiền!");
        }
    }

    public void OnUpgradeEnergy()
    {
        int cost = strengthCosts[energyLevel];
        if (playerController.TrySpendGold(cost))
        {
            if (energyLevel < maxLevel)
            {
                energyLevel++;
                playerController.UpgradeMaxEnergy(energyIncreaseAmount);
                UpdateAllUI();
                // ví dụ: playerController.IncreaseMaxEnergy(10);
            }
        }
        else
        {
            energyCostText.text = "Nâng cấp thất bại, không đủ tiền!";
            Debug.Log("Nâng cấp thất bại, không đủ tiền!");
        }
    }

    // --- HÀM CẬP NHẬT GIAO DIỆN ---

    private void UpdateAllUI()
    {
        // Cập nhật UI cho Sức mạnh
        UpdateSingleUI(strengthLevel, strengthIcon, strengthSprites, strengthTiers, strengthButton, strengthCosts, strengthCostText);

        // Cập nhật UI cho Máu
        UpdateSingleUI(healthLevel, healthIcon, healthSprites, healthTiers, healthButton, healthCosts, healthCostText);

        // Cập nhật UI cho Năng lượng
        UpdateSingleUI(energyLevel, energyIcon, energySprites, energyTiers, energyButton, energyCosts, energyCostText);
    }

    // Hàm chung để cập nhật một mục
    private void UpdateSingleUI(int currentLevel, Image icon, Sprite[] iconSprites, Image[] tiers, Button button, int[] costs, TextMeshProUGUI costText)
    {
        // 1. Đổi màu Icon chính
        // (Đảm bảo mảng iconSprites có đủ 6 phần tử [0-5])
        icon.sprite = iconSprites[currentLevel];

        // 2. Cập nhật các bậc trắng
        for (int i = 0; i < tiers.Length; i++)
        {
            if (i < currentLevel)
            {
                tiers[i].sprite = tierUnlockedSprite; // Bậc đã mở
            }
            else
            {
                tiers[i].sprite = tierLockedSprite; // Bậc bị khóa
            }
        }

        // 3. Nếu đã max level, tắt nút Upgrade đi
        if (currentLevel >= maxLevel)
        {
            button.interactable = false;
            costText.text = "MAX";
        }
        else
        {
            button.interactable = true;
            // Hiển thị giá của cấp tiếp theo
            costText.text = "Nâng cấp tốn: " + costs[currentLevel].ToString();
        }
    }

}
