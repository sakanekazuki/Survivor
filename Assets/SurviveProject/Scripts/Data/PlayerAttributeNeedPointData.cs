using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/PlayerAttributeNeedPointData")]
public class PlayerAttributeNeedPointData : ScriptableObject
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

