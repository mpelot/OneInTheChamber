using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public TargetGuide tg;
    private Renderer r;

    private void Start()
    {
        tg = Instantiate(tg, Camera.main.transform.position, Quaternion.identity);
        tg.Init(gameObject);
        r = GetComponent<Renderer>();
        if (r.isVisible)
            OnBecameInvisible();
    }

    private void OnBecameVisible()
    {
        if (tg != null)
            tg.GetComponentInChildren<SpriteRenderer>().enabled = false;
    }

    private void OnBecameInvisible()
    {
        if (tg != null)
            tg.GetComponentInChildren<SpriteRenderer>().enabled = true;
    }

    public void Shatter()
    {
        // Call this when the beam raycast detects the target
    }
}
