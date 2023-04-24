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
    private bool timerEnabled;
    // Start is called before the first frame update
    void Start()
    {
        timerEnabled = timer > 0;
        timerText = GetComponentInChildren<TextMeshProUGUI>();
        if (timerEnabled)
        {
            timerText.text = ((int)timer).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
        }
        else
        {
            timerText.enabled = false;
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (timerEnabled)
        {
            timer -= Time.deltaTime;
            timerText.text = ((int)timer).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
            if (timer <= 0)
            {
                Lose();
            }
        }
    }

    public void Lose()
    {
		StartCoroutine(DeathRoutine());
    }
	
	IEnumerator DeathRoutine()
    {
        AudioManager.instance.PlaySFX("Target Break");
        StartCoroutine(AudioManager.instance.SweepLPF(6000f, 10f, 0.15f));
        GameObject.Find("Player").GetComponent<Animator>().Play("Death");
        yield return new WaitForSecondsRealtime(.583f * 2);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void Win()
    {
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}
