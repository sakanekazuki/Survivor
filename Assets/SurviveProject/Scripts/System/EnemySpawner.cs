using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using System.Linq;

public class EnemySpawner : TopDownMonoBehaviour
{
	/// the object pooler associated to this spawner
	public EnemyPooler ObjectPooler { get; set; }
	public List<EnemyPooler> ObjectPoolerList = new List<EnemyPooler>();

	[Header("Spawn")]
	/// whether or not this spawner can spawn
	[Tooltip("whether or not this spawner can spawn")]
	public bool CanSpawn = true;
	/// the minimum frequency possible, in seconds
	[Tooltip("the minimum frequency possible, in seconds")]
	public float MinFrequency = 1f;
	/// the maximum frequency possible, in seconds
	[Tooltip("the maximum frequency possible, in seconds")]
	public float MaxFrequency = 1f;

	[Header("Debug")]
	[MMInspectorButton("ToggleSpawn")]
	/// a test button to spawn an object
	public bool CanSpawnButton;

	protected float _lastSpawnTimestamp = 0f;
	protected float _nextFrequency = 0f;

	private EnemyDatas enemys;
	private List<EnemyDatas.EnemyData> enemyDataList;
	private EnemyGroupDatas groups;
	private SurviveRuleManager surviveRuleManager;

	private EnemyGroupDatas.EnemyGroupData currentGroup;
	private List<EnemyGroupDatas.EnemyGroupData> exCurrentGroups = new List<EnemyGroupDatas.EnemyGroupData>();
	private Character player;

#if DEBUG || UNITY_EDITOR
	private int _addTime10s = 10;
	private int _addTime60s = 60;
	private int _addTime600s = 600;
	public static Action<int> OnCurrentObjChanges;
	public static Action<EnemyGroupDatas.EnemyGroupData, int> OnSetGroup;
#endif

	private List<GameObject> currentObjs = new List<GameObject>();

	public Material enemyMatNormal;
	public Material enemyMatElite;
	public Character Player
	{
		get
		{
			if (player == null)
			{
				SetCharacter();

			}
			return player;
		}
		set => player = value;
	}
	public void SetCharacter()
	{
		Character character = LevelManager.Instance.Players[0];
		if (character != null)
		{
			Player = character;
		}


	}

	/// <summary>
	/// On Start we initialize our spawner
	/// </summary>
	protected virtual void Start()
	{
		Initialization();
	}

	/// <summary>
	/// Grabs the associated object pooler if there's one, and initalizes the frequency
	/// </summary>
	protected virtual void Initialization()
	{

		var sm = SurviveLevelManager.GetInstance();
		groups = sm.enemyGroupDatas;
		enemys = sm.enemyDatas;
		enemyDataList = new List<EnemyDatas.EnemyData>(enemys.datas);
		surviveRuleManager = SurviveGameManager.GetInstance().surviveRuleManager;




		if (GetComponent<EnemyPooler>() != null)
		{
			ObjectPooler = GetComponent<EnemyPooler>();
		}

		if (ObjectPooler == null)
		{
			Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
			return;
		}

		//まずプール内の初期化　使用する敵データを詰める
		foreach (var enemy in enemys.datas)
		{
			var pEnemy = new PoolerEnemy();
			pEnemy.ID = enemy.ID;
			pEnemy.GameObjectToPool = enemy.Prefab;

			//一旦こう
			pEnemy.PoolCanExpand = true;
			pEnemy.PoolSize = /*100/**/enemy.POOL_MAX;

			pEnemy.Enabled = false;


			var obj = new GameObject(enemy.NAME);
			var ep = obj.AddComponent<EnemyPooler>();
			ep.PoolingMethod = MMPoolingMethods.OriginalOrder;
			obj.transform.parent = ObjectPooler.transform.parent;
			ep.Pool = new List<PoolerEnemy>();
			ep.Pool.Add(pEnemy);
			ObjectPoolerList.Add(ep);
			ObjectPooler.Pool.Add(pEnemy);
			ep.FillObjectPool();

		}
		//初期化してあげる
		ObjectPooler.FillObjectPool();
		SetGroup();
		DetermineNextFrequency();
		currentObjs = new List<GameObject>();
	}

	private void SetGroup()
	{
		var ctime = surviveRuleManager.GetCurrentTime();
		currentGroup = Array.Find(groups.datas, _ => { return _.startTime <= ctime && (_.endTime > ctime || _.endTime == -1); });
		if (groups.exDatas != null && groups.exDatas.Length > 0)
		{
			if (Array.Exists(groups.exDatas, _ => { return _.startTime <= ctime && (_.endTime > ctime || _.endTime == -1); }))
			{
				exCurrentGroups = Array.FindAll(groups.exDatas, _ => { return _.startTime <= ctime && (_.endTime > ctime || _.endTime == -1); }).ToList();
			}
			else
			{
				exCurrentGroups = new List<EnemyGroupDatas.EnemyGroupData>();
			}
		}
#if DEBUG || UNITY_EDITOR
		OnSetGroup?.Invoke(currentGroup, Array.FindIndex(groups.datas, _ => { return _ == currentGroup; }));
#endif
		var ids = new List<int>(currentGroup.enemyIDs);

#if UNITY_EDITOR
		Debug.LogWarning("敵リスト->" + string.Join(",", ids));
#endif
		foreach (var enemy in ObjectPooler.Pool)
		{
			if (ids.Contains(enemy.ID))
			{
				enemy.Enabled = true;

			}
			else
			{
				enemy.Enabled = false;

			}

		}

		foreach (var p in ObjectPoolerList)
		{
			foreach (var enemy in p.Pool)
			{
				if (ids.Contains(enemy.ID))
				{
					enemy.Enabled = true;

				}
				else
				{
					enemy.Enabled = false;

				}

			}
		}

		foreach (var d in exCurrentGroups)
		{
			var exids = new List<int>(d.enemyIDs);

#if UNITY_EDITOR
			Debug.LogWarning("敵リストEX->" + string.Join(",", ids));
#endif
			foreach (var enemy in ObjectPooler.Pool)
			{
				if (exids.Contains(enemy.ID))
				{
					enemy.Enabled = true;

				}

			}

			foreach (var p in ObjectPoolerList)
			{
				foreach (var enemy in p.Pool)
				{
					if (exids.Contains(enemy.ID))
					{
						enemy.Enabled = true;

					}

				}
			}
		}
		//出現時間管理
		List<EnemyIntervalData> newTimeList = new List<EnemyIntervalData>();
		for (int i = 0; i < ids.Count; i++)
		{
			var data = enemyDataList.Find(_ => _.ID == ids[i]);
			//既にある場合、時間は引き継ぐ
			if (CountTimeList.Exists(_ => _.EnemyID == ids[i]))
			{
				var cdata = CountTimeList.Find(_ => _.EnemyID == ids[i]);
				newTimeList.Add(
					cdata
				);
			}
			else
			{
				newTimeList.Add(new EnemyIntervalData()
				{
					EnemyID = ids[i],
					_lastSpawnTimestamp = Time.time - data.INTERVAL,
					_nextFrequency = data.INTERVAL,
					Pattern = data.PATTERN,
					_pooler = ObjectPoolerList.Find(_ => _.Pool.Exists(_ => _.ID == ids[i]))
				});
			}


		}
		List<EnemyIntervalData> newEXTimeList = new List<EnemyIntervalData>();
		foreach (var d in exCurrentGroups)
		{
			var exids = new List<int>(d.enemyIDs);
			//出現時間管理
			for (int i = 0; i < exids.Count; i++)
			{
				var data = enemyDataList.Find(_ => _.ID == exids[i]);
				//既にある場合、時間は引き継ぐ
				if (EXCountTimeList.Exists(_ => _.EnemyID == exids[i]))
				{
					var cdata = EXCountTimeList.Find(_ => _.EnemyID == exids[i]);
					newEXTimeList.Add(
						cdata
					);
				}
				else
				{
					newEXTimeList.Add(new EnemyIntervalData()
					{
						EnemyID = exids[i],
						_lastSpawnTimestamp = Time.time - data.INTERVAL,
						_nextFrequency = data.INTERVAL,
						Pattern = data.PATTERN,
						_pooler = ObjectPoolerList.Find(_ => _.Pool.Exists(_ => _.ID == exids[i]))
					});
				}


			}
		}
		CountTimeList.Clear();
		CountTimeList = newTimeList;
		EXCountTimeList.Clear();
		EXCountTimeList = newEXTimeList;
		if (currentGroup.patternObj != null)
		{
			var pattern = Instantiate(currentGroup.patternObj);
			//pattern.transform.position = player.transform.position;
			pattern.GetComponent<EnemyEventSystem>().Spawn(this);
		}
	}



	//敵データごとにインターバルを設定できるように改良
	protected List<EnemyIntervalData> CountTimeList = new List<EnemyIntervalData>();
	protected List<EnemyIntervalData> EXCountTimeList = new List<EnemyIntervalData>();

	protected class EnemyIntervalData
	{
		public float _nextFrequency = 0f;
		public float _lastSpawnTimestamp = 0f;
		public EnemyPooler _pooler;

		public int EnemyID = 0;
		public int Pattern = 1;
	}

	/// <summary>
	/// Every frame we check whether or not we should spawn something
	/// </summary>
	protected virtual void Update()
	{
#if DEBUG || UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.R))
		{
			surviveRuleManager.ResetCurrentTime();
		}
		if (Input.GetKeyDown(KeyCode.T))
		{
			surviveRuleManager.AddCurrentTime(_addTime10s);
		}
		if (Input.GetKeyDown(KeyCode.Y))
		{
			surviveRuleManager.AddCurrentTime(_addTime60s);
		}
		if (Input.GetKeyDown(KeyCode.U))
		{
			surviveRuleManager.AddCurrentTime(_addTime600s);
		}
#endif
		var ctime = surviveRuleManager.GetCurrentTime();

		if (currentGroup == null)
		{
			return;
		}
		if (currentGroup.endTime > 0 && (currentGroup.endTime > 0 && currentGroup.endTime < ctime))
		{
			SetGroup();
		}



		//生成
		foreach (var t in CountTimeList)
		{
			if ((Time.time - t._lastSpawnTimestamp > t._nextFrequency) && CanSpawn)
			{
				Spawn(t);
			}

		}
		foreach (var t in EXCountTimeList)
		{
			if ((Time.time - t._lastSpawnTimestamp > t._nextFrequency) && CanSpawn)
			{
				Spawn(t);
			}

		}

		//if ((Time.time - _lastSpawnTimestamp > _nextFrequency) && CanSpawn)
		//{
		//	Spawn();
		//}
	}

	/// <summary>
	/// Spawns an object out of the pool if there's one available.
	/// If it's an object with Health, revives it too.
	/// </summary>
	protected virtual void Spawn()
	{
		if (currentGroup.max > 0 && currentObjs.Count >= currentGroup.max)
		{
			return;
		}
		(GameObject nextGameObject, int id) = ObjectPooler.GetPooledGameObjectWithId();

		var data = enemyDataList.Find(_ => _.ID == id);

		// mandatory checks
		if (nextGameObject == null) { return; }
		if (nextGameObject.GetComponent<MMPoolableObject>() == null)
		{
			throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
		}

		// we activate the object
		nextGameObject.gameObject.SetActive(true);
		nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

		// we check if our object has an Health component, and if yes, we revive our character
		Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health>();
		if (objectHealth != null)
		{
			objectHealth.InitialHealth = data.MAX_HP;
			objectHealth.MaximumHealth = data.MAX_HP;
			objectHealth.Revive();
		}

		// we position the object
		if (Player == null)
			return;
		float x = UnityEngine.Random.Range(-30f, 30f);
		float y = UnityEngine.Random.Range(-30f, 30f);
		if (Mathf.Abs(x) < 10f)
			x += x >= 0f ? 10f : -10f;
		if (Mathf.Abs(y) < 10f)
			y += y >= 0f ? 10f : -10f;
		var pos = Player.transform.position + new Vector3(x, y, 0f);

		nextGameObject.transform.position = pos;/*this.transform.position;*/
		var pobj = nextGameObject.GetComponent<MMPoolableObject>();
		pobj.ExecuteOnDisable.AddListener(() => PoolableObjectDisable(pobj));

		var se = nextGameObject.MMGetComponentNoAlloc<SurviveEnemy>();
		if (se != null && data != null)
		{
			se.ReflectionData(data);
		}
		//今存在する敵カウント用
		currentObjs.Add(nextGameObject);
#if DEBUG || UNITY_EDITOR
		OnCurrentObjChanges?.Invoke(currentObjs.Count);
#endif
		// we reset our timer and determine the next frequency
		_lastSpawnTimestamp = Time.time;
		DetermineNextFrequency();
	}
	protected virtual void Spawn(EnemyIntervalData enemyIntervalData)
	{

		if (enemyIntervalData.Pattern < 3 && currentGroup.max > 0 && currentObjs.Count >= currentGroup.max)
		{
			return;
		}


		(GameObject nextGameObject, int i) = enemyIntervalData._pooler.GetPooledGameObjectWithId();

		var data = enemyDataList.Find(_ => _.ID == enemyIntervalData.EnemyID);

		// mandatory checks
		if (nextGameObject == null) { return; }
		if (nextGameObject.GetComponent<MMPoolableObject>() == null)
		{
			throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
		}

		// we activate the object
		nextGameObject.gameObject.SetActive(true);
		nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

		// we check if our object has an Health component, and if yes, we revive our character
		Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health>();
		if (objectHealth != null)
		{
			objectHealth.InitialHealth = data.MAX_HP;
			objectHealth.MaximumHealth = data.MAX_HP;
			objectHealth.Revive();
		}

		// we position the object
		if (Player == null)
			return;
		float x = UnityEngine.Random.Range(-30f, 30f);
		float y = UnityEngine.Random.Range(-30f, 30f);
		if (Mathf.Abs(x) < 10f)
			x += x >= 0f ? 10f : -10f;
		if (Mathf.Abs(y) < 10f)
			y += y >= 0f ? 10f : -10f;
		var pos = Player.transform.position + new Vector3(x, y, 0f);

		nextGameObject.transform.position = pos;/*this.transform.position;*/
		var pobj = nextGameObject.GetComponent<MMPoolableObject>();
		pobj.ExecuteOnDisable.AddListener(() => PoolableObjectDisable(pobj));

		var se = nextGameObject.MMGetComponentNoAlloc<SurviveEnemy>();
		if (se != null && data != null)
		{
			se.ReflectionData(data);

			if (data.PATTERN == 3)
			{
				var brain = Camera.main.GetComponent<CinemachineBrain>();
				//brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView += 4;
				var from = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
				var to = brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView + 4;
				//DOVirtual.Float(from, to, 1f, (_) => { brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = _; });
				//曲変える
				BossStartEvent.Trigger();


			}
			if (data.PATTERN >= 2)
			{
				se.CharacterModel.GetComponent<SpriteRenderer>().material = enemyMatElite;
			}
			else
			{
				if (enemyMatNormal != null)
				{
					//foreach (var e in se.CharacterModel.GetComponentsInChildren<SpriteRenderer>())
					//{
					//	e.material = enemyMatNormal;
					//}
					se.CharacterModel.GetComponent<SpriteRenderer>().material = enemyMatNormal;
				}
				else
				{
					//               if(se.CharacterModel.GetComponent<SpriteRenderer>().material.name == "Burn2")
					//{

					//}
					//else
					//{

					//}
				}
			}
		}
		//今存在する敵カウント用
		currentObjs.Add(nextGameObject);
#if DEBUG || UNITY_EDITOR

		OnCurrentObjChanges?.Invoke(currentObjs.Count);
#endif
		// we reset our timer and determine the next frequency
		enemyIntervalData._lastSpawnTimestamp = Time.time;
		//DetermineNextFrequency(enemyIntervalData.EnemyID);
	}
	public void PoolableObjectDisable(MMPoolableObject target)
	{
		if (currentObjs.Contains(target.gameObject))
		{
			currentObjs.Remove(target.gameObject);
#if DEBUG || UNITY_EDITOR

			OnCurrentObjChanges?.Invoke(currentObjs.Count);
#endif
		}
		target.ExecuteOnDisable.RemoveAllListeners();
	}

	/// <summary>
	/// Determines the next frequency by randomizing a value between the two specified in the inspector.
	/// </summary>
	protected virtual void DetermineNextFrequency()
	{
		//出現のランダム性はどうするか
		//_nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
		_nextFrequency = currentGroup.interval;
	}

	//protected virtual void DetermineNextFrequency(EnemyIntervalData enemyIntervalData)
	//{
	//	//出現のランダム性はどうするか
	//	//_nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
	//	_nextFrequency = currentGroup.interval;
	//}
	protected virtual void DetermineNextFrequencyIndex(int index)
	{
		//出現のランダム性はどうするか
		//_nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
		_nextFrequency = currentGroup.interval;
	}
	/// <summary>
	/// Toggles spawn on and off
	/// </summary>
	public virtual void ToggleSpawn()
	{
		CanSpawn = !CanSpawn;
	}

	/// <summary>
	/// Turns spawning off
	/// </summary>
	public virtual void TurnSpawnOff()
	{
		CanSpawn = false;
	}

	/// <summary>
	/// Turns spawning on
	/// </summary>
	public virtual void TurnSpawnOn()
	{
		CanSpawn = true;
	}



	static public void ReSpawn(SurviveEnemy surviveEnemy)
	{
		var Player = LevelManager.Instance.Players[0];
		// we position the object
		if (Player == null)
			return;
		float x = UnityEngine.Random.Range(-30f, 30f);
		float y = UnityEngine.Random.Range(-30f, 30f);
		if (Mathf.Abs(x) < 10f)
			x += x >= 0f ? 10f : -10f;
		if (Mathf.Abs(y) < 10f)
			y += y >= 0f ? 10f : -10f;
		var pos = Player.transform.position + new Vector3(x, y, 0f);

		surviveEnemy.transform.position = pos;/*this.transform.position;*/

	}

#if DEBUG || UNITY_EDITOR
	public int GetCurrentObjsCount()
	{
		return currentObjs.Count;
	}
#endif
}