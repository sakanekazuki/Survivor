using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/EnemyGroup")]
public class EnemyGroupDatas : ScriptableObject
{
    [Serializable]
    public class EnemyGroupData
    {
        public int startTime = 0;
        public int endTime = 60;

        public int max = 30;
        public float interval = 3f;

        public GameObject patternObj;

        [SerializeField]
        public int[] enemyIDs;
    }
    [SerializeField]
    public EnemyGroupData[] datas;
    public EnemyGroupData[] exDatas;
}
