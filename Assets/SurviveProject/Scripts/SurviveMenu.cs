using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.UI;
using EroSurvivor;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Simple start screen class.
	/// </summary>
	[AddComponentMenu("TopDown Engine/GUI/SurviveMenu")]
	public class SurviveMenu : TopDownMonoBehaviour
	{
		/// the level to load after the start screen
		[Tooltip("the level to load after the start screen")]
		public string NextLevel;
		/// the delay after which the level should auto skip (if less than 1s, won't autoskip)
		[Tooltip("the delay after which the level should auto skip (if less than 1s, won't autoskip)")]
		public float AutoSkipDelay = 0f;

		[Header("Fades")]
		/// the duration of the fade from black at the start of the level
		[Tooltip("the duration of the fade from black at the start of the level")]
		public float FadeInDuration = 1f;
		/// the duration of the fade to black at the end of the level
		[Tooltip("the duration of the fade to black at the end of the level")]
		public float FadeOutDuration = 1f;
		/// the tween type to use to fade the startscreen in and out 
		[Tooltip("the tween type to use to fade the startscreen in and out ")]
		public MMTweenType Tween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);

		[Header("Sound Settings Bindings")]
		/// the switch used to turn the music on or off
		[Tooltip("the switch used to turn the music on or off")]
		public MMSwitch MusicSwitch;
		/// the switch used to turn the SFX on or off
		[Tooltip("the switch used to turn the SFX on or off")]
		public MMSwitch SfxSwitch;

		public Transform upGradeMenuParent;
		public TextMeshProUGUI coinText;

		[Header("CanvasBind")]
		public Canvas StartMenu;
		public Canvas SelectMenu;
		public Canvas UpgradeMenu;
		public Canvas SettingMenu;
		//public Canvas StageSelectCanvas;

		//public GameObject StageSelectMenuObj;

		public LocalizeFontTMProManager localizeFontTMProManager;

		//public GameObject baseStageSelectObj;

		//public Transform StageSelectMenuObjParaent;

		//public List<StageSelectObj> StageSelectObjList = new List<StageSelectObj>();

		private bool StageSelectOpened = false;

		//public GameObject StageSelectPlayObj;

		//public GameObject EndlessObj;

		//public Toggle EndlessToggle;

		public GameObject StageNormalObj;
		public GameObject StageHardObj;
		public GameObject StageEndlessObj;

		public Sprite StageHardLock;
		public Sprite StageEndlessLock;
		public GameObject StageHardLockText;
		public GameObject StageEndlessLockText;


		//ステージ選択用　たぶんないけどできるようにリスト化
		public List<string> LevelList = new List<string>();

		private MENU_STATE state = MENU_STATE.START;

		public enum MENU_STATE
		{
			START,
			SELECT,
			UPGRADE,
			SETTING,
			CLOSE,
			//STAGE_SELECT,
			GALLERY,
		}

		public MENU_STATE State
		{
			get => state; set
			{

				state = value;
				switch (state)
				{
				case MENU_STATE.START:
					StartMenu.gameObject.SetActive(true);
					SelectMenu.gameObject.SetActive(false);
					UpgradeMenu.gameObject.SetActive(false);
					SettingMenu.gameObject.SetActive(false);
					GalleryManager.Instance.GallerySelectCanvasHidden();
					break;
				case MENU_STATE.SELECT:
					StartMenu.gameObject.SetActive(false);
					SelectMenu.gameObject.SetActive(true);
					UpgradeMenu.gameObject.SetActive(false);
					SettingMenu.gameObject.SetActive(false);
					//StageSelectMenuObj.SetActive(false);
					GalleryManager.Instance.GallerySelectCanvasHidden();
					break;
				case MENU_STATE.UPGRADE:
					StartMenu.gameObject.SetActive(false);
					SelectMenu.gameObject.SetActive(false);
					UpgradeMenu.gameObject.SetActive(true);
					SettingMenu.gameObject.SetActive(false);
					GalleryManager.Instance.GallerySelectCanvasHidden();
					break;
				case MENU_STATE.SETTING:
					StartMenu.gameObject.SetActive(false);
					SelectMenu.gameObject.SetActive(false);
					UpgradeMenu.gameObject.SetActive(false);
					SettingMenu.gameObject.SetActive(true);
					GalleryManager.Instance.GallerySelectCanvasHidden();
					break;
				case MENU_STATE.CLOSE:
					//ゲーム終了
					GameEnd();
					break;
				//case MENU_STATE.STAGE_SELECT:
				//	//StageSelectMenuObj.SetActive(true);
				//	//if(!StageSelectOpened)
				//	{
				//		var stageDatas = SurviveGameManager.GetInstance().stageDatas;
				//		var clearDatas = SurviveProgressManager.Current.StageClearDatas;
				//		//if (stageDatas != null)
				//		//{
				//		//    List<int> endlessDataIDs = new List<int>();
				//		//    foreach (var data in stageDatas.datas)
				//		//    {
				//		//        if (data.Endless > 0)
				//		//        {
				//		//            endlessDataIDs.Add(data.ID);
				//		//            continue;
				//		//        }
				//		//        var obj = Instantiate(baseStageSelectObj, StageSelectMenuObjParaent);
				//		//        obj.SetActive(true);
				//		//        var stageSelectObj = obj.GetComponent<StageSelectObj>();
				//		//        stageSelectObj.SetData(data);
				//		//        StageSelectObjList.Add(stageSelectObj);
				//		//    }
				//		//    //foreach (var id in endlessDataIDs)
				//		//    //{
				//		//    //    var endless = Array.Find(stageDatas.datas, (_) => _.ID == id);
				//		//    //    StageSelectObjList.Find(_ => _.GetData().ID == endless.Endless).SetEndlessData(endless);
				//		//    //}
				//		//}
				//		//UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(StageSelectObjList[0].gameObject);
				//		UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(StageNormalObj);

				//		if (clearDatas.Count > 0 && clearDatas.Exists(_ => _.ID == 1 && _.isCleared == true))
				//		{
				//			StageHardLockText.SetActive(false);
				//			StageEndlessLockText.SetActive(false);
				//		}
				//		else
				//		{
				//			StageHardObj.GetComponent<Image>().sprite = StageHardLock;
				//			StageEndlessObj.GetComponent<Image>().sprite = StageEndlessLock;

				//			StageHardObj.GetComponent<Button>().interactable = false;
				//			StageEndlessObj.GetComponent<Button>().interactable = false;
				//			StageHardObj.GetComponent<Button>().enabled = false;
				//			StageEndlessObj.GetComponent<Button>().enabled = false;
				//			StageHardObj.GetComponent<SurviveTouchButton>().Interactable = false;
				//			StageEndlessObj.GetComponent<SurviveTouchButton>().Interactable = false;

				//		}
				//		//StageSelectOpened = true;
				//	}
				//	//foreach (var obj in StageSelectObjList)
				//	//{
				//	//	obj.GetComponent<Button>().interactable = true;
				//	//}
				//	//StageSelectPlayObj.SetActive(false);
				//	break;
				case MENU_STATE.GALLERY:
					GalleryManager.Instance.GallerySelectCanvasDisplay();
					StartMenu.gameObject.SetActive(false);
					SelectMenu.gameObject.SetActive(false);
					UpgradeMenu.gameObject.SetActive(false);
					SettingMenu.gameObject.SetActive(false);
					break;

				}
			}
		}

		[SerializeField]
		GameObject uiIgnoreObj;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake()
		{
			GUIManager.Instance.SetHUDActive(false);
			MMFadeOutEvent.Trigger(FadeInDuration, Tween);
			Cursor.visible = true;
			if (AutoSkipDelay > 1f)
			{
				FadeOutDuration = AutoSkipDelay;
				StartCoroutine(LoadFirstLevel());
			}
		}

		/// <summary>
		/// On Start, initializes the music and sfx switches
		/// </summary>
		protected async void Start()
		{
			await Task.Delay(1);

			if (MusicSwitch != null)
			{
				MusicSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.MusicOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				MusicSwitch.InitializeState();
			}

			if (SfxSwitch != null)
			{
				SfxSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.SfxOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				SfxSwitch.InitializeState();
			}
		}

		/// <summary>
		/// During update we simply wait for the user to press the "jump" button.
		/// </summary>
		protected virtual void Update()
		{
			//	if (!Input.GetButtonDown ("Player1_Jump"))
			//		return;

			//	ButtonPressed ();

			if ((SurviveInputManager.Instance as SurviveInputManager).GetInputMode() == SurviveInputManager.InputMode.KeyBoardMouse)
			{
				Cursor.visible = true;
				StartMenu.GetComponent<GraphicRaycaster>().enabled = true;
				SelectMenu.GetComponent<GraphicRaycaster>().enabled = true;
				UpgradeMenu.GetComponent<GraphicRaycaster>().enabled = true;
				SettingMenu.GetComponent<GraphicRaycaster>().enabled = true;
				//StageSelectCanvas.GetComponent<GraphicRaycaster>().enabled = true;
			}
			else
			{
				Cursor.visible = false;
				StartMenu.GetComponent<GraphicRaycaster>().enabled = false;
				SelectMenu.GetComponent<GraphicRaycaster>().enabled = false;
				UpgradeMenu.GetComponent<GraphicRaycaster>().enabled = false;
				SettingMenu.GetComponent<GraphicRaycaster>().enabled = false;
				//StageSelectCanvas.GetComponent<GraphicRaycaster>().enabled = false;
			}

			//if (state == MENU_STATE.STAGE_SELECT)
			//{
			//	if ((SurviveInputManager.Instance as SurviveInputManager).CancelButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			//	{
			//		CloseStageSelect();

			//		return;
			//	}
			//}
			if (state != MENU_STATE.START)
			{
				if ((SurviveInputManager.Instance as SurviveInputManager).CancelButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
				{
					SwitchStartState();
				}
			}

		}

		public async void CloseStageSelect()
		{
			SwitchSelectStateWait();
		}
		private async void SwitchSelectStateWait()
		{
			await Task.Delay(1);
			var charaSelect = SelectMenu.gameObject.GetComponent<SurviveCharacterSelect>();
			charaSelect.isCharaSelected = false;

			foreach (var select in charaSelect.selectors)
			{
				select.GetCharaSelectObj().GetComponent<SurviveTouchButton>().Interactable = true;
			}

			SwitchSelectState();
		}

		/// <summary>
		/// What happens when the main button is pressed
		/// </summary>
		public virtual void PlayButtonPressed()
		{
			var charaSelect = SelectMenu.gameObject.GetComponent<SurviveCharacterSelect>();
			if (!charaSelect.canStartGame)
			{
				charaSelect.OpenUnlock();
				return;
			}
			charaSelect.isCharaSelected = true;

			foreach (var select in charaSelect.selectors)
			{
				select.GetCharaSelectObj().GetComponent<SurviveTouchButton>().Interactable = false;
				select.GetCharaSelectObj().GetComponent<SurviveTouchButton>().OffButton();
			}
			//         MMFadeInEvent.Trigger(FadeOutDuration, Tween);
			//// if the user presses the "Jump" button, we start the first level.
			//StartCoroutine (LoadFirstLevel ());
			StageSelectOpen();
		}


		public virtual void StageSelectOpen()
		{
			localizeFontTMProManager.UpdateFont();

			//SwitchStageSelectState();
		}

		public AudioClip MouseClickSe;
		public virtual void StageSelectObjPressed(int id)
		{

			//foreach(var obj in  StageSelectObjList)
			//         {
			//	obj.GetComponent<Button>().interactable = false;

			//	if(obj.GetEndlessData() != null)
			//             {
			//		EndlessObj.SetActive(true);
			//		EndlessToggle.isOn = false;
			//             }
			//             else
			//             {
			//		EndlessObj.SetActive(false);
			//	}
			//}

			//StageSelectPlayObj.SetActive(true);
			uiIgnoreObj.SetActive(true);

			PlaySe(MouseClickSe);


			StageSelectPlay(id);

		}
		private void PlaySe(AudioClip ac)
		{
			if (ac == null) return;

			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.Loop = false;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

			MMSoundManagerSoundPlayEvent.Trigger(ac, options);
		}
		public virtual void StageSelectPlay(int i)
		{
			MMFadeInEvent.Trigger(FadeOutDuration, Tween);
			// if the user presses the "Jump" button, we start the first level.
			SurviveGameManager.GetInstance().SetStageID(i);
			StartCoroutine(LoadSelectLevel(i));
		}
		protected virtual IEnumerator LoadSelectLevel(int i)
		{
			yield return new WaitForSeconds(FadeOutDuration);



			var stage = Array.Find(SurviveGameManager.GetInstance().stageDatas.datas, (_) => _.ID == i);

			//SurviveSceneLoadingManager.LoadScene (NextLevel);
			SurviveSceneLoadingManager.LoadScene(stage.SceneName);
		}
		public virtual void CloseStageSelectPlayObj()
		{
			//StageSelectPlayObj.SetActive(false);
			//foreach (var obj in StageSelectObjList)
			//{
			//	obj.GetComponent<Button>().interactable = true;
			//}
		}

		/// <summary>
		/// Loads the next level.
		/// </summary>
		/// <returns>The first level.</returns>
		protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds(FadeOutDuration);


			var stage = Array.Find(SurviveGameManager.GetInstance().stageDatas.datas, (_) => _.ID == 1);

			//SurviveSceneLoadingManager.LoadScene (NextLevel);
			SurviveSceneLoadingManager.LoadScene(stage.SceneName);
		}

		public virtual void SwitchSelectState()
		{
			State = MENU_STATE.SELECT;
		}

		public virtual void SwitchUpgradeState()
		{
			State = MENU_STATE.UPGRADE;
		}

		public virtual void SwitchStartState()
		{
			State = MENU_STATE.START;
		}

		public virtual void SwitchSettingState()
		{
			State = MENU_STATE.SETTING;
		}

		public virtual void SwitchCloseState()
		{
			State = MENU_STATE.CLOSE;
		}

		//public virtual void SwitchStageSelectState()
		//{
		//	State = MENU_STATE.STAGE_SELECT;
		//}

		public virtual void SwitchGalleryState()
		{
			State = MENU_STATE.GALLERY;
		}

		//終了するか聞くUI後のやつ　とりあえず直通
		protected virtual void GameEnd()
		{
			//エディタ側の終了無くていいかな
			Application.Quit();
		}
	}
}