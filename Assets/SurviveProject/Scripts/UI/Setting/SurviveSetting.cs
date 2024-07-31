using EroSurvivor;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

/// <summary>
/// 設定情報保存クラス
/// </summary>
[Serializable]
public class SurviveSettingClass
{
	public int graphic = 2;
	public int fullScreenMode = 1;
	public int language = -1;
}

/// <summary>
/// 設定画面処理
/// </summary>

public class SurviveSetting : MonoBehaviour
{
	public static SurviveSetting Instance { get; set; }

	private SurviveSettingUI t_surviveSetting = null;
	private SurviveSettingUI _surviveSetting
	{
		get
		{
			if (t_surviveSetting == null) t_surviveSetting = GameObject.Find("SettingUI")?.GetComponent<SurviveSettingUI>();
			return t_surviveSetting;
		}
	}

	public enum ScreenSize
	{
		SIZE_854_480,
		SIZE_1280_720, //def?
		SIZE_1980_1080
	}
	public Dictionary<ScreenSize, int[]> ScreenSizeList = new Dictionary<ScreenSize, int[]>{
		{ ScreenSize.SIZE_854_480   , new int[]{    854,    480     }},
		{ ScreenSize.SIZE_1280_720  , new int[]{    1280,   720     }},
		{ ScreenSize.SIZE_1980_1080 , new int[]{    1980,   1080    }},
	};
	[SerializeField] ScreenSize graphic_size_0;
	[SerializeField] ScreenSize graphic_size_1;
	[SerializeField] ScreenSize graphic_size_2;

	private int lang_count = 4; //一旦解決のために言語数直打ちで…

	protected const string _saveFileName = "survive.settings";
	private SurviveSettingClass surviveSettingData;
	public SurviveSettingClass SurviveSettingData
	{
		get => surviveSettingData;
	}

	private bool isInit = false;

	public AudioClip MouseInSe;
	public AudioClip MouseClickSe;

	private bool ChangeFullScreenStay = false;
	private bool FlgToggleFullScreenChange = false;
	private bool oldToggle = false;
	private bool oldScreen = false;

	private void Start()
	{
		// Debug.Log("setting start");

		if (Instance != null)
		{
			Destroy(Instance.gameObject);
			Instance = null;
		}
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		InitSettingUI();
		if (SurviveSettingData.language == 2)
		{
			GalleryManager.LocaleName = "en";
		}
		else
		{
			GalleryManager.LocaleName = "ja";
		}
	}

	private void LateUpdate()
	{
		//フルスクリーン監視...
		if (_surviveSetting != null && surviveSettingData != null)
		{
			if (ChangeFullScreenStay)
			{
				if (Screen.fullScreen == _surviveSetting.ToggleFullScreen.isOn)
				{
					ChangeFullScreenStay = false;
				}
			}
			else
			{
				if (_surviveSetting.ToggleFullScreen.isOn != Screen.fullScreen)
				{
					if (oldToggle != _surviveSetting.ToggleFullScreen.isOn)
					{
						//Toggleで変えた
						Screen.fullScreen = _surviveSetting.ToggleFullScreen.isOn;
						surviveSettingData.fullScreenMode = (_surviveSetting.ToggleFullScreen.isOn ? 1 : 0);
					}
					else
					{
						//Alt+Enterで変えた
						_surviveSetting.ToggleFullScreen.isOn = Screen.fullScreen;
						surviveSettingData.fullScreenMode = (Screen.fullScreen ? 1 : 0);

						//スクリーンサイズも変わるのでToggleを合わせる
						int newPix = 0;
						foreach (KeyValuePair<ScreenSize, int[]> pair in ScreenSizeList)
						{
							if (pair.Value[0] >= Screen.currentResolution.width)
							{
								newPix = (int)pair.Key;
							}
						}
						if (newPix != surviveSettingData.graphic)
						{
							_surviveSetting.ToggleGraphics[newPix].isOn = true;
							surviveSettingData.graphic = newPix;
						}
					}
					MMSaveLoadManager.Save(surviveSettingData, _saveFileName);

					ChangeFullScreenStay = true;
				}
				oldToggle = _surviveSetting.ToggleFullScreen.isOn;
			}
		}
	}

