using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyEventSystem : MonoBehaviour
{
    //敵の特殊な出現をするやつ
    //AIまわりを変えないといけない　強制移動系
    
    // Start is called before the first frame update

    public enum PATTERN
    {
        CIRCLE,//中心との距離(プレイヤー位置から)
        FIX,//固定位置(プレイヤー位置から)
        Horizontal,//横並べ
        Vertical,//縦並べ
        StraightToPlayer,//どっかからプレイヤーに向けて
        HorizontalVertical,//横縦同時
    }
    public PATTERN pattern;
    public float value1;
    public float value2;
    public float speed;
    //時間終了時このオブジェクトと生成したキャラ全消し (基本殺さないように消す)
    public float MaxTime;
    public float currentTime;
    private List<Character> CurrentEnemies = new List<Character>();
    public List<EnemyData> EnemyDatas;
    private EnemySpawner enemySpawner;
    [MMInspectorButton("Spawn")]
    /// a button to test the shoot method
    public bool SpawnButton;

    [Serializable]
    public class EnemyData
    {
        public int HP;
        public GameObject prefab;
        public GameObject prefab2;
        public int num;
        public Vector3 pos;
        public Vector3 direction;
        public int damage;
        public int enemyDataId = -1;
        public float InsatantiateTime = 0f;
        public int PATTERN = 1;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn(EnemySpawner spawner)
    {
        enemySpawner = spawner;
        StartCoroutine(SpawnCoroutine());
      


    }

    private IEnumerator SpawnCoroutine()
    {
        //プレイヤーがいないと終わらない
        while (SurviveLevelManager.GetInstance().Players == null|| SurviveLevelManager.GetInstance().Players[0] == null)
        {
            yield return 0;
        }
        CurrentEnemies.Clear();
        //プレイヤー中心に生成
        var player = SurviveLevelManager.GetInstance().Players[0];
        var slm = SurviveLevelManager.GetInstance();

        int currentIndex = 0;
        StartCoroutine(TimeLimit());
        this.transform.position = player.transform.position;
        foreach (var enemy in EnemyDatas)
        {
            yield return new WaitForSeconds(enemy.InsatantiateTime);
            var pos = player.transform.position;
            for (int i = 0; i < enemy.num; i++)
            {
                GameObject newGameObject = (GameObject)Instantiate(enemy.prefab);
                newGameObject.transform.SetParent(this.transform);
                Health objectHealth = newGameObject.gameObject.MMGetComponentNoAlloc<Health>();
                if (objectHealth != null)
                {
                    objectHealth.InitialHealth = enemy.HP;
                    objectHealth.MaximumHealth = enemy.HP;
                    objectHealth.Revive();
                }
                newGameObject.transform.position = pos + enemy.pos;
                if(pattern == PATTERN.StraightToPlayer)
                {
                    newGameObject.transform.position  +=  new Vector3(UnityEngine.Random.Range(-value1, value1), UnityEngine.Random.Range(-value1, value1), 0f);
                }
                var AIfm = newGameObject.gameObject.MMGetComponentNoAlloc<SurviveAIForcedMovement>();
                var data = new SurviveAIForcedMovement.ForcedMovementData();
                
                data.direction = enemy.direction;
                data.speed = speed;
                AIfm.SetData(data);
                var damage = newGameObject.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
                damage.MaxDamageCaused = enemy.damage;
                damage.MinDamageCaused = enemy.damage;
                var se = newGameObject.MMGetComponentNoAlloc<SurviveEnemy>();
                if (se != null&& enemy.enemyDataId >= 0)
                {
                    se.ReflectionData(Array.Find(slm.enemyDatas.datas, _ => _.ID == enemy.enemyDataId), true);
                }
                else
                {
                    se.IsEvent = true;
                    se.enemyData = new EnemyDatas.EnemyData();
                    se.enemyData.PATTERN = enemy.PATTERN;
                }

                CurrentEnemies.Add(newGameObject.MMGetComponentNoAlloc<Character>());
                if (enemy.prefab2 != null)
                {
                    GameObject newGameObject2 = (GameObject)Instantiate(enemy.prefab2);
                    newGameObject2.transform.SetParent(this.transform);
                    CurrentEnemies.Add(newGameObject2.MMGetComponentNoAlloc<Character>());

                }
               
            }

            if (pattern == PATTERN.CIRCLE)
            {
                float angledDiff = 360f / (CurrentEnemies.Count - currentIndex);
                //配置
                for (int i = currentIndex; i < CurrentEnemies.Count; i++)
                {

                    var CIRCLEpos = pos;
                    var angle = (angledDiff * (i - currentIndex)) * Mathf.Deg2Rad;
                    CIRCLEpos.x += value1 * MathF.Cos(angle);
                    CIRCLEpos.y += value1 * MathF.Sin(angle);
                    CurrentEnemies[i].transform.position = CIRCLEpos;

                    var AIfm = CurrentEnemies[i].gameObject.MMGetComponentNoAlloc<SurviveAIForcedMovement>();

                    var data = new SurviveAIForcedMovement.ForcedMovementData();
                    data.direction = (pos - CurrentEnemies[i].transform.position).normalized;
                    data.speed = speed;
                    AIfm.SetData(data);
                }
            }
            else if (pattern == PATTERN.FIX)
            {
                float angledDiff = 360f / CurrentEnemies.Count;
                //float angledDiff = 360f / CurrentEnemies.Count;
                ////配置
                //for (int i = 0; i < CurrentEnemies.Count; i++)
                //{

                //    var pos = this.transform.position;
                //    var angle = (angledDiff * i) * Mathf.Deg2Rad;
                //    pos.x += value1 * MathF.Cos(angle);
                //    pos.y += value1 * MathF.Sin(angle);
                //    CurrentEnemies[i].transform.position = pos;

                //    var AIfm = CurrentEnemies[i].gameObject.MMGetComponentNoAlloc<SurviveAIForcedMovement>();

                //    var data = new SurviveAIForcedMovement.ForcedMovementData();
                //    data.direction = (this.transform.position - CurrentEnemies[i].transform.position).normalized;
                //    data.speed = value2;
                //    AIfm.SetData(data);
                //}
            }
            else if (pattern == PATTERN.Vertical)
            {
                // value1 1列における個数　グループ内個数と同じにする
                // value2 間隔

                for (int i = currentIndex; i < CurrentEnemies.Count; i++)
                {

                    var Verticalpos = CurrentEnemies[i].transform.position;
                    Verticalpos.y += (i % value1) * value2;
                    CurrentEnemies[i].transform.position = Verticalpos;
                }
            }
            else if (pattern == PATTERN.Horizontal)
            {
                // value1 1列における個数　グループ内個数と同じにする
                // value2 間隔

                for (int i = currentIndex; i < CurrentEnemies.Count; i++)
                {

                    var Horizonpos = CurrentEnemies[i].transform.position;
                    Horizonpos.x += (i % value1) * value2;
                    CurrentEnemies[i].transform.position = Horizonpos;
                }
            }
            else if (pattern == PATTERN.StraightToPlayer)
            {

                //for (int i = 0; i < CurrentEnemies.Count; i++)
                //{

                //    var AIfm = CurrentEnemies[i].gameObject.MMGetComponentNoAlloc<SurviveAIForcedMovement>();
                //    var data = AIfm.GetData();
                //    //data.speed = 




                //}
            }
            else if (pattern == PATTERN.HorizontalVertical)
            {
                // value1 1列における個数　グループ内個数と同じにする
                // value2 間隔
                for (int i = currentIndex; i < CurrentEnemies.Count; i++)
                {
                    var hOrv = ((currentIndex / value1) % 2) == 0;
                    if (hOrv)
                    {
                        var Verticalpos = CurrentEnemies[i].transform.position;
                        Verticalpos.y += (i % value1) * value2;
                        CurrentEnemies[i].transform.position = Verticalpos;
                    }
                    else
                    {
                        var Horizonpos = CurrentEnemies[i].transform.position;
                        Horizonpos.x += (i % value1) * value2;
                        CurrentEnemies[i].transform.position = Horizonpos;
                    }
                }
            }
            currentIndex = CurrentEnemies.Count;
        }

      
    }
    private IEnumerator TimeLimit()
    {
        yield return new WaitForSeconds(MaxTime);

        for (int i = 0; i < CurrentEnemies.Count; i++)
        {
            //敵全滅　非殺傷
            (CurrentEnemies[i].CharacterHealth as SurviveHealth).ForceKill();
            Destroy(CurrentEnemies[i].gameObject);
        }

        CurrentEnemies.Clear();
        //自分も消す
        Destroy(this.gameObject);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
