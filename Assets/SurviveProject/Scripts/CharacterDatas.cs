using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//デフォルトのキャラデータ

[CreateAssetMenu(menuName = "CreateDatas/Character")]
[Serializable]
public class CharacterDatas : ScriptableObject
{
    [Serializable]
    public class CharacterData
    {
        //多言語対応するべし
        public int ID;
        public string NAME;
        public float ATTACK;
        public float SPEED;
        public float MAX_HP;
        public int DASH_NUM;
        public float DASH_INTERVAL;
        public float DASH_POWER;
        public float CRITICAL;
        public int COINCOST;

        //キャラ選択
        public bool isLocked;
        public string localizedNameKey;
        public string localizedUnlockNameKey;
        public string localizedWeaponIntroKey;
        public Sprite charaImage;
        public Sprite charaLockImage;
        public Sprite charaWeapon;
        public AnimationClip charaSD;

        public GameObject Prefab;

        public float PickUP;
        public float WeaponInterval;
        public float SkillInterval;
        public int Luck;
        public float Regeneration;
        public float EXPUp;
        public float GoldUp;
        public float AREA;
        public int ThrowNum;
        public float KnockBack;
        public int Shield; 
        public int Reroll;
        public int WeaponNum;
        public int ItemNum;
        
        public int[] AttributeStatusLevel;
    }

    [SerializeField]
    public CharacterData[] datas;
}
