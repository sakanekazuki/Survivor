using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using System.Linq;
using System;

public class ScrollSystem : MonoBehaviour
{
    public Character player;

    public　List<GroundData> groundDatas;

    [Serializable]
    public class GroundData
    {
        public Collider2D ground;
        public int x;
        public int y;
    }

    //位置の計算とか用
    public float groundWidth;
    public float groundHeight;
    public float cameraWidth;
    public float cameraHeight;

    private int widthCount;
    private int heightCount;

    public void SetCharacter()
    {
        Character character = LevelManager.Instance.Players[0];
        if (character !=null)
        {
            player = character;
        }
        

    }


    // Start is called before the first frame update
    void Start()
    {
        List<GroundData> heightList = new List<GroundData>();
        List<GroundData> widthList = new List<GroundData>();

        var children = transform.GetComponentsInChildren<Collider2D>();
        groundDatas = new List<GroundData>();
        foreach(var child in children )
        {
            if (child.name.Substring(0, 4) == "grid")
            {
                groundDatas.Add(new GroundData()
                {
                    ground = child
                });
            }
        }
        foreach (var data in groundDatas)
        {
            heightList.Add(data);
            widthList.Add(data);
        }
        heightList.Sort((_1, _2) =>
        {
            return _1.ground.transform.localPosition.y.CompareTo(_2.ground.transform.localPosition.y);
        });
        widthList.Sort((_1, _2) =>
        {
            return _1.ground.transform.localPosition.x.CompareTo(_2.ground.transform.localPosition.x);
        });

        float width = 0f;
        widthCount = 0;
        float height = 0f;
        heightCount = 0;
        for (int i = 0; i < widthList.Count; i++)
        {
            if (i == 0)
            {
                width = widthList[i].ground.transform.localPosition.x;
            }
            if (width != widthList[i].ground.transform.localPosition.x)
            {
                widthCount++;
                width = widthList[i].ground.transform.localPosition.x;
            }
            widthList[i].x = widthCount;
        }
        for (int i = 0; i < heightList.Count; i++)
        {
            if (i == 0)
            {
                height = heightList[i].ground.transform.localPosition.y;
            }
            if (height != heightList[i].ground.transform.localPosition.y)
            {
                heightCount++;
                height = heightList[i].ground.transform.localPosition.y;
            }
            heightList[i].y = heightCount;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
        {
            SetCharacter();
        }
        else
        {
            var _groundedTest = Physics2D.OverlapPoint((Vector2)player.transform.position, LayerManager.GroundLayerMask);
            //いったん単純にやってみる
            if (_groundedTest != null)
            {
                var data = groundDatas.First(_ => _.ground == _groundedTest);

                if(data != null)
                {
                    int x = 0;
                    int y = 0;
                    float newXPos = 0f;
                    float newYPos = 0f;
                    if (data.x >= widthCount/2 +1)
                    {
                        x--;
                        newXPos = groundDatas.First(_ => _.x==widthCount).ground.transform.localPosition.x + groundWidth;
                    }
                    if (data.x <= widthCount / 2 - 1)
                    {
                        x++;
                        newXPos = groundDatas.First(_ => _.x==0).ground.transform.localPosition.x - groundWidth;
                    }
                    if (data.y >= heightCount / 2 + 1)
                    {
                        y--;
                        newYPos = groundDatas.First(_ => _.y== heightCount).ground.transform.localPosition.y + groundHeight;
                    }
                    if (data.y <= heightCount / 2 - 1)
                    {
                        y++;
                        newYPos = groundDatas.First(_ => _.y == 0).ground.transform.localPosition.y - groundHeight;

                    }
                    if (x != 0 || y != 0)
                    {
                        foreach (var d in groundDatas)
                        {
                            var pos = d.ground.transform.localPosition;
                            d.x += x;
                            d.y += y;
                            if (d.x > widthCount)
                            {
                                d.x = 0;
                                pos.x = newXPos;
                            }
                            if (d.x < 0)
                            {
                                d.x = widthCount;
                                pos.x = newXPos;
                            }
                            if (d.y > heightCount)
                            {
                                d.y = 0;
                                pos.y = newYPos;
                            }
                            if (d.y < 0)
                            {
                                d.y = heightCount;
                                pos.y = newYPos;
                            }
                            d.ground.transform.localPosition = pos;
                        }
                    }

                }
               

            }

        }
    }

}
