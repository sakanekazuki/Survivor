using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;
using static ShopItemDatas;

public class SurviveShop : MonoBehaviour
{
    [SerializeField] TMP_Text coinText;
    [SerializeField] Image itemIntroIcon;
    [SerializeField] TMP_Text itemCoinCost;
    [SerializeField] LocalizeStringEvent localizedShopItem;
    [SerializeField] ScrollRect shopItemContent;
    public ShopItemDatas shopItemDatas;
    public List<SurviveShopItem> shopItems = new List<SurviveShopItem>();
    
    public static Action<int> OnUpgrade;
    public static Action OnReset;
    string localizedTextTable = "ShopItem";
    ShopItemData shopItemData;
    int selectedID;
    

    public void SelectItem(int id)
    {
        selectedID = id;
        shopItemData = shopItemDatas.GetShopItemData(selectedID);
        itemIntroIcon.sprite = shopItemData.icon;
        UpdateCoinCost();
        CheckCanBuy();
        localizedShopItem.StringReference.SetReference(localizedTextTable, shopItemData.localizedKey);
        UpdateEffectValue();
    }

    public void InitItemIcon()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].SetLevelLimitBG(shopItemDatas.datas[i].shopItemLevelDatas.Count);
            shopItems[i].SetLevelIcon();
        }
    }

    private void OnEnable()
    {
        UpdateCoin();
        InitItemIcon();
        
        //Set default item ID 1 and reset scroll view pos
        shopItemContent.verticalNormalizedPosition = 1;
        SelectItem(1);
    }

    private void Awake()
    {
        InitShopItemID();
    }
    
    private void InitShopItemID()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].ID = shopItemDatas.datas[i].ID;
            shopItems[i].SetItemIcon(shopItemDatas.datas[i].icon);
        }
    }

    public void UpGrade()
    {
        //Å‘åƒŒƒxƒ‹‚ÌÝ’è“™‚µ‚Ä‚¨‚­
        var levels = SurviveProgressManager.Instance.permanentEffectLevels;
        if (!CheckCanBuy()) return;
        //check if can upgrade with coin
        if (levels.Count > selectedID)
        {
            if (levels[selectedID] < shopItemDatas.GetShopItemData(selectedID).shopItemLevelDatas.Count)
            {
                levels[selectedID]++;
                SurviveProgressManager.Instance.Coin -= shopItemDatas.GetShopItemData(selectedID).shopItemLevelDatas[levels[selectedID] - 1].price;
                UpdateCoin();
            }  
            else
                return;
        }
        SaveData();
        UpdateCoinCost();
        UpdateEffectValue();
        CheckCanBuy();
        OnUpgrade?.Invoke(selectedID);
    }

    public void ResetGrade()
    {
        //Å‘åƒŒƒxƒ‹‚ÌÝ’è“™‚µ‚Ä‚¨‚­
        var levels = SurviveProgressManager.Instance.permanentEffectLevels;
        var allSpendCoin = GetTotalSpendCoin();
        for (int i = 0; i < levels.Count; i++)
        {
            levels[i] = 0;
        }
        SurviveProgressManager.Instance.Coin += allSpendCoin;
        SaveData();
        UpdateCoin();
        UpdateCoinCost();
        UpdateEffectValue();
        OnReset?.Invoke();
    }

    //calcute all spend coin
    public int GetTotalSpendCoin()
    {
        var levels = SurviveProgressManager.Instance.permanentEffectLevels;
        int total = 0;
        for (int i = 0; i < levels.Count; i++)
        {
            if (i == 0) continue;
            for (int j = 0; j < levels[i]; j++)
            {
                if (levels[i] == 0)
                    break;
                total += shopItemDatas.GetShopItemData(i).shopItemLevelDatas[j].price;
            }
        }
        return total;
    }

    //check if can buy with coin
    public bool CheckCanBuy()
    {
        var levels = SurviveProgressManager.Instance.permanentEffectLevels;
        if (levels.Count > selectedID)
        {
            if (levels[selectedID] < shopItemDatas.GetShopItemData(selectedID).shopItemLevelDatas.Count)
            {
                var price = shopItemDatas.GetShopItemData(selectedID).shopItemLevelDatas[levels[selectedID]].price;
                if (SurviveProgressManager.Instance.Coin >= price)
                {
                    itemCoinCost.color = Color.white;
                    return true;
                }
            }
        }
        itemCoinCost.color = Color.red;
        return false;
    }

    public virtual void SaveData()
    {
        SurviveSaveEvent.Trigger();
    }

    private void UpdateEffectValue()
    {
        int offsettedindex;
        if (SurviveProgressManager.Instance.permanentEffectLevels[selectedID] == shopItemData.shopItemLevelDatas.Count)
        { 
            offsettedindex = shopItemData.shopItemLevelDatas.Count - 1;
        }
        else
        {
            offsettedindex = SurviveProgressManager.Instance.permanentEffectLevels[selectedID];
        }
            
        localizedShopItem.StringReference["itemPara"] = new FloatVariable()
        {
            Value = shopItemData.levelUpValue * shopItemData.shopItemLevelDatas[offsettedindex].level
        };
        localizedShopItem.StringReference.RefreshString();
    }

    private void UpdateCoin()
    {
        coinText.text = SurviveProgressManager.Instance.Coin.ToString();
    }

    private void UpdateCoinCost()
    {
        CheckCanBuy();
        if (SurviveProgressManager.Instance.permanentEffectLevels[selectedID] == shopItemData.shopItemLevelDatas.Count)
            itemCoinCost.text = "MAX";
        else
            itemCoinCost.text = shopItemData.shopItemLevelDatas[SurviveProgressManager.Instance.permanentEffectLevels[selectedID]].price.ToString();
    }

    public void AddCoin()
    {
        SurviveProgressManager.Instance.Coin += 10000;
        SaveData();
        UpdateCoin();
    }

    public void ResetCoin()
    {
        SurviveProgressManager.Instance.Coin = 0;
        SaveData();
        UpdateCoin();
    }
}
