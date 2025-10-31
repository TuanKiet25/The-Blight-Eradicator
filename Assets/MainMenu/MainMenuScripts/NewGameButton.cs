using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    private const string SaveKey = "LastSavedScene";
    public void LoadSceneOnClick()
    {
        PlayerPrefs.SetString(SaveKey, "1.TheDecayingFringe");
        PlayerPrefs.Save();

        SceneManager.LoadScene("1.TheDecayingFringe");
    }
}
