using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�f�t�H���g�̃L�����f�[�^

[CreateAssetMenu(menuName = "CreateDatas/ThrowAbilityList")]
[Serializable]

public class ThrowAbilityListData : ScriptableObject
{
    public List<int> IDs = new List<int>();
}
