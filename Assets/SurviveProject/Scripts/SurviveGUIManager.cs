using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Handles all GUI effects and changes
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/SurviveGUIManager")]
	public class SurviveGUIManager : GUIManager, MMEventListener<LevelUpEvent>, MMEventListener<BoxEvent>
	{
        public MMProgressBar EXPBars;
        public TextMeshProUGUI Level;
        public TextMeshProUGUI Coin;

		public LevelUpUI levelUpUI;
		public BoxUI boxUI;

		public Transform DashCountParent;
		public Image DashCountBase;
		private List<Image> dashCounts = new List<Image>();

		public List<Image> itemInventoryImages = new List<Image>();
		public List<Image> skillInventoryImages = new List<Image>();
		public List<int> itemInventoryIDs = new List<int>();
		public List<int> skillInventoryIDs = new List<int>();

		public TextMeshProUGUI HPText;
		public TextMeshProUGUI EnemyDeathCountText;

		public Image CharacterFaceImage;

#if DEBUG || UNITY_EDITOR
		public TextMeshProUGUI _enemycount;
#endif

		[System.Serializable]
		public class CharaImageData
        {
			public Sprite Image;
			public int ID; 
			public Vector3 Pos;
		}
		[SerializeField]
		public List<CharaImageData> charaImageDatas = new List<CharaImageData>();
		public ItemDisplayUI itemDisplayUI;

		public GameObject FirstPlayTipsUI;
		public GameObject FirstPlayTipsUISubmit;
        public static SurviveGUIManager GetInstance()
		{
			return Instance as SurviveGUIManager;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

				itemInventoryIDs = new List<int>();
			for (int i = 0; i < itemInventoryImages.Count; i++)
			{
				itemInventoryIDs.Add(0);

			}

				skillInventoryIDs = new List<int>();
			for (int i = 0; i < skillInventoryImages.Count; i++)
			{
				skillInventoryIDs.Add(0);
			}
			//Initialization();
		}

		protected override void Initialization()
		{
			if (_initialized)
			{
				return;
			}

			if (Joystick != null)
			{
				_initialJoystickAlpha = Joystick.alpha;
			}
			if (Buttons != null)
			{
				_initialButtonsAlpha = Buttons.alpha;
			}

			_initialized = true;

			this.MMEventStartListening<LevelUpEvent>();
			this.MMEventStartListening<BoxEvent>();
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected override void Start()
		{
			RefreshPoints();
			SetPauseScreen(false);
			SetDeathScreen(false);
#if DEBUG || UNITY_EDITOR

			EnemySpawner.OnCurrentObjChanges += OnCurrentObjChanges;
#endif
		}

        /// <summary>
        /// Sets the HUD active or inactive
        /// </summary>
        /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
        public override void SetHUDActive(bool state)
		{
			if (HUD!= null)
			{ 
				HUD.SetActive(state);
			}
			if (PointsText!= null)
			{ 
				PointsText.enabled = state;
			}
		}

		/// <summary>
		/// Sets the avatar active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public override void SetAvatarActive(bool state)
		{
			if (HUD != null)
			{
				HUD.SetActive(state);
			}
		}

		/// <summary>
		/// Called by the input manager, this method turns controls visible or not depending on what's been chosen
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="movementControl">Movement control.</param>
		public override void SetMobileControlsActive(bool state, InputManager.MovementControls movementControl = InputManager.MovementControls.Joystick)
		{
			Initialization();
            
			if (Joystick != null)
			{
				Joystick.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Joystick)
				{
					Joystick.alpha=_initialJoystickAlpha;
				}
				else
				{
					Joystick.alpha=0;
					Joystick.gameObject.SetActive (false);
				}
			}

			if (Arrows != null)
			{
				Arrows.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Arrows)
				{
					Arrows.alpha=_initialJoystickAlpha;
				}
				else
				{
					Arrows.alpha=0;
					Arrows.gameObject.SetActive (false);
				}
			}

			if (Buttons != null)
			{
				Buttons.gameObject.SetActive(state);
				if (state)
				{
					Buttons.alpha=_initialButtonsAlpha;
				}
				else
				{
					Buttons.alpha=0;
					Buttons.gameObject.SetActive (false);
				}
			}
		}

		/// <summary>
		/// Sets the pause screen on or off.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public override void SetPauseScreen(bool state)
		{
			if (PauseScreen != null)
			{
				PauseScreen.SetActive(state);
				PauseScreen.SetActive(state);
				if(state == true)
					PauseScreen.GetComponent<PauseMenuUI>()?.SetData();
				EventSystem.current.sendNavigationEvents = state;
			}
		}

		/// <summary>
		/// Sets the death screen on or off.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public override void SetDeathScreen(bool state)
		{
			if (DeathScreen != null)
			{
				DeathScreen.SetActive(state);
				if (state == true)
				{
					DeathScreen.GetComponent<GameEndUI>().SetData(false);
					Cursor.visible = true;
				}
				EventSystem.current.sendNavigationEvents = state;
			}
		}
		public void SetGameClearScreen(bool state)
		{
			if (DeathScreen != null)
			{
				DeathScreen.SetActive(state);
				if (state == true)
				{
					DeathScreen.GetComponent<GameEndUI>().SetData(true);
				}
				EventSystem.current.sendNavigationEvents = state;
			}
		}
		/// <summary>
		/// Sets the jetpackbar active or not.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public override void SetDashBar(bool state, string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar jetpackBar in DashBars)
			{
				if (jetpackBar != null)
				{ 
					if (jetpackBar.PlayerID == playerID)
					{
						jetpackBar.gameObject.SetActive(state);
					}					
				}
			}	        
		}

		/// <summary>
		/// Sets the ammo displays active or not
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="playerID">Player I.</param>
		public override void SetAmmoDisplays(bool state, string playerID, int ammoDisplayID)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay != null)
				{ 
					if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
					{
						ammoDisplay.gameObject.SetActive(state);
					}					
				}
			}
		}
        		
		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public override void RefreshPoints()
		{
			if (PointsText!= null)
			{ 
				PointsText.text = GameManager.Instance.Points.ToString(PointsTextPattern);
			}
		}

		/// <summary>
		/// Updates the health bar.
		/// </summary>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="minHealth">Minimum health.</param>
		/// <param name="maxHealth">Max health.</param>
		/// <param name="playerID">Player I.</param>
		public override void UpdateHealthBar(float currentHealth,float minHealth,float maxHealth,string playerID)
		{
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0)	{ return; }

			foreach (MMProgressBar healthBar in HealthBars)
			{
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID)
				{
					healthBar.UpdateBar(currentHealth,minHealth,maxHealth);
				}
			}

		}

		/// <summary>
		/// Updates the dash bars.
		/// </summary>
		/// <param name="currentFuel">Current fuel.</param>
		/// <param name="minFuel">Minimum fuel.</param>
		/// <param name="maxFuel">Max fuel.</param>
		/// <param name="playerID">Player I.</param>
		public override void UpdateDashBars(float currentFuel, float minFuel, float maxFuel,string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar dashbar in DashBars)
			{
				if (dashbar == null) { return; }
				if (dashbar.PlayerID == playerID)
				{
					dashbar.UpdateBar(currentFuel,minFuel,maxFuel);	
				}    
			}
		}

		/// <summary>
		/// Updates the (optional) ammo displays.
		/// </summary>
		/// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
		/// <param name="totalAmmo">Total ammo.</param>
		/// <param name="maxAmmo">Max ammo.</param>
		/// <param name="ammoInMagazine">Ammo in magazine.</param>
		/// <param name="magazineSize">Magazine size.</param>
		/// <param name="playerID">Player I.</param>
		/// <param name="displayTotal">If set to <c>true</c> display total.</param>
		public override void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, string playerID, int ammoDisplayID, bool displayTotal)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay == null) { return; }
				if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
				{
					ammoDisplay.UpdateAmmoDisplays (magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, displayTotal);
				}    
			}
		}

		public void UpdateExpValue(float current, float max )
		{
			EXPBars.UpdateBar(current, 0, max);
		}

		public void UpdateLevelValue(int current)
		{
			Level.text = current.ToString();
		}
		public void UpdateCoin(int current)
		{
			Coin.text = current.ToString();
		}
		public void UpdateCharaImage()
		{
			var data = charaImageDatas.Find(_ => _.ID == SurviveGameManager.GetInstance().currentCharacterID);
			CharacterFaceImage.sprite = data.Image;
			CharacterFaceImage.GetComponent<RectTransform>().anchoredPosition3D = data.Pos;
		}
		public void UpdateDashCount(int current, int max)
		{
			if(dashCounts.Count != max)
            {
				for(int i = dashCounts.Count; i < max; i++)
                {
					dashCounts.Add(Instantiate(DashCountBase, DashCountParent));
					dashCounts[i].gameObject.SetActive(true);

				}
            }

			for (int i = 0; i < dashCounts.Count; i++)
            {
				if(i < current)
                {
					dashCounts[i].color = Color.white;
                }
                else
                {
					dashCounts[i].color = Color.gray;
				}
			}
		}

		public void UpdateItemInventory(int abilityId, int index)
        {
			itemInventoryImages[index].sprite = SurviveGameManager.GetInstance().GetAbilitysCopy().Find(_ => _.ID == abilityId).image;
			itemInventoryImages[index].enabled = true;
			itemInventoryIDs[index] = abilityId;
		}
		public void UpdateSkillInventory(int abilityId, int index)
        {
			skillInventoryImages[index].sprite = SurviveGameManager.GetInstance().GetAbilitysCopy().Find(_ => _.ID == abilityId).image;
			skillInventoryImages[index].enabled = true;
			skillInventoryIDs[index] = abilityId;
		}

		public void UpdateInventory(int skillNum, int itemNum, Dictionary<int, int> getAbilityDic)
        {
			if(skillInventoryImages.Count<skillNum)
            {
				for(int i = skillInventoryImages.Count; i < skillNum; i++)
                {
					var obj = Instantiate( skillInventoryImages[0].transform.parent.gameObject, skillInventoryImages[0].transform.parent.parent);
					var target = obj.transform.Find("Image").GetComponent<Image>();
					target.enabled = false;
					skillInventoryImages.Add(target);
					skillInventoryIDs.Add(0);
				}
            }
			if (itemInventoryImages.Count < itemNum)
			{
				for (int i = itemInventoryImages.Count; i < itemNum; i++)
				{
					var obj = Instantiate(itemInventoryImages[0].transform.parent.gameObject, itemInventoryImages[0].transform.parent.parent);
					var target = obj.transform.Find("Image").GetComponent<Image>();
					target.enabled = false;
					itemInventoryImages.Add(target);
					itemInventoryIDs.Add(0);
				}
			}
			var abiData = SurviveGameManager.GetInstance().GetAbilitysCopy();

			for (int i = 0; i < skillInventoryImages.Count;  i++)
			{
				var level = skillInventoryImages[i].transform.Find("Level");
				var value = skillInventoryImages[i].transform.Find("LevelValue");

				if (skillInventoryIDs.Count > i && skillInventoryIDs[i] > 0)
				{
					var data = SurviveGameManager.GetInstance().GetAbilitysCopy().Find(_ => _.ID == skillInventoryIDs[i]);
					var l = (getAbilityDic[skillInventoryIDs[i]] + 1);
					level.gameObject.SetActive(true);
					value.gameObject.SetActive(true);
					value.GetComponent<TextMeshProUGUI>().text = l.ToString();
					if (data.WeaponLevelDatas.Count == l)
					{
						value.GetComponent<TextMeshProUGUI>().text = "MAX";
						level.GetComponent<TextMeshProUGUI>().color = Color.yellow;
						value.GetComponent<TextMeshProUGUI>().color = Color.yellow;

					}
					else
					{
						level.GetComponent<TextMeshProUGUI>().color = Color.white;
						value.GetComponent<TextMeshProUGUI>().color = Color.white;

					}
				}
				else
				{
					level.gameObject.SetActive(false);
					value.gameObject.SetActive(false);

				}
			}
			for (int i = 0; i < itemInventoryImages.Count; i++)
			{
				var level = itemInventoryImages[i].transform.Find("Level");
				var value = itemInventoryImages[i].transform.Find("LevelValue");

				if (itemInventoryIDs.Count > i && itemInventoryIDs[i] > 0)
				{
					var data = SurviveGameManager.GetInstance().GetAbilitysCopy().Find(_ => _.ID == itemInventoryIDs[i]);
					var l = (getAbilityDic[itemInventoryIDs[i]] + 1);
					level.gameObject.SetActive(true);
					value.gameObject.SetActive(true);
					value.GetComponent<TextMeshProUGUI>().text = l.ToString();
					if(data.ItemLevelDatas.Count == l)
                    {
						value.GetComponent<TextMeshProUGUI>().text = "MAX";
						level.GetComponent<TextMeshProUGUI>().color = Color.yellow;
						value.GetComponent<TextMeshProUGUI>().color = Color.yellow;

					}
                    else
                    {
						level.GetComponent<TextMeshProUGUI>().color = Color.white;
						value.GetComponent<TextMeshProUGUI>().color = Color.white;

					}
                }
                else
                {
					level.gameObject.SetActive(false);
					value.gameObject.SetActive(false);

				}

			}
		}

		public void UpdateHPText()
        {
			if (LevelManager.Instance != null&&LevelManager.Instance.Players != null && LevelManager.Instance.Players.Count > 0)
			{
				var h = LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<SurviveHealth>();
				HPText.text = h.CurrentHealth.ToString() + "/" + h.MaximumHealth.ToString();
			}
		}
		public void UpdateHPText(int current, int max)
		{
			{
				HPText.text = current.ToString() + "/" + max.ToString();
			}
		}


		public void UpdateEnemyDeathCountText(int count)
		{
			EnemyDeathCountText.text = count.ToString();
		}
		protected void OnDisable()
		{
			this.MMEventStopListening<LevelUpEvent>();
			this.MMEventStopListening<BoxEvent>();

		}
		public void OnMMEvent(LevelUpEvent eventData)
		{
			levelUpUI.Open(eventData);
		}
		public void OnMMEvent(BoxEvent eventData)
		{
			boxUI.Open(eventData);
		}
		public void SetDisplayItem(InventoryEngine.SurviveItemPicker itemPicker)
		{
			itemDisplayUI.SetDisplayItem(itemPicker);
		}
		public void RemoveDisplayItem(InventoryEngine.SurviveItemPicker itemPicker)
		{
			itemDisplayUI.RemoveDisplayItem(itemPicker);
		}

#if DEBUG || UNITY_EDITOR
        private void OnCurrentObjChanges(int obj)
		{
			if(_enemycount != null)
			_enemycount.text = obj.ToString();
        }
#endif



		public void OpenFirstPlayTipsUI()
        {
			

			StartCoroutine(OpenFirstPlayTipsUICoroutine());
		}

		public IEnumerator OpenFirstPlayTipsUICoroutine()
        {
			yield return 0;
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			SurviveGameManager.GetInstance().OpendFirstPlayUI = true;
			SurviveGameManager.Instance.Paused = true;

			LevelManager.Instance.ToggleCharacterPause();

			//FirstPlayTipsUI.SetActive(true);
            EventSystem.current.SetSelectedGameObject(FirstPlayTipsUISubmit);
			EventSystem.current.sendNavigationEvents = true;
        }
        public void CloseFirstPlayTipsUI()
        {
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			SurviveGameManager.GetInstance().OpendFirstPlayUI = false;
			SurviveGameManager.Instance.Paused = false;
			LevelManager.Instance.ToggleCharacterPause();
			FirstPlayTipsUI.SetActive(false);
        EventSystem.current.sendNavigationEvents = false;
        }
    }
}