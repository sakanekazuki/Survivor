using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

//


public class SurvivePlayer : Character, MMEventListener<MMInventoryEvent>, MMEventListener<AbilityReflectEvent>, MMEventListener<StatusReflectEvent>, MMEventListener<DashEndEvent>
{

	public Collider2D currentGround;

	[NonSerialized]
	public SurviveCharacterAim characterAim;

	//ID、レベル
	public Dictionary<int, int> GetAbilityDic = new Dictionary<int, int>();

	public List<WeaponSet> weaponSets = new List<WeaponSet>();
	public List<WeaponSet> skillSets = new List<WeaponSet>();
	public List<ItemSet> itemSets = new List<ItemSet>();
	//あとで
	public int InitialWeaponID;

	public bool CursolOn = true;
	public class WeaponSet
    {
		public int AbilityID;
		public GameDefine.StatusAttribute statusAttribute;
		public int statusAttributeValue;
		//多分一個だけ
		public List<SurviveWeaponBase> surviveWeaponBases = new List<SurviveWeaponBase>();
    }
	public class ItemSet
	{
		public int AbilityID;
		public GameDefine.StatusAttribute statusAttribute;
		public int statusAttributeValue;
		public List<AbilityData.ItemStatusData> itemStatusDatas = new List<AbilityData.ItemStatusData>();
	}


	//とりあえず継続回復間隔3秒にしてみる
	private float recoveryTime = 3f;
	private float currentRecoveryTime = 0f;
	/// <summary>
	/// This is called every frame.
	/// </summary>
	protected override void Update()
	{
		EveryFrame();

		//アイテム回収
		AbsorpItem();

		AutoRecovery();

		GuardUpdate();

		KillPowerUpdate();
		EXPAction(this.GetCancellationTokenOnDestroy());
		BoxAction(this.GetCancellationTokenOnDestroy());
	}
	private List<int> AddEXPTasks = new List<int>();
	private List<int> AddBoxTasks = new List<int>();
	private List<int> CurrentEXPTasks = new List<int>();
	private List<int> CurrentBoxTasks = new List<int>();
	bool waitAddExp = false;
	bool waitBox = false;

	private async void EXPAction(System.Threading.CancellationToken cancellationToken)
    {
		if (waitAddExp || waitBox || AddEXPTasks.Count == 0)
			return;
		waitAddExp = true;
		CurrentEXPTasks = new List<int>(AddEXPTasks);
		AddEXPTasks.Clear();
		for(int i = 0; i < CurrentEXPTasks.Count; i++)
        {
			await AddEXP2(CurrentEXPTasks[i], cancellationToken);

		}
		CurrentEXPTasks.Clear();
		waitAddExp = false;

	}
	private async void BoxAction(System.Threading.CancellationToken cancellationToken)
	{
		if (waitAddExp || waitBox ||  AddBoxTasks.Count == 0)
			return;
		waitBox = true;
		CurrentBoxTasks = new List<int>(AddBoxTasks);
		AddBoxTasks.Clear();
		for (int i = 0; i < CurrentBoxTasks.Count; i++)
		{
			await AddBox(CurrentBoxTasks[i], cancellationToken);

		}
		CurrentEXPTasks.Clear();
		waitBox = false;

	}

	public async UniTask AddBox(int add, System.Threading.CancellationToken cancellationToken)
	{
		var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetBoxAbilityDatas();
		int coin = 0;
		switch (datas.Count)
		{
			case 1:
				coin = 50;
				break;
			case 3:
				coin = 100;
				break;
			case 5:
				coin = 200;
				break;
		}
		var time = SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime();
		coin = UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Max(0f, (coin + battleParameter.Level + battleParameter.EnemyDeathCount * 0.01f) * (1 + battleParameter.Luck * 0.1f) * (1 + time * 0.0001f) * battleParameter.GoldUp));
        float bonus = Array.Find(SurviveGameManager.GetInstance().stageDatas.datas, (_) => _.ID == SurviveGameManager.GetInstance().currentStageID).BonusCoinRate;
        coin = Mathf.FloorToInt(coin * bonus);
        bool wait = true;
		BoxEvent.Trigger(datas, coin, 0, this, () =>
		{
			AddCoin(coin);
			wait = false;
		});

		while (wait)
		{
			await UniTask.Yield(cancellationToken);
		}
	}
	public async UniTask AddEXP2(int add, System.Threading.CancellationToken cancellationToken)
	{
		float newExp = battleParameter.EXP + add;
		int addLevel = 0;
		if (newExp >= battleParameter.currentExpData.exp/*プール参照*/)
		{
			addLevel++;
			newExp -= battleParameter.currentExpData.exp;
			//レベルアップ時処理を行う
			//返ってくるの待つか
			var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetSelectAbilityDatas();
			var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
			//player.battleParameter.Level++;
			var level = player.battleParameter.Level;



			//ポイントのやつ後で追加sita

			var pointData = SurviveGameManager.GetInstance().playerLevelGainPointData.data;
			int addPoint = 0;
			if (pointData[pointData.Length - 1].level < level + addLevel)
			{
				addPoint = pointData[pointData.Length - 1].point;

			}
			else
			{
				addPoint = Array.Find(pointData, _ => _.level == level + addLevel).point;

			}

			int point = addPoint + player.battleParameter.ReservePoint;
			bool wait = true;
			LevelUpEvent.Trigger(datas, level, point, player, async () =>
			{
				await AddEXP2(0, cancellationToken);
				wait = false;
			});

			battleParameter.Level += addLevel;
			battleParameter.EXP = newExp;

			var expData = new List<PlayerEXPData.Data>(SurviveGameManager.GetInstance().playerEXPData.data);
			if (expData.Exists(_ => _.level == battleParameter.Level))
			{
				battleParameter.currentExpData = expData.Find(_ => _.level == battleParameter.Level);
			}
			else
			{
				battleParameter.currentExpData = expData[expData.Count - 1];
			}

			SurviveGUIManager sgm2 = SurviveGUIManager.GetInstance();

			sgm2.UpdateLevelValue(battleParameter.Level);
			//
			sgm2.UpdateExpValue(battleParameter.EXP, battleParameter.currentExpData.exp);
			
			while(wait)
            {
				await UniTask.Yield(cancellationToken);
			}
			return;
		}
		battleParameter.Level += addLevel;
		battleParameter.EXP = newExp;
		SurviveGUIManager sgm = SurviveGUIManager.GetInstance();

		sgm.UpdateLevelValue(battleParameter.Level);
		//とりあえずマックス10
		sgm.UpdateExpValue(battleParameter.EXP, battleParameter.currentExpData.exp);
	}
	protected virtual void FixedUpdate()
    {

    }
	float AbsorpMaxPower = 1000f;
	float AbsorpMaxTime = 5f;
	float AbsorpCurrentTime = 0f;
	bool AbsorpMax = false;
	private void AbsorpItem()
	{
        Vector2 _raycastOrigin = new Vector2();

   
            _raycastOrigin.x = transform.position.x/* + _facingDirection.x * DetectionOriginOffset.x / 2*/;
            _raycastOrigin.y = transform.position.y /*+ DetectionOriginOffset.y*/;


		if(AbsorpMax)
        {

			AbsorpCurrentTime += Time.deltaTime;

			if(AbsorpCurrentTime >= AbsorpMaxTime)
            {
				AbsorpCurrentTime = 0f;
				AbsorpMax = false;
            }
			var _AbsorpMaxResults = Physics2D.OverlapCircleAll(_raycastOrigin, AbsorpMaxPower);
			// if there are no targets around, we exit
			//if (numberOfResults == 0)
			//{
			//	_lastReturnValue = false;
			//	return false;
			//}

			// we go through each collider found
			//int min = Mathf.Min(OverlapMaximum, numberOfResults);
			if (_AbsorpMaxResults == null || _AbsorpMaxResults.Length == 0)
			{
				return;
			}
			for (int i = 0; i < _AbsorpMaxResults.Length; i++)
			{
				if (_AbsorpMaxResults[i] == null)
				{
					continue;
				}
				if (_AbsorpMaxResults[i].tag != "Item")
				{
					continue;
				}
				if (_AbsorpMaxResults[i].gameObject.MMGetComponentNoAlloc<SurviveItemPicker>().Item.ItemName != "EXP")
				{
					continue;
				}

				_AbsorpMaxResults[i].transform.position = Vector3.MoveTowards(
				 _AbsorpMaxResults[i].transform.position,
				 transform.position,
				 30f * Time.deltaTime);
			}
			

		}
		//要値調整
        var _results = Physics2D.OverlapCircleAll(_raycastOrigin, battleParameter.PickUP);
        // if there are no targets around, we exit
        //if (numberOfResults == 0)
        //{
        //	_lastReturnValue = false;
        //	return false;
        //}

        // we go through each collider found
        //int min = Mathf.Min(OverlapMaximum, numberOfResults);
        GameObject target = null;
		float PickSpeed = 30f;
        string targetTag = "Item";
        if (_results == null || _results.Length == 0)
        {
            return;
        }
        for (int i = 0; i < _results.Length; i++)
        {
            if (_results[i] == null)
            {
                continue;
            }
            if (_results[i].tag != targetTag)
            {
                continue;
            }
            _results[i].transform.position = Vector3.MoveTowards(
			 _results[i].transform.position,
			 transform.position,
			 PickSpeed * Time.deltaTime);	
        }
    }
	private void AutoRecovery()
	{
		if(battleParameter.NoRegeneration)
        {
			return;
        }
		var re = battleParameter.Regeneration + (MovementState.CurrentState != CharacterStates.MovementStates.Idle ? battleParameter.WalkRegeneration : 0f);
		if (re > 0f)
		{
			currentRecoveryTime += Time.deltaTime;
			if (currentRecoveryTime >= recoveryTime)
			{
				HealPer(re);
				currentRecoveryTime = 0f;
			}
		}
    }

	private void GuardUpdate()
	{
        if (battleParameter.Guard60)
        {
		(CharacterHealth as SurviveHealth).GuardUpdate();
		}

	}
	private void KillPowerUpdate()
    {
		if(KillPower)
        {
			KillPowerTime += Time.deltaTime;
			if(KillPowerTime >= KillPowerMax)
            {
				KillPowerTime = 0f;
				KillPower = false;
            }
        }
    }

	/// <summary>
	/// Initializes this instance of the character
	/// </summary>
	protected override void Awake()
	{
		Initialization();
		this.MMEventStartListening<MMInventoryEvent>();
		this.MMEventStartListening<AbilityReflectEvent>();
		this.MMEventStartListening<StatusReflectEvent>();
		this.MMEventStartListening<DashEndEvent>();
	}

	/// <summary>
	/// Gets and stores input manager, camera and components
	/// </summary>
	protected override void Initialization()
	{
		if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
		{
			CharacterDimension = CharacterDimensions.Type2D;
		}
		if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
		{
			CharacterDimension = CharacterDimensions.Type3D;
		}

		// we initialize our state machines
		MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject, SendStateChangeEvents);
		ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject, SendStateChangeEvents);

		// we get the current input manager
		SetInputManager();
		// we store our components for further use 
		CharacterState = new CharacterStates();
		_controller = this.gameObject.GetComponent<TopDownController>();
		if (CharacterHealth == null)
		{
			CharacterHealth = this.gameObject.GetComponent<Health>();
		}

		CacheAbilitiesAtInit();
		if (CharacterBrain == null)
		{
			CharacterBrain = this.gameObject.GetComponent<AIBrain>();
		}

		if (CharacterBrain != null)
		{
			CharacterBrain.Owner = this.gameObject;
		}

		Orientation2D = FindAbility<CharacterOrientation2D>();
		Orientation3D = FindAbility<CharacterOrientation3D>();
		_characterPersistence = FindAbility<CharacterPersistence>();

		AssignAnimator();

		// instantiate camera target
		if (CameraTarget == null)
		{
			CameraTarget = new GameObject();
		}
		CameraTarget.transform.SetParent(this.transform);
		CameraTarget.transform.localPosition = Vector3.zero;
		CameraTarget.name = "CameraTarget";

		if (LinkedInputManager != null)
		{
			if (OptimizeForMobile && LinkedInputManager.IsMobile)
			{
				if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
				{
					this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
				}
			}
		}
		if(this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>() != null)
        {
            //初期武器取得
            var weapon = SurviveGameManager.GetInstance().GetAbility(InitialWeaponID);

			if (!GetAbilityDic.ContainsKey(weapon.ID))
            {
                GetAbilityDic.Add(weapon.ID, 0);
            }

            var handle = this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>();


            var newweapon = handle.AddWeapon(weapon.basePrefab, weapon.basePrefab.WeaponName, true);
            newweapon.SetParameter(weapon.WeaponLevelDatas[0], GameDefine.StatusAttribute.NONE, weapon.ID);
            var set = new WeaponSet();
            set.AbilityID = weapon.ID;
			newweapon.main = true;
            set.surviveWeaponBases.Add(newweapon);
			set.statusAttribute = GameDefine.StatusAttribute.NONE;
			set.statusAttributeValue = 0;
			SurviveGUIManager.GetInstance().UpdateSkillInventory(set.AbilityID, skillSets.Count);
			skillSets.Add(set);

			if (weapon.ItemLevelDatas != null && weapon.ItemLevelDatas.Count > 0)
			{
				var itemset = new ItemSet();
				itemset.AbilityID = weapon.ID;
				itemset.statusAttribute = GameDefine.StatusAttribute.NONE;
				itemset.statusAttributeValue = 0;
				itemset.itemStatusDatas = weapon.ItemLevelDatas[0].itemStatusDatas;
				itemSets.Add(itemset);

			}
		}
	}




	//バトルパラメータ周りいったんこのへんにまとめる
	public BattleParameter battleParameter = new BattleParameter();

	public bool KillPower = false;
	public float KillPowerMax = 5f;
	public float KillPowerTime = 0f;

    public class BattleParameter
    {
		//完全なベース値の設定もあると楽かも　

		//バトル開始時のベースになる値（直接の強化などで上がるかも？）

		public float ATTACK;
		public float SPEED;
		public int MAX_HP;
		public int DASH_NUM;
		public float DASH_INTERVAL;
		public float DASH_POWER;
		public float CRITICAL;
        public float PickUP;
        public float WeaponInterval;
        public float SkillInterval;
        public int Luck;
        public float Regeneration;
        public float EXPUp;
        public float GoldUp;
        public float AREA;
        public int ThrowNum;
        public float KnockBack;
		public int Shield;
		public int Reroll;
		public int WeaponNum;
		public int ItemNum;
		public float DamageTaken;
		public bool DropBind = false;
		public bool Guard60 = false;
		public bool NoRegeneration = false;
		public float WalkRegeneration = 0f;
		public float MainWeaponInterval = 0f;
		public float MainWeaponArea = 0f;

		public CharacterDatas.CharacterData BaseData;
		public List<int> BasePermanentEffectLevels;
		public List<int> StatusAttributeLevels = new List<int>();
		public List<int> AbilityAttributeLevels = new List<int>();
		public List<int> DefaultStatusAttributeLevels = new List<int>();
		//属性による補正値
		public List<float> StatusCollection = new List<float>(new float[(int)GameDefine.StatusCorrection.NONE]);
		//アイテムによる補正値
		public List<float> ItemCalc = new List<float>(new float[(int)AbilityData.ItemStatusData.ItemStatus.NONE]);
		//ここに置くかは未定
		//経験値プールが必要
		public float EXP;
		public int Level;
		public int Coin;
		public int EnemyDeathCount;

		public int ReservePoint;
		//今持っているスキル等もまとめて持っていてもいいかも


		public int ReserveReroll;

		public PlayerEXPData.Data currentExpData;

		public float GetTargetCollection(GameDefine.StatusCorrection statusCorrection)
        {
			return StatusCollection[(int)statusCorrection];

		}


		public float GetSkillInterval(bool main = false)
        {
			if(main)
            {
				return SkillInterval - MainWeaponInterval;
            }
            else
            {
				return SkillInterval;
			}
        }

		public float GetSkillArea(bool main = false)
        {
			if (main)
			{
				return AREA + MainWeaponArea;
			}
			else
			{
				return AREA;
			}
		}

		public int GetThrowNum(int AbilityID)
        {
			int result = 0;
			if(SurviveGameManager.GetInstance().throwAbilityListData != null  && SurviveGameManager.GetInstance().throwAbilityListData.IDs.Contains(AbilityID))
            {
				result = ThrowNum;
			}
			return result;
        }
	}

	//初期化
	public void ReflectionData(CharacterDatas.CharacterData currentData, List<int> permanentEffectLevels)
    {
		battleParameter = new BattleParameter();
		battleParameter.BaseData = currentData;
		battleParameter.BasePermanentEffectLevels = permanentEffectLevels;
		battleParameter.StatusAttributeLevels = new List<int>(new int[(int)GameDefine.StatusAttribute.NONE]);
		battleParameter.AbilityAttributeLevels = new List<int>(new int[(int)GameDefine.StatusAttribute.NONE]);
		battleParameter.DefaultStatusAttributeLevels = new List<int>(new int[(int)GameDefine.StatusAttribute.NONE]);
		
		for (int i =0;i< battleParameter.DefaultStatusAttributeLevels.Count;i++)
        {
			if (battleParameter.BaseData.AttributeStatusLevel.Length > i)
			{
				battleParameter.DefaultStatusAttributeLevels[i] = battleParameter.BaseData.AttributeStatusLevel[i];
            }
            else
            {
				battleParameter.DefaultStatusAttributeLevels[i] = 1;
			}
		}

		UpdateStatus();

		//      var movement = this.gameObject.MMGetComponentNoAlloc<CharacterMovement>();
		////良い感じにしないと
		//movement.WalkSpeed = battleParameter.SPEED;

		//var dash = this.gameObject.MMGetComponentNoAlloc<SurviveDash>();
		//dash.SetDashParameter(battleParameter.DASH_NUM, battleParameter.DASH_POWER, battleParameter.DASH_INTERVAL);
		////dash2D.DashDuration
		//this.gameObject.MMGetComponentNoAlloc<Health>().MaximumHealth = battleParameter.MAX_HP;
		//this.gameObject.MMGetComponentNoAlloc<Health>().InitialHealth = battleParameter.MAX_HP;
		//体力初期化
		var h = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		h.InitialHealth = h.MaximumHealth;
		h.ResetHealthToMaxHealth();

		battleParameter.Level = 1;
		battleParameter.EXP = 0;
		battleParameter.Coin = 0;
		battleParameter.EnemyDeathCount = 0;
		battleParameter.ReserveReroll = battleParameter.Reroll;
		//ついでに表示初期化するか
		SurviveGUIManager sgm = SurviveGUIManager.GetInstance();
		sgm.UpdateLevelValue(battleParameter.Level);
		//とりあえずマックス10 <- 改修

		//今回のレベルに応じたポイントに余っているものを足す
		var expData = new List<PlayerEXPData.Data>(SurviveGameManager.GetInstance().playerEXPData.data);
		if (expData.Exists(_ => _.level == battleParameter.Level))
		{
			battleParameter.currentExpData = expData.Find(_ => _.level == battleParameter.Level);
		}
        else
        {
			battleParameter.currentExpData = expData[expData.Count - 1];
		}


		sgm.UpdateExpValue(battleParameter.EXP, battleParameter.currentExpData.exp);
		sgm.UpdateCoin(battleParameter.Coin);
		
		//キャラ画像更新
		sgm.UpdateCharaImage();

		//敵倒した数初期化
		sgm.UpdateEnemyDeathCountText(battleParameter.EnemyDeathCount);

		var dash = this.gameObject.MMGetComponentNoAlloc<SurviveDash>();
		sgm.UpdateDashCount(dash.CurrentDashNum, dash.DashNum);
		sgm.UpdateHPText((int)h.CurrentHealth, (int)h.MaximumHealth);
	}
	//アイテムピックのイベントでよぶ;
	//取得量はステータスなどの関係で変わるかも
	public void AddEXP(int add)
    {
		float newExp = battleParameter.EXP + add;
		int addLevel = 0;
		if (newExp >= battleParameter.currentExpData.exp/*プール参照*/)
        {
			addLevel++;
			newExp -= battleParameter.currentExpData.exp;
			//レベルアップ時処理を行う
			//返ってくるの待つか
			var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetSelectAbilityDatas();
			var player = SurviveLevelManager.Instance.Players[0] as SurvivePlayer;
			//player.battleParameter.Level++;
			var level = player.battleParameter.Level;



			//ポイントのやつ後で追加sita

			var pointData = SurviveGameManager.GetInstance().playerLevelGainPointData.data;
			int addPoint = 0;
			if(pointData[pointData.Length-1].level < level+addLevel)
            {
				addPoint = pointData[pointData.Length - 1].point;

			}
            else
            {
				addPoint = Array.Find(pointData, _ => _.level == level + addLevel).point;

			}

			int point = addPoint + player.battleParameter.ReservePoint;
			LevelUpEvent.Trigger(datas, level, point, player, () =>
			{
                AddEXP(0);
            });

			battleParameter.Level += addLevel;
			battleParameter.EXP = newExp;

			var expData = new List<PlayerEXPData.Data>(SurviveGameManager.GetInstance().playerEXPData.data);
			if (expData.Exists(_ => _.level == battleParameter.Level))
			{
				battleParameter.currentExpData = expData.Find(_ => _.level == battleParameter.Level);
			}
			else
			{
				battleParameter.currentExpData = expData[expData.Count - 1];
			}

			SurviveGUIManager sgm2 = SurviveGUIManager.GetInstance();

			sgm2.UpdateLevelValue(battleParameter.Level);
			//
			sgm2.UpdateExpValue(battleParameter.EXP, battleParameter.currentExpData.exp);
			return;
		}
		battleParameter.Level += addLevel;
		battleParameter.EXP = newExp;
		SurviveGUIManager sgm = SurviveGUIManager.GetInstance();

		sgm.UpdateLevelValue(battleParameter.Level);
		//とりあえずマックス10
		sgm.UpdateExpValue(battleParameter.EXP, battleParameter.currentExpData.exp);
	}

	public void AddCoin(int add, bool updateUI = true)
	{
		//さすがに上限超えないだろ……
		battleParameter.Coin += add;
		if (updateUI)
		{
			SurviveGUIManager sgm = SurviveGUIManager.GetInstance();

		
			sgm.UpdateCoin(battleParameter.Coin);
		}
	}


	//アイテムの取得はゲームの進行上一時的なものなので、固有のインベントリのシステムを使う
	//元のインベントリの機能がほとんど動かないようにしてイベントの遷移だけ残す
	public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
		if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
		{
			bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryWeapon)));
			bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryWeapon));

			if (inventoryEvent.EventItem.ItemID == "EXP")
			{
				int e_q = Mathf.FloorToInt(inventoryEvent.Quantity * battleParameter.EXPUp);
				AddEXPTasks.Add(e_q);// AddEXP(e_q);

			}
			else if (inventoryEvent.EventItem.ItemID == "COIN")
			{
				int c_q = Mathf.FloorToInt(inventoryEvent.Quantity * battleParameter.GoldUp);
				AddCoin(c_q);
			}
			else if (inventoryEvent.EventItem.ItemID == "HEALTH")
			{
				//アイテム回復
				HealPer(inventoryEvent.Quantity / 100f);
			}
			else if (inventoryEvent.EventItem.ItemID == "BOX")
			{
				AddBoxTasks.Add(1);// AddEXP(e_q);

				//var datas = SurviveGameManager.GetInstance().abilitySelectSystem.GetBoxAbilityDatas();
				//int coin = 0;
				//switch (datas.Count)
				//{
				//	case 1:
				//		coin = 50;
				//		break;
				//	case 3:
				//		coin = 100;
				//		break;
				//	case 5:
				//		coin = 200;
				//		break;
				//}
				//var time = SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime();
				//coin = UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Max(0f, (coin + battleParameter.Level + battleParameter.EnemyDeathCount * 0.01f) * (1 + battleParameter.Luck * 0.1f) * (1 + time * 0.0001f) * battleParameter.GoldUp));
				//BoxEvent.Trigger(datas, coin, 0, this, () =>
				//{
				//	AddCoin(coin);
				//});

			}
			else if (inventoryEvent.EventItem.ItemID == "HAMAYA")
			{
				Vector2 _raycastOrigin = new Vector2();


				_raycastOrigin.x = transform.position.x/* + _facingDirection.x * DetectionOriginOffset.x / 2*/;
				_raycastOrigin.y = transform.position.y /*+ DetectionOriginOffset.y*/;

				var EnemyDestroy = Physics2D.OverlapCircleAll(_raycastOrigin, 100f);
				// if there are no targets around, we exit
				//if (numberOfResults == 0)
				//{
				//	_lastReturnValue = false;
				//	return false;
				//}

				// we go through each collider found
				//int min = Mathf.Min(OverlapMaximum, numberOfResults);
				if (EnemyDestroy == null || EnemyDestroy.Length == 0)
				{
					return;
				}
				for (int i = 0; i < EnemyDestroy.Length; i++)
				{
					if (EnemyDestroy[i] == null)
					{
						continue;
					}
					if (EnemyDestroy[i].tag != "Enemy")
					{
						continue;
					}
					var enemy = EnemyDestroy[i].GetComponent<SurviveEnemy>();
					if (enemy!= null && enemy.enemyData.PATTERN > 1)
                    {
						continue;
                    }
					if (!EnemyDestroy[i].gameObject.GetComponentInChildren<Renderer>().isVisible)
					{
						continue;
					}

					var h = EnemyDestroy[i].GetComponent<SurviveHealth>();
					if(h != null)
                    {
						h.Kill();
                    }
				}


				//KillPower = true;
			}
			else if (inventoryEvent.EventItem.ItemID == "TSUBO")
			{
				AbsorpMax = true;
			}
			//if (isClass || isSubclass)
			//{
			//	InventoryWeapon inventoryWeapon = (InventoryWeapon)inventoryEvent.EventItem;
			//	switch (inventoryWeapon.AutoEquipMode)
			//	{
			//		case InventoryWeapon.AutoEquipModes.NoAutoEquip:
			//			// we do nothing
			//			break;

			//		case InventoryWeapon.AutoEquipModes.AutoEquip:
			//			_nextFrameWeapon = true;
			//			_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
			//			break;

			//		case InventoryWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
			//			if (CharacterHandleWeapon.CurrentWeapon == null)
			//			{
			//				_nextFrameWeapon = true;
			//				_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
			//			}
			//			break;
			//	}
			//}
		}
	}
	
	
	public void HealPer(float rate)
	{
		if(rate == 0f)
        {
			return;
        }
        var health = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		health.ReceiveHealth(Mathf.Max(1f, health.MaximumHealth * rate), this.gameObject);
    }

    /// <summary>
    /// OnDisable, we unregister our OnRevive event
    /// </summary>
    protected override void OnDisable()
	{
		if (CharacterHealth != null)
		{
			CharacterHealth.OnDeath -= OnDeath;
			CharacterHealth.OnHit -= OnHit;
		}
		this.MMEventStopListening<MMInventoryEvent>();
		this.MMEventStopListening<AbilityReflectEvent>(); 
		this.MMEventStopListening<StatusReflectEvent>(); 
		this.MMEventStopListening<DashEndEvent>();
	}

	public void OnMMEvent(AbilityReflectEvent eventType)
	{
		if (eventType.Ability.abilityDataType == AbilityData.AbilityDataType.Item)
		{
			if (eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas[0].exItemStatus == AbilityData.ItemStatusData.ItemStatus.HEAL_ITEM)
			{
				HealPer(eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas[0].value);
				return;
			}
			else if (eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas[0].exItemStatus == AbilityData.ItemStatusData.ItemStatus.GOLD_ITEM)
			{
				AddCoin((int)eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas[0].value);
				return;
			}
			if (eventType.Level == 0)//初ゲット
			{
				if (!GetAbilityDic.ContainsKey(eventType.Ability.ID))
				{
					GetAbilityDic.Add(eventType.Ability.ID, eventType.Level);
				}

				var set = new ItemSet();
				set.AbilityID = eventType.Ability.ID;
				set.statusAttribute = eventType.statusAttribute;
				set.statusAttributeValue = eventType.statusAttributeValue;
				set.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;

				//ui 更新
				SurviveGUIManager.GetInstance().UpdateItemInventory(set.AbilityID, itemSets.FindAll(_=>!skillSets.Exists(s=>s.AbilityID == _.AbilityID)).Count);
				itemSets.Add(set);
			}
			else
			{
				GetAbilityDic[eventType.Ability.ID] = eventType.Level;
				var target = itemSets.Find(_ => _.AbilityID == eventType.Ability.ID);
				target.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;
			}
			UpdateStatus();
		}
		else if (eventType.Ability.abilityDataType == AbilityData.AbilityDataType.Weapon)
		{
			if (eventType.Level == 0)//初ゲット
			{
				if (!GetAbilityDic.ContainsKey(eventType.Ability.ID))
				{
					GetAbilityDic.Add(eventType.Ability.ID, eventType.Level);
				}

				var handle = this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>();


				var newweapon = handle.AddWeapon(eventType.Ability.basePrefab, eventType.Ability.basePrefab.WeaponName, true);
				newweapon.SetParameter(eventType.Ability.WeaponLevelDatas[eventType.Level], eventType.statusAttribute, eventType.Ability.ID);
				var set = new WeaponSet();
				set.AbilityID = eventType.Ability.ID;
				set.statusAttribute = eventType.statusAttribute;
				set.statusAttributeValue = eventType.statusAttributeValue;
				set.surviveWeaponBases.Add(newweapon);

				
				//ui 更新
				SurviveGUIManager.GetInstance().UpdateSkillInventory(set.AbilityID, skillSets.Count);
				skillSets.Add(set);
				if (eventType.Ability.ItemLevelDatas != null && eventType.Ability.ItemLevelDatas.Count > 0)
				{
					var itemset = new ItemSet();
					itemset.AbilityID = eventType.Ability.ID;
					itemset.statusAttribute = eventType.statusAttribute;
					itemset.statusAttributeValue = eventType.statusAttributeValue;
					itemset.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;
					itemSets.Add(itemset);

				}
			}
			else
			{
				GetAbilityDic[eventType.Ability.ID] = eventType.Level;
				var target = skillSets.Find(_ => _.AbilityID == eventType.Ability.ID);
				foreach (var w in target.surviveWeaponBases)
				{
					w.SetParameter(eventType.Ability.WeaponLevelDatas[eventType.Level], eventType.statusAttribute, target.AbilityID);
				}

				if (eventType.Ability.ItemLevelDatas != null && eventType.Ability.ItemLevelDatas.Count > 0)
				{
					GetAbilityDic[eventType.Ability.ID] = eventType.Level;
					var itemtarget = itemSets.Find(_ => _.AbilityID == eventType.Ability.ID);
					itemtarget.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;

				}
			}

			UpdateStatus();
		}
		else
		{
			if (eventType.Level == 0)//初ゲット
			{
				if (!GetAbilityDic.ContainsKey(eventType.Ability.ID))
				{
					GetAbilityDic.Add(eventType.Ability.ID, eventType.Level);
				}

				var handle = this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>();


				var newweapon = handle.AddWeapon(eventType.Ability.basePrefab, eventType.Ability.basePrefab.WeaponName, true);
				newweapon.SetParameter(eventType.Ability.WeaponLevelDatas[eventType.Level], eventType.statusAttribute, eventType.Ability.ID);
				var set = new WeaponSet();
				set.AbilityID = eventType.Ability.ID;
				set.statusAttribute = eventType.statusAttribute;
				set.statusAttributeValue = eventType.statusAttributeValue;
				set.surviveWeaponBases.Add(newweapon);

				if (eventType.Ability.ItemLevelDatas != null && eventType.Ability.ItemLevelDatas.Count > 0)
				{
					var itemset = new ItemSet();
					itemset.AbilityID = eventType.Ability.ID;
					itemset.statusAttribute = eventType.statusAttribute;
					itemset.statusAttributeValue = eventType.statusAttributeValue;
					itemset.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;
					itemSets.Add(itemset);

				}
				//ui 更新
				SurviveGUIManager.GetInstance().UpdateSkillInventory(set.AbilityID, skillSets.Count);
				skillSets.Add(set);
			}
			else
			{
				GetAbilityDic[eventType.Ability.ID] = eventType.Level;
				var target = skillSets.Find(_ => _.AbilityID == eventType.Ability.ID);
				foreach (var w in target.surviveWeaponBases)
				{
					w.SetParameter(eventType.Ability.WeaponLevelDatas[eventType.Level], eventType.statusAttribute, target.AbilityID);
				}

				if (eventType.Ability.ItemLevelDatas != null && eventType.Ability.ItemLevelDatas.Count > 0)
				{
					GetAbilityDic[eventType.Ability.ID] = eventType.Level;
					var itemtarget = itemSets.Find(_ => _.AbilityID == eventType.Ability.ID);
					itemtarget.itemStatusDatas = eventType.Ability.ItemLevelDatas[eventType.Level].itemStatusDatas;

				}
			}

			UpdateStatus();

		}
	}

    private void UpdateStatus()
    {
		var tempData = new List<int>();
		for (int i = 0; i< battleParameter.StatusAttributeLevels.Count; i++)
        {
			tempData.Add(battleParameter.StatusAttributeLevels[i] + battleParameter.AbilityAttributeLevels[i]);
		}
		battleParameter.StatusCollection = CalcAttributionStatus(tempData);

		battleParameter.ItemCalc = CalcItemDataStatus();

		//ショップ購入レベルデータ
		var PermanentEffects = CalcPermanentDataStatus();


		

		{
            //攻撃力更新
            battleParameter.ATTACK = battleParameter.BaseData.ATTACK + battleParameter.BaseData.ATTACK * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Attack] / 100) + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DAMAGE] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DAMAGE]);
			//HPマックス更新 hpは計算後切り捨て
			battleParameter.MAX_HP = Mathf.FloorToInt(battleParameter.BaseData.MAX_HP + battleParameter.BaseData.MAX_HP * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.MaxHP]/100) + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.HP])+ battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.HP]);
			
			this.gameObject.MMGetComponentNoAlloc<Health>().MaximumHealth = battleParameter.MAX_HP;
			if(this.gameObject.MMGetComponentNoAlloc<Health>().MaximumHealth < this.gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth)
            {
				this.gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth = this.gameObject.MMGetComponentNoAlloc<Health>().MaximumHealth;
			}
			this.gameObject.MMGetComponentNoAlloc<Health>().UpdateHealthBar(true);

			//速度更新
			battleParameter.SPEED = battleParameter.BaseData.SPEED + battleParameter.BaseData.SPEED * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.MoveSpeed] / 100) + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.SPD] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.SPD]);

			//ダッシュParameter更新
            battleParameter.DASH_NUM = battleParameter.BaseData.DASH_NUM + (int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashNum] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashNum];
			battleParameter.DASH_INTERVAL = battleParameter.BaseData.DASH_INTERVAL + (battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashInterval] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashInterval])* battleParameter.BaseData.DASH_INTERVAL;
			battleParameter.DASH_POWER = battleParameter.BaseData.DASH_POWER + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashPower] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashPower];


			//クリティカル率に関しては要検討 属性ステータス値は整数値に上げるのでｘ100
			battleParameter.CRITICAL = battleParameter.BaseData.CRITICAL + (
			(battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.CRT]*100f) 
			+ battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.CRT] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Critical]);


            battleParameter.PickUP = battleParameter.BaseData.PickUP + (
			battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.PickUP] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.PickUP]+ PermanentEffects[(int)SurviveProgressManager.PermanentEffect.PickRange] / 100f);
            battleParameter.WeaponInterval = battleParameter.BaseData.WeaponInterval - (
        battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Haste] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Haste] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackInterval] / 100f);
            battleParameter.SkillInterval = battleParameter.BaseData.SkillInterval - (
      battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Haste] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Haste] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackInterval] / 100f);

            battleParameter.Luck = battleParameter.BaseData.Luck + (
     (int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Luck] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Luck]);
            //継続回復　 ショップでーた/100
			battleParameter.Regeneration = battleParameter.BaseData.Regeneration + (
			battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Regeneration] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Regeneration] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.HPRecovery] /100f);
            battleParameter.EXPUp = battleParameter.BaseData.EXPUp + (
			battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.EXP] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.EXP]+ PermanentEffects[(int)SurviveProgressManager.PermanentEffect.GainExp] / 100f);
            battleParameter.GoldUp = battleParameter.BaseData.GoldUp + (
			battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.GOLD] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.GOLD] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.GainCoin] / 100f);
            
			
			//攻撃範囲
			battleParameter.AREA = battleParameter.BaseData.AREA + (
			battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.AREA] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.AREA]+PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackRange]/100f);


            battleParameter.ThrowNum = battleParameter.BaseData.ThrowNum + (
            (int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.ThrowNum] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.ThrowNum]);
            battleParameter.KnockBack = battleParameter.BaseData.KnockBack + (
            battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.KnockBack] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.KnockBack]);


			//防御値
			battleParameter.Shield = battleParameter.BaseData.Shield + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.DamageCut];
			battleParameter.Reroll = battleParameter.BaseData.Reroll + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Reroll];
			battleParameter.WeaponNum = battleParameter.BaseData.WeaponNum + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.WeaponLimit];
			battleParameter.ItemNum = battleParameter.BaseData.ItemNum + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.RelicLimit];
			battleParameter.DamageTaken = battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DamageTaken];



			battleParameter.MainWeaponInterval = battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.MainWeaponInterval];
			battleParameter.MainWeaponInterval = battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.MainWeaponAREA];
		}

		var movement = this.gameObject.MMGetComponentNoAlloc<CharacterMovement>();
		//良い感じにしないと
		movement.WalkSpeed = battleParameter.SPEED;
		movement.ResetSpeed();
		var dash = this.gameObject.MMGetComponentNoAlloc<SurviveDash>();
		dash.SetDashParameter(battleParameter.DASH_NUM, battleParameter.DASH_POWER, battleParameter.DASH_INTERVAL);
		//dash2D.DashDuration

		var h = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		h.DamageShield = battleParameter.Shield; 
		h.DamageTaken = battleParameter.DamageTaken;
		h.Guard60 = battleParameter.Guard60;
		//回復アイテムドロップしなくなる
		if (itemSets.Exists(_=>_.itemStatusDatas.Exists(__=>__.exItemStatus == AbilityData.ItemStatusData.ItemStatus.DROP_BIND)))
        {
			battleParameter.DropBind = true;
		}
        else
        {
			battleParameter.DropBind = false;

		}
		//自然回復しなくなる
		if (itemSets.Exists(_ => _.itemStatusDatas.Exists(__ => __.exItemStatus == AbilityData.ItemStatusData.ItemStatus.NO_RECOVERY)))
		{
			battleParameter.NoRegeneration = true;

		}else
        {
			battleParameter.NoRegeneration = false;
		}
		//歩いていると回復する
		if (itemSets.Exists(_ => _.itemStatusDatas.Exists(__ => __.exItemStatus == AbilityData.ItemStatusData.ItemStatus.WALK_RECOVERY)))
		{
			for (int i = 0; i < itemSets.Count; i++)
			{
				foreach(var item in  itemSets[i].itemStatusDatas)
                {
					if(item.exItemStatus == AbilityData.ItemStatusData.ItemStatus.WALK_RECOVERY)
                    {
						battleParameter.WalkRegeneration = item.value;
					}
				}
			}
		}

		//60秒に一回分の無敵状態に
		if (itemSets.Exists(_ => _.itemStatusDatas.Exists(__ => __.exItemStatus == AbilityData.ItemStatusData.ItemStatus.GUARD)))
		{
			battleParameter.Guard60 = true;
        }
        else
        {
			battleParameter.Guard60 = false;
		}
		//武器更新
		foreach (var weaponset in weaponSets)
        {
			foreach(var weapon in weaponset.surviveWeaponBases)
            {
				weapon.UpdateParameter();
            }
        }
		foreach (var weaponset in skillSets)
		{
			foreach (var weapon in weaponset.surviveWeaponBases)
			{
				weapon.UpdateParameter();
			}
		}

		//ui更新
		SurviveGUIManager sgm = SurviveGUIManager.GetInstance();

		sgm.UpdateHPText();
		sgm.UpdateInventory(battleParameter.WeaponNum, battleParameter.ItemNum, GetAbilityDic);

	}

	private List<float> CalcItemDataStatus()
    {
		List<float> itemCalc = new List<float>(new float[(int)GameDefine.StatusCorrection.NONE]);

		for(int i = 0; i < itemSets.Count; i++)
        {
			foreach (var data in itemSets[i].itemStatusDatas)
			{
				if (data.exItemStatus == AbilityData.ItemStatusData.ItemStatus.STATUS_UP)
				{
					itemCalc[(int)data.StatusType] += data.value;
				}
			}

		}

		return itemCalc;

	}


	public List<float> CalcPermanentDataStatus()
	{
		//0は未使用
		List<float> permanentCalc = new List<float>(new float[(int)SurviveProgressManager.PermanentEffect.END]);
		var PermanentEffectLevels = battleParameter.BasePermanentEffectLevels;
		var data = SurviveGameManager.GetInstance().shopItemDatas;
		for (int i = 1; i < PermanentEffectLevels.Count; i++)
		{
			var target = data.GetShopItemData(i);
			permanentCalc[i] = PermanentEffectLevels[i] * target.levelUpValue;
		}

		return permanentCalc;

	}

	public (int,int) GetHP()
    {
		var h = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		return ((int)h.CurrentHealth, (int)h.MaximumHealth);
	}

    public void OnMMEvent(StatusReflectEvent eventType)
	{
		for(int i = 0; i < battleParameter.StatusAttributeLevels.Count; i++)
        {
			int abilityValue = 0;
			if((int)eventType.statusAttribute == i)
            {
				abilityValue = eventType.statusAttributeValue;

			}
			battleParameter.StatusAttributeLevels[i] += eventType.AddPointList[i];
			battleParameter.AbilityAttributeLevels[i] += abilityValue;
		}
		battleParameter.ReservePoint = eventType.ReservePoint;

		UpdateStatus();

	}
	public List<float> CalcAttributionStatus(List<int> attributeStatus)
    {
		List<float> correction = new List<float>(new float[(int)GameDefine.StatusCorrection.NONE]);
		//for(int i = 0; i < attributeStatus.Count; i++)
  //      {
  //          switch ((GameDefine.StatusAttribute)i)
  //          {
  //              case GameDefine.StatusAttribute.Str:
		//			correction[(int)GameDefine.StatusCorrection.DAMAGE] = Mathf.Floor(attributeStatus[i]*(GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] +0.1f)) * 0.01f;

		//			break;
  //              case GameDefine.StatusAttribute.Vit:
		//			correction[(int)GameDefine.StatusCorrection.HP] = Mathf.Floor(attributeStatus[i] * (GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] + 0.1f)) * 2;
		//			break;
		//		case GameDefine.StatusAttribute.Agi:
		//			correction[(int)GameDefine.StatusCorrection.SPD] = Mathf.Floor(attributeStatus[i] * (GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] + 0.1f)) * 0.01f;
		//			break;
		//		case GameDefine.StatusAttribute.Int:
		//			correction[(int)GameDefine.StatusCorrection.Haste] = Mathf.Floor(attributeStatus[i] * (GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] + 0.1f)) * 0.002f;
		//			break;
		//		case GameDefine.StatusAttribute.Dex:
		//			correction[(int)GameDefine.StatusCorrection.CRT] = Mathf.Floor(attributeStatus[i] * (GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] + 0.1f)) * 0.002f;//調整
		//			break;
		//		case GameDefine.StatusAttribute.Luk:
		//			correction[(int)GameDefine.StatusCorrection.PickUP] = Mathf.Floor(attributeStatus[i] * (GameDefine.StatusAttributeCorrection[GameDefine.GetAttributeLevel(attributeStatus[i], battleParameter.DefaultStatusAttributeLevels[i])] + 0.1f)) * 0.01f;
		//			break;
		//		case GameDefine.StatusAttribute.NONE:
  //                  break;
  //          }
  //      }
		return correction;

	}
	public void OnMMEvent(DashEndEvent eventType)
	{
	
		foreach (var set  in skillSets)
        {
			foreach(var w in set.surviveWeaponBases)
            {
				if(w.main)
                {
					//即発射可能に
					w.ResetdelayBetweenUsesCounter();

				}
            }
        }
	}
}


public struct AbilityReflectEvent
{
	public int Level;
	public AbilityData Ability;
	public GameDefine.StatusAttribute statusAttribute;
	public int statusAttributeValue;
	static AbilityReflectEvent e;
	public static void Trigger(int Level, AbilityData Ability, GameDefine.StatusAttribute Attribute, int AttributeValue)
	{
		e.Ability = Ability;
		e.Level = Level;
		e.statusAttribute = Attribute;
		e.statusAttributeValue = AttributeValue;
		MMEventManager.TriggerEvent(e);
	}
}
public struct StatusReflectEvent
{
	//残ったポイント　仕様上持ち越しするしか...
	public int ReservePoint;
	//割り振ったポイント
	public List<int> AddPointList;
	public GameDefine.StatusAttribute statusAttribute;
	public int statusAttributeValue;
	static StatusReflectEvent e;
	public static void Trigger(int ReservePoint, List<int> AddPointList, GameDefine.StatusAttribute Attribute, int AttributeValue)
	{
		e.ReservePoint = ReservePoint;
		e.AddPointList = AddPointList;
		e.statusAttribute = Attribute;
		e.statusAttributeValue = AttributeValue;
		MMEventManager.TriggerEvent(e);
	}
}

public struct DashEndEvent
{
	
	static DashEndEvent e;
	public static void Trigger()
	{
		MMEventManager.TriggerEvent(e);
	}
}