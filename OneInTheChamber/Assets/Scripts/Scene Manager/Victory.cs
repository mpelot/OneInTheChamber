using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Victory : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float timer = PlayerPrefs.GetFloat("GTime");
        GetComponent<TextMeshProUGUI>().text = ((int)(timer/60)).ToString() + ":" + ((int)(timer % 60)).ToString("D2") + ":" + ((int)(timer % 1 * 100)).ToString("D2");
        PlayerPrefs.SetFloat("GTime", 0);
    }

}
