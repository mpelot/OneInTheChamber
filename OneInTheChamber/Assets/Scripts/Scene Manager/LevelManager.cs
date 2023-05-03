using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public float timer;
    private float global;
    public string nextScene;
    public GameObject startingWhite;
    private TextMeshProUGUI timerText;
    private bool timerEnabled;
    private bool dying = false;
    // Start is called before the first frame update

    private void Awake()
    {
        startingWhite.SetActive(true);
        global = PlayerPrefs.GetFloat("GTime");
    }

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
            if(!dying && timer > 0)
            {
                timer -= Time.deltaTime;
            }
            timerText.text = ((int)timer).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
            if (timer <= 0)
            {
                timer = 0;
                Lose();
            }
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerPrefs.SetFloat("GTime", global);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);
        }
        global += Time.unscaledDeltaTime;
    }

    public void Lose()
    {
        if(!dying)
        {
            dying = true;
            StartCoroutine(DeathRoutine());
        }
    }
	
	IEnumerator DeathRoutine()
    {
        GameObject.Find("Player").GetComponent<PlayerMovement>().enabled = false;
        GameObject.Find("Player").GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GameObject.Find("Player").GetComponent<Rigidbody2D>().gravityScale = 0;
        AudioManager.instance.PlaySFX("Target Break");
        StartCoroutine(AudioManager.instance.SweepLPF(6000f, 10f, 0.15f));
        GameObject.Find("Player").GetComponent<Animator>().Play("Death");
        yield return new WaitForSecondsRealtime(.583f);
        PlayerPrefs.SetFloat("GTime", global);
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void Win()
    {
        PlayerPrefs.SetString("CurrentScene", nextScene);
        PlayerPrefs.SetFloat("GTime", global);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }

}
