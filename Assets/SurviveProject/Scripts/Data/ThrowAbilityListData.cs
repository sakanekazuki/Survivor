using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//デフォルトのキャラデータ

[CreateAssetMenu(menuName = "CreateDatas/ThrowAbilityList")]
[Serializable]

public class ThrowAbilityListData : ScriptableObject
{
    public List<int> IDs = new List<int>();
}
