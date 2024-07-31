using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderUnScaleTime : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
           //_UnScaleTimeÇÕShaderGraphÇ≈ê›íËÇµÇΩReferenceÇ…Ç∑ÇÈ
        Shader.SetGlobalFloat("_UnScaleTime", Time.unscaledTime);
        //cosine timeÇÃèÍçá
       // Shader.SetGlobalFloat("_UnScaleTime", Mathf.Cos(Time.unscaledTime));
 
    }
}