	public void InitSettingUI()
	{
		LoadSettingSerial();
		StartCoroutine(InitLanguage());
		InitSoundVolumes();
		InitGraphics();
	}

	private void LoadSettingSerial()
	{
		if (surviveSettingData == null)
		{
			SurviveSettingClass _surviveSettingData = (SurviveSettingClass)MMSaveLoadManager.Load(typeof(SurviveSettingClass), _saveFileName);
			if (_surviveSettingData != null)
			{
				surviveSettingData = _surviveSettingData;
			}
			if (surviveSettingData == null)
			{
				surviveSettingData = new SurviveSettingClass();
			}
		}
	}


	/// <summary>
	/// 言語切り替えToggle
	/// </summary>
	/// <param name="index"></param>
	public void ToggleUpdateLanguage(int index = 0)
	{
		if (!isInit) return;

		//音
		PlaySe(MouseClickSe);

		StartCoroutine(ToggleUpdateLanguage_cor(index));
	}
	IEnumerator ToggleUpdateLanguage_cor(int index)
	{
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

		surviveSettingData.language = index;
		MMSaveLoadManager.Save(surviveSettingData, _saveFileName);
	}
	IEnumerator InitLanguage()
	{
		yield return LocalizationSettings.InitializationOperation;

		//言語設定初期化
		if (surviveSettingData.language >= 0)
		{
			//設定ファイルがある場合
			if (_surviveSetting != null) _surviveSetting.ToggleLanguage[surviveSettingData.language].isOn = true;
			LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[surviveSettingData.language];
		}
		else
		{
			//ない場合
			for (int i = 0; i < lang_count; i++)
			{
				var locale = LocalizationSettings.AvailableLocales.Locales[i];
				if ((LocalizationSettings.SelectedLocale == locale))
				{
					LocalizationSettings.SelectedLocale = locale;
					if (_surviveSetting != null) _surviveSetting.ToggleLanguage[i].isOn = true;
					surviveSettingData.language = i;
				}
			}
		}

		//if (_surviveSetting != null)
		//{
		//    //謎エラー対策 OnValueChanged手動でつける
		//    for (int i = 0; i < lang_count; i++)
		//    {
		//        int index = i;
		//        _surviveSetting.ToggleLanguage[i].onValueChanged.RemoveAllListeners();
		//        _surviveSetting.ToggleLanguage[i].onValueChanged.AddListener((b)=> {
		//            if(b) ToggleUpdateLanguage(index);
		//        });
		//    }
		//}



		isInit = true;
		yield break;
	}






