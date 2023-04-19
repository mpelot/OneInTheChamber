using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardButtons : MonoBehaviour
{
    public KeyCode key;
    private SpriteRenderer sprender;
    // Start is called before the first frame update
    void Start()
    {
        sprender = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(key))
        {
            sprender.color = new Color(.5f, .5f, .5f, .9f);
        }
        else
        {
            sprender.color = Color.white;
        }
    }
}
