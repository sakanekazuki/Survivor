using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MoreMountains.TopDownEngine;
using UnityEngine.Localization.Settings;

[CreateAssetMenu(menuName = "CreateDatas/AbilityData")]
[Serializable]
public class AbilityData : ScriptableObject
{
    public int ID;
    public string Name;//jp
    public string Name_cn;
    public string Name_en;
    public string Name_tw;
    public SurviveWeaponBase basePrefab;
    public Sprite image;

    //�^�C�v�ŕ\�����e�ς���@�\��
    [SerializeField]
    public AbilityDataType abilityDataType = AbilityDataType.Weapon;


    [Serializable]
    public enum AbilityDataType
    {
        Weapon,
        Skill,
        Item,
    }
    //�o���Ή��L�����@-1�S��
    public int TargetCharacterID = -1;
    public string Description = "";
    public string Description_en = "";
    public string Description_tw = "";
    public string Description_cn = "";
    [SerializeField]
    public List<WeaponLevelData> WeaponLevelDatas = new List<WeaponLevelData>();
    [Serializable]
    public class WeaponLevelData
    {
        // �_���[�W�{��
        public float DamageRate = 10f;
        // �U���Ԋu
        public float Interval = 1f;
        // ���x
        public float Speed = 1f;
        //�@�U���͈́@�i�\���ł̂ݎg�p�j
        public string AreaStr = "��";
        //�@�U���͈͔{���@���ۂɑ傫�����Ă����ꍇ�ȂǂɎg�p
        public float Size = 1f;
        //�@�����񐔁i�e���j //�^�C�v�ɍ��킹��
        public int Num = 1;
        //�^�C�v 1�͈͌n�i�a���A���͍U���Ȃǁj2�ˌ� num���f
        public int Type = 1;
        //�m�b�N�o�b�N
        public float Knockback = 0f;

        //�p������ �e��������܂� �ݒ�͕K�v
        public float Time = 0.5f;
        //�������@���x�����\�L
        public string Description = "";//jp
        public string Description_cn = "";
        public string Description_en = "";
        public string Description_tw = "";

        //�q�b�g��//���񓖂�����������邩�@-1����
        public int Penetration = -1;


        public List<ExWeaponStatusData> exWeaponStatuses = new List<ExWeaponStatusData>();


        public string GetLocalizeDescription()
        {
            string description = Description;
            switch ( LocalizationSettings.SelectedLocale.Formatter.ToString())
            {
                case "ja":
                    description = Description;
                    break;
                case "en":
                    description = Description_en;
                    break;
                case "zh-TW":
                    description = Description_tw;
                    break;
                case "zh-CN":
                    description = Description_cn;
                    break;
                default:
                    break;
            }
            return description;
        }
    }
    public string GetLocalizeDescription()
    {
        string description = Description;
        switch (LocalizationSettings.SelectedLocale.Formatter.ToString())
        {
            case "ja":
                description = Description;
                break;
            case "en":
                description = Description_en;
                break;
            case "zh-TW":
                description = Description_tw;
                break;
            case "zh-CN":
                description = Description_cn;
                break;
            default:
                break;
        }
        return description;
    }
    public string GetLocalizeName()
    {
        string name = Name;
        switch (LocalizationSettings.SelectedLocale.Formatter.ToString())
        {
            case "ja":
                name = Name;
                break;
            case "en":
                name = Name_en;
                break;
            case "zh-TW":
                name = Name_tw;
                break;
            case "zh-CN":
                name = Name_cn;
                break;
            default:
                break;
        }
        return name;
    }
    //�ǂ����ɈڐA�����ق��������Ǝv�� ����������

    [Serializable]
    public class ExWeaponStatusData
    {
        [Serializable]
        public enum ExWeaponStatus
        {
            NONE,
            CHAIN,
            ShockWave,
            Embers,
            STUN,
            SLOW,
            Bomb,
            IAI,
            KillPower,
            BombAddDamage,
            SHINKU6,
            OukaEx,
            IAI2,
            DASH_SLASH_EX,
        }
        public int value1 = 0;
        public int value2 = 0;
        public ExWeaponStatus exWeaponStatus = ExWeaponStatus.NONE;
        public ExStatus exStatus;
        [Serializable]
        public class ExStatus {
            // �_���[�W�{��
            public float DamageRate = 10f;
            // �U���Ԋu
            public float Interval = 1f;
            // ���x
            public float Speed = 1f;
            //�@�U���͈́@�i�\���ł̂ݎg�p�j
            public string AreaStr = "��";
            //�@�U���͈͔{���@���ۂɑ傫�����Ă����ꍇ�ȂǂɎg�p
            public float Size = 1f;
            //�@�����񐔁i�e���j //�^�C�v�ɍ��킹��
            public int Num = 1;
            //�^�C�v 1�͈͌n�i�a���A���͍U���Ȃǁj2�ˌ� num���f
            public int Type = 1;
            //�m�b�N�o�b�N
            public float Knockback = 0f;
            //�p������ �e��������܂� �ݒ�͕K�v
            public float Time = 0.5f;
            //�q�b�g��//���񓖂�����������邩�@-1����
            public int Penetration = -1;
            //����ɒǉ����ʂ�^������
            public ExWeaponStatus exWeaponStatus = ExWeaponStatus.NONE;
            public float exWeaponStatusValue =0f;
        }
    }

    [SerializeField]
    public List<ItemLevelData> ItemLevelDatas = new List<ItemLevelData>();
    [Serializable]
    public class ItemLevelData
    {
        //�������@���x�����\�L
        public string Description = "";
        public string Description_en = "";
        public string Description_tw = "";
        public string Description_cn = "";
        public List<ItemStatusData> itemStatusDatas = new List<ItemStatusData>();

        public string GetLocalizeDescription()
        {
            string description = Description;
            switch (LocalizationSettings.SelectedLocale.Formatter.ToString())
            {
                case "ja":
                    description = Description;
                    break;
                case "en":
                    description = Description_en;
                    break;
                case "zh-TW":
                    description = Description_tw;
                    break;
                case "zh-CN":
                    description = Description_cn;
                    break;
                default:
                    break;
            }
            return description;
        }
    }

    [Serializable]
    public class ItemStatusData
    {
        
        [Serializable]
        public enum ItemStatus
        {
            STATUS_UP,
            DROP_BIND,
            NO_RECOVERY,
            HEAL_ITEM,
            GOLD_ITEM,
            GUARD,
            WALK_RECOVERY,
            NONE,
        }
        public float value = 0;
        public ItemStatus exItemStatus = ItemStatus.NONE;
        [Tooltip("�X�e�[�^�X�ω����ʂŎg�p")]
        public GameDefine.StatusCorrection StatusType = GameDefine.StatusCorrection.NONE;
    }

}
