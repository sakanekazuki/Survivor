using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//設定周りまとめ
public static class GameDefine
{
    public enum StatusAttribute
    {
        Str = 0,
        Vit,
        Agi,
        Int,
        Dex,
        Luk,
        NONE,
    }
    public enum StatusCorrection
    {
        HP = 0,
        DAMAGE,
        SPD,
        CRT,
        PickUP,
        Haste,
        //WeaponInterval,
        //SkillInterval,
        Luck,
        Regeneration,
        DashPower,
        DashNum,
        DashInterval,
        EXP,
        GOLD,
        AREA,
        ThrowNum,
        KnockBack,
        DamageTaken,
        MainWeaponInterval,
        MainWeaponAREA,
        NONE,
    }

    readonly public static Dictionary<int, string> StatusAttributeStr = new Dictionary<int, string>()
    {
        {1,"E-" },
        {2,"E" },
        {3,"E+" },
        {4,"D-" },
        {5,"D" },
        {6,"D+" },
        {7,"C-" },
        {8,"C" },
        {9,"C+" },
        {10,"B-" },
        {11,"B" },
        {12,"B+" },
        {13,"A-" },
        {14,"A" },
        {15,"A+" },
        {16,"S-" },
        {17,"S" },
        {18,"S+" },
        {19,"SS" },
    };
    //カラーコードで
    //readonly public static Dictionary<int, string> StatusAttributeColor = new Dictionary<int, string>()
    //{
    //    {1,"E-" },
    //    {2,"E" },
    //    {3,"E+" },
    //    {4,"D-" },
    //    {5,"D" },
    //    {6,"D+" },
    //    {7,"C-" },
    //    {8,"C" },
    //    {9,"C+" },
    //    {10,"B-" },
    //    {11,"B" },
    //    {12,"B+" },
    //    {13,"A-" },
    //    {14,"A" },
    //    {15,"A+" },
    //    {16,"S-" },
    //    {17,"S" },
    //    {18,"S+" },
    //    {19,"SS" },
    //};
    readonly public static Dictionary<int, int> StatusAttributeLevel = new Dictionary<int, int>()
    {
        {1,0},
        {2,10 },
        {3, 20},
        {4,30 },
        {5,40 },
        {6,50 },
        {7,60 },
        {8,70 },
        {9,80},
        {10,90},
        {11,100 },
        {12,110},
        {13,120 },
        {14,130 },
        {15,140},
        {16,150 },
        {17,160},
        {18,170 },
        {19,180 },
    };
    readonly public static Dictionary<int, float> StatusAttributeCorrection = new Dictionary<int, float>()
    {
        {1,0.25f},
        {2,0.35f },
        {3, 0.45f},
        {4,0.55f },
        {5,0.65f },
        {6,0.75f },
        {7,0.85f },
        {8,0.95f },
        {9,1.15f},
        {10,1.25f},
        {11,1.4f },
        {12,1.55f},
        {13,1.7f },
        {14,1.85f },
        {15,2f},
        {16,2.25f },
        {17,2.5f},
        {18,2.75f },
        {19,3f },
    };

    public enum ABILITY_RARITY
    {
        NONE = 0,
        NORMAL = 1,
        RARE = 2,
        EPIC = 3,
        LEGEND = 5,
    }
    public readonly static Dictionary<ABILITY_RARITY, string> AbilityRarityColor = new Dictionary<ABILITY_RARITY, string>() {
        { ABILITY_RARITY.NORMAL, "#98fb98"},{ ABILITY_RARITY.RARE, "#1e90ff" },{ ABILITY_RARITY.EPIC, "#8a2be2" },{ ABILITY_RARITY.LEGEND, "#ffd700" }
    };
    public static string GetAttributeLevelStr(int point, int defaultLevel)
    {
        int before = 0;
        point += StatusAttributeLevel[defaultLevel];
        foreach (var data in StatusAttributeLevel)
        {
            //最初に越えたところの前が対象
            if(data.Value > point)
            {
                return StatusAttributeStr[before];
            }
            before = data.Key;
        }

        return StatusAttributeStr[before];
    }

    public static int GetAttributeLevel(int point, int defaultLevel)
    {
        
        int before = 0;
        point += StatusAttributeLevel[defaultLevel];
        foreach (var data in StatusAttributeLevel)
        {
            //最初に越えたところの前が対象
            if (data.Value > point)
            {
                return before;
            }
            before = data.Key;
        }

        return before;
    }

    /*必要なプラスされるステータスの一覧
     * 
     * HP上限　体
     * ダメージ　力
     * 速度
     * クリティカル率　心
     * 取得範囲　　運
     * 攻撃速度　武器　敏
     * 攻撃速度　スキル　智
     * 抽選が良くなる値　運
     * 自然回復　　
     * ダッシュ距離
     * ダッシュ回数
     * ダッシュ間隔
     * 経験値量
     * 金量
     * 回復量
     * 攻撃範囲
     * 飛び道具個数
     * ノックバック
     * 
     * 
     * 
     * 
     */

}
