using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
public class ObjectLayer : MonoBehaviour
{
    //public Transform parent;
    //SurvivePlayer player;
    //SpriteRenderer sprite;

    public List<SpriteRenderer> sprites;
    // Start is called before the first frame update
    void Start()
    {
        //sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //if(player == null)
        //{
        //    if (SurviveLevelManager.Instance !=null && SurviveLevelManager.Instance.Players != null)
        //        player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        //}

        //if(player.transform.position.y > parent.position.y)
        //{
        //    sprite.color = new Color(1, 1, 1, 0.5f);
        //}
        //else
        //{
        //    sprite.color = Color.white;
        //}
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
    
        if (collision.gameObject.tag == "Player")
        {
            foreach (var sprite in sprites)
            {
                {
                    sprite.color = new Color(1, 1, 1, 0.5f);
                }
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
    
        if (collision.gameObject.tag == "Player")
        {
            foreach (var sprite in sprites)
            {
                 sprite.color = Color.white;
            }
        }
    }
}
