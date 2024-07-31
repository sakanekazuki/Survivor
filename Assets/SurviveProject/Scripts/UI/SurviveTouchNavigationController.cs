using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SurviveTouchNavigationController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, ISubmitHandler
{
    //trueでキーボード操作使う
    private bool use_flg = true;

    public enum NavigationType { 
        SurviveTouchButton,
        ToggleGroup,
        Slider,
        ShopScrollContent
    }
    [SerializeField] public NavigationType navigationType = NavigationType.SurviveTouchButton;
    [SerializeField] public SurviveTouchButton thisSelectable;
    //[SerializeField] EventSystem TargetEventSystem;

    [SerializeField] public GameObject SelectMarkImage;

    [SerializeField] public SurviveTouchNavigationController upSelectable;
    [SerializeField] public SurviveTouchNavigationController downSelectable;
    [SerializeField] public SurviveTouchNavigationController rightSelectable;
    [SerializeField] public SurviveTouchNavigationController leftSelectable;
    [SerializeField] public SurviveTouchNavigationController afterSelectable;
    [SerializeField] public SurviveShopItem thisShopItem;

    [SerializeField] public List<Toggle> Toggles;
    private int SelectTogglesIndex = 0;

    [SerializeField] public Slider slider;
    [SerializeField] public float sliderStep = 0.05f;

    [SerializeField] public float ScrollLineY = 0f;
    [SerializeField] public float ScrollLineYMax = 0f;
    [SerializeField] public Transform ScrollContent;

    [SerializeField] public bool OnEnabledSelect = false;

    [SerializeField] public AudioClip SelectSfx;

    [SerializeField] public bool UseLastController = false; //Afterの代わりに保持しておいたものを使う
    [SerializeField] public bool UseLastControllerTarget = false; //Afterの代わりに保持しておいたものを使う


    private bool f_mouseInChangeCharaId = false;
    private MouseInChangeCharaId mouseInChangeCharaId;

    private bool isInit = false;

    private SurviveTouchNavigationManager _surviveTouchNavigationManager;
    private SurviveTouchNavigationManager surviveTouchNavigationManager { get {
            if (_surviveTouchNavigationManager == null)
            {
                GameObject obj = GameObject.Find("SurviveTouchNavigationManager");
                if (obj != null)
                {
                    _surviveTouchNavigationManager = obj.GetComponent<SurviveTouchNavigationManager>();
                }
            }
            return _surviveTouchNavigationManager;
        } }

    private EventSystem _eventSystem;
    private EventSystem eventSystem
    {
        get
        {
            if (_eventSystem == null)
            {
                //if (TargetEventSystem != null)
                //{
                //    _eventSystem = TargetEventSystem;
                //}
                //else
                {
                    GameObject obj = GameObject.Find("EventSystem");
                    if (obj != null)
                    {
                        _eventSystem = obj.GetComponent<EventSystem>();
                    }
                }
            }
            return _eventSystem;
        }
    }


    private string _path = "";
    private string path { 
        get {
            if (_path == "")
            {
                _path = createRootPath(gameObject);
            }
            return _path;
        } }

    private string createRootPath(GameObject _obj)
    {
        _path = $"/{_obj.name}{_path}";
        if (_obj.transform.parent != null)
        {
            createRootPath(_obj.transform.parent.gameObject);
        }
        return _path;
    }



    private void Awake()
    {
#if UNITY_EDITOR
        Debug.Log("SurviveTouchNavigationController Awake");
#endif
        isInit = false;
    }
    private void Reset()
    {
        thisSelectable = GetComponent<SurviveTouchButton>();
        if (thisSelectable == null) Debug.Log($"SurviveTouchButton misssing ({name})");
    }
    private void OnEnable()
    {
        //Debug.Log($"OnEnable {path}");
        if (SelectMarkImage != null) SelectMarkImage.SetActive(false);

        if (isInit && OnEnabledSelect)
        {
            surviveTouchNavigationManager.SetSelectController(this);
        }
    }
    private void OnDisable()
    {
        //Debug.Log($"OnDisable {path}");
    }

    public void Start()
    {
        if (!use_flg) return;
        isInit = true;

        if (OnEnabledSelect)
        {
            surviveTouchNavigationManager.SetSelectController(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerDown {path}");
        //surviveTouchNavigationManager.SetSelectController(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerUp {path}");
        //surviveTouchNavigationManager.SetSelectController(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log($"OnPointerExit {path}");
        //surviveTouchNavigationManager.SetSelectController(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!use_flg) return;
        //Debug.Log($"OnPointerEnter {path}");
        surviveTouchNavigationManager.SetSelectController(this);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        //Debug.Log($"BaseEventData {path}");
        //surviveTouchNavigationManager.SetSelectController(afterSelectable);
    }

    public string GetRootPath()
    {
        return path;
    }



    public void SelectThisSelectable(bool isKey=false)
    {
        if (!use_flg) return;

        bool se_flg = false; //変なタイミングで鳴らないようにタイミング指定
        if(SelectMarkImage != null)
        {
            if (!SelectMarkImage.activeSelf)
            {
               se_flg = true;
            }
            SelectMarkImage.SetActive(true);
        }

        switch (navigationType) {
            case NavigationType.SurviveTouchButton:
            case NavigationType.ShopScrollContent:
                if (thisSelectable != null)
                {
                    if(!isKey) thisSelectable.isSelectSfx = false;
                    thisSelectable.OnPointerEnter(new PointerEventData(eventSystem));
                }
                if (thisShopItem != null) thisShopItem.OnPointerEnter(new PointerEventData(eventSystem));
                
                //特別対応
                if(!f_mouseInChangeCharaId)
                {
                    f_mouseInChangeCharaId = true;
                    mouseInChangeCharaId = thisSelectable.gameObject.GetComponent<MouseInChangeCharaId>();
                }
                if (mouseInChangeCharaId != null)
                {
                    mouseInChangeCharaId.OnPointerEnter(new PointerEventData(eventSystem));
                }

                break;

            case NavigationType.ToggleGroup:
                for (int i=0; i<Toggles.Count; i++)
                {
                    if (Toggles[i].isOn)
                    {
                        SelectTogglesIndex = i;
                        break;
                    }
                }

                if(se_flg) PlaySe(SelectSfx);

                break;

            case NavigationType.Slider:

                if (se_flg) PlaySe(SelectSfx);

                break;
        }

    }
    public void SelectUpSelectable()
    {
        if (!use_flg) return;

        if (upSelectable != null)
        {
            surviveTouchNavigationManager.SetSelectController(upSelectable, true);
        }
    }
    public void SelectDownSelectable()
    {
        if (!use_flg) return;

        if (downSelectable != null)
        {
            surviveTouchNavigationManager.SetSelectController(downSelectable, true);
        }
    }
    public void SelectRightSelectable()
    {
        if (!use_flg) return;

        switch (navigationType)
        {
            case NavigationType.SurviveTouchButton:
            case NavigationType.ShopScrollContent:
                if (rightSelectable != null)
                {
                    surviveTouchNavigationManager.SetSelectController(rightSelectable, true);
                }
                break;

            case NavigationType.ToggleGroup:
                IsOnToggle(1);
                break;

            case NavigationType.Slider:
                AddValueSlider(sliderStep);
                break;
        }
    }
    public void SelectLeftSelectable()
    {
        if (!use_flg) return;

        switch (navigationType)
        {
            case NavigationType.SurviveTouchButton:
            case NavigationType.ShopScrollContent:
                if (leftSelectable != null)
                {
                    surviveTouchNavigationManager.SetSelectController(leftSelectable, true);
                }
                break;

            case NavigationType.ToggleGroup:
                IsOnToggle(-1);
                break;

            case NavigationType.Slider:
                AddValueSlider(-sliderStep);
                break;
        }
    }
    public void SelectSubmit()
    {
        if (!use_flg) return;

        switch (navigationType)
        {
            case NavigationType.SurviveTouchButton:
            case NavigationType.ShopScrollContent:
                thisSelectable.OnSubmit(new PointerEventData(eventSystem));
                if (surviveTouchNavigationManager.PointaLastNavigation != null)
                {
                    surviveTouchNavigationManager.SetSelectController(surviveTouchNavigationManager.PointaLastNavigation);
                }
                else if (afterSelectable != null)
                {
                    surviveTouchNavigationManager.SetSelectController(afterSelectable);
                }
                if (UseLastControllerTarget)
                {
                    surviveTouchNavigationManager.PointaLastNavigation = this;
                }
                else
                {
                    surviveTouchNavigationManager.PointaLastNavigation = null;
                }
                break;

            case NavigationType.ToggleGroup:
                IsOnToggle(1);
                break;

            case NavigationType.Slider:
                AddValueSlider(sliderStep);
                break;
        }
    }
    public void ExitPointer()
    {
        if (!use_flg) return;

        if (SelectMarkImage != null) SelectMarkImage.SetActive(false);
        if (thisSelectable != null) thisSelectable.OnPointerExit(new PointerEventData(eventSystem));
    }





    private void IsOnToggle(int add)
    {
        if (Toggles.Count == 1)
        {
            Toggles[SelectTogglesIndex].isOn = !Toggles[SelectTogglesIndex].isOn;
        }
        else
        {
            int _next = SelectTogglesIndex + add;
            _next = (_next >= Toggles.Count) ? 0 : ((_next < 0) ? Toggles.Count - 1 : _next);
            SelectTogglesIndex = _next;
            //Debug.Log($"IsOnToggle {_next}");
            Toggles[_next].isOn = true;
        }
    }



    private void AddValueSlider(float add)
    {

        float old = slider.value;
        slider.value += add;
    }
    public void OnPointerUpAction()
    {
        if (!use_flg) return;

        //音を鳴らすeventが使われているのを考慮
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null) return;

        List<EventTrigger.Entry> entrys = eventTrigger.triggers;
        if (entrys.Count == 0) return;

        foreach (EventTrigger.Entry entry in entrys)
        {
            if (entry.eventID == EventTriggerType.PointerUp)
            {
                entry.callback.Invoke(new BaseEventData(eventSystem));
            }
        }
    }


    //クリック音再生などの処理
    private void PlaySe(AudioClip ac)
    {
        if (!use_flg) return;
        if (ac == null) return;

        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
        options.Loop = false;
        options.Location = Vector3.zero;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

        MMSoundManagerSoundPlayEvent.Trigger(ac, options);
    }

}
