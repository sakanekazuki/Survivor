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

    //タイプで表示内容変える　予定
    [SerializeField]
    public AbilityDataType abilityDataType = AbilityDataType.Weapon;


    [Serializable]
    public enum AbilityDataType
    {
        Weapon,
        Skill,
        Item,
    }
    //出現対応キャラ　-1全部
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
        // ダメージ倍率
        public float DamageRate = 10f;
        // 攻撃間隔
        public float Interval = 1f;
        // 速度
        public float Speed = 1f;
        //　攻撃範囲　（表示でのみ使用）
        public string AreaStr = "中";
        //　攻撃範囲倍率　実際に大きくしていく場合などに使用
        public float Size = 1f;
        //　発生回数（弾数） //タイプに合わせて
        public int Num = 1;
        //タイプ 1範囲系（斬撃、周囲攻撃など）2射撃 num反映
        public int Type = 1;
        //ノックバック
        public float Knockback = 0f;

        //継続時間 弾が消えるまで 設定は必要
        public float Time = 0.5f;
        //説明文　レベル時表記
        public string Description = "";//jp
        public string Description_cn = "";
        public string Description_en = "";
        public string Description_tw = "";

        //ヒット回数//何回当たったら消えるか　-1無限
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
    //どこかに移植したほうがいいと思う 武器特殊効果

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
            // ダメージ倍率
            public float DamageRate = 10f;
            // 攻撃間隔
            public float Interval = 1f;
            // 速度
            public float Speed = 1f;
            //　攻撃範囲　（表示でのみ使用）
            public string AreaStr = "中";
            //　攻撃範囲倍率　実際に大きくしていく場合などに使用
            public float Size = 1f;
            //　発生回数（弾数） //タイプに合わせて
            public int Num = 1;
            //タイプ 1範囲系（斬撃、周囲攻撃など）2射撃 num反映
            public int Type = 1;
            //ノックバック
            public float Knockback = 0f;
            //継続時間 弾が消えるまで 設定は必要
            public float Time = 0.5f;
            //ヒット回数//何回当たったら消えるか　-1無限
            public int Penetration = -1;
            //さらに追加効果を与えるやつ
            public ExWeaponStatus exWeaponStatus = ExWeaponStatus.NONE;
            public float exWeaponStatusValue =0f;
        }
    }

    [SerializeField]
    public List<ItemLevelData> ItemLevelDatas = new List<ItemLevelData>();
    [Serializable]
    public class ItemLevelData
    {
        //説明文　レベル時表記
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
        [Tooltip("ステータス変化効果で使用")]
        public GameDefine.StatusCorrection StatusType = GameDefine.StatusCorrection.NONE;
    }

}
