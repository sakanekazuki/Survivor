using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/PlayerEXP")]
public class PlayerEXPData : ScriptableObject
{
    [SerializeField]
    public Data[] data;
    [Serializable]
    public class Data
    {
        public int level;
        public int exp;
    }
}

