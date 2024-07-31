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
using UnityEngine.Timeline;
using UnityEngine.Playables;

using DG.Tweening;

public class BoxUI : MonoBehaviour
{
    public List<AbilityUI> abilityUIs = new List<AbilityUI>();
    public Button submit;
    private SurviveGameManager sgm;
    public bool initialized = false;
    public bool opened = false;

    private PlayableDirector pd;

    public List<Image> getImages = new List<Image>();


    public TextMeshProUGUI abilityCount;

    public TextMeshProUGUI coin;
    
    [Serializable]
    public class AbilityUI
    {
        public GameObject Base;
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Description;
        public Image image ;
        public TextMeshProUGUI Type;
        //public TextMeshProUGUI Attribute;
        //public TextMeshProUGUI AttributeValue;
    }

    



    private BoxEvent CurrentEventData;

    private int CurrentAbilityID = -1;

    public Sprite NormalAbilityFrame;
    public Sprite NormalAbilityFramePush;
    public Sprite RareAbilityFrame;
    public Sprite RareAbilityFramePush;
    public Sprite EpicAbilityFrame;
    public Sprite EpicAbilityFramePush;
    public Sprite LegendAbilityFrame;
    public Sprite LegendAbilityFramePush;

    public AudioClip NormalAudio;
    public AudioClip RareAudio;

    public TimelineAsset timeline1;
    public TimelineAsset timeline3;
    public TimelineAsset timeline5;

    private AudioSource source;
    Tween tween = null;
    public void Inititalize()
    {
        sgm =  SurviveGameManager.GetInstance();
        initialized = true;

        for (int i = 0; i < abilityUIs.Count; i++)
        {
            int set = i;
            //abilityUIs[i].Base.GetComponent<Button>().onClick.AddListener(() => SetAbility(set));
            abilityUIs[i].Base.GetComponent<Image>().color = Color.white;
        }


        submit.onClick.AddListener(() => Submit());

        pd = gameObject.GetComponent<PlayableDirector>();
        pd.stopped += (_) => {
            submit.gameObject.SetActive(true);
            submit.enabled = true;
            EventSystem.current.SetSelectedGameObject(submit.gameObject);
            MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, 0, source);
            AbilityDisplay(0);
         
        };

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

    void AbilityDisplay(int i)
    {
        if(CurrentEventData.abilityDatas.Count == 1 )
        {
            abilityCount.text = "";
            abilityUIs[0].Base.transform.localScale = new Vector3(1f, 0, 1);//1.61556
			tween = abilityUIs[0].Base.transform.DOScaleY(1f, 0.5f).SetRelative(true).SetUpdate(true);
            tween.onComplete = () => {
            };
            tween.Play();
        }
        else
        {
           if( CurrentEventData.abilityDatas.Count <= i)
            {
                i = 0;
            }
            abilityCount.text = (i+1).ToString()+"/"+ CurrentEventData.abilityDatas.Count.ToString();
            abilityUIs[i].Base.transform.localScale = new Vector3(1f, 0, 1);
            tween = abilityUIs[i].Base.transform.DOScaleY(1f, 0.5f).SetRelative(true).SetUpdate(true);
            tween.onComplete = () => {
                tween = DOVirtual.DelayedCall(3, () =>
                {
                    tween = abilityUIs[i].Base.transform.DOScaleY(-1f, 0.5f).SetRelative(true).SetUpdate(true);
                    tween.onComplete = () =>
                    {
                        AbilityDisplay(i+1);
                    };
                    tween.Play();
                }).SetUpdate(true).Play();
                //tween.Play();
            };
            tween.Play();
        }

    }

    public void Open(BoxEvent eventData)
    {
        submit.gameObject.SetActive(false);
        submit.enabled = false;
        gameObject.SetActive(true);
        abilityCount.text = "";
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
        for (int i = 0; i < 5; i++)
        {
            if (CurrentEventData.abilityDatas.Count <= i)
            {
                if(abilityUIs.Count > i)
                    abilityUIs[i].Base.SetActive(false);
                getImages[i].gameObject.SetActive(false);
            }
            else
            {

                var data = CurrentEventData.abilityDatas[i];
                //アイテム用のデータ参照
                if (data.ability.abilityDataType == AbilityData.AbilityDataType.Item)
                {
                    var targetlevelItem = data.ability.ItemLevelDatas[data.level];
                    abilityUIs[i].Base.SetActive(true);
                    abilityUIs[i].Name.text = data.ability.GetLocalizeName();
                    abilityUIs[i].image.sprite = data.ability.image;

                    abilityUIs[i].Description.text = targetlevelItem.GetLocalizeDescription();
                    abilityUIs[i].Type.text = data.ability.abilityDataType.ToString();
                    //abilityUIs[i].Attribute.text = data.statusAttribute == GameDefine.StatusAttribute.NONE ? "" : data.statusAttribute.ToString();
                    //abilityUIs[i].AttributeValue.text = data.statusAttributeValue == 0?"":"+" + data.statusAttributeValue.ToString();
                }//武器スキル用のデータ参照
                else
                {


                    var targetlevel = data.ability.WeaponLevelDatas[data.level];
                    abilityUIs[i].Name.text = data.ability.GetLocalizeName();
                    abilityUIs[i].Base.SetActive(true);
                    abilityUIs[i].Description.text = targetlevel.GetLocalizeDescription();
                    abilityUIs[i].image.sprite = data.ability.image;
                    abilityUIs[i].Type.text = data.ability.abilityDataType.ToString();
                    //abilityUIs[i].Attribute.text = data.statusAttribute == GameDefine.StatusAttribute.NONE ? "" : data.statusAttribute.ToString();
                    //abilityUIs[i].AttributeValue.text = data.statusAttributeValue == 0 ? "" : "+" + data.statusAttributeValue.ToString();
                }
                Color color = Color.white;
                switch (data.statusAttributeValue)
                {
                    case (int)GameDefine.ABILITY_RARITY.NORMAL:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = NormalAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.NORMAL], out color);
                        //abilityUIs[i].Attribute.color = color;
                        //abilityUIs[i].AttributeValue.color = color; 
                        break;
                    case (int)GameDefine.ABILITY_RARITY.RARE:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = RareAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = RareAbilityFramePush;
                         ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.RARE], out color);
                        //abilityUIs[i].Attribute.color = color;
                        //abilityUIs[i].AttributeValue.color = color;
                        break;
                    case (int)GameDefine.ABILITY_RARITY.EPIC:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = EpicAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = EpicAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.EPIC], out color);
                        //abilityUIs[i].Attribute.color = color;
                        //abilityUIs[i].AttributeValue.color = color;
                        break;
                    case (int)GameDefine.ABILITY_RARITY.LEGEND:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = LegendAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = LegendAbilityFramePush;
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.LEGEND], out color);
                        //abilityUIs[i].Attribute.color = color;
                        //abilityUIs[i].AttributeValue.color = color;
                        break;
                   default:
                        abilityUIs[i].Base.GetComponent<ChangeImage>().FaulseSprite = NormalAbilityFramePush;
                        abilityUIs[i].Base.GetComponent<ChangeImage>().TrueSprite = NormalAbilityFramePush;
                       
                        ColorUtility.TryParseHtmlString(GameDefine.AbilityRarityColor[GameDefine.ABILITY_RARITY.NORMAL], out color);
                        //abilityUIs[i].Attribute.color = color;
                        //abilityUIs[i].AttributeValue.color = color;
                        break;
                }
                var sn = new Navigation();
                var rn = new Navigation();

                sn.mode = Navigation.Mode.Explicit;
                rn.mode = Navigation.Mode.Explicit;
                sn.selectOnUp = abilityUIs[i].Base.GetComponent<Button>();
    
                rn.selectOnUp = abilityUIs[i].Base.GetComponent<Button>();
                rn.selectOnRight = submit;

                submit.navigation = sn;

                getImages[i].gameObject.SetActive(true);
                getImages[i].sprite = data.ability.image;

            }
            abilityUIs[i].Base.transform.localScale = new Vector3(1, 0, 1);
        }
        SetAbility(-1);

        opened = true;

        //EventSystem.current.SetSelectedGameObject(abilityUIs[0].Base);
		EventSystem.current.sendNavigationEvents = true;
        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
        options.Loop = false;
        options.Location = Vector3.zero;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

        if (CurrentEventData.abilityDatas.Count == 5)
        {
            source = MMSoundManagerSoundPlayEvent.Trigger(RareAudio, options);
            pd.playableAsset = timeline5;
        }
        else if (CurrentEventData.abilityDatas.Count == 3)
        {
            source = MMSoundManagerSoundPlayEvent.Trigger(NormalAudio, options);
            pd.playableAsset = timeline3;
        }
        else
        {
            source = MMSoundManagerSoundPlayEvent.Trigger(NormalAudio, options);
            pd.playableAsset = timeline1;
        }
        pd.Play();

        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Pause, SurviveBackGroundMusic.Instance.CurrentBGMID/* 9999*/);

        coin.text = eventData.Coin.ToString();
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
        if (tween != null)
        {
            if(!tween.IsComplete())
            {
                tween.Kill();
            }
            tween = null;
        }
        MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Resume, SurviveBackGroundMusic.Instance.CurrentBGMID/*9999*/);
        EventSystem.current.sendNavigationEvents = false;

    }

    public void TestOpen()
    {
        Debug.Log("てすと　レベルあっぷ");
        var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetBoxAbilityDatas();
        var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
        player.battleParameter.Level++;
        var level = player.battleParameter.Level;
        //今回のレベルに応じたポイントに余っているものを足す
        int point = 10 + player.battleParameter.ReservePoint;
        BoxEvent.Trigger(datas, level, point, player, () =>
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
            }
            else
            {
                abilityUIs[i].Base.GetComponent<ChangeImage>().Change(false);
            }
        }

    }

    
    public void Submit()
    {
        //if (CurrentAbilityID == -1)
        //{
        //    return;
        //}
        Close();
        CurrentEventData.Callback();
        for (int i = 0; i < CurrentEventData.abilityDatas.Count; i++)
        {
            AbilityReflectEvent.Trigger(
                CurrentEventData.abilityDatas[i].level,
                CurrentEventData.abilityDatas[i].ability,
                CurrentEventData.abilityDatas[i].statusAttribute,
                CurrentEventData.abilityDatas[i].statusAttributeValue
                );
        }
    }

    //public void Reroll()
    //{
    //    if(CurrentEventData.Player.battleParameter.ReserveReroll<=0)
    //    {
    //        return;
    //    }
    //    CurrentEventData.Player.battleParameter.ReserveReroll--;
    //    CurrentEventData.abilityDatas =
    //     SurviveGameManager.GetInstance().abilitySelectSystem.GetSelectAbilityDatas();
    //    Open(CurrentEventData);
    //}
}

public struct BoxEvent
{
    public int Coin;
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

    static BoxEvent e;
    public static void Trigger(List<AbilityInLevel> abilityDatas, int Coin, int Point, SurvivePlayer Player, Action Callback)
    {
        e.abilityDatas = abilityDatas;
        e.Coin = Coin;
        e.Point = Point;
        e.Player = Player;
        e.Callback = Callback;
        MMEventManager.TriggerEvent(e);
    }
}
