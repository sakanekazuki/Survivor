using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CreateDatas/ShopItemDatas")]
public class ShopItemDatas : ScriptableObject
{
    [Serializable]
    public class ShopItemData
    {
        public int ID;
        public float levelUpValue;
        public string localizedKey;
        public Sprite icon;

        public List<ShopItemLevelData> shopItemLevelDatas = new List<ShopItemLevelData>();
        
        [Serializable]
        public class ShopItemLevelData
        {
            public int level;
            public int price;
            
        }
    }

    public ShopItemData[] datas;

    public ShopItemData GetShopItemData(int ID)
    {
        return datas[ID - 1];
    }
}

