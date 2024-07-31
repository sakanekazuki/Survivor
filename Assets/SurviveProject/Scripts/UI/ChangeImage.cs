using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeImage : MonoBehaviour
{

    public Sprite TrueSprite;
    public Sprite FaulseSprite;
    private Image image = null;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Change(bool flag)
    {
        if(image == null)
        {
            image = this.gameObject.GetComponent<Image>();
        }
        image.sprite = flag ? TrueSprite : FaulseSprite;
    }
}
