using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using static LevelUpEvent;
using System.Linq;


//�A�r���e�B�I���̓��e�̌���Ȃ�
[Serializable]
public class AbilitySelectSystem
{
    //�A�r���e�B�̊J���󋵓�����������K�v����

    [SerializeField]
    public List<AbilityData> abilityDatas;


    //����g�p�\�ȃf�[�^
    public List<AbilityData> UseableDatas;


    //�����Ԏ擾���ď�����
    public void Initiralize()
    {
        //�Ƃ肠�������̂܂�
        UseableDatas = SurviveGameManager.GetInstance().GetAbilitysCopy();
    }


    public List<AbilityInLevel> GetSelectAbilityDatas()
    {
        List<AbilityInLevel> abilitys = new List<AbilityInLevel>();
        List<AbilityData> UseableDatasCopy = new List<AbilityData>(UseableDatas);
        //�v���C���[���擾

        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        var sgm = SurviveGameManager.GetInstance();
        //�Ƃ肠�����I���\�Ȃ��̂𒊏o
        List<AbilityInLevel> tempList = new List<AbilityInLevel>();
        for(int i = 0; i < UseableDatasCopy.Count; i++)
        {
            int level = 0; 
            //����̃L�����̎��̂ݏo������A�r���e�B�Ή�
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
            //��ނł��ő�Ŏ��Ă���͂���̂�
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
            //�X�L�����ǂ����̔��� �������񂷂���łȂ��Ă�
            if (true)
            {
                //if (level == 0)
                {
                    //����̓����_��
                    attribute = UnityEngine.Random.Range((int)GameDefine.StatusAttribute.Str, (int)GameDefine.StatusAttribute.NONE);
                    //�������f�[�^�Q��

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
            //���C�����킪�o�₷���悤�ɂȂ�
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
            //�����Ƃ�����ɓ��������銴��
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

        //�v���C���[���擾
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


        //�{�b�N�X���̌��̒��I
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
        //��Ɏg�p�\�ȃA�r���e�B�����W
        List<AbilityData> abiDatas = new List<AbilityData>();
        for (int i = 0; i < UseableDatasCopy.Count; i++)
        {
            int level = 0;
            //����̃L�����̎��̂ݏo������A�r���e�B�Ή�
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
            else//�����Ă�����̂̂�
            {
                continue;
            }

            //���p�\�Ȃ��̃��X�g�ɓ����
            abiDatas.Add(UseableDatasCopy[i]);

        }

        for (int count = 0; count < boxNum; count++)
        {
            //���o
            List<BoxEvent.AbilityInLevel> tempList = new List<BoxEvent.AbilityInLevel>();
            int attribute = 0;
            int attributeLevel = 0;

            //����̓����_��
            attribute = UnityEngine.Random.Range((int)GameDefine.StatusAttribute.Str, (int)GameDefine.StatusAttribute.NONE);
            //�������f�[�^�Q��

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
                //��
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