	/// <summary>
	/// 音量設定
	/// </summary>
	public void SlideUpdateMusicVolume()
	{
		float vol = _surviveSetting.SliderMusic.value;
		//if (vol > 0f)
		//{
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Music, vol);
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, MMSoundManager.MMSoundManagerTracks.Music, vol);
		//}
		//else
		//{
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Music);
		//}

		Debug.Log($"SlideUpdateMusicVolume:{vol}");
		MMSoundManager.Instance.SetVolumeMusic(vol);
		MMSoundManager.Instance.SaveSettings();
	}
	public void SlideUpdateSfxVolume()
	{
		float vol = _surviveSetting.SliderSfx.value;
		//if (vol > 0f)
		//{
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx, vol);
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, MMSoundManager.MMSoundManagerTracks.Sfx, vol);
		//}
		//else
		//{
		//    MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx);
		//}

		Debug.Log($"SlideUpdateSfxVolume:{vol}");
		MMSoundManager.Instance.SetVolumeSfx(vol);
		MMSoundManager.Instance.SaveSettings();
	}
	public void InitSoundVolumes()
	{
		float vol_music = MMSoundManagerSettings._defaultVolume;
		float vol_sfx = MMSoundManagerSettings._defaultVolume;
		if (MMSoundManager.Instance.CheckLoadSettings())
		{
			MMSoundManager.Instance.LoadSettings();

			vol_music = MMSoundManager.Instance.settingsSo.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music);
			vol_sfx = MMSoundManager.Instance.settingsSo.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx);
		}

		MMSoundManager.Instance.SetVolumeMusic(vol_music);
		MMSoundManager.Instance.SetVolumeSfx(vol_sfx);

		Debug.Log($"vol_music:{vol_music}");
		Debug.Log($"vol_sfx:{vol_sfx}");

		if (_surviveSetting != null)
		{
			_surviveSetting.SliderMusic.value = vol_music;
			_surviveSetting.SliderSfx.value = vol_sfx;
		}
	}
	public void PlaySliderChangedSfx()
	{
		//音
		PlaySe(MouseClickSe);
	}
	public void PlaySelectSfx()
	{
		//音
		PlaySe(MouseInSe);
	}


	/// <summary>
	/// スクリーンサイズ
	/// </summary>
	public void ToggleUpdateScreenSize(int ss)
	{
		if (_surviveSetting == null) return;

		for (int i = 0; i < _surviveSetting.ToggleGraphics.Count; i++)
		{
			//ToggleのOFF時にも反応してしまうので、押したToggle判定を入れる
			if (_surviveSetting.ToggleGraphics[i].isOn && ss == i)
			{
				SetScreenSize(ss);

				surviveSettingData.graphic = ss;
				//Debug.Log($"save {ss}");
				MMSaveLoadManager.Save(surviveSettingData, _saveFileName);

				//音
				PlaySe(MouseClickSe);

				break;
			}
		}

	}
	private void SetScreenSize(int ss)
	{
		//Debug.Log($"SetScreenSize {ss}");
		int size_x = ScreenSizeList[ScreenSize.SIZE_1280_720][0];
		int size_y = ScreenSizeList[ScreenSize.SIZE_1280_720][1];
		if (Enum.IsDefined(typeof(ScreenSize), ss))
		{
			var _ss = (ScreenSize)ss;
			size_x = ScreenSizeList[_ss][0];
			size_y = ScreenSizeList[_ss][1];
		}
		//Debug.Log($"SetScreenSize size_x {size_x} size_y {size_y}");

		Screen.SetResolution(size_x, size_y, (surviveSettingData.fullScreenMode == 1));
	}
	public void ToggleUpdateFullScreen()
	{
		if (!isInit) return;

		//音
		PlaySe(MouseClickSe);
	}
	private void InitGraphics()
	{
		// Debug.Log($"InitGraphics S");
		//Debug.Log($"InitGraphics surviveSettingData.graphic {surviveSettingData.graphic}");
		Screen.fullScreen = (surviveSettingData.fullScreenMode == 1);
		if (_surviveSetting != null)
		{
			// Debug.Log($"InitGraphics A");
			_surviveSetting.ToggleFullScreen.isOn = (surviveSettingData.fullScreenMode == 1);

			for (int i = 0; i < _surviveSetting.ToggleGraphics.Count; i++)
			{
				//Debug.Log($"InitGraphics {i} {(i == surviveSettingData.graphic)}");
				_surviveSetting.ToggleGraphics[i].isOn = (i == surviveSettingData.graphic);
			}
		}

		SetScreenSize(surviveSettingData.graphic);
	}


	//クリック音再生などの処理
	private void PlaySe(AudioClip ac)
	{
		if (!isInit) return;
		if (ac == null) return;

		MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
		options.Loop = false;
		options.Location = Vector3.zero;
		options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

		MMSoundManagerSoundPlayEvent.Trigger(ac, options);
	}

}
