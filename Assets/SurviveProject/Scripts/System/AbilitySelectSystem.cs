using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using static LevelUpEvent;
using System.Linq;


//アビリティ選択の内容の決定など
[Serializable]
public class AbilitySelectSystem
{
    //アビリティの開放状況等も加味する必要あり

    [SerializeField]
    public List<AbilityData> abilityDatas;


    //今回使用可能なデータ
    public List<AbilityData> UseableDatas;


    //解放状態取得して初期化
    public void Initiralize()
    {
        //とりあえずそのまま
        UseableDatas = SurviveGameManager.GetInstance().GetAbilitysCopy();
    }


    public List<AbilityInLevel> GetSelectAbilityDatas()
    {
        List<AbilityInLevel> abilitys = new List<AbilityInLevel>();
        List<AbilityData> UseableDatasCopy = new List<AbilityData>(UseableDatas);
        //プレイヤー情報取得

        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        var sgm = SurviveGameManager.GetInstance();
        //とりあえず選択可能なものを抽出
        List<AbilityInLevel> tempList = new List<AbilityInLevel>();
        for(int i = 0; i < UseableDatasCopy.Count; i++)
        {
            int level = 0; 
            //特定のキャラの時のみ出現するアビリティ対応
            if(UseableDatasCopy[i].TargetCharacterID != -1 && UseableDatasCopy[i].TargetCharacterID  != sgm.currentCharacterID)
            {
                continue;
            }
            if (player.GetAbilityDic.ContainsKey(UseableDatasCopy[i].ID))
            {
                int pLevel = player.GetAbilityDic[UseableDatasCopy[i].ID];
                if (UseableDatasCopy[i].abilityDataType == AbilityData.AbilityDataType.Item)
                {
                    if (UseableDatasCopy[i].ItemLevelDatas.Count > pLevel + 1)
                    {
                        level = pLevel + 1;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {

                    if (UseableDatasCopy[i].WeaponLevelDatas.Count > pLevel + 1)
                    {
                        level = pLevel + 1;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
            //種類でも最大で持てる個数はあるので
                if(UseableDatasCopy[i].abilityDataType == AbilityData.AbilityDataType.Skill ||
                    UseableDatasCopy[i].abilityDataType == AbilityData.AbilityDataType.Weapon)
                {
                    if (player.skillSets.Count >= player.battleParameter.WeaponNum)
                    {
                        continue;
                    }
                }
                if (UseableDatasCopy[i].abilityDataType == AbilityData.AbilityDataType.Item)
                {
                    if (player.itemSets.FindAll(_ => !player.skillSets.Exists(s => s.AbilityID == _.AbilityID)).Count >= player.battleParameter.ItemNum)
                    {
                        continue;
                    }
                }
            }

            int attribute = 0;
            int attributeLevel = 0;
            //スキルかどうかの判定 いったんすきるでなくても
            if (true)
            {
                //if (level == 0)
                {
                    //これはランダム
                    attribute = UnityEngine.Random.Range((int)GameDefine.StatusAttribute.Str, (int)GameDefine.StatusAttribute.NONE);
                    //こっちデータ参照

                    int beforeX = 0;
                    AbilityRarityData.Data data = null;
                    for (int x = 0; x <  sgm.abilityRarityData.data.Length; x++)
                    {
                        if (sgm.abilityRarityData.data[x].LuckLevel > player.battleParameter.Luck)
                        {
                            data = sgm.abilityRarityData.data[beforeX];
                            break;
                        }
                        beforeX = x;
                    }
                    if(data == null)
                    {
                        data = sgm.abilityRarityData.data[beforeX];
                    }
                    
                    switch( UnityEngine.Random.Range(0f, 100f))
                    {
                        case float r when r <= data.Rate[0]:
                            attributeLevel = (int)GameDefine.ABILITY_RARITY.NORMAL;
                            break;
                        case float r when r <= data.Rate[1]+ data.Rate[0]:
                            attributeLevel = (int)GameDefine.ABILITY_RARITY.RARE;
                            break;
                        case float r when r <= data.Rate[2]+ data.Rate[1] + data.Rate[0]:
                            attributeLevel = (int)GameDefine.ABILITY_RARITY.EPIC;
                            break;
                        case float r when r <= data.Rate[3] + data.Rate[2] + data.Rate[1] + data.Rate[0]:
                            attributeLevel = (int)GameDefine.ABILITY_RARITY.LEGEND;
                            break;
                        default:
                            attributeLevel = (int)GameDefine.ABILITY_RARITY.NORMAL;
                            break;
                    }
                    
                }
                //else
                //{
                //    if (UseableDatas[i].abilityDataType == AbilityData.AbilityDataType.Item)
                //    {
                //        attribute = (int)player.itemSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttribute;
                //        attributeLevel = (int)player.itemSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttributeValue;
                //    }
                //    else if (UseableDatas[i].abilityDataType == AbilityData.AbilityDataType.Weapon)
                //    {
                //        attribute = (int)player.weaponSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttribute;
                //        attributeLevel = (int)player.weaponSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttributeValue;
                //    }
                //    else
                //    {
                //        attribute = (int)player.skillSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttribute;
                //        attributeLevel = (int)player.skillSets.Find(_ => _.AbilityID == UseableDatas[i].ID).statusAttributeValue;
                //    }
                //}
            }
            else
            {
                attribute = (int)GameDefine.StatusAttribute.NONE;
            }

            tempList.Add(new AbilityInLevel() { level = level, ability = UseableDatasCopy[i], 
                statusAttribute = (GameDefine.StatusAttribute)attribute,
                statusAttributeValue = attributeLevel
            });;
        }

        if(tempList.Count > 4)
        {

            abilitys = tempList.OrderBy(a => Guid.NewGuid()).ToList();
            //メイン武器が出やすいようになる
            if(UnityEngine.Random.Range(0,100) < 25)
            {
                if(abilitys.Exists(_ => _.ability.abilityDataType == AbilityData.AbilityDataType.Weapon))
                {
                    var target = abilitys.Find(_ => _.ability.abilityDataType == AbilityData.AbilityDataType.Weapon);
                    var targetIndex = abilitys.IndexOf(target);
                    if(targetIndex>0)
                    {
                        abilitys.RemoveAt(targetIndex);
                        abilitys.Insert(0, target);
                    }
                    
                }
            }
            abilitys = abilitys.GetRange(0, 4);
        }
        else if(tempList.Count == 0)
        {
            //SurviveGameManager.GetInstance().HealAbility.ItemLevelDatas
            //お金とかが手に入るやつが入る感じ
            tempList.Add(new AbilityInLevel()
            {
                level = 0,
                ability = SurviveGameManager.GetInstance().HealAbility,
                statusAttribute = GameDefine.StatusAttribute.NONE,
                statusAttributeValue = 0
            }); ;
            tempList.Add(new AbilityInLevel()
            {
                level = 0,
                ability = SurviveGameManager.GetInstance().CoinAbility,
                statusAttribute = GameDefine.StatusAttribute.NONE,
                statusAttributeValue = 0
            }); ;
            abilitys = tempList;
        }
        else
        {
            abilitys = tempList;
        }

        return abilitys;
    }
    public AbilityData GetAbility(int id)
    {
        return abilityDatas.Find(_ => _.ID == id);
    }



    public List<BoxEvent.AbilityInLevel> GetBoxAbilityDatas()
    {
        List<BoxEvent.AbilityInLevel> abilitys = new List<BoxEvent.AbilityInLevel>();
        List<AbilityData> UseableDatasCopy = new List<AbilityData>(UseableDatas);

        //プレイヤー情報取得
        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;


        int boxID = 0;
        AbilityRarityData.Data boxData = null;
        var sgm = SurviveGameManager.GetInstance();
        for (int x = 0; x < sgm.boxRarityData.data.Length; x++)
        {
            if (sgm.boxRarityData.data[x].LuckLevel > player.battleParameter.Luck)
            {
                boxData = sgm.boxRarityData.data[boxID];
                break;
            }
            boxID = x;
        }
        if (boxData == null)
        {
            boxData = sgm.boxRarityData.data[boxID];
        }


        //ボックス内の個数の抽選
        int boxNum = 1;

        switch (UnityEngine.Random.Range(0f, 100f))
        {
            case float r when r <= boxData.Rate[0]:
                boxNum = 1;
                break;
            case float r when r <= boxData.Rate[1] + boxData.Rate[0]:
                boxNum = 3;
                break;
            case float r when r <= boxData.Rate[2] + boxData.Rate[1] + boxData.Rate[0]:
                boxNum = 5;
                break;
            default:
                boxNum = 1;
                break;
        }
        //boxNum = 5;
        //先に使用可能なアビリティを収集
        List<AbilityData> abiDatas = new List<AbilityData>();
        for (int i = 0; i < UseableDatasCopy.Count; i++)
        {
            int level = 0;
            //特定のキャラの時のみ出現するアビリティ対応
            if (UseableDatasCopy[i].TargetCharacterID != -1 && UseableDatasCopy[i].TargetCharacterID != sgm.currentCharacterID)
            {
                continue;
            }
            if (player.GetAbilityDic.ContainsKey(UseableDatasCopy[i].ID))
            {
                int pLevel = player.GetAbilityDic[UseableDatasCopy[i].ID];
                if (UseableDatasCopy[i].abilityDataType == AbilityData.AbilityDataType.Item)
                {
                    if (UseableDatasCopy[i].ItemLevelDatas.Count > pLevel + 1)
                    {
                        level = pLevel + 1;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {

                    if (UseableDatasCopy[i].WeaponLevelDatas.Count > pLevel + 1)
                    {
                        level = pLevel + 1;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else//もっているもののみ
            {
                continue;
            }

            //利用可能なものリストに入れる
            abiDatas.Add(UseableDatasCopy[i]);

        }

        for (int count = 0; count < boxNum; count++)
        {
            //抽出
            List<BoxEvent.AbilityInLevel> tempList = new List<BoxEvent.AbilityInLevel>();
            int attribute = 0;
            int attributeLevel = 0;

            //これはランダム
            attribute = UnityEngine.Random.Range((int)GameDefine.StatusAttribute.Str, (int)GameDefine.StatusAttribute.NONE);
            //こっちデータ参照

            int beforeX = 0;
            AbilityRarityData.Data data = null;
            for (int x = 0; x < sgm.abilityRarityData.data.Length; x++)
            {
                if (sgm.abilityRarityData.data[x].LuckLevel > player.battleParameter.Luck)
                {
                    data = sgm.abilityRarityData.data[beforeX];
                    break;
                }
                beforeX = x;
            }
            if (data == null)
            {
                data = sgm.abilityRarityData.data[beforeX];
            }

            switch (UnityEngine.Random.Range(0f, 100f))
            {
                case float r when r <= data.Rate[0]:
                    attributeLevel = (int)GameDefine.ABILITY_RARITY.NORMAL;
                    break;
                case float r when r <= data.Rate[1] + data.Rate[0]:
                    attributeLevel = (int)GameDefine.ABILITY_RARITY.RARE;
                    break;
                case float r when r <= data.Rate[2] + data.Rate[1] + data.Rate[0]:
                    attributeLevel = (int)GameDefine.ABILITY_RARITY.EPIC;
                    break;
                case float r when r <= data.Rate[3] + data.Rate[2] + data.Rate[1] + data.Rate[0]:
                    attributeLevel = (int)GameDefine.ABILITY_RARITY.LEGEND;
                    break;
                default:
                    attributeLevel = (int)GameDefine.ABILITY_RARITY.NORMAL;
                    break;
            }

            foreach (var d in  abiDatas)
            {
                
                if (player.GetAbilityDic.ContainsKey(d.ID))
                {
                    int level = 0;

                    
                    foreach(var a in abilitys)
                    {
                        if(a.ability.ID == d.ID && level < a.level + 1)
                        {
                            level = a.level + 1;
                        }
                    }
                    if (level == 0)
                    {
                        int pLevel = player.GetAbilityDic[d.ID];
                        if (d.abilityDataType == AbilityData.AbilityDataType.Item)
                        {
                            if (d.ItemLevelDatas.Count > pLevel + 1)
                            {
                                level = pLevel + 1;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {

                            if (d.WeaponLevelDatas.Count > pLevel + 1)
                            {
                                level = pLevel + 1;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        if (d.abilityDataType == AbilityData.AbilityDataType.Item)
                        {
                            if (d.ItemLevelDatas.Count <= level)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (d.WeaponLevelDatas.Count <= level)
                            {
                                continue;
                            }
                        }
                    }
                    tempList.Add(new BoxEvent.AbilityInLevel() { level = level, ability = d, statusAttribute = (GameDefine.StatusAttribute)attribute, statusAttributeValue = attributeLevel });
                }
            }
            
            if (tempList.Count > 0)
            {
                abilitys.Add(tempList.OrderBy(a => Guid.NewGuid()).ToList().First());
            }
            else
            {
                //金
                abilitys.Add( new BoxEvent.AbilityInLevel()
                {
                    level = 0,
                    ability = SurviveGameManager.GetInstance().BoxCoinAbility,
                    statusAttribute = GameDefine.StatusAttribute.NONE,
                    statusAttributeValue = 0
                });
            }
        }
        
       

        return abilitys;
    }

}
