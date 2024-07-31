using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/AbilityRarityData")]
public class AbilityRarityData : ScriptableObject
{
    [SerializeField]
    public Data[] data;
    [Serializable]
    public class Data
    {
        public int LuckLevel;
        //count�@4�@0����m�[�}���@����
        public List<int> Rate;
    }
}

