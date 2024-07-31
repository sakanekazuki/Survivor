using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/AbilityLocalizeData")]
public class AbilityLocalizeData : ScriptableObject
{
    [SerializeField]
    public Data[] data;
    [Serializable]
    public class Data
    {
        public int ID;
        public string Name = "";
        public string Name_en = "";
        public string Name_cn = "";
        public string Name_tw = "";
        public string Description = "";
        public string Description_en = "";
        public string Description_tw = "";
        public string Description_cn = "";
        [SerializeField]
        public List<LevelData> levelDatas;


    }
    [Serializable]
    public class LevelData
    {
        public string Description = "";
        public string Description_en = "";
        public string Description_tw = "";
        public string Description_cn = "";
    }
}
