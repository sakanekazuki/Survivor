using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;

public class LevelUpUI : MonoBehaviour
{
    public List<AbilityUI> abilityUIs = new List<AbilityUI>();
    public List<StatusAssignUI> statusAssigns = new List<StatusAssignUI>();
    public List<StatusUI> statusUIs = new List<StatusUI>();
    public TextMeshProUGUI pointText;
    public Button submit;
    public Button reroll;
    public TextMeshProUGUI rerollCount;
    private SurviveGameManager sgm;
    public bool initialized = false;
    public bool opened = false;
    [Serializable]
    public class AbilityUI
    {
        public GameObject Base;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Description;
        public Image image ;
        public TextMeshProUGUI Type;
        public TextMeshProUGUI Attribute;
        public TextMeshProUGUI AttributeValue;
    }

    [Serializable]
    public class StatusAssignUI
    {
        public GameObject Base;
        public TextMeshProUGUI CurrentPoint;
        public TextMeshProUGUI NeedPoint;
        public TextMeshProUGUI AddPoint; //アビリティで追加されている分
        public TextMeshProUGUI CurrentRank;
        public TextMeshProUGUI UpRank;

        public Button MinusBtn;
        public Button PlusBtn;
    }

    [Serializable]
    public class StatusUI
    {
        public GameObject Base;
        public TextMeshProUGUI Value;
        public Image image;
    }



    private LevelUpEvent CurrentEventData;

    private int CurrentAbilityID = -1;

    private List<int> CurrentAddStatusPointList = new List<int>();
    private int CurrentStatusPoint = 0;

    private List<int> AddStatusPointList = new List<int>();

    public Sprite NormalAbilityFrame;
    public Sprite NormalAbilityFramePush;
    public Sprite RareAbilityFrame;
    public Sprite RareAbilityFramePush;
    public Sprite EpicAbilityFrame;
    public Sprite EpicAbilityFramePush;
    public Sprite LegendAbilityFrame;
    public Sprite LegendAbilityFramePush;


