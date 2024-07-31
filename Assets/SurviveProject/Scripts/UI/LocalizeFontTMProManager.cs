using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalizeFontTMProManager : MonoBehaviour
{
    [SerializeField] public TMP_FontAsset jpFont;
    [SerializeField] public TMP_FontAsset twFont;
    [SerializeField] public TMP_FontAsset cnFont;
    [SerializeField] public TMP_FontAsset enFont;

    [MMInspectorButton("SetAll")]
    public bool setButton;
    public List<TextMeshProUGUI> textMeshProUGUIs = new List<TextMeshProUGUI>();
    private void Awake()
    {
        TMP_FontAsset current;

        switch (LocalizationSettings.SelectedLocale.Formatter.ToString())
        {
            case "ja":
                current = jpFont;
                break;
            case "en":
                current = enFont;
                break;
            case "zh-TW":
                current = twFont;
                break;
            case "zh-CN":
                current = cnFont;
                break;
            default:
                current = jpFont;
                break;
        }
        UpdateAll(current);
    }

 
    private void UpdateAll(TMP_FontAsset f)
    {
        foreach (var tmp in textMeshProUGUIs)
        {
            if (tmp)
            {
                tmp.font = f;
            }
        }
    }

    public void UpdateFont()
    {
        TMP_FontAsset current;

        switch (LocalizationSettings.SelectedLocale.Formatter.ToString())
        {
            case "ja":
                current = jpFont;
                break;
            case "en":
                current = enFont;
                break;
            case "zh-TW":
                current = twFont;
                break;
            case "zh-CN":
                current = cnFont;
                break;
            default:
                current = jpFont;
                break;
        }
        UpdateAll(current);
    }

#if UNITY_EDITOR
    public void SetAll()
    {
        textMeshProUGUIs = new List<TextMeshProUGUI>();
        var target = Resources.FindObjectsOfTypeAll<GameObject>() //GameObjectÇëSÇƒéÊìæ
.Where(go => AssetDatabase.GetAssetOrScenePath(go).Contains(".unity")); //Hierarchyè„ÇÃÇ‡ÇÃÇæÇØëIï 
        foreach (GameObject obj in target)
        {
            var tmp = obj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    textMeshProUGUIs.Add(tmp);
                }
        }
    }
#endif
}