using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    public Material material;
    [Range(0f,1f)]
    public float ShineLocation = 1f;
    private void Awake()
    {
    }
    void Start()
    {
        material = GetComponent<Image>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (material != null)
        {
            material.SetFloat("_ShineLocation", ShineLocation);
        } 
    }
}
