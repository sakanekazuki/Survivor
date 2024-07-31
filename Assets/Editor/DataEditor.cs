using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
public class DataEditor : EditorWindow
{
    private Editor _editor;
    private ScriptableObject _target;

    private AbilityLocalizeData _localizeData;
    private ShopLevelData _shopLevelData;
    
    

    [MenuItem("Custom/DataEditor")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<DataEditor>();
        window._localizeData = (AbilityLocalizeData)AssetDatabase.LoadAssetAtPath("Assets/SurviveProject/Resources/Datas/Localize/AbilityLocalizeData.asset", typeof(AbilityLocalizeData));
        window._shopLevelData = (ShopLevelData)AssetDatabase.LoadAssetAtPath("Assets/SurviveProject/Resources/Datas/Shop/ShopLevelDatas.asset", typeof(ShopLevelData));
        window.Show();
    }


    private Vector2 _scrollPosition = Vector2.zero;
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        _target = EditorGUILayout.ObjectField("ScriptableObject", _target, typeof(ScriptableObject)) as ScriptableObject;

        if (EditorGUI.EndChangeCheck())
        {
            _editor = Editor.CreateEditor(_target);
        }
        if (_editor == null)
            return;
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        _editor.OnInspectorGUI();

        if(GUILayout.Button("JSON書き出し"))
        {
            if(_target is PlayerEXPData) //とりあえず種類ごとで使えるように　
            {
                //var str = JsonUtility.ToJson(_target as PlayerEXPData);
                //// 保存先のファイルパスを取得する
                //var filePath = EditorUtility.SaveFilePanel("Save", "Assets", "PlayerEXPDataJson", "json");

                //// パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
                //if (!string.IsNullOrEmpty(filePath))
                //{
                //    File.WriteAllText(filePath, str, System.Text.Encoding.UTF8);
                //    AssetDatabase.Refresh();
                //}
                Save<PlayerEXPData>("PlayerEXPDataJson", _target as PlayerEXPData);

            }
            else if (_target is PlayerLevelGainPointData)
            {
                Save<PlayerLevelGainPointData>("PlayerLevelGainPointData", _target as PlayerLevelGainPointData);
            }
            else if (_target is PlayerAttributeNeedPointData)
            {
                Save("PlayerAttributeNeedPointData", _target as PlayerAttributeNeedPointData);
            }
            else if (_target is AbilityRarityData)
            {
                Save("AbilityRarityData", _target as AbilityRarityData);
            }
            else if (_target is AbilityLocalizeData)
            {
                Save("AbilityLocalizeData", _target as AbilityLocalizeData);
            }
            else if(_target is ShopLevelData)
            {
                Save("ShopLevelData", _target as ShopLevelData);
            }
            else if (_target is EnemyGroupDatas)
            {
                Save("EnemyGroupData", _target as EnemyGroupDatas);
            }
            
        }
        if (GUILayout.Button("JSON読み込み"))
        {
            if (_target is PlayerEXPData) //とりあえず種類ごとで使えるように　
            {
                // 保存先のファイルパスを取得する
                //var filePath = EditorUtility.OpenFilePanel("PlayerEXPDataJson", "Assets", "json");

                //// パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
                //if (!string.IsNullOrEmpty(filePath))
                //{
                //    string text = File.ReadAllText(filePath);
                //    var target = _target as PlayerEXPData;
                //    JsonUtility.FromJsonOverwrite(text, target);
                //    //ダーティとしてマークする(変更があった事を記録する)
                //    EditorUtility.SetDirty(target);

                //    //保存する
                //    AssetDatabase.SaveAssets();
                //    AssetDatabase.Refresh();
                //}

                Load<PlayerEXPData>("PlayerEXPDataJson", _target as PlayerEXPData);
            }
            else if (_target is PlayerLevelGainPointData)
            {
                Load<PlayerLevelGainPointData>("PlayerLevelGainPointData", _target as PlayerLevelGainPointData);
            }
            else if (_target is PlayerAttributeNeedPointData)
            {
                Load("PlayerAttributeNeedPointData", _target as PlayerAttributeNeedPointData);
            }
            else if (_target is AbilityRarityData)
            {
                Load("AbilityRarityData", _target as AbilityRarityData);
            }
            else if (_target is AbilityLocalizeData)
            {
                Load("AbilityLocalizeData", _target as AbilityLocalizeData);
            }
            else if (_target is ShopLevelData)
            {
                Load("ShopLevelData", _target as ShopLevelData);
            }//アビリティは特別
            else if (_target is AbilityData)
            {
                AbilityLoad("AbilityData", _target as AbilityData);
            }
            else if (_target is EnemyGroupDatas)
            {
                EnemyGroupLoad("EnemyGroupData", _target as EnemyGroupDatas);
            }
            else if (_target is EnemyDatas)
            {
                EnemyLoad("EnemyDatas", _target as EnemyDatas);
            }
        }
        if (_target is AbilityData)
        {
            if (GUILayout.Button("ローカライズ割り当て"))
            {
                var abiData =  _target as AbilityData;
                if (_localizeData != null && _localizeData.data != null && Array.Exists(_localizeData.data, _ => _.ID == abiData.ID))
                {
                    SetAbilityLocalize(abiData);
                }
            }
        }
        if (_target is ShopItemDatas)
        {
            if (GUILayout.Button("ショップ割り当て"))
            {
                var shopData = _target as ShopItemDatas;
                if (_shopLevelData != null && _shopLevelData.data != null)
                {
                    SetShopItemLocalize(shopData);
                }
            }
        }

