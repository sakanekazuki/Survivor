using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudObject : MonoBehaviour
{

    public float Speed;

    SurvivePlayer player;
    // Start is called before the first frame update
    void Start()
    {
        player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x + Speed * Time.deltaTime, transform.position.y, 0f);

        if (transform.position.x - player.transform.position.x >30f)
        {
            this.GetComponent< MMPoolableObject >(). Destroy();
        }
    }
}
