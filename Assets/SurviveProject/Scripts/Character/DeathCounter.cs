using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCounter : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //ìGì|ÇµÇΩêîë´Ç∑ÇæÇØ

    public void Count()
    {
        var levelmanager = LevelManager.Instance;
        if (levelmanager != null && levelmanager.Players[0] != null)
        {
            (levelmanager.Players[0] as SurvivePlayer).battleParameter.EnemyDeathCount++;
            SurviveGUIManager.GetInstance().UpdateEnemyDeathCountText((levelmanager.Players[0] as SurvivePlayer).battleParameter.EnemyDeathCount);
        }
    }
}
