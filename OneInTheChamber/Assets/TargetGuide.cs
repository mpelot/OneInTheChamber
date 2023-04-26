using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGuide : MonoBehaviour
{
    private GameObject target;

    public void Init(GameObject target)
    {
        this.target = target;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Camera.main.transform.position;
        transform.position = new Vector3(pos.x, pos.y, 0f);
        Vector2 dir = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
    }
}
