using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//デフォルトのキャラデータ

[CreateAssetMenu(menuName = "CreateDatas/Stage")]
[Serializable]
public class StageDatas : ScriptableObject
{
    [Serializable]
    public class StageData
    {
        //多言語対応するべし
        public int ID;
        public string NAME;
       
        public int UnlockID;

        public string SceneName;


        public int Endless;

        public float BonusCoinRate = 1.0f;


    }

    [SerializeField]
    public StageData[] datas;
}
