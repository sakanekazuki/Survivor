using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/PlayerLevelGainPointData")]
public class PlayerLevelGainPointData : ScriptableObject
{
    [SerializeField]
    public Data[] data;
    [Serializable]
    public class Data
    {
        public int level;
        public int point;
    }
}

