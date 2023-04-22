using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public TargetGuide tg;
    private Renderer r;
    private Animator animator;

    private void Start()
    {
        tg = Instantiate(tg, Camera.main.transform.position, Quaternion.identity);
        tg.Init(gameObject);
        r = GetComponent<Renderer>();
        if (r.isVisible)
            OnBecameInvisible();
        animator = GetComponent<Animator>();
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
        StartCoroutine(ShatterRoutine());
    }

    IEnumerator ShatterRoutine()
    {
        AudioManager.instance.PlaySFX("Target Break");
        animator.Play("Shatter");
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(.5f);
        Time.timeScale = 1f;
        GameObject.Find("LevelManager").GetComponent<LevelManager>().Win();
    }
}
