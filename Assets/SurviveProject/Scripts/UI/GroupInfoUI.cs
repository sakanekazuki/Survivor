using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if DEBUG || UNITY_EDITOR

public class GroupInfoUI : MonoBehaviour
{
    [SerializeField] private GameObject element;
    private void Start()
    {
        EnemySpawner.OnSetGroup += SetGroup;
    }

    private void SetGroup(EnemyGroupDatas.EnemyGroupData obj,int index)
    {
        var groupElement = Instantiate(element,this.transform);
        groupElement.GetComponent<GroupInfoSingleUI>().SetGroupSingleUI(obj,index);
    }
}
#endif
