using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System;
using UnityEngine.U2D.Animation;

public class SurviveEnemy : Character
{

	public Collider2D currentGround;

	[NonSerialized]
	public SurviveCharacterAim characterAim;



	public List<WeaponSet> weaponSets = new List<WeaponSet>();
	public List<WeaponSet> skillSets = new List<WeaponSet>();
	//あとで
	public int InitialWeaponID;


	public float TimeOverMinitHPBase;
	public float MinitHPPer;

	public class WeaponSet
	{
		public int AbilityID;
		public GameDefine.StatusAttribute statusAttribute;
		public int statusAttributeValue;
		//多分一個だけ
		public List<SurviveWeaponBase> surviveWeaponBases = new List<SurviveWeaponBase>();
	}

	public int GetEnemyType()
	{
		return enemyData.PATTERN;
	}

	//とりあえず継続回復間隔3秒にしてみる
	private float recoveryTime = 3f;
	private float currentRecoveryTime = 0f;
	/// <summary>
	/// This is called every frame.
	/// </summary>
	float nonVisibleTime = 0f;
	protected override void Update()
	{
		EveryFrame();


		AutoRecovery();



		//再配置
		if (!IsEvent)
		{
			//foreach (var e in CharacterModel.GetComponentsInChildren<Renderer>())
			//{
			//	if (!e.isVisible)
			//	{
			//		nonVisibleTime += Time.deltaTime;
			//		if (nonVisibleTime >= 3f)
			//		{
			//			EnemySpawner.ReSpawn(this);
			//			nonVisibleTime = 0f;
			//		}
			//	}
			//	else
			//	{
			//		nonVisibleTime = 0f;

			//	}
			//}

			//if (CharacterModel.GetComponent<SpriteLibrary>().enabled)
			//{

			//}

			var mainCamera = Camera.main;
			Vector3 viewportPoint = mainCamera.WorldToViewportPoint(transform.position);

			// オブジェクトがカメラビュー内にあるかどうかを判定
			if (viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
			{
				return;
			}

			if (!CharacterModel.GetComponent<Renderer>().isVisible)
			{
				nonVisibleTime += Time.deltaTime;
				if (nonVisibleTime >= 3f)
				{
					EnemySpawner.ReSpawn(this);
					nonVisibleTime = 0f;
				}
			}
			else
			{
				nonVisibleTime = 0f;

			}
		}
	}

	private void AutoRecovery()
	{
		//if (battleParameter.Regeneration > 0f)
		//{
		//	currentRecoveryTime += Time.deltaTime;
		//	if (currentRecoveryTime >= recoveryTime)
		//	{
		//		HealPer(battleParameter.Regeneration);
		//		currentRecoveryTime = 0f;
		//	}
		//}
	}
	/// <summary>
	/// Initializes this instance of the character
	/// </summary>
	protected override void Awake()
	{
		Initialization();
	}

	/// <summary>
	/// Gets and stores input manager, camera and components
	/// </summary>
	//protected override void Initialization()
	//{
	//	if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
	//	{
	//		CharacterDimension = CharacterDimensions.Type2D;
	//	}
	//	if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
	//	{
	//		CharacterDimension = CharacterDimensions.Type3D;
	//	}

	//	// we initialize our state machines
	//	MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject, SendStateChangeEvents);
	//	ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject, SendStateChangeEvents);

	//	// we get the current input manager
	//	SetInputManager();
	//	// we store our components for further use 
	//	CharacterState = new CharacterStates();
	//	_controller = this.gameObject.GetComponent<TopDownController>();
	//	if (CharacterHealth == null)
	//	{
	//		CharacterHealth = this.gameObject.GetComponent<Health>();
	//	}

	//	CacheAbilitiesAtInit();
	//	if (CharacterBrain == null)
	//	{
	//		CharacterBrain = this.gameObject.GetComponent<AIBrain>();
	//	}

	//	if (CharacterBrain != null)
	//	{
	//		CharacterBrain.Owner = this.gameObject;
	//	}

	//	Orientation2D = FindAbility<CharacterOrientation2D>();
	//	Orientation3D = FindAbility<CharacterOrientation3D>();
	//	_characterPersistence = FindAbility<CharacterPersistence>();

	//	AssignAnimator();

	//	// instantiate camera target
	//	if (CameraTarget == null)
	//	{
	//		CameraTarget = new GameObject();
	//	}
	//	CameraTarget.transform.SetParent(this.transform);
	//	CameraTarget.transform.localPosition = Vector3.zero;
	//	CameraTarget.name = "CameraTarget";

	//	if (LinkedInputManager != null)
	//	{
	//		if (OptimizeForMobile && LinkedInputManager.IsMobile)
	//		{
	//			if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
	//			{
	//				this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
	//			}
	//		}
	//	}
	//	if (this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>() != null)
	//	{
	//		//初期武器取得
	//		var weapon = SurviveGameManager.GetInstance().GetAbility(InitialWeaponID);


	//		var handle = this.gameObject.MMGetComponentNoAlloc<SurviveHandleWeapon>();


	//		var newweapon = handle.AddWeapon(weapon.basePrefab, weapon.basePrefab.WeaponName, true);
	//		newweapon.SetParameter(weapon.WeaponLevelDatas[0], GameDefine.StatusAttribute.NONE);
	//		var set = new WeaponSet();
	//		set.AbilityID = weapon.ID;
	//		set.surviveWeaponBases.Add(newweapon);
	//		set.statusAttribute = GameDefine.StatusAttribute.NONE;
	//		set.statusAttributeValue = 0;
	//		SurviveGUIManager.GetInstance().UpdateSkillInventory(set.AbilityID, skillSets.Count);
	//		skillSets.Add(set);

	//	}
	//}




	//敵データ

	public EnemyDatas.EnemyData enemyData;
	public bool IsEvent = false;
	//初期化
	public void ReflectionData(EnemyDatas.EnemyData setData, bool isEvent = false)
	{
		enemyData = setData;
		UpdateStatus();

		IsEvent = isEvent;

		//体力初期化
		var h = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		h.InitialHealth = h.MaximumHealth;
		h.ResetHealthToMaxHealth();


	}





	public void HealPer(float rate)
	{
		var health = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		health.ReceiveHealth(health.MaximumHealth * rate, this.gameObject);
	}

	/// <summary>
	/// OnDisable, we unregister our OnRevive event
	/// </summary>
	protected override void OnDisable()
	{
		if (enemyData.PATTERN == 3)
		{
			BossEndEvent.Trigger();
		}
		if (CharacterHealth != null)
		{
			CharacterHealth.OnDeath -= OnDeath;
			CharacterHealth.OnHit -= OnHit;
		}
	}


	private void UpdateStatus()
	{


		Health objectHealth = gameObject.MMGetComponentNoAlloc<Health>();
		SurvivePlayer player = SurviveLevelManager.GetInstance().Players[0] as SurvivePlayer;
		if (objectHealth != null)
		{

			float setHP = enemyData.MAX_HP;
			if (SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime() > 1200)
			{
				var time = SurviveGameManager.GetInstance().surviveRuleManager.GetCurrentTime();
				var m = Mathf.FloorToInt((time - 1200) / 60);
				setHP += m * (TimeOverMinitHPBase + MinitHPPer * m);
			}

			//プレイヤーのレベルでHP変化
			if (enemyData.PATTERN == 2)
			{
				setHP *= (player.battleParameter.Level / 10 + 1);

			}
			else if (enemyData.PATTERN == 3)
			{
				setHP *= player.battleParameter.Level;
			}


			//ノックバック判定
			if (enemyData.PATTERN > 1)
			{
				objectHealth.ImmuneToKnockback = true;
			}
			else
			{
				objectHealth.ImmuneToKnockback = false;
			}

			objectHealth.InitialHealth = setHP;
			objectHealth.MaximumHealth = setHP;
			objectHealth.Revive();

			var deathFeedback = (objectHealth.DeathMMFeedbacks.AddFeedback(typeof(MoreMountains.Feedbacks.MMFeedbackEvents)) as MoreMountains.Feedbacks.MMFeedbackEvents);
			deathFeedback.PlayEvents = new UnityEngine.Events.UnityEvent();
			deathFeedback.Timing = new MoreMountains.Feedbacks.MMFeedbackTiming();
			deathFeedback.PlayEvents.AddListener(() => { KillEvent.Trigger(); });

			if (SurviveLevelManager.GetInstance().FinalBossID == enemyData.ID)
			{
				var feedback = (objectHealth.DeathMMFeedbacks.AddFeedback(typeof(MoreMountains.Feedbacks.MMFeedbackEvents)) as MoreMountains.Feedbacks.MMFeedbackEvents);
				feedback.PlayEvents = new UnityEngine.Events.UnityEvent();
				feedback.Timing = new MoreMountains.Feedbacks.MMFeedbackTiming();
				feedback.PlayEvents.AddListener(() => { SurviveGameManager.GetInstance().GameEndMethod(); });
			}
		}

		//	//攻撃力更新
		//	battleParameter.ATTACK = battleParameter.BaseData.ATTACK + battleParameter.BaseData.ATTACK * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Attack] / 100) + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DAMAGE] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DAMAGE]);
		//	//HPマックス更新 hpは計算後切り捨て
		//	battleParameter.MAX_HP = Mathf.FloorToInt(battleParameter.BaseData.MAX_HP + battleParameter.BaseData.MAX_HP * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.MaxHP] / 100) + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.HP]) + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.HP]);
		//	this.gameObject.MMGetComponentNoAlloc<Health>().MaximumHealth = battleParameter.MAX_HP;
		//	this.gameObject.MMGetComponentNoAlloc<Health>().UpdateHealthBar(true);

		//	//速度更新
		//	battleParameter.SPEED = battleParameter.BaseData.SPEED + battleParameter.BaseData.SPEED * ((PermanentEffects[(int)SurviveProgressManager.PermanentEffect.MoveSpeed] / 100) + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.SPD] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.SPD]);

		//	//ダッシュParameter更新
		//	battleParameter.DASH_NUM = battleParameter.BaseData.DASH_NUM + (int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashNum] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashNum];
		//	battleParameter.DASH_INTERVAL = battleParameter.BaseData.DASH_INTERVAL + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashInterval] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashInterval];
		//	battleParameter.DASH_POWER = battleParameter.BaseData.DASH_POWER + battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.DashPower] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.DashPower];


		//	//クリティカル率に関しては要検討 属性ステータス値は整数値に上げるのでｘ100
		//	battleParameter.CRITICAL = battleParameter.BaseData.CRITICAL + (
		//	(battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.CRT] * 100f)
		//	+ battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.CRT] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Critical]);


		//	battleParameter.PickUP = battleParameter.BaseData.PickUP + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.PickUP] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.PickUP] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.PickRange] / 100f);
		//	battleParameter.WeaponInterval = battleParameter.BaseData.WeaponInterval + (
		//battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Haste] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Haste]);
		//	battleParameter.SkillInterval = battleParameter.BaseData.SkillInterval + (
		// battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Haste] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Haste]);

		//	battleParameter.Luck = battleParameter.BaseData.Luck + (
		//(int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Luck] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Luck]);
		//	//継続回復　 ショップでーた/100
		//	battleParameter.Regeneration = battleParameter.BaseData.Regeneration + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.Regeneration] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.Regeneration] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.HPRecovery] / 100f);
		//	battleParameter.EXPUp = battleParameter.BaseData.EXPUp + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.EXP] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.EXP] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.GainExp] / 100f);
		//	battleParameter.GoldUp = battleParameter.BaseData.GoldUp + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.GOLD] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.GOLD] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.GainCoin] / 100f);


		//	//攻撃範囲
		//	battleParameter.AREA = battleParameter.BaseData.AREA + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.AREA] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.AREA] + PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackRange] / 100f);


		//	battleParameter.ThrowNum = battleParameter.BaseData.ThrowNum + (
		//	(int)battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.ThrowNum] + (int)battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.ThrowNum]);
		//	battleParameter.KnockBack = battleParameter.BaseData.KnockBack + (
		//	battleParameter.StatusCollection[(int)GameDefine.StatusCorrection.KnockBack] + battleParameter.ItemCalc[(int)GameDefine.StatusCorrection.KnockBack]);


		//	//防御値
		//	battleParameter.Shield = battleParameter.BaseData.Shield + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.DamageCut];
		//	battleParameter.Reroll = battleParameter.BaseData.Reroll + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.Reroll];
		//	battleParameter.WeaponNum = battleParameter.BaseData.WeaponNum + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.WeaponLimit];
		//	battleParameter.ItemNum = battleParameter.BaseData.ItemNum + (int)PermanentEffects[(int)SurviveProgressManager.PermanentEffect.RelicLimit];


		var movement = this.gameObject.MMGetComponentNoAlloc<CharacterMovement>();
		////良い感じにしないと
		movement.WalkSpeed = enemyData.SPEED;
		var DoT = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
		DoT.MaxDamageCaused = enemyData.ATTACK;
		DoT.MinDamageCaused = enemyData.ATTACK;
		//var dash = this.gameObject.MMGetComponentNoAlloc<SurviveDash>();
		//dash.SetDashParameter(battleParameter.DASH_NUM, battleParameter.DASH_POWER, battleParameter.DASH_INTERVAL);
		////dash2D.DashDuration

		//var h = this.gameObject.MMGetComponentNoAlloc<SurviveHealth>();
		//h.DamageShield = battleParameter.Shield;

		this.transform.localScale = new Vector3(enemyData.SIZE, enemyData.SIZE, 1);


	}

}

