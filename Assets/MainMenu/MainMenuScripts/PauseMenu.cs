using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button PauseButton;
    [SerializeField] private Button ResumeButton;
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button QuitButton;
    [SerializeField] private GameObject PauseMenuPanel;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Awake()
    {
        PauseButton.onClick.AddListener(() =>
        {
            if (PauseMenuPanel != null)
            {
                Time.timeScale = 0f;
                PauseMenuPanel.SetActive(true);
            }
        });
        ResumeButton.onClick.AddListener(() =>
        {
            if (PauseMenuPanel != null)
            {
                Time.timeScale = 1f;
                PauseMenuPanel.SetActive(false);
            }
        });
        QuitButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
}
