using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public float timer;
    public string nextScene;
    private TextMeshProUGUI timerText;
    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponentInChildren<TextMeshProUGUI>();
        timerText.text = ((int)timer).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
        Time.timeScale = 1f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        timerText.text = ((int)timer).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
        if(timer <= 0)
        {
            Lose();
        }
    }

    public void Lose()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void Win()
    {
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}
