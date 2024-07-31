using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Set());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Set()
    {
        while(Camera.main == null)
        {
            yield return 0;
        }
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
}
