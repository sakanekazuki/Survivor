using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/Enemy")]
public class EnemyDatas : ScriptableObject
{
    [Serializable]
    public class EnemyData
    {
        public int ID;
        public string NAME;
        public float ATTACK;
        public float SPEED;
        public float MAX_HP;
        public int PATTERN;//����
        public float SIZE = 1f;//����
        public float INTERVAL = 1f;//����
        public int POOL_MAX = 30;
        public GameObject Prefab;
    }
    [SerializeField]
    public EnemyData[] datas;
}
