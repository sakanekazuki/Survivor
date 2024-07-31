using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SurviveGameManager : GameManager
{
    //ゲームの基本ルールの管理
    public SurviveRuleManager surviveRuleManager = new SurviveRuleManager();

	public CharacterDatas characterDatas;
	public CharacterDatas.CharacterData currentData;

	public StageDatas stageDatas;

	public int currentCharacterID = 0;
	public int currentStageID = 0;

	public float TestLimitTime = 0f;
	public float LimitTime = 1800f;

	public int TestInitialTime = 0;


	public AbilitySelectSystem abilitySelectSystem;

    public List<AbilityData> abilityDatas;
    public AbilityData HealAbility;
    public AbilityData CoinAbility;
    public AbilityData BoxCoinAbility;
	public PlayerEXPData playerEXPData;
	public PlayerLevelGainPointData playerLevelGainPointData;
	public PlayerAttributeNeedPointData playerAttributeNeedPointData;
	public AbilityRarityData abilityRarityData;
	public ShopItemDatas shopItemDatas;
	public AbilityRarityData boxRarityData;
	public ThrowAbilityListData throwAbilityListData;

	public GameObject ShieldEffect;

	[MMInspectorButton("ClearSingltone")]
	public bool ClearSingltoneSingleton;

	public void ClearSingltoneButton()
    {
		SurviveGameManager.ClearSingltone();

	}

	static public void ClearSingltone()
    {
		_instance = null;

	}

	public AbilityData GetAbility(int id)
	{
		return GetAbilitysCopy().Find(_ => _.ID == id);
	}

	public List<AbilityData> GetAbilitysCopy()
    {
		return new List<AbilityData>(abilityDatas);
    }

	//public void SetCharaData(int index)
	//{
	//       currentData = characterDatas.datas[index];
	//}
	[Serializable]
	public class AbilityDataJson
    {
		public int ID;
		public string NAME;
		public List<AbilityData.WeaponLevelData> weaponLevelDatas;
		public List<AbilityData.ItemLevelData> itemLevelDatas;
	}
	protected override void Awake()
    {
        base.Awake();


#if DEBUG || UNITY_EDITOR
		for(int i = 0; i < abilityDatas.Count; i++)
        {
			var abi = abilityDatas[i];
			AbilityDataJson abilityData = (AbilityDataJson)TestDataLoad(typeof(AbilityDataJson), "ability"+abi.ID, "TEST_DATAS");
		if (abilityData != null)
		{
				abilityDatas[i] = new AbilityData() {  ID = abi.ID, abilityDataType = abi.abilityDataType, basePrefab = abi.basePrefab, Description = abi.Description, Description_cn = abi.Description_cn, Description_en = abi.Description_en, Description_tw = abi.Description_tw, hideFlags = abi.hideFlags, image = abi.image, ItemLevelDatas = abilityData.itemLevelDatas, Name = abi.Name, Name_cn = abi.Name_cn, Name_tw = abi.Name_tw, Name_en = abi.Name_en, TargetCharacterID = abi.TargetCharacterID, WeaponLevelDatas = abilityData.weaponLevelDatas };
		}
		
			TestDataSave(new AbilityDataJson() {  itemLevelDatas = abilityDatas[i].ItemLevelDatas, weaponLevelDatas = abilityDatas[i].WeaponLevelDatas, ID = abilityDatas[i].ID, NAME = abilityDatas[i].Name }, "ability" + abilityDatas[i].ID, "TEST_DATAS");
		}


#endif


	}
	object TestDataLoad(System.Type objectType, string fileName, string foldername = "")
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
			returnObject = JsonUtility.FromJson(json, objectType);
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
	void TestDataSave(object saveObject, string fileName, string foldername = "")
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
	public void SetCharaData()
	{
		currentData = Array.Find( characterDatas.datas, _=>_.ID == currentCharacterID);
	}
	public void SetCharaID(int id)
	{
		currentCharacterID = id;
	}

	public void SetStageID(int id)
    {
		currentStageID = id;
    }
	//インスタンス取得でこれ自体を取りたい時用　
	public static SurviveGameManager GetInstance()
    {
		return Instance as SurviveGameManager;
    }
	public bool Playing = false;


	public bool muteki = false;

    public bool OpendLevelUpUI { get;  set; }
    public bool OpendSettingUI { get;  set; }
    public bool OpendFirstPlayUI { get;  set; }

	/// <summary>
	/// Catches TopDownEngineEvents and acts on them, playing the corresponding sounds
	/// </summary>
	/// <param name="engineEvent">TopDownEngineEvent event.</param>
	public override void OnMMEvent(TopDownEngineEvent engineEvent)
	{
		switch (engineEvent.EventType)
		{
			case TopDownEngineEventTypes.TogglePause:
				if(OpendLevelUpUI)
                {
					return;
                }
				if(OpendSettingUI)
				{
					return;
				}
				if(OpendFirstPlayUI)
                {
					return;
				}
				if (Paused)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
				}
				else
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
				}
				break;
			case TopDownEngineEventTypes.Pause:
				Pause();
				Playing = false;
				break;

			case TopDownEngineEventTypes.UnPause:
				UnPause();
				Playing = true;
				break;
			case TopDownEngineEventTypes.LevelStart:
				Playing = true;
#if UNITY_EDITOR
				if (TestLimitTime != 0)
				{
					surviveRuleManager.Initialize(TestLimitTime);
                }
                else
                {
					surviveRuleManager.Initialize(LimitTime);
				}
				if (TestInitialTime != 0)
				{
					surviveRuleManager.AddCurrentTime(TestInitialTime);
				}

#else
				surviveRuleManager.Initialize(LimitTime);
#endif
				if(SurviveProgressManager.Current.Played == false)
                {
					
					//SurviveGUIManager.GetInstance().OpenFirstPlayTipsUI();
					SurviveProgressManager.Current.Played = true;
				}
				break;
			case TopDownEngineEventTypes.LevelComplete:
				Playing = false;
				break;
			case TopDownEngineEventTypes.LevelEnd:
				Playing = false;
				break;
			case TopDownEngineEventTypes.GameOver:
				Playing = false;
				break;

			case TopDownEngineEventTypes.PlayerDeath:
				Playing = false;
				break;
		}
	}

  //  public override void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
  //  {
		//if(pauseMethod == PauseMethods.PauseMenu)
		//{
		//	SurviveGUIManager.GetInstance().PauseScreen
		//}
  //      MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
  //      Instance.Paused = false;
  //      if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
  //      {
  //          GUIManager.Instance.SetPauseScreen(false);
  //          _pauseMenuOpen = false;
  //          SetActiveInventoryInputManager(true);
  //      }
  //      if (_inventoryOpen)
  //      {
  //          _inventoryOpen = false;
  //      }
  //      LevelManager.Instance.ToggleCharacterPause();
  //  }

    /// <summary>
    /// Catches MMGameEvents and acts on them, playing the corresponding sounds
    /// </summary>
    /// <param name="gameEvent">MMGameEvent event.</param>
    public override void OnMMEvent(MMGameEvent gameEvent)
	{
		switch (gameEvent.EventName)
		{
			case "inventoryOpens":
				if (PauseGameWhenInventoryOpens)
				{
					Pause(PauseMethods.NoPauseMenu);
				}
				break;

			case "inventoryCloses":
				if (PauseGameWhenInventoryOpens)
				{
					Pause(PauseMethods.NoPauseMenu);
				}
				break;
		}
	}
	protected virtual void Update()
    {
        if(SurviveGameManager.Instance != this)
        {
			return;
        }
        if (!Paused && Playing == true)
        {
            //surviveRuleManager.TimeUpdate();
            if(surviveRuleManager.TimeEnd()&& !surviveRuleManager.GameEnd && SurviveLevelManager.GetInstance().FinalBossID == -1)
			{
				surviveRuleManager.GameEnd = true;
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelComplete, null);
				Playing = false;
				//MMGameEvent.Trigger("Save");
				//LevelManager.Instance.GotoLevel("SurviveStartScreen");


			}
		}
    }

	public void GameEndMethod()
	{
		surviveRuleManager.GameEnd = true;
		TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelComplete, null);
		Playing = false;
	}
}
