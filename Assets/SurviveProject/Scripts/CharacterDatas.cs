using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�f�t�H���g�̃L�����f�[�^

[CreateAssetMenu(menuName = "CreateDatas/Character")]
[Serializable]
public class CharacterDatas : ScriptableObject
{
    [Serializable]
    public class CharacterData
    {
        //������Ή�����ׂ�
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

        //�L�����I��
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
