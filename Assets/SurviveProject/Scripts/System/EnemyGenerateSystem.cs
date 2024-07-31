using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;

public class EnemyGenerateSystem : MonoBehaviour
{

    //とりあえず生成してみる
    public List<Character> Enemys = new List<Character>();
    public float generateTime = 0.5f;
    private float countTime = 0f;
    private Character player;

    public Character Player { get {
            if (player == null)
            {
                SetCharacter();

            }
            return player;
        } set => player = value; }

    //　発生の時間の設定、いつからいつまで、間隔　一度に出る数　等　種類
    //　設定毎に秒数カウント
    //　発生位置はプレイヤーから？離れた場所、ある程度の距離ならどこでも　まあランダムか

    //設定毎に実体化したデータに何体存在しているかとかある程度の限度を与えたりする？
    //ある程度キャラは中身の作りが簡単なものにして軽量化が必要と思われる

    //離れすぎると再配置するとかする
    public void SetCharacter()
    {
        Character character = LevelManager.Instance.Players[0];
        if (character != null)
        {
            Player = character;
        }


    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Player == null)
            return;
        if(countTime >= generateTime)
        {
            float x = Random.Range(-30f, 30f);
            float y = Random.Range(-30f, 30f);
            if (Mathf.Abs(x) < 10f)
                x += x >= 0f ? 10f :  -10f;
            if (Mathf.Abs(y) < 10f)
                y += y >= 0f ? 10f : -10f;
            GameObject.Instantiate(Enemys[0], Player.transform.position + new Vector3(x, y, 0f), Quaternion.identity);
            countTime = 0f;
        }
        countTime += Time.deltaTime;

    }
}
