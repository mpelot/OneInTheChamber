using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildDebug : MonoBehaviour
{
    int qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();

    int frameCounter = 0;
    float timeCounter = 0.0f;
    float lastFramerate = 0.0f;

    public bool showLog = false;
    public float refreshTime = 0.5f;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Started up logging.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showLog = !showLog;
        }
        
        if (showLog)
        {
            if (timeCounter < refreshTime)
            {
                timeCounter += Time.deltaTime;
                frameCounter++;
            }
            else
            {
                lastFramerate = (float)frameCounter / timeCounter;
                frameCounter = 0;
                timeCounter = 0.0f;
            }
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        if (showLog)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
            GUILayout.Label("\nFPS:" + lastFramerate.ToString() + "\n" + string.Join("\n", myLogQueue.ToArray()));
            GUILayout.EndArea();
        }
    }
}
