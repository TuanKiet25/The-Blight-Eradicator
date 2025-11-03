using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    private const string SaveKey = "LastSavedScene";
    public void LoadSceneOnClick()
    {
        PlayerPrefs.SetString(SaveKey, "Intro");
        PlayerPrefs.Save();

        SceneManager.LoadScene("Intro");
    }
}
