using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurviveSettingUI : MonoBehaviour
{
    [SerializeField] public List<Toggle> ToggleLanguage;
    [SerializeField] public Slider SliderMusic;
    [SerializeField] public Slider SliderSfx;
    [SerializeField] public Toggle ToggleFullScreen;
    [SerializeField] public List<Toggle> ToggleGraphics;

    private SurviveSetting t_surviveSetting = null;
    private SurviveSetting surviveSettingManager
    {
        get
        {
            if (t_surviveSetting == null) t_surviveSetting = GameObject.Find("SettingManager")?.GetComponent<SurviveSetting>();
            return t_surviveSetting;
        }
    }

    private void OnEnable()
    {
        surviveSettingManager.InitSettingUI();
    }

}
