using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/ShopLevelData")]
public class ShopLevelData : ScriptableObject
{
    [SerializeField]
    public Data[] data;
    [Serializable]
    public class Data
    {
        public int ID;
        public float levelUpValue;
        public string localizedKey;
        public List<LevelData> levelDatas;
    }
    [Serializable]
    public class LevelData
    {
        public int level;
        public int price;
    }
}
