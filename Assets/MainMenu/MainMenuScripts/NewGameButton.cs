using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    private const string SaveKey = "LastSavedScene";
    public void LoadSceneOnClick()
    {
<<<<<<< HEAD
        PlayerPrefs.SetString(SaveKey, "1.TheDecayingFringe");
        PlayerPrefs.Save();

        SceneManager.LoadScene("1.TheDecayingFringe");
=======
        PlayerPrefs.SetString(SaveKey, "Intro");
        PlayerPrefs.Save();

        SceneManager.LoadScene("Intro");
>>>>>>> ec19981d9ab3e62b25d6ac46b5a551d4c4d487cc
    }
}