        if (GUILayout.Button("☆JSON敵完全読み込み"))
        {
            if (_target is EnemyDatas)
            {
                EnemyAllLoad("EnemyDatas", _target as EnemyDatas);
            }
        }
        EditorGUILayout.EndScrollView();

    }

    void Load<T> (string name, T target) where T : ScriptableObject
    {
        T data;
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.OpenFilePanel("Load", "Assets", "json");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            string text = File.ReadAllText(filePath);
            //var target = _target as T;
            JsonUtility.FromJsonOverwrite(text, target);
            //ダーティとしてマークする(変更があった事を記録する)
            EditorUtility.SetDirty(target);

            //保存する
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return;
    }

    void Save<T>(string name, T target) where T : ScriptableObject
    {
        var str = JsonUtility.ToJson(target);
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.SaveFilePanel("Save", "Assets", name, "json");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, str, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
    void SetAbilityLocalize(AbilityData target)
    {
        var data = Array.Find(_localizeData.data, _ => _.ID == target.ID);
        target.Name = data.Name;
        target.Name_en = data.Name_en;
        target.Name_cn = data.Name_cn;
        target.Name_tw = data.Name_tw;
        target.Description = data.Description;
        target.Description_en = data.Description_en;
        target.Description_cn = data.Description_cn;
        target.Description_tw = data.Description_tw;
        if (target.abilityDataType == AbilityData.AbilityDataType.Item)
        {
            for (int i = 0; i < target.ItemLevelDatas.Count; i++)
            {
                if (i < data.levelDatas.Count)
                {
                    target.ItemLevelDatas[i].Description = data.levelDatas[i].Description;
                    target.ItemLevelDatas[i].Description_en = data.levelDatas[i].Description_en;
                    target.ItemLevelDatas[i].Description_cn = data.levelDatas[i].Description_cn;
                    target.ItemLevelDatas[i].Description_tw = data.levelDatas[i].Description_tw;
                }
            }
        }
        else
        {
            for (int i = 0; i < target.WeaponLevelDatas.Count; i++)
            {
                if (i < data.levelDatas.Count)
                {
                    target.WeaponLevelDatas[i].Description = data.levelDatas[i].Description;
                    target.WeaponLevelDatas[i].Description_en = data.levelDatas[i].Description_en;
                    target.WeaponLevelDatas[i].Description_cn = data.levelDatas[i].Description_cn;
                    target.WeaponLevelDatas[i].Description_tw = data.levelDatas[i].Description_tw;
                }
            }
        }
        //ダーティとしてマークする(変更があった事を記録する)
        EditorUtility.SetDirty(target);

        //保存する
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void SetShopItemLocalize(ShopItemDatas shopData)
    {
        for(int i = 0; i < _shopLevelData.data.Length; i++)
        {
            shopData.datas[i].ID = _shopLevelData.data[i].ID;
            shopData.datas[i].levelUpValue = _shopLevelData.data[i].levelUpValue;
            shopData.datas[i].localizedKey = _shopLevelData.data[i].localizedKey;
            for (int j = 0; j < _shopLevelData.data[i].levelDatas.Count; j++)
            {
                var item = new ShopItemDatas.ShopItemData.ShopItemLevelData();
                item.level = _shopLevelData.data[i].levelDatas[j].level;
                item.price = _shopLevelData.data[i].levelDatas[j].price;
                shopData.datas[i].shopItemLevelDatas.Add(item);
            }
        }
        //ダーティとしてマークする(変更があった事を記録する)
        EditorUtility.SetDirty(shopData);

        //保存する
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }



    void AbilityLoad(string name, AbilityData target)
    {
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.OpenFilePanel("Load", "Assets","");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            string text = File.ReadAllText(filePath);
            //var target = _target as T;
            var json = JsonUtility.FromJson<SurviveGameManager.AbilityDataJson>(text);
            target.ItemLevelDatas = json.itemLevelDatas;
            target.WeaponLevelDatas = json.weaponLevelDatas;

            //JsonUtility.FromJsonOverwrite(text, target);
            //ダーティとしてマークする(変更があった事を記録する)
            EditorUtility.SetDirty(target);

            //保存する
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return;
    }
    void EnemyGroupLoad(string name, EnemyGroupDatas target)
    {
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.OpenFilePanel("Load", "Assets", "");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            string text = File.ReadAllText(filePath);
            //var target = _target as T;
            var json = JsonUtility.FromJson<MoreMountains.TopDownEngine.SurviveLevelManager.EnemyDataGroupJson>(text);
            target.datas = json.datas;

            //JsonUtility.FromJsonOverwrite(text, target);
            //ダーティとしてマークする(変更があった事を記録する)
            EditorUtility.SetDirty(target);

            //保存する
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return;
    }
    void EnemyLoad(string name, EnemyDatas target)
    {
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.OpenFilePanel("Load", "Assets", "");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            string text = File.ReadAllText(filePath);
            //var target = _target as T;
            var json = JsonUtility.FromJson<MoreMountains.TopDownEngine.SurviveLevelManager.EnemyDataJson>(text);

            for (int i = 0; i < json.datas.Length; i++)
            {
                if(target.datas.Length > i && target.datas[i].ID == json.datas[i].ID)
                {
                    json.datas[i].Prefab = target.datas[i].Prefab;
                }
            }
            target.datas = json.datas;

            //JsonUtility.FromJsonOverwrite(text, target);
            //ダーティとしてマークする(変更があった事を記録する)
            EditorUtility.SetDirty(target);

            //保存する
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return;
    }
    void EnemyAllLoad(string name, EnemyDatas target)
    {
        // 保存先のファイルパスを取得する
        var filePath = EditorUtility.OpenFilePanel("Load", "Assets", "");

        // パスが入っていれば選択されたということ（キャンセルされたら入ってこない）
        if (!string.IsNullOrEmpty(filePath))
        {
            string text = File.ReadAllText(filePath);
            //var target = _target as T;
            var json = JsonUtility.FromJson<MoreMountains.TopDownEngine.SurviveLevelManager.EnemyDataJson>(text);

           
            target.datas = json.datas;

            //JsonUtility.FromJsonOverwrite(text, target);
            //ダーティとしてマークする(変更があった事を記録する)
            EditorUtility.SetDirty(target);

            //保存する
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return;
    }
}