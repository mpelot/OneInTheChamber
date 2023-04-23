using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logo : MonoBehaviour
{
    public float displacement;
    public float cycleTime;

    private RectTransform rectTransform;
    private float yStart;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        yStart = rectTransform.position.y;
    }

    // Update is called once per frame
    private void Update()
    {
        rectTransform.position = new Vector3(rectTransform.position.x, yStart + Mathf.Sin(Time.time * (2 * Mathf.PI) / cycleTime) * displacement, rectTransform.position.z);
    }
}