    public Image exclamation;
    public GameObject SubmitButtonPoints;
    public void Inititalize()
    {
        sgm =  SurviveGameManager.GetInstance();
        initialized = true;

        for (int i = 0; i < abilityUIs.Count; i++)
        {
            int set = i;
            abilityUIs[i].Base.GetComponent<Button>().onClick.AddListener(() => SetAbility(set));
            abilityUIs[i].Base.GetComponent<Image>().color = Color.white;
        }

        for (int i = 0; i < statusAssigns.Count; i++)
        {
            int set = i;
            statusAssigns[i].PlusBtn.onClick.AddListener(() => AddStatusPoint(set, 1));
            statusAssigns[i].MinusBtn.onClick.AddListener(() => AddStatusPoint(set, -1));
        }

        submit.onClick.AddListener(() => Submit());
        reroll.onClick.AddListener(() => Reroll());

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized || !opened)
            return;
    }
    public AudioClip openSE;

    public void Open(LevelUpEvent eventData)
    {

        gameObject.SetActive(true);
        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
        options.Loop = false;
        options.Location = Vector3.zero;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
       /* var source =*/ MMSoundManagerSoundPlayEvent.Trigger(openSE, options);

        //MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Pause, 9999);
        if (!initialized)
        {
            Inititalize();
        }
        // if time is not already stopped		
        if (Time.timeScale > 0.0f)
        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
            SurviveGameManager.GetInstance().OpendLevelUpUI = true;
            SurviveGameManager.Instance.Paused = true;
            //if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
            //{
            //GUIManager.Instance.SetPauseScreen(true);
            //    _pauseMenuOpen = true;
            //    SetActiveInventoryInputManager(false);
            //}
            //if (pauseMethod == PauseMethods.NoPauseMenu)
            //{
            //    _inventoryOpen = true;
            //}
            LevelManager.Instance.ToggleCharacterPause();
        }

        CurrentEventData = eventData;
        //CurrentAbilityID = -1;
        CurrentAddStatusPointList = new List<int>();
        CurrentStatusPoint = CurrentEventData.Point;
        AddStatusPointList = new List<int>(new int[(int)GameDefine.StatusAttribute.NONE]);
        var rerollNumber = CurrentEventData.Player.battleParameter.ReserveReroll;
        if(rerollNumber == 0)
        {
            rerollCount.color = Color.red;
        }
        else
        {
            rerollCount.color = Color.black;
        }
		rerollCount.text = rerollNumber.ToString();

        if(CurrentEventData.Player.battleParameter.ReserveReroll <= 0)
        {
            reroll.interactable = false;
        }
        else
        {
            reroll.interactable = true;
        }
        for (int i = 0; i < 4; i++)
        {
            if (CurrentEventData.abilityDatas.Count <= i)
            {
                abilityUIs[i].Base.SetActive(false);
            }
            else
            {

                var data = CurrentEventData.abilityDatas[i];
                //アイテム用のデータ参照
                if (data.ability.abilityDataType == AbilityData.AbilityDataType.Item)
                {
                    var targetlevelItem = data.ability.ItemLevelDatas[data.level];
                    abilityUIs[i].Base.SetActive(true);
                    abilityUIs[i].Name.text = data.ability.GetLocalizeName() + (data.level+1 == data.ability.ItemLevelDatas .Count? " <color=#ffd700>Lv.MAX</color>" : " Lv."+(data.level+1).ToString());
                    abilityUIs[i].image.sprite = data.ability.image;

                    abilityUIs[i].Description.text = targetlevelItem.GetLocalizeDescription();
                    abilityUIs[i].Type.text = "Relic";
                    abilityUIs[i].Attribute.text = data.statusAttribute == GameDefine.StatusAttribute.NONE ? "" : data.statusAttribute.ToString();
                    abilityUIs[i].AttributeValue.text = data.statusAttributeValue == 0?"":"+" + data.statusAttributeValue.ToString();
                }//武器スキル用のデータ参照
                else
                {


                    var targetlevel = data.ability.WeaponLevelDatas[data.level];
                    abilityUIs[i].Name.text = data.ability.GetLocalizeName() + (data.level + 1 == data.ability.WeaponLevelDatas.Count ? " <color=#ffd700>Lv.MAX</color>" : " Lv." + (data.level + 1).ToString());
                    abilityUIs[i].Base.SetActive(true);
                    abilityUIs[i].Description.text = targetlevel.GetLocalizeDescription();
                    abilityUIs[i].image.sprite = data.ability.image;
                    abilityUIs[i].Type.text = "Weapon";
                    abilityUIs[i].Attribute.text = data.statusAttribute == GameDefine.StatusAttribute.NONE ? "" : data.statusAttribute.ToString();
                    abilityUIs[i].AttributeValue.text = data.statusAttributeValue == 0 ? "" : "+" + data.statusAttributeValue.ToString();
                }
                Color color = Color.white;
                switch (data.statusAttributeValue)
                {
                    case (int)GameDefine.ABILITY_RARITY.NORMAL:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = NormalAbilityFrame;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(NormalAbilityFrame);
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().DisabledSprite = NormalAbilityFrame;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().HighlightedSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().PressedSprite = NormalAbilityFramePush;

                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.NORMAL], out color);
                        abilityUIs[i].Attribute.color = color;
                        abilityUIs[i].AttributeValue.color = color; 
                        break;
                    case (int)GameDefine.ABILITY_RARITY.RARE:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = RareAbilityFrame;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = RareAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(RareAbilityFrame);
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().DisabledSprite = RareAbilityFrame;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().HighlightedSprite = RareAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().PressedSprite = RareAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.RARE], out color);
                        abilityUIs[i].Attribute.color = color;
                        abilityUIs[i].AttributeValue.color = color;
                        break;
                    case (int)GameDefine.ABILITY_RARITY.EPIC:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = EpicAbilityFrame;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = EpicAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(EpicAbilityFrame);
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().DisabledSprite = EpicAbilityFrame;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().HighlightedSprite = EpicAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().PressedSprite = EpicAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.EPIC], out color);
                        abilityUIs[i].Attribute.color = color;
                        abilityUIs[i].AttributeValue.color = color;
                        break;
                    case (int)GameDefine.ABILITY_RARITY.LEGEND:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = LegendAbilityFrame;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = LegendAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(LegendAbilityFrame);
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().DisabledSprite = LegendAbilityFrame;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().HighlightedSprite = LegendAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().PressedSprite = LegendAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.LEGEND], out color);
                        abilityUIs[i].Attribute.color = color;
                        abilityUIs[i].AttributeValue.color = color;
                        break;
                   default:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = NormalAbilityFrame;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(NormalAbilityFrame);
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().DisabledSprite = NormalAbilityFrame;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().HighlightedSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<SurviveTouchButton>().PressedSprite = NormalAbilityFramePush;

                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.NORMAL], out color);
                        abilityUIs[i].Attribute.color = color;
                        abilityUIs[i].AttributeValue.color = color;
                        break;
                }
                var sn = new Navigation();
                var rn = new Navigation();

                sn.mode = Navigation.Mode.Explicit;
                rn.mode = Navigation.Mode.Explicit;
                sn.selectOnUp = abilityUIs[i].Base.GetComponent<Button>();
    
                rn.selectOnUp = abilityUIs[i].Base.GetComponent<Button>();
                sn.selectOnLeft = reroll;
                rn.selectOnRight = submit;

                submit.navigation = sn;
                reroll.navigation = rn;
            }
        }
        SetAbility(-1);

        //StatusUpdate();
        opened = true;

        EventSystem.current.SetSelectedGameObject(abilityUIs[0].Base);
		EventSystem.current.sendNavigationEvents = true;


    }
    public void Close()
    { 
        gameObject.SetActive(false);

        {
            MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
            SurviveGameManager.GetInstance().OpendLevelUpUI = false;
            SurviveGameManager.Instance.Paused = false;
            LevelManager.Instance.ToggleCharacterPause();
        }
        opened = false;
        //MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Resume, 9999);
        EventSystem.current.sendNavigationEvents = false;

    }

    public void TestOpen()
    {
        Debug.Log("てすと　レベルあっぷ");
        var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetSelectAbilityDatas();
        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        player.battleParameter.Level++;
        var level = player.battleParameter.Level;
        //今回のレベルに応じたポイントに余っているものを足す
        int point = 10 + player.battleParameter.ReservePoint;
        LevelUpEvent.Trigger(datas, level, point, player, () =>
        {

        });

    }
    public void TestOpen2(int id)
    {
        Debug.Log("てすと　レベルあっぷ");
        var datas = new List<LevelUpEvent.AbilityInLevel>() { new LevelUpEvent.AbilityInLevel() { ability = SurviveGameManager.GetInstance().abilitySelectSystem.UseableDatas.Find(_ => _.ID == id), level = 0, statusAttribute = GameDefine.StatusAttribute.NONE, statusAttributeValue = 0 } };
        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        player.battleParameter.Level++;
        var level = player.battleParameter.Level;
        //今回のレベルに応じたポイントに余っているものを足す
        int point = 10 + player.battleParameter.ReservePoint;
        LevelUpEvent.Trigger(datas, level, point, player, () =>
        {

        });

    }








    public void SetAbility(int target )
    {
        CurrentAbilityID = target;
        for (int i = 0; i < abilityUIs.Count; i++)
        {
            if(i == target)
            {
                abilityUIs[i].Base.GetComponent<ChangeImage>().Change(true);
                abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite);
                abilityUIs[i].Description.color = Color.black;
                abilityUIs[i].Name.color = Color.white;
                abilityUIs[i].image.color = Color.white;
                abilityUIs[i].Type.color = Color.white;
                abilityUIs[i].Attribute.color = Color.white;
                abilityUIs[i].AttributeValue.color = Color.white;
            }
            else
            {
                abilityUIs[i].Base.GetComponent<ChangeImage>().Change(false);
                abilityUIs[i].Base.GetComponent<SurviveTouchButton>().SetInitialSprite(abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite);
                abilityUIs[i].Description.color = Color.gray;
                abilityUIs[i].Name.color = Color.gray;
                abilityUIs[i].image.color = Color.gray;
                abilityUIs[i].Type.color = Color.gray;
                abilityUIs[i].Attribute.color = Color.gray;
                abilityUIs[i].AttributeValue.color = Color.gray;
            }
        }

        //StatusUpdate();
    }

    public void AddStatusPoint(int target, int i)
    {
        var needPDatas = sgm.playerAttributeNeedPointData.data;
        var attributeList = CurrentEventData.Player.battleParameter.StatusAttributeLevels;
        //var abilityAttributeList = CurrentEventData.Player.battleParameter.AbilityAttributeLevels;
        var nextLevel = attributeList[target] + AddStatusPointList[target] + (i>0?i:0);
        if(nextLevel <= 0)
        {
            return;
        }
        //ステータス割り振り必要ポイント参照
        var needpoint = needPDatas[needPDatas.Length - 1].level < nextLevel ? needPDatas[needPDatas.Length - 1].point : Array.Find(needPDatas, _ => _.level == nextLevel).point;

        if (i > 0 && CurrentStatusPoint < needpoint)
        {
            return;
        }
        if(i < 0 && AddStatusPointList[target] == 0)
        {
            return;
        }

        //減らすときは前の値を参照するように
        CurrentStatusPoint -= needpoint * i;
        AddStatusPointList[target] += i;

        //StatusUpdate();
    }

    public void StatusUpdate()
    {
        pointText.text = CurrentStatusPoint.ToString();



        List<int> temp = new List<int>( CurrentEventData.Player.battleParameter.StatusAttributeLevels);
        List<int> tempAbi = new List<int>( CurrentEventData.Player.battleParameter.AbilityAttributeLevels);
        //ステータス計算
        for (int i = 0; i <  temp.Count; i++)
        {
            int abilityPoint = 0;
            if (CurrentAbilityID != -1 && /*CurrentEventData.abilityDatas[CurrentAbilityID].level == 0 && */(int)CurrentEventData.abilityDatas[CurrentAbilityID].statusAttribute == i)
            {

                abilityPoint = CurrentEventData.abilityDatas[CurrentAbilityID].statusAttributeValue;
            }
            temp[i] += AddStatusPointList[i] + abilityPoint+ tempAbi[i];
        }
        var calcedData = CurrentEventData.Player.CalcAttributionStatus(temp);

        var attributeList = CurrentEventData.Player.battleParameter.StatusAttributeLevels;
        var DefaultAttributeList = CurrentEventData.Player.battleParameter.DefaultStatusAttributeLevels;
        for (int i = 0; i < statusUIs.Count; i++)
        {
            if(i == (int)GameDefine.StatusCorrection.HP)
            {
                (int current, int max) = CurrentEventData.Player.GetHP();
                //マックス値は計算した値にする

                //shopの対応が済み次第補正値調整
                max = Mathf.FloorToInt(CurrentEventData.Player.battleParameter.BaseData.MAX_HP + CurrentEventData.Player.battleParameter.BaseData.MAX_HP * (CurrentEventData.Player.battleParameter.BasePermanentEffectLevels[(int)SurviveProgressManager.PermanentEffect.MaxHP] * 0.03f + CurrentEventData.Player.battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.HP])+ calcedData[i]);
               
                statusUIs[i].Value.text = current.ToString() +"/"+ max.ToString();
            }
            else
            {
                statusUIs[i].Value.text = "+"+Mathf.FloorToInt(calcedData[i] * 100f).ToString()+"%";
            }
        }
        var needPDatas = sgm.playerAttributeNeedPointData.data;

        for (int i = 0; i < statusAssigns.Count; i++)
        {
            statusAssigns[i].CurrentPoint.text = (attributeList[i] + AddStatusPointList[i]).ToString();
            if(AddStatusPointList[i] > 0)
            {
                statusAssigns[i].CurrentPoint.color = Color.green;
            }
            else
            {
                statusAssigns[i].CurrentPoint.color = Color.white;
            }
            //レベルアップ時の値を参照0->1になる際は１の値を見る
            var needpoint = needPDatas[needPDatas.Length -1].level < (attributeList[i] + AddStatusPointList[i]+1) ? needPDatas[needPDatas.Length - 1].point :  Array.Find(needPDatas, _ => _.level == attributeList[i] + AddStatusPointList[i] + 1).point;
            statusAssigns[i].NeedPoint.text = needpoint.ToString();

            int abilityPoint = 0;
            if(CurrentAbilityID != -1 && /*CurrentEventData.abilityDatas[CurrentAbilityID].level == 0 &&*/ (int)CurrentEventData.abilityDatas[CurrentAbilityID].statusAttribute == i)
            {

                abilityPoint = CurrentEventData.abilityDatas[CurrentAbilityID].statusAttributeValue;
            }

            statusAssigns[i].AddPoint.text = (abilityPoint + tempAbi[i]).ToString();

            
            statusAssigns[i].CurrentRank.text = GameDefine.GetAttributeLevelStr(attributeList[i]+ tempAbi[i], DefaultAttributeList[i]);
            statusAssigns[i].UpRank.text = GameDefine.GetAttributeLevelStr(temp[i], DefaultAttributeList[i]);
        }



        if (CurrentStatusPoint > 0)
        {
            exclamation.enabled = false;
            SubmitButtonPoints.SetActive(false);
            for (int i = 0; i < attributeList.Count; i++)
            {
                //var abilityAttributeList = CurrentEventData.Player.battleParameter.AbilityAttributeLevels;
                var nextLevel = attributeList[i] + AddStatusPointList[i] + 1;
                if (nextLevel <= 0)
                {
                    return;
                }
                //ステータス割り振り必要ポイント参照
                var needpoint = needPDatas[needPDatas.Length - 1].level < nextLevel ? needPDatas[needPDatas.Length - 1].point : Array.Find(needPDatas, _ => _.level == nextLevel).point;
                if(needpoint <= CurrentStatusPoint)
                {
                    exclamation.enabled = true;
            SubmitButtonPoints.SetActive(true);
                    break;
                }
            }

        }
        else
        {
            exclamation.enabled = false;
            SubmitButtonPoints.SetActive(false);
        }
    }

    public void Submit()
    {
        if (CurrentAbilityID == -1 && CurrentEventData.abilityDatas.Count > CurrentAbilityID )
        {
            return;
        }
        AbilityReflectEvent.Trigger(
            CurrentEventData.abilityDatas[CurrentAbilityID].level,
            CurrentEventData.abilityDatas[CurrentAbilityID].ability,
            CurrentEventData.abilityDatas[CurrentAbilityID].statusAttribute,
            CurrentEventData.abilityDatas[CurrentAbilityID].statusAttributeValue
            );
        //ステータス反映
        //StatusReflectEvent.Trigger(
        //  CurrentStatusPoint, AddStatusPointList,
        //  /*CurrentEventData.abilityDatas[CurrentAbilityID].level != 0 ? GameDefine.StatusAttribute.NONE : */CurrentEventData.abilityDatas[CurrentAbilityID].statusAttribute,
        //   /*CurrentEventData.abilityDatas[CurrentAbilityID].level != 0 ? 0 : */CurrentEventData.abilityDatas[CurrentAbilityID].statusAttributeValue
        //  );
        Close();
        CurrentEventData.Callback();
    }

    public void Reroll()
    {
        if(CurrentEventData.Player.battleParameter.ReserveReroll<=0)
        {
            return;
        }
        CurrentEventData.Player.battleParameter.ReserveReroll--;
        CurrentEventData.abilityDatas =
         SurviveGameManager.GetInstance().abilitySelectSystem.GetSelectAbilityDatas();
        Open(CurrentEventData);
    }
}

public struct LevelUpEvent
{
    public int Level;
    public int Point;
    public SurvivePlayer Player;
    public List<AbilityInLevel> abilityDatas;

    public Action Callback;
 
    public struct AbilityInLevel
    {
        public AbilityData ability;
        public int level;
        public GameDefine.StatusAttribute statusAttribute;
        public int statusAttributeValue;
    }

    static LevelUpEvent e;
    public static void Trigger(List<AbilityInLevel> abilityDatas, int Level, int Point, SurvivePlayer Player, Action Callback)
    {
        e.abilityDatas = abilityDatas;
        e.Level = Level;
        e.Point = Point;
        e.Player = Player;
        e.Callback = Callback;
        MMEventManager.TriggerEvent(e);
    }
}
