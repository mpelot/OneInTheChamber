using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Start is called before the first frame update
    
    public void StartButton()
    {
        SceneManager.LoadScene("Tutorial 1", LoadSceneMode.Single);

        /*if (PlayerPrefs.GetString("CurrentScene") == "" || PlayerPrefs.GetString("CurrentScene") == "Main Menu")
        {
            SceneManager.LoadScene("Tutorial 1", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("CurrentScene"), LoadSceneMode.Single);
        }*/
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.SetString("CurrentScene", "");
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }
}
