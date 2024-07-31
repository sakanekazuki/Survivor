using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class SurviveCharacterSelect : MonoBehaviour
{
    [Header("Chara Components")]
    public Image charaImage;
    public Image charaWeapon;
    public GameObject charaUnlock;
    public GameObject charaLock;
    public Animator charaAnimator;
    public TMP_Text atk;
    public TMP_Text hp;
    public TMP_Text speed;
    public TMP_Text dash;
    public TMP_Text crit;
    public TMP_Text[] statusAttribute;
    public LocalizeTextureEvent localizedCharaNameTexture;
    //public LocalizeTextureEvent localizedUnlockCharaNameTexture;
    public LocalizeStringEvent localizeWeaponIntro;
    public TMP_Text coin;
    public TMP_Text cost;

    [Header("Other References")]
    public bool canStartGame;
    public List<SurviveCharaSelectSwitch> selectors;
    [SerializeField] private GameObject unlockWindow;
    [SerializeField] private SurviveTouchButton unlockBtn;

    int selectedCharaCost;
    int selectedCharaId;
    public bool isCharaSelected;
    string localizedSpriteTable = "UI Sprite";
    string localizedTextTable = "UI Text";
    AnimatorOverrideController charaAnimatorOverride;
    CharacterDatas.CharacterData charaData;
    public static Action<int> OnUnlockChara;

    public void SelectChara(int id)
    {
        selectedCharaId = id;
        SurviveGameManager.GetInstance().SetCharaID(id);
        charaData = Array.Find(SurviveGameManager.GetInstance().characterDatas.datas, _ => _.ID == id);
        selectedCharaCost = charaData.COINCOST;
        int coinCount = SurviveProgressManager.Instance.Coin;
        if (selectors.Count > 0)
        {
            selectors.ForEach(s => s.VisibleCoin(id, charaData.COINCOST, coinCount));
        }
        if (SurviveProgressManager.Instance.charaUnlocks.Find(_=>_.ID == id).isLocked)
        {
            SetCharaInfo(false);
            canStartGame = false;
            return;
        }
        SetCharaInfo(true);
        canStartGame = true;
        charaImage.sprite = charaData.charaImage;
        charaWeapon.sprite = charaData.charaWeapon;
        atk.text = charaData.ATTACK.ToString();
        hp.text = charaData.MAX_HP.ToString();
        speed.text = charaData.SPEED.ToString();
        dash.text = charaData.DASH_NUM.ToString();
        crit.text = charaData.CRITICAL.ToString();
        localizedCharaNameTexture.AssetReference.SetReference(localizedSpriteTable, charaData.localizedNameKey);
        //charaAnimatorOverride["AyaneIdle"] = charaData.charaSD;
        localizeWeaponIntro.StringReference.SetReference(localizedTextTable, charaData.localizedWeaponIntroKey);
        SetStatusAttributeStr();

       
    }

    public void SetCharaInfo(bool isOn)
    {
        if(isOn)
        {
            charaImage.sprite = charaData.charaImage;
        }
        else
        {
            charaImage.sprite = charaData.charaLockImage;
        }
        charaLock.SetActive(!isOn);
        charaUnlock.SetActive(isOn);
    }

    #region Unlock Chara
    
    public void OpenUnlock()
    {
        UpdateAndCheckInfo();
        unlockWindow.SetActive(true);
    } 

    public void UpdateAndCheckInfo()
    {
        cost.text = selectedCharaCost.ToString();
        int coinCount = SurviveProgressManager.Instance.Coin;
        coin.text = coinCount.ToString();
        bool canUnlock = coinCount >= selectedCharaCost;
        if (!canUnlock) {
            cost.color = Color.red;
            unlockBtn.Interactable = false;
        } 
        else
        {
            cost.color = Color.white;
            unlockBtn.Interactable = true;
        }
        //localizedUnlockCharaNameTexture.AssetReference.SetReference(localizedSpriteTable, charaData.localizedUnlockNameKey);
    }
    
    public void UnlockChara()
    {
        SurviveProgressManager.Instance.charaUnlocks.Find(_ => _.ID == charaData.ID).isLocked = false;
        SurviveProgressManager.Instance.Coin -= selectedCharaCost;
        SurviveProgressManager.Instance.SaveSurviveProgress();
        OnUnlockChara?.Invoke(selectedCharaId);
        SelectChara(charaData.ID);
        unlockWindow.SetActive(false);
    }
    
    #endregion
    
    private void Start()
    {
        if (charaAnimatorOverride == null)
        {
            charaAnimatorOverride = new AnimatorOverrideController(charaAnimator.runtimeAnimatorController);
            charaAnimator.runtimeAnimatorController = charaAnimatorOverride;
        }
    }

    private void OnEnable()
    {
        isCharaSelected = false;
        if (charaAnimatorOverride == null)
        {
            charaAnimatorOverride = new AnimatorOverrideController(charaAnimator.runtimeAnimatorController);
            charaAnimator.runtimeAnimatorController = charaAnimatorOverride;
        }
        SelectChara(1);

        //test
        //SurviveProgressManager.Instance.charaUnlocks[1].isLocked = true;
        //SurviveProgressManager.Instance.charaUnlocks[2].isLocked = true;
        //SurviveProgressManager.Instance.SaveSurviveProgress();
    }

    private void SetStatusAttributeStr()
    {
        for (int i = 0; i < statusAttribute.Length; i++)
        {
            GameDefine.StatusAttributeStr.TryGetValue(charaData.AttributeStatusLevel[i], out string statusStr);
            statusAttribute[i].text = statusStr;
        }
    }
}
