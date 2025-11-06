using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour
{
    [SerializeField] private Button BackToMenu;

    void Awake()
    {
        BackToMenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
}
