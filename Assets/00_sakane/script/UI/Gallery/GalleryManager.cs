using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Naninovel;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace EroSurvivor
{
	// �M�������[�Ǘ��N���X
	public class GalleryManager : Singleton<GalleryManager>
	{
		// �M�������[�̉摜
		[SerializeField]
		SO_GallerySprite gallerySprite;

		// �M�������[�I���L�����o�X
		[SerializeField]
		GameObject gallerySelectCanvas;

		[SerializeField]
		GameObject loadingScreen;

		// �M�������[�L�����o�X
		//[SerializeField]
		//GameObject galleryCanvasPrefab;
		//GameObject galleryCanvas;

		// true = �A�C�e�����J�����ꂢ�Ă�
		//public static List<bool> isItemOpens = new List<bool>();
		// true = �X�g�[���[���J������Ă���
		//public static List<bool> isStoryOpens = new List<bool>()
		//{false, false, false, false, false, false};
		// true = �����G���J������Ă���
		//public static List<bool> isPictureOpens = new List<bool>()
		//{false,false,false };

		public class storystate
		{
			//public List<bool> storyopens = new List<bool>() { false, false, false, false, false, false };
			//public List<bool> pictureopens = new List<bool>() { false, false, false };

			public List<int> charaUseNum = new List<int>() { 0, 0, 0 };
		}

		public static storystate state = new storystate();

		static string datapath = Application.dataPath + "galleryState.json";

		//GalleryState state = new GalleryState();

		// �N���b�N�𖳎�����I�u�W�F�N�g�z��
		[SerializeField]
		List<GameObject> itemClickIgnoreObjs = new List<GameObject>();
		[SerializeField]
		List<GameObject> storyClickIgnoreObjs = new List<GameObject>();
		[SerializeField]
		List<GameObject> pictureClickIgnoreObjs = new List<GameObject>();

		[SerializeField]
		SurviveTouchNavigationManager surviveTouchNavigationManager;

		public static string LocaleName
		{
			get;
			set;
		}

		private void Start()
		{
			gallerySelectCanvas.SetActive(false);
			loadingScreen.SetActive(false);

			//foreach (var v in isItemOpens)
			//{
			//	var indexNumber = isItemOpens.IndexOf(v);
			//	if (v)
			//	{
			//		itemClickIgnoreObjs[indexNumber].SetActive(false);
			//	}
			//	else
			//	{
			//		itemClickIgnoreObjs[indexNumber].SetActive(true);
			//	}
			//}

			state = Load();

			OpenCheck();

			//// �X�g�[���[���J������Ă�����{�^����L����
			//for (var v = 0; v < state.storyopens.Count; ++v)
			//{
			//	if (state.storyopens[v])
			//	{
			//		storyClickIgnoreObjs[v].SetActive(false);
			//	}
			//	else
			//	{
			//		storyClickIgnoreObjs[v].SetActive(true);
			//	}
			//}

			//// �s�N�`���[���J������Ă���΃{�^����L����
			//for (var v = 0; v < state.pictureopens.Count; ++v)
			//{
			//	if (state.pictureopens[v])
			//	{
			//		pictureClickIgnoreObjs[v].SetActive(false);
			//	}
			//	else
			//	{
			//		pictureClickIgnoreObjs[v].SetActive(true);
			//	}
			//}
		}

		public void OpenCheck()
		{
			if (state == null)
			{
				state = new storystate();
				Save();
			}

			foreach (var v in storyClickIgnoreObjs)
			{
				v.SetActive(true);
			}

			foreach (var v in pictureClickIgnoreObjs)
			{
				v.SetActive(true);
			}

			for (int i = 0; i < state.charaUseNum.Count; ++i)
			{
				if (state.charaUseNum[i] >= 1)
				{
					storyClickIgnoreObjs[i].SetActive(false);
					pictureClickIgnoreObjs[i].SetActive(false);
					if (state.charaUseNum[i] >= 10)
					{
						storyClickIgnoreObjs[i + 3].SetActive(false);
					}
				}
			}
		}

		/// <summary>
		/// �M�������[�I����ʕ\��
		/// </summary>
		[ContextMenu("�M�������[�I����ʕ\��")]
		public void GallerySelectCanvasDisplay()
		{
			gallerySelectCanvas.SetActive(true);
		}

		public void GallerySelectCanvasHidden()
		{
			gallerySelectCanvas.SetActive(false);
		}

		/// <summary>
		/// �M�������[������
		/// </summary>
		/// <param name="number">����M�������[�̔ԍ�</param>
		public async void GalleryDisplay(/*int number*/string galleryName)
		{
			gallerySelectCanvas.SetActive(false);
			GameObject.Find("MMAudioSourcePool_0").GetComponent<AudioSource>().Pause();
			surviveTouchNavigationManager.enabled = false;
			loadingScreen.SetActive(true);
			//galleryCanvas.SetActive(true);
			//GalleryEvent.Trigger(gallerySprite.gallerys[number]);
			await Naninovel.RuntimeInitializer.InitializeAsync();

			var localization = Engine.GetService<ILocalizationManager>();
			var configuration = Engine.GetConfiguration<LocalizationConfiguration>();
			configuration.DefaultLocale = LocaleName;
			//configuration.SourceLocale = "en";
			await localization.SelectLocaleAsync(LocaleName);
			var scriptManager = Engine.GetService<IScriptManager>();
			await scriptManager.ReloadAllScriptsAsync();

			var player = Engine.GetService<IScriptPlayer>();
			await player.PreloadAndPlayAsync(galleryName);
		}

		public void LoadingScreenDisable()
		{
			loadingScreen.SetActive(false);
		}

		/// <summary>
		/// �X�g�[���[�I��
		/// </summary>
		public void StoryFinish()
		{
			gallerySelectCanvas.SetActive(true);
			loadingScreen.SetActive(false);
			GameObject.Find("MMAudioSourcePool_0").GetComponent<AudioSource>().UnPause();
			surviveTouchNavigationManager.enabled = true;
		}

		//public static void PictureOpen(int number)
		//{
		//	++state.charaUseNum[number];
		//	Save();
		//}

		public static void StoryOpen(int number)
		{
			++state.charaUseNum[number];
			Save();
		}

		public static int GetUseNum(int number)
		{
			return state.charaUseNum[number];
		}

		public static void Save()
		{
			StreamWriter writer;

			//player�f�[�^��JSON�ɕϊ�
			string jsonstr = JsonUtility.ToJson(state);

			//JSON�t�@�C���ɏ�������
			writer = new StreamWriter(datapath, false);
			writer.Write(jsonstr);
			writer.Flush();
			writer.Close();
		}

		private static storystate Load()
		{
			string datastr = "";
			StreamReader reader;
			reader = new StreamReader(datapath);
			datastr = reader.ReadToEnd();
			reader.Close();

			return JsonUtility.FromJson<storystate>(datastr);
		}
	}
}