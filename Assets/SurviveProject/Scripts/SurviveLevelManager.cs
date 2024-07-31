using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.IO;

namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// Spawns the player, handles checkpoints and respawn
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/Survive Level Manager")]
	public class SurviveLevelManager : LevelManager
    {
		///// the prefab you want for your player
		//[Header("Instantiate Characters")]
		//[MMInformation("The LevelManager is responsible for handling spawn/respawn, checkpoints management and level bounds. Here you can define one or more playable characters for your level..",MMInformationAttribute.InformationType.Info,false)]
		///// should the player IDs be auto attributed (usually yes)
		//[Tooltip("should the player IDs be auto attributed (usually yes)")]
		//public bool AutoAttributePlayerIDs = true;
		///// the list of player prefabs to instantiate
		//[Tooltip("The list of player prefabs this level manager will instantiate on Start")]
		//public Character[] PlayerPrefabs ;

		//[Header("Characters already in the scene")]
		//[MMInformation("It's recommended to have the LevelManager instantiate your characters, but if instead you'd prefer to have them already present in the scene, just bind them in the list below.", MMInformationAttribute.InformationType.Info, false)]
		///// a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored
		//[Tooltip("a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored")]
		//public List<Character> SceneCharacters;

		//[Header("Checkpoints")]
		///// the checkpoint to use as initial spawn point if no point of entry is specified
		//[Tooltip("the checkpoint to use as initial spawn point if no point of entry is specified")]
		//public CheckPoint InitialSpawnPoint;
		///// the currently active checkpoint (the last checkpoint passed by the player)
		//[Tooltip("the currently active checkpoint (the last checkpoint passed by the player)")]
		//public CheckPoint CurrentCheckpoint;

		//[Header("Points of Entry")]
		///// A list of this level's points of entry, which can be used from other levels as initial targets
		//[Tooltip("A list of this level's points of entry, which can be used from other levels as initial targets")]
		//public Transform[] PointsOfEntry;

		//[Space(10)]
		//[Header("Intro and Outro durations")]
		//[MMInformation("Here you can specify the length of the fade in and fade out at the start and end of your level. You can also determine the delay before a respawn.",MMInformationAttribute.InformationType.Info,false)]
		///// duration of the initial fade in (in seconds)
		//[Tooltip("the duration of the initial fade in (in seconds)")]
		//public float IntroFadeDuration=1f;

		//public float SpawnDelay = 0f;
		///// duration of the fade to black at the end of the level (in seconds)
		//[Tooltip("the duration of the fade to black at the end of the level (in seconds)")]
		//public float OutroFadeDuration=1f;
		///// the ID to use when triggering the event (should match the ID on the fader you want to use)
		//[Tooltip("the ID to use when triggering the event (should match the ID on the fader you want to use)")]
		//public int FaderID = 0;
		///// the curve to use for in and out fades
		//[Tooltip("the curve to use for in and out fades")]
		//public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
		///// duration between a death of the main character and its respawn
		//[Tooltip("the duration between a death of the main character and its respawn")]
		//public float RespawnDelay = 2f;

		//[Header("Respawn Loop")]
		///// the delay, in seconds, before displaying the death screen once the player is dead
		//[Tooltip("the delay, in seconds, before displaying the death screen once the player is dead")]
		//public float DelayBeforeDeathScreen = 1f;

		//[Header("Bounds")]
		///// if this is true, this level will use the level bounds defined on this LevelManager. Set it to false when using the Rooms system.
		//[Tooltip("if this is true, this level will use the level bounds defined on this LevelManager. Set it to false when using the Rooms system.")]
		//public bool UseLevelBounds = true;

		//[Header("Scene Loading")]
		///// the method to use to load the destination level
		//[Tooltip("the method to use to load the destination level")]
		//public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
		///// the name of the MMSceneLoadingManager scene you want to use
		//[Tooltip("the name of the MMSceneLoadingManager scene you want to use")]
		//[MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
		//public string LoadingSceneName = "LoadingScreen";
		///// the settings to use when loading the scene in additive mode
		//[Tooltip("the settings to use when loading the scene in additive mode")]
		//[MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
		//public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings; 

		//[Header("Feedbacks")] 
		///// if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it
		//[Tooltip("if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it")]
		//public bool SetPlayerAsFeedbackRangeCenter = false;

		///// the level limits, camera and player won't go beyond this point.
		//public Bounds LevelBounds {  get { return (_collider==null)? new Bounds(): _collider.bounds; } }
		//public Collider BoundsCollider { get; protected set; }

		///// the elapsed time since the start of the level
		//public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}

		//// private stuff
		//public List<CheckPoint> Checkpoints { get; protected set; }
		//public List<Character> Players { get; protected set; }

		//protected DateTime _started;
		//protected int _savedPoints;
		//protected Collider _collider;
		//protected Vector3 _initialSpawnPointPosition;

		public EnemyDatas enemyDatas;
		public EnemyGroupDatas enemyGroupDatas;
		public int FinalBossID = -1;


		public Guid currentBattleGUID;

#if DEBUG || UNITY_EDITOR


        public bool useEnemySaveData = false;
#endif
		public class EnemyDataJson
        {
			[SerializeField]
			public EnemyDatas.EnemyData[] datas;
		}
		public class EnemyDataGroupJson
		{
			[SerializeField]
			public EnemyGroupDatas.EnemyGroupData[] datas;
		}
		public static SurviveLevelManager GetInstance()
        {
			return Instance as SurviveLevelManager;
        }

		/// <summary>
		/// On awake, instantiates the player
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			_collider = this.GetComponent<Collider>();
			_initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;


#if DEBUG || UNITY_EDITOR

			if (useEnemySaveData)
			{
				EnemyDataJson eDatas = (EnemyDataJson)TestDataLoad(typeof(EnemyDataJson), "eDatas", "TEST_DATAS");
				if (eDatas != null)
				{
					enemyDatas = new EnemyDatas() { datas = eDatas.datas };
				}

				TestDataSave(new EnemyDataJson() { datas = enemyDatas.datas }, "eDatas", "TEST_DATAS");

				EnemyDataGroupJson groupDatas = (EnemyDataGroupJson)TestDataLoad(typeof(EnemyDataGroupJson), "groupDatas", "TEST_DATAS");
				if (groupDatas != null)
				{
					enemyGroupDatas = new EnemyGroupDatas() { datas = groupDatas.datas };
				}

				TestDataSave(new EnemyGroupDatas() { datas = enemyGroupDatas.datas }, "groupDatas", "TEST_DATAS");
			}
#endif


			currentBattleGUID = Guid.NewGuid();
		}


		public object TestDataLoad(System.Type objectType, string fileName, string foldername = "")
		{
			string savePath;
			string _baseFolderName = "/MMData/";
			object returnObject;
			// depending on the device we're on, we assemble the path
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
			else
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
#if UNITY_EDITOR
			savePath = Application.dataPath + _baseFolderName;
#endif
			savePath = savePath + foldername + "/";
			string saveFileName = savePath + fileName;
			if (!Directory.Exists(savePath) || !File.Exists(saveFileName))
			{
				return null;
			}

			FileStream saveFile = File.Open(saveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			try
			{
				StreamReader streamReader = new StreamReader(saveFile, System.Text.Encoding.UTF8);
				string json = streamReader.ReadToEnd();
				returnObject  = JsonUtility.FromJson(json, objectType);
				// if you prefer using NewtonSoft's JSON lib uncomment the line below and commment the line above
				//savedObject = Newtonsoft.Json.JsonConvert.DeserializeObject(json,objectType);
				streamReader.Close();

				saveFile.Close();
			}
			catch
			{
				returnObject = null;
				saveFile.Close();
			}
			return returnObject;
		}
		public void TestDataSave(object saveObject, string fileName, string foldername = "")
		{
		
			string savePath;
			string _baseFolderName = "/MMData/";
			// depending on the device we're on, we assemble the path
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
			else
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
#if UNITY_EDITOR
			savePath = Application.dataPath + _baseFolderName;
#endif
			string saveFileName = fileName;
			savePath = savePath + foldername + "/";
			// if the directory doesn't already exist, we create it
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}
			// we serialize and write our object into a file on disk

			FileStream saveFile = File.Create(savePath + saveFileName);

			string json = JsonUtility.ToJson(saveObject);
			// if you prefer using NewtonSoft's JSON lib uncomment the line below and commment the line above
			//string json = Newtonsoft.Json.JsonConvert.SerializeObject(objectToSave);
			StreamWriter streamWriter = new StreamWriter(saveFile);
			streamWriter.Write(json);
			streamWriter.Close();
			saveFile.Close();
		}

		/// <summary>
		/// On Start we grab our dependencies and initialize spawn
		/// </summary>
		protected override void Start()
		{
			StartCoroutine(InitializationCoroutine());
		}

		protected void Update()
        {
			if (SurviveGameManager.GetInstance() != null && SurviveGameManager.GetInstance().surviveRuleManager != null && SurviveGameManager.GetInstance().Playing)
			{
				SurviveGameManager.GetInstance().surviveRuleManager.TimeUpdate();

			}
		}
		protected override IEnumerator InitializationCoroutine()
		{
			if (SpawnDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(SpawnDelay);    
			}

			BoundsCollider = _collider;
			InstantiatePlayableCharacters();

			if (UseLevelBounds)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider);
			}            
            
			if (Players == null || Players.Count == 0) { yield break; }

			Initialization();

			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);

			// we handle the spawn of the character(s)
			if (Players.Count == 1)
			{
				SpawnSingleCharacter();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment();

			// we trigger a fade
			MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

			// we trigger a level start event
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			MMGameEvent.Trigger("CameraBound");
		}

		/// <summary>
		/// A method meant to be overridden by each multiplayer level manager to describe how to spawn characters
		/// </summary>
		protected override void SpawnMultipleCharacters()
		{

		}

		/// <summary>
		/// Instantiate playable characters based on the ones specified in the PlayerPrefabs list in the LevelManager's inspector.
		/// </summary>
		protected override void InstantiatePlayableCharacters()
		{
			Players = new List<Character> ();

			var gameManager = SurviveGameManager.GetInstance();
			gameManager.SetCharaData();
			gameManager.abilitySelectSystem.Initiralize();
			if (gameManager.currentData != null)
			{
                Character newPlayer = Instantiate(gameManager.currentData.Prefab, _initialSpawnPointPosition, Quaternion.identity).GetComponent<Character>();
				//ステータスなどの反映はここで行った方がよさそう

				
				newPlayer.name = gameManager.currentData.NAME;

				var sPlayer = newPlayer as SurvivePlayer;
				var permanentEffectLevels = SurviveProgressManager.Instance.permanentEffectLevels;
				sPlayer.ReflectionData(gameManager.currentData, permanentEffectLevels);
#if UNITY_EDITOR || DEBUG
				if(gameManager.muteki)
					sPlayer.CharacterHealth.enabled = false;
#endif
				//いったん
				sPlayer.CursolOn = true;



                Players.Add(newPlayer);
				return;
			}
            //if (GameManager.Instance.PersistentCharacter != null)
            //{
            //	Players.Add(GameManager.Instance.PersistentCharacter);
            //	return;
            //}

            //// we check if there's a stored character in the game manager we should instantiate
            //if (GameManager.Instance.StoredCharacter != null)
            //{
            //	Character newPlayer = (Character)Instantiate(GameManager.Instance.StoredCharacter, _initialSpawnPointPosition, Quaternion.identity);
            //	newPlayer.name = GameManager.Instance.StoredCharacter.name;
            //	Players.Add(newPlayer);
            //	return;
            //}

            //if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
            //{
            //	foreach (Character character in SceneCharacters)
            //	{
            //		Players.Add(character);
            //	}
            //	return;
            //}

            //if (PlayerPrefabs == null) { return; }

            // player instantiation
            if (PlayerPrefabs.Count() != 0)
			{ 
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = (Character)Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
					newPlayer.name = playerPrefab.name;
					Players.Add(newPlayer);

					if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
					}
				}
			}
		}

		/// <summary>
		/// Assigns all respawnable objects in the scene to their checkpoint
		/// </summary>
		protected override void CheckpointAssignment()
		{
			// we get all respawnable objects in the scene and attribute them to their corresponding checkpoint
			IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>().OfType<Respawnable>();
			AutoRespawn autoRespawn;
			foreach (Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i >= 0; i--)
				{
					autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
					if (autoRespawn == null)
					{
						Checkpoints[i].AssignObjectToCheckPoint(listener);
						continue;
					}
					else
					{
						if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
						{
							Checkpoints[i].AssignObjectToCheckPoint(listener);
							continue;
						}
						else
						{
							if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
							{
								Checkpoints[i].AssignObjectToCheckPoint(listener);
								continue;
							}
							continue;
						}
					}
				}
			}
		}


		/// <summary>
		/// Gets current camera, points number, start time, etc.
		/// </summary>
		protected override void Initialization()
		{
			Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
			_savedPoints =GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

		/// <summary>
		/// Spawns a playable character into the scene
		/// </summary>
		protected override void SpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], point.FacingDirection);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
				return;
			}

			if (InitialSpawnPoint != null)
			{
				InitialSpawnPoint.SpawnPlayer(Players[0]);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
				return;
			}

		}

		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public override void GotoLevel(string levelName)
		{
			TriggerEndLevelEvents();
			StartCoroutine(GotoLevelCo(levelName));
		}

		/// <summary>
		/// Triggers end of level events
		/// </summary>
		public override void TriggerEndLevelEvents()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
			MMGameEvent.Trigger("Save");
		}

		/// <summary>
		/// Waits for a short time and then loads the specified level
		/// </summary>
		/// <returns>The level co.</returns>
		/// <param name="levelName">Level name.</param>
		protected override IEnumerator GotoLevelCo(string levelName)
		{
			if (Players != null && Players.Count > 0)
			{ 
				foreach (Character player in Players)
				{
					player.Disable ();	
				}	    		
			}

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);
            
			if (Time.timeScale > 0.0f)
			{ 
				yield return new WaitForSeconds(OutroFadeDuration);
			}
			// we trigger an unPause event for the GameManager (and potentially other classes)
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

			switch (LoadingSceneMode)
			{
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);			        
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}

		/// <summary>
		/// Kills the player.
		/// </summary>
		public override void PlayerDead(Character playerCharacter)
		{
			if (Players.Count < 2)
			{
				StartCoroutine (PlayerDeadCo ());
			}
		}

		/// <summary>
		/// Triggers the death screen display after a short delay
		/// </summary>
		/// <returns></returns>
		protected override IEnumerator PlayerDeadCo()
		{
			yield return new WaitForSeconds(DelayBeforeDeathScreen);
			//金計算しておく
			var player = (Players[0] as SurvivePlayer);
			var c_q = UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Max(0, player.battleParameter.EnemyDeathCount * 0.1f * player.battleParameter.GoldUp));


            float bonus = Array.Find(SurviveGameManager.GetInstance().stageDatas.datas, (_) => _.ID == SurviveGameManager.GetInstance().currentStageID).BonusCoinRate;
			c_q = Mathf.FloorToInt( c_q * bonus);
            player.AddCoin(c_q, false);

            GUIManager.Instance.SetDeathScreen(true);
		}

		/// <summary>
		/// Initiates the respawn
		/// </summary>
		protected override void Respawn()
		{
			if (Players.Count < 2)
			{
				StartCoroutine(SoloModeRestart());
			}
		}

		/// <summary>
		/// Coroutine that kills the player, stops the camera, resets the points.
		/// </summary>
		/// <returns>The player co.</returns>
		protected override IEnumerator SoloModeRestart()
		{
			if ((PlayerPrefabs.Count() <= 0) && (SceneCharacters.Count <= 0))
			{
				yield break;
			}

			// if we've setup our game manager to use lives (meaning our max lives is more than zero)
			if (GameManager.Instance.MaximumLives > 0)
			{
				// we lose a life
				GameManager.Instance.LoseLife();
				// if we're out of lives, we check if we have an exit scene, and move there
				if (GameManager.Instance.CurrentLives <= 0)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
					if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
					{
						MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
					}
				}
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
			yield return new WaitForSeconds(OutroFadeDuration);

			yield return new WaitForSeconds(RespawnDelay);
			GUIManager.Instance.SetPauseScreen(false);
			GUIManager.Instance.SetDeathScreen(false);
			MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = InitialSpawnPoint;
			}

			if (Players[0] == null)
			{
				InstantiatePlayableCharacters();
			}

			if (CurrentCheckpoint != null)
			{
				CurrentCheckpoint.SpawnPlayer(Players[0]);
			}
			else
			{
				Debug.LogWarning("LevelManager : no checkpoint or initial spawn point has been defined, can't respawn the Player.");
			}

			_started = DateTime.UtcNow;
			
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
			yield break;
		}


		/// <summary>
		/// Toggles Character Pause
		/// </summary>
		public override void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{
				CharacterPause characterPause = player.FindAbility<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}

		/// <summary>
		/// Freezes the character(s)
		/// </summary>
		public override void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

		/// <summary>
		/// Unfreezes the character(s)
		/// </summary>
		public override void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

		/// <summary>
		/// Sets the current checkpoint with the one set in parameter. This checkpoint will be saved and used should the player die.
		/// </summary>
		/// <param name="newCheckPoint"></param>
		public override void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			if (newCheckPoint.ForceAssignation)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}
			if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
			{
				CurrentCheckpoint = newCheckPoint;
			}
		}

		/// <summary>
		/// Catches TopDownEngineEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="engineEvent">TopDownEngineEvent event.</param>
		public override void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					PlayerDead(engineEvent.OriginCharacter);
					break;
				case TopDownEngineEventTypes.RespawnStarted:
					Respawn();
					break;
			}
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected override void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected override void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}