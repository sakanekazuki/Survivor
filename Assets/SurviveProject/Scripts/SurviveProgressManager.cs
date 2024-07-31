using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

using MoreMountains.TopDownEngine;
using MoreMountains.Feedbacks;

[System.Serializable]
/// <summary>
/// A serializable entity used to store progress : a list of scenes with their internal status (see above), how many lives are left, and how much we can have
/// 
/// ゲーム進行度　永続的な強化とか解放とかしたものを保存　データを先に作成、ID管理　
/// 別スクリプト化
/// 
/// 
/// </summary>
public class SurviveProgress
{
	public int Coin;

	//public DeadlineScene[] Scenes;
	public string[] Collectibles;
	public int[] PermanentEffectLevels;
	public CharaUnlock[] CharaUnlocks;
	public bool Played;
	public StageClearData[] StageClearDatas;
}
[Serializable]
public class TESTDATA
{
	public int ID;
	public bool isLocked;
	public bool nnnn;
}


[Serializable]
	public class CharaUnlock
    {
		public int ID;
		public bool isLocked;
    }
[Serializable]
public class StageClearData
{
	public int ID;
	public bool isCleared;
}

/// <summary>
/// The DeadlineProgressManager class acts as an example of how you can implement progress management in your game.
/// There's no general class for that in the engine, for the simple reason that no two games will want to save the exact same things.
/// But this should show you how it's done, and you can then copy and paste that into your own class (or extend this one, whatever you prefer).
/// </summary>
public class SurviveProgressManager : MMPersistentSingleton<SurviveProgressManager>, MMEventListener<TopDownEngineEvent>, MMEventListener<SurviveSaveEvent>
{
	public int InitialMaximumLives { get; set; }
	public int InitialCurrentLives { get; set; }

	//[Header("Characters")] 
	//public Character Naomi;
	//public Character Jules;

	/// the list of scenes that we'll want to consider for our game
	//[Tooltip("the list of scenes that we'll want to consider for our game")]
	//public DeadlineScene[] Scenes;

	[MMInspectorButton("CreateSaveGame")]
	/// A test button to test creating the save file
	public bool CreateSaveGameBtn;

	/// the current amount of collected stars
	public int CurrentStars { get; protected set; }

	//保存ファイル
	protected const string _saveFolderName = "SProgressData";
	protected const string _saveFileName = "SProgress.data";

	public List<string> FoundCollectibles { get; private set; }

	//一旦永続強化パラメータ系ここで
	[Serializable]
	public enum PermanentEffect
	{
        MaxHP = 1,
        Attack,
		MoveSpeed,
		Critical,
		ItemFlySpeed,
		AttackRange,
		LastTime,
		PickRange,
		AttackInterval,
        HPRecovery,
        DamageCut,
        GainExp,
        GainCoin,
        Reroll,
        WeaponLimit,
        RelicLimit,
        END //ここまで
	}

	[SerializeField]
	public List<int> permanentEffectLevels = new List<int>();
	public int Coin = 0;
	public List<CharaUnlock> charaUnlocks = new List<CharaUnlock>();
	public bool Played = false; 
	public List<StageClearData> StageClearDatas = new List<StageClearData>();
	
	/// <summary>
	/// OnEnable, we start listening to events.
	/// </summary>
	protected virtual void OnEnable()
	{
		this.MMEventStartListening<SurviveSaveEvent>();
		this.MMEventStartListening<TopDownEngineEvent>();

		InitializeDatas();
	}
	//各オブジェクトのStartからアクセスできるようにOnEnableで初期化する
	protected void InitializeDatas()
	{
		//base.Awake ();
		permanentEffectLevels = new List<int>();
		for (int i = 0; i < (int)PermanentEffect.END; i++)
		{
			permanentEffectLevels.Add(0);
		}
		//初期解放状態反映
		var charaData = SurviveGameManager.GetInstance().characterDatas.datas;
		for (int i = 0; i < charaData.Length; i++)
		{
			charaUnlocks.Add(new CharaUnlock { ID = charaData[i].ID, isLocked = charaData[i].isLocked });
		}

		var stageDatas = SurviveGameManager.GetInstance().stageDatas.datas;
		for (int i = 0; i < stageDatas.Length; i++)
		{
			StageClearDatas.Add(new StageClearData { ID = stageDatas[i].ID, isCleared = false });
		}
		LoadSavedProgress();


		//InitializeStars ();
		if (FoundCollectibles == null)
		{
			FoundCollectibles = new List<string>();
		}
	}

	/// <summary>
	/// When a level is completed, we update our progress
	/// </summary>
	//protected virtual void LevelComplete()
	//{
	//	for (int i = 0; i < Scenes.Length; i++)
	//	{
	//		if (Scenes[i].SceneName == SceneManager.GetActiveScene().name)
	//		{
	//			Scenes[i].LevelComplete = true;
	//			Scenes[i].LevelUnlocked = true;
	//			if (i < Scenes.Length - 1)
	//			{
	//				Scenes [i + 1].LevelUnlocked = true;
	//			}
	//		}
	//	}
	//}

	/// <summary>
	/// Goes through all the scenes in our progress list, and updates the collected stars counter
	/// </summary>
	protected virtual void InitializeStars()
	{
		//foreach (DeadlineScene scene in Scenes)
		//{
		//	if (scene.SceneName == SceneManager.GetActiveScene().name)
		//	{
		//		int stars = 0;
		//		foreach (bool star in scene.CollectedStars)
		//		{
		//			if (star) { stars++; }
		//		}
		//		CurrentStars = stars;
		//	}
		//}
	}

	/// <summary>
	/// Saves the progress to a file
	/// </summary>
	protected virtual void SaveProgress()
	{
		SurviveProgress progress = new SurviveProgress();
		//キャラ名　いらない
		//progress.StoredCharacterName = GameManager.Instance.StoredCharacter.name;


		//ライフ数　いらない
		//progress.MaximumLives = GameManager.Instance.MaximumLives;
		////progress.CurrentLives = GameManager.Instance.CurrentLives;
		//progress.InitialMaximumLives = InitialMaximumLives;
		//progress.InitialCurrentLives = InitialCurrentLives;

		//ステージごとの攻略状況らしい
		//progress.Scenes = Scenes;


		//収集アイテム　つかわない　
		//if (FoundCollectibles != null)
		//{
		//	progress.Collectibles = FoundCollectibles.ToArray();
		//}

		progress.PermanentEffectLevels = permanentEffectLevels.ToArray();

		progress.Coin = Coin;

		progress.CharaUnlocks = charaUnlocks.ToArray();

		progress.Played = Played;
		progress.StageClearDatas = StageClearDatas.ToArray();

		//暗号化予定

		var json = JsonUtility.ToJson(progress);
		SurviveSaveLoadManager.Save(json, _saveFileName, _saveFolderName);
	}

	/// <summary>
	/// A test method to create a test save file at any time from the inspector
	/// </summary>
	public virtual void SaveSurviveProgress()
	{
		SaveProgress();
	}
    public virtual void LoadSurviveProgress()
	{
		LoadSavedProgress();
	}
	/// <summary>
	/// Loads the saved progress into memory
	/// </summary>
	protected virtual void LoadSavedProgress()
	{
		try
		{
			//SurviveProgress progress = (SurviveProgress)MMSaveLoadManager.Load(typeof(SurviveProgress), _saveFileName, _saveFolderName);
			SurviveProgress progress = JsonUtility.FromJson<SurviveProgress>((string)SurviveSaveLoadManager.Load(typeof(string), _saveFileName, _saveFolderName));
			if (progress != null)
			{
				//GameManager.Instance.StoredCharacter = (progress.StoredCharacterName == Jules.name) ? Jules : Naomi;
				//GameManager.Instance.MaximumLives = progress.MaximumLives;
				//GameManager.Instance.CurrentLives = progress.CurrentLives;
				//InitialMaximumLives = progress.InitialMaximumLives;
				//InitialCurrentLives = progress.InitialCurrentLives;
				//Scenes = progress.Scenes;
				if (progress.Collectibles != null)
				{
					FoundCollectibles = new List<string>(progress.Collectibles);
				}
				if (progress.PermanentEffectLevels != null)
				{
					//入れる前のリストのカウントのほうが正
					for (int i = 0; i < progress.PermanentEffectLevels.Length; i++)
					{
						if (permanentEffectLevels.Count <= i)
						{
							break;
						}
						permanentEffectLevels[i] = progress.PermanentEffectLevels[i];
					}
				}
				Coin = progress.Coin;

				if (progress.CharaUnlocks != null)
				{
					for (int i = 0; i < progress.CharaUnlocks.Length; i++)
					{
						if (charaUnlocks.Exists(_ => _.ID == progress.CharaUnlocks[i].ID))
						{
							var target = charaUnlocks.Find(_ => _.ID == progress.CharaUnlocks[i].ID);
							target.isLocked = progress.CharaUnlocks[i].isLocked;
						}

					}
				}

				if (progress.StageClearDatas != null)
				{
					for (int i = 0; i < progress.StageClearDatas.Length; i++)
					{
						if (StageClearDatas.Exists(_ => _.ID == progress.StageClearDatas[i].ID))
						{
							var target = StageClearDatas.Find(_ => _.ID == progress.StageClearDatas[i].ID);
							target.isCleared = progress.StageClearDatas[i].isCleared;
						}

					}
				}

				Played = progress.Played;
			}
			else
			{
				//InitialMaximumLives = GameManager.Instance.MaximumLives;
				//InitialCurrentLives = GameManager.Instance.CurrentLives;

			}
        }
        catch
        {
			//セーブデータが正しくないとき、とりあえず強制初期化してセーブ、もう一回ロード 基本的に通らなくなった
			SaveProgress();
			LoadSavedProgress();
		}
	}

	public virtual void FindCollectible(string collectibleName)
	{
		FoundCollectibles.Add(collectibleName);
	}

	/// <summary>
	/// When we grab a star event, we update our scene status accordingly
	/// </summary>
	/// <param name="deadlineStarEvent">Deadline star event.</param>
	public virtual void OnMMEvent(TopDownEngineStarEvent deadlineStarEvent)
	{
		//foreach (DeadlineScene scene in Scenes)
		//{
		//	if (scene.SceneName == deadlineStarEvent.SceneName)
		//	{
		//		scene.CollectedStars [deadlineStarEvent.StarID] = true;
		//		CurrentStars++;
		//	}
		//}
	}

	/// <summary>
	/// When we grab a level complete event, we update our status, and save our progress to file
	/// </summary>
	/// <param name="gameEvent">Game event.</param>
	public virtual void OnMMEvent(TopDownEngineEvent gameEvent)
	{
		switch (gameEvent.EventType)
		{
			case TopDownEngineEventTypes.LevelComplete:
				//LevelComplete ();
				var player = (SurviveLevelManager.GetInstance().Players[0] as SurvivePlayer);
				var c_q = UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Max(0, player.battleParameter.EnemyDeathCount * 0.1f * 1.5f * 1.3f * player.battleParameter.GoldUp));

                float bonus = Array.Find(SurviveGameManager.GetInstance().stageDatas.datas, (_) => _.ID == SurviveGameManager.GetInstance().currentStageID).BonusCoinRate;
                c_q = Mathf.FloorToInt(c_q * bonus);
                player.AddCoin(c_q, false);
				SurviveGUIManager.GetInstance().SetGameClearScreen(true);
				ReflectBattleData();
				ClearStage(SurviveGameManager.GetInstance().currentStageID);

				SaveProgress();
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
				SurviveGameManager.Instance.Paused = true;
				LevelManager.Instance.ToggleCharacterPause();

				//LevelManager.Instance.GotoLevel("SurviveMenu");
				break;
			case TopDownEngineEventTypes.GameOver:
				//GameOver ();
				ReflectBattleData();
				SaveProgress();
				break;
			case TopDownEngineEventTypes.LevelEnd:
				ReflectBattleData();
				SaveProgress();
				SurviveGameManager.GetInstance().surviveRuleManager.ResetCurrentTime();

				break;
		}
	}

    protected virtual void ClearStage(int i)
	{
		StageClearDatas.Find(_ => _.ID == i).isCleared = true;
	}

	/// <summary>
	/// This method describes what happens when the player loses all lives. In this case, we reset its progress and all lives will be reset.
	/// </summary>
	protected virtual void GameOver()
	{
		ResetProgress();
		ResetLives();
	}

	/// <summary>
	/// Resets the number of lives to its initial values
	/// </summary>
	protected virtual void ResetLives()
	{
		GameManager.Instance.MaximumLives = InitialMaximumLives;
		GameManager.Instance.CurrentLives = InitialCurrentLives;
	}

	/// <summary>
	/// A method used to remove all save files associated to progress
	/// </summary>
	public virtual void ResetProgress()
	{
		MMSaveLoadManager.DeleteSaveFolder(_saveFolderName);
	}



	/// <summary>
	/// OnDisable, we stop listening to events.
	/// </summary>
	protected virtual void OnDisable()
	{
		this.MMEventStopListening<SurviveSaveEvent>();
		this.MMEventStopListening<TopDownEngineEvent>();
	}


	public virtual void OnMMEvent(SurviveSaveEvent SaveEvent)
	{
		//foreach (DeadlineScene scene in Scenes)
		//{
		//	if (scene.SceneName == deadlineStarEvent.SceneName)
		//	{
		//		scene.CollectedStars [deadlineStarEvent.StarID] = true;
		//		CurrentStars++;
		//	}
		//}

		SaveProgress();
	}


	public void ReflectBattleData()
	{
		if (SurviveLevelManager.Instance != null && SurviveLevelManager.Instance.Players != null && SurviveLevelManager.Instance.Players[0] != null)
		{
#if DEBUG || UNITY_EDITOR
			var dataName = SurviveLevelManager.GetInstance().currentBattleGUID.ToString();
			BATTLE_LOG_JSON logDatas = (BATTLE_LOG_JSON)SurviveLevelManager.GetInstance().TestDataLoad(typeof(BATTLE_LOG_JSON), dataName, "BATTLE_LOG");
			if (logDatas == null)
			{
				var player = (SurviveLevelManager.Instance.Players[0] as SurvivePlayer);
				List<BATTLE_LOG_JSON.BATTLE_LOG_INVENTORY> abiList = new List<BATTLE_LOG_JSON.BATTLE_LOG_INVENTORY>();
				foreach (var dic in player.GetAbilityDic)
                {
					abiList.Add(new BATTLE_LOG_JSON.BATTLE_LOG_INVENTORY() { ID = dic.Key, LEVEL = dic.Value + 1 });

				}
				logDatas = new BATTLE_LOG_JSON()
				{
					Coin = player.battleParameter.Coin,
					EnemyKillCount = player.battleParameter.EnemyDeathCount,
					Level = player.battleParameter.Level,
					SkillInventory = abiList,
					TIME = SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime(),
					PlayerCharaID = player.battleParameter.BaseData.ID,
					AbilityAttributeData = player.battleParameter.AbilityAttributeLevels,
					LevelUpAttributeData = player.battleParameter.StatusAttributeLevels,
					ShopData = player.battleParameter.BasePermanentEffectLevels,
				};


				SurviveLevelManager.GetInstance().TestDataSave(logDatas, dataName, "BATTLE_LOGS");
			}
#endif



			Coin += (SurviveLevelManager.Instance.Players[0] as SurvivePlayer).battleParameter.Coin;
			(SurviveLevelManager.Instance.Players[0] as SurvivePlayer).battleParameter.Coin = 0;



		}
	}

		[Serializable]
	public class BATTLE_LOG_JSON
	{
		public int Level;
		public int PlayerCharaID;
		public int Coin;
		public int EnemyKillCount;
		public float TIME;
		public List<int> ShopData;
		public List<int> LevelUpAttributeData;
		public List<int> AbilityAttributeData;

		[Serializable]
		public class BATTLE_LOG_INVENTORY
        {
			public int ID;
			public int LEVEL;
        }

		[SerializeField]
		public List< BATTLE_LOG_INVENTORY> SkillInventory;
	}
}

public struct SurviveSaveEvent
{
	//public string SceneName;
	//public SurviveSaveEvent(string sceneName)
	//{
	//	SceneName = sceneName;
	//}

	static SurviveSaveEvent e;
	public static void Trigger()
	{
		MMEventManager.TriggerEvent(e);
	}
}
