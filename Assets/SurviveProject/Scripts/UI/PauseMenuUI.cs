using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuUI : MonoBehaviour
{
    public List<LevelUpUI.StatusAssignUI> statusAssigns = new List<LevelUpUI.StatusAssignUI>();
    public List<LevelUpUI.StatusUI> statusUIs = new List<LevelUpUI.StatusUI>();
    public TextMeshProUGUI pointText;
    public GameObject FirstButton;

    public GameObject SettingMenuObj;
    public SurviveSettingUI uiSetting;

    public GameObject StatusObj;
    public GameObject MenuObj;


    public LocalizeFontTMProManager localizeFontTMProManager;

    public GameObject SettingCloseButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

       
        if( (SurviveInputManager.Instance as SurviveInputManager).GetInputMode()== SurviveInputManager.InputMode.KeyBoardMouse)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
        if (SurviveGameManager.GetInstance().OpendSettingUI &&
            (SurviveInputManager.Instance as SurviveInputManager).CancelButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
        {
            SettingClose();
          
        }
    }
    
    public void SettingClose()
    {
        SurviveGameManager.GetInstance().OpendSettingUI = false;
        SettingMenuObj.SetActive(false);
        //uiSetting;
        //StatusObj.SetActive(true);
        MenuObj.SetActive(true);
        //全フォントアセット更新
        localizeFontTMProManager.UpdateFont();
    }

    public void SetData()
    {
        StatusUpdate();



        EventSystem.current.firstSelectedGameObject = FirstButton;
    }

    public void StatusUpdate()
    {
        var sgm = SurviveGameManager.GetInstance();
        var slm = SurviveLevelManager.GetInstance();
        var player = slm.Players[0] as SurvivePlayer;
        pointText.text = player.battleParameter.ReservePoint.ToString();


        List<int> temp = new List<int>(player.battleParameter.StatusAttributeLevels);
        List<int> tempAbi = new List<int>(player.battleParameter.AbilityAttributeLevels);
        //ステータス計算
        for (int i = 0; i < temp.Count; i++)
        {
            temp[i] += tempAbi[i];
        }
        var calcedData = player.CalcAttributionStatus(temp);

        var attributeList = player.battleParameter.StatusAttributeLevels;
        var DefaultAttributeList = player.battleParameter.DefaultStatusAttributeLevels;
        for (int i = 0; i < statusUIs.Count; i++)
        {
            if (i == (int)GameDefine.StatusCorrection.HP)
            {
                (int current, int max) = player.GetHP();
                statusUIs[i].Value.text = current.ToString() + "/" + max.ToString();
            }
            else
            {
                statusUIs[i].Value.text = "+" + Mathf.FloorToInt(calcedData[i] * 100f).ToString() + "%";
            }
        }
        var needPDatas = sgm.playerAttributeNeedPointData.data;

        for (int i = 0; i < statusAssigns.Count; i++)
        {
            statusAssigns[i].CurrentPoint.text = (attributeList[i]).ToString();

            statusAssigns[i].AddPoint.text = ( tempAbi[i]).ToString();


            statusAssigns[i].CurrentRank.text = GameDefine.GetAttributeLevelStr(attributeList[i], DefaultAttributeList[i]);
        }
    }

    public void OpenSetting()
    {
        SettingMenuObj.SetActive(true);
        //uiSetting;
        //StatusObj.SetActive(false);
        MenuObj.SetActive(false);

        SurviveGameManager.GetInstance().OpendSettingUI = true;
        EventSystem.current.firstSelectedGameObject = SettingCloseButton;


    }
}
