using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizeFontTMPro : MonoBehaviour
{
    TMPro.TextMeshProUGUI tmpro;


    private void Awake()
    {
        tmpro = GetComponent<TMPro.TextMeshProUGUI>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tmpro != null)
        {
            //tmpro.font = f;
        }
    }

    public void SetFontAsset()
    {

    }
}
