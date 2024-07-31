using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropSystem : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject HealthItem;//通常定格率ドロップ回復
    public GameObject Coin; //通常低確率ドロップコイン
    public GameObject EXPItem1;
    public GameObject EXPItem2;
    public GameObject EXPItem3;
    public GameObject Box;//宝箱
    //public GameObject Hamaya;//破魔矢
    public GameObject Rainbow;//経験値回収

    /*経験値プール 通常敵
     * 時間	    小	    中	    大
     * 0～2分	100％	0％  	0％
     * 3～5分	97％	3％  	0％
     * 6～9分	95％	5％  	0％
     * 10～12分	85％	13％	2％
     * 13～15分	80％	18％	2％
     * 16～18分	70％	25％	5％
     * 19～21分	60％	33％	7％
     * 22～24分	40％	50％	10％
     * 24～27分	30％	60％	10％
     * 28～30分	10％	80％	20％
    */
    private struct DropPool
    {
        public float time;
        public float expNone;
        public float exp1;
        public float exp2;
        public float exp3;
        public float coin;
        public float heal;

        public static DropPool NONE = new DropPool();
    }
    readonly private List<DropPool> dropPool = new List<DropPool>
    {
        new DropPool(){ time = 0, expNone = 40, exp1 = 60, exp2 = 0, exp3 = 0, coin = 2, heal = 1},
        new DropPool(){ time = 180, expNone = 34, exp1 = 63, exp2 = 3, exp3 = 0, coin = 2, heal = 1},
        new DropPool(){ time = 360, expNone = 30, exp1 = 65, exp2 = 5, exp3 = 0, coin = 2, heal = 2},
        new DropPool(){ time = 600, expNone = 25, exp1 = 60, exp2 = 13, exp3 = 2, coin = 3, heal = 2},
        new DropPool(){ time = 780, expNone = 20, exp1 = 57, exp2 = 18, exp3 = 5, coin = 3, heal = 2},
        new DropPool(){ time = 960, expNone = 20, exp1 = 50, exp2 = 23, exp3 = 7, coin = 4, heal = 2},
        new DropPool(){ time = 1140, expNone = 15, exp1 = 45, exp2 = 30, exp3 = 10, coin = 4, heal = 3},
        new DropPool(){ time = 1320, expNone = 12, exp1 = 40, exp2 = 35, exp3 = 13, coin = 5, heal = 3},
        new DropPool(){ time = 1500, expNone = 10, exp1 = 35, exp2 = 40, exp3 = 15, coin = 5, heal = 3},
        new DropPool(){ time = 1680, expNone = 10, exp1 = 30, exp2 = 35, exp3 = 15, coin = 5, heal = 3},
     };

    //敵専用クラスに書き換え、設定値からドロップ率計算
    public Character parent;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Drop()
    {
        var time = SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime();
        if(parent == null)
        {
            parent = transform.parent.gameObject.GetComponent<Character>();
        }
        Vector3 pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),0f);/*位置を少し揺らがせる*/
        DropPool currentPool = DropPool.NONE;
        for (int i = 0; i < dropPool.Count; i++)
        {
            if(time >= dropPool[i].time)
            {
                currentPool =  dropPool[i];
            }
            else
            {
                break;
            }
        }
        
        //経験値ドロップ
        switch( Random.Range(0f, 100f))
        {
            case float i when i <= currentPool.expNone:
                break;
            case float i when i <= currentPool.exp1 + currentPool.expNone:
                Instantiate(EXPItem1).transform.position = pos;
                break;
            case float i when i <= currentPool.exp2 + currentPool.exp1 + currentPool.expNone:
                Instantiate(EXPItem2).transform.position = pos;
                break;
            case float i when i <= currentPool.exp3 + currentPool.exp2 + currentPool.exp1 + currentPool.expNone:
                Instantiate(EXPItem3).transform.position = pos;
                break;
            default:
                break;
        }
        if(Random.Range(0f, 100f) <= currentPool.coin)
        {
            pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),0f);/*位置を少し揺らがせる*/
            Instantiate(Coin).transform.position = pos;
        }
        if (Random.Range(0f, 100f) <= currentPool.heal)
        {
            pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);/*位置を少し揺らがせる*/

            if ((SurviveLevelManager.GetInstance().Players[0] as SurvivePlayer).battleParameter.DropBind)
            {
                Instantiate(Coin).transform.position = pos;
            }
            else
            {
                Instantiate(HealthItem).transform.position = pos;
            }
        }
        //if (Random.Range(0f, 100f) <= 0.5f)
        //{
        //    pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);/*位置を少し揺らがせる*/

        //    Instantiate(Hamaya).transform.position = pos;
        //}
        //if (Random.Range(0f, 100f) <= 0.5f)
        //{
        //    pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);/*位置を少し揺らがせる*/

        //    Instantiate(Rainbow).transform.position = pos;
        //}
        if (parent is SurviveEnemy)
        {
            if((parent as SurviveEnemy).GetEnemyType() == 3)
            {
                //確定
                pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f),0f);/*位置を少し揺らがせる*/
                Instantiate(Box).transform.position = pos;
            }
            else if((parent as SurviveEnemy).GetEnemyType() == 2)
            {
                //3％
                if (Random.Range(0f, 100f) <= 0.5f)
                {
                    pos = parent.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);/*位置を少し揺らがせる*/
                    Instantiate(Box).transform.position = pos;
                }
            }
        }
    }
    public void DropTsubo()
    {
        Vector3 pos = transform.parent.position;
        
        switch (Random.Range(0f, 100f))
        {
            case float i when i <= 20f://20
                Instantiate(HealthItem).transform.position = pos;
                break;
            case float i when i <= 90f://70
                Instantiate(Coin).transform.position = pos;
                break;
            //case float i when i <= 95f://5
            //    Instantiate(Hamaya).transform.position = pos;
            //    break;
            case float i when i <= 100f://5
                Instantiate(Rainbow).transform.position = pos;
                break;
            default:
                break;
        }
       
    }
}
