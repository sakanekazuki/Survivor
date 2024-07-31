using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;

public class ItemSpawner : TopDownMonoBehaviour
{
	/// the object pooler associated to this spawner
	public MMObjectPooler ObjectPooler { get; set; }

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

	
	private SurviveRuleManager surviveRuleManager;

	
	private Character player;


	public List<ItemPosWithOn> itemPosWithOns = new List<ItemPosWithOn>();

	public float allGroundWidth;
	public float allGroundHeight;

	[Serializable]
	public class ItemPosWithOn
    {
		public Transform PosObj;
		public bool on;
    }


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
		
		surviveRuleManager =  SurviveGameManager.GetInstance().surviveRuleManager;


	

		if (GetComponent<MMObjectPooler>() != null)
		{
			ObjectPooler = GetComponent<MMObjectPooler>();
		}
		
		if (ObjectPooler == null)
		{
			Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
			return;
		}


		//初期化してあげる
		ObjectPooler.FillObjectPool();
		DetermineNextFrequency();
	}





	/// <summary>
	/// Every frame we check whether or not we should spawn something
	/// </summary>
	protected virtual void Update()
	{
		var ctime = surviveRuleManager.GetCurrentTime();
	


		
		if (Time.time - _lastSpawnTimestamp > _nextFrequency)
        {
            Spawn();
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
		
		GameObject nextGameObject = ObjectPooler.GetPooledGameObject();
		var datas = itemPosWithOns.FindAll(_ => _.on == false);
		if(datas == null || datas.Count == 0)
        {
			return;
        }
		var rnd = new System.Random();
		var targetData = datas.OrderBy(_ => rnd.Next()).First();
		// mandatory checks
		if (nextGameObject == null) { return; }
		if (nextGameObject.GetComponent<MMPoolableObject>() == null)
		{
			throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
		}

		// we activate the object
		nextGameObject.gameObject.SetActive(true);
		nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

		

		// we position the object
		if (Player == null)
			return;
		var pos = player.transform.position;


		nextGameObject.transform.parent = targetData.PosObj;
		nextGameObject.transform.localPosition =Vector3.zero;/*this.transform.position;*/
		var pobj = nextGameObject.GetComponent<MMPoolableObject>();
		pobj.ExecuteOnDisable.RemoveAllListeners();
		nextGameObject.GetComponent<Collider2D>().enabled = true;
		pobj.ExecuteOnDisable.AddListener(() =>
		{
			pobj.Destroy();
			targetData.on = false;
		});
		targetData.on = true;

		

		// we reset our timer and determine the next frequency
		_lastSpawnTimestamp = Time.time;
		DetermineNextFrequency();
	}
	

    /// <summary>
    /// Determines the next frequency by randomizing a value between the two specified in the inspector.
    /// </summary>
    protected virtual void DetermineNextFrequency()
	{
        //出現のランダム性はどうするか
        _nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
	}

	//protected virtual void DetermineNextFrequency(EnemyIntervalData enemyIntervalData)
	//{
	//	//出現のランダム性はどうするか
	//	//_nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
	//	_nextFrequency = currentGroup.interval;
	//}

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
}