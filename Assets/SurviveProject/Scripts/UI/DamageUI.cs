using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    List<GameObject> numbers = new List<GameObject>();
    public Material material;

    //一旦数字の画像持っておく　実装時は読み込んでいる画像を使うように
    public List<Sprite> SpriteList;
    public List<Sprite> CriticalSpriteList;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Play(GameObject target, int damage)
    {
        SetData(target, damage, false);
    }
    public void Play(float damage, bool critical)
    {
        SetData(this.gameObject, (int)damage, critical);
    }
    /// <summary>
    /// ダメージ　とりあえず表示する感じにしてみる
    /// まだ仮
    /// </summary>
    /// <param name="target"></param>
    /// <param name=""></param>
    public void SetData(GameObject target, int damage, bool critical)
    {
        List<int> damageList = new List<int>();
        int currentCheck = damage;
        while (currentCheck >= 10)
        {
            var value = currentCheck % 10;
            if (damageList.Count > 0)
            {
                damageList.Insert(0, value);
            }
            else
            {
                damageList.Add(value);
            }

            currentCheck /= 10;
        }
        damageList.Insert(0, currentCheck);
        var a = damageList.Count / 2f;

        float width = 0.7f;

        for (int i = 0; i < damageList.Count; i++)
        {
            float x = (i - a) * width;

            var obj = new GameObject();
            obj.transform.parent = transform;
            var s = obj.AddComponent<SpriteRenderer>();
            s.sprite = critical ? CriticalSpriteList[damageList[i]] :SpriteList[damageList[i]];
            s.material = material;
            s.transform.localPosition = new Vector3(x, 0, 0);
            obj.name = (Mathf.Pow(10, (damageList.Count - 1) - i) * damageList[i]).ToString();
            numbers.Add(obj);

            s.sortingLayerName = "Above";
            s.sortingOrder = 99;

            Move(obj.transform, damageList.Count-1 - i);
        }

        //位置に揺らぎを持たせる
        transform.position = target.transform.position;

        //仮位置
        transform.localPosition += new Vector3(0, 1.5f, 0);
        //Invoke("Destroy", 1f);
        DOVirtual.DelayedCall(1f, () =>
        {
            Destroy(gameObject);
        }).Play();

    }
    Transform _target;

    public void Move(Transform target, int index)
    {
        //_target = target;
        //Invoke("Move2", index * 0.2f);
        DOVirtual.DelayedCall(index * 0.2f, () =>
        {
            var moving = target.DOLocalMoveY(0.5f, 0.5f).SetRelative(true);
            moving.onComplete = () => { };
            moving.Play();
        }).Play();

    }

    public void Move2()
    {
        var moving = _target.DOLocalMoveY(0.5f, 0.5f).SetRelative(true);
        moving.onComplete = () => { };
        moving.Play();
    }

}
