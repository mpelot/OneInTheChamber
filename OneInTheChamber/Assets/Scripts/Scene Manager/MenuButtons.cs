using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Start is called before the first frame update
    
    public void StartButton()
    {
        if(PlayerPrefs.GetString("CurrentScene") == "" || PlayerPrefs.GetString("CurrentScene") == "Main Menu" || PlayerPrefs.GetString("CurrentScene") == "Victory Screen")
        {
            PlayerPrefs.SetFloat("GTime", 0);
            SceneManager.LoadScene("Tutorial 1", LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(PlayerPrefs.GetString("CurrentScene"), LoadSceneMode.Single);
        }
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.SetString("CurrentScene", "");
            PlayerPrefs.SetFloat("GTime", 0);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
    }
}
