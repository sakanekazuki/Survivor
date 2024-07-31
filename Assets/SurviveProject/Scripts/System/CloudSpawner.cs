using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.TopDownEngine;
using System.Collections.Generic;

public class CloudSpawner : TopDownMonoBehaviour
{
	/// the object pooler associated to this spawner
	public MMMultipleObjectPooler ObjectPooler { get; set; }

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


	private Character player;


	private List<GameObject> currentObjs = new List<GameObject>();
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
	

		if (GetComponent<MMMultipleObjectPooler>() != null)
		{
			ObjectPooler = GetComponent<MMMultipleObjectPooler>();
		}
		
		if (ObjectPooler == null)
		{
			Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
			return;
		}

		currentObjs = new List<GameObject>();
	}


	/// <summary>
	/// Every frame we check whether or not we should spawn something
	/// </summary>
	protected virtual void Update()
	{
		//ê∂ê¨
		if ((Time.time - _lastSpawnTimestamp > _nextFrequency) && CanSpawn)
		{
			Spawn();
		}
	}

	/// <summary>
	/// Spawns an object out of the pool if there's one available.
	/// If it's an object with Health, revives it too.
	/// </summary>
	protected virtual void Spawn()
	{
	
		GameObject nextGameObject = ObjectPooler.GetPooledGameObject();


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
			float x = UnityEngine.Random.Range(-20f, -40f);
			float y = UnityEngine.Random.Range(-15f, 15f);
			if (Mathf.Abs(x) < 10f)
				x += x >= 0f ? 10f : -10f;
			if (Mathf.Abs(y) < 10f)
				y += y >= 0f ? 10f : -10f;
		var pos = Player.transform.position + new Vector3(x, y, 0f);

		nextGameObject.transform.position = pos;/*this.transform.position;*/
		var pobj = nextGameObject.GetComponent<MMPoolableObject>();
		pobj.ExecuteOnDisable.AddListener(()=>PoolableObjectDisable(pobj));

		var co = nextGameObject.MMGetComponentNoAlloc<CloudObject>();
		co.Speed = UnityEngine.Random.Range(1f, 3f);
	
		_lastSpawnTimestamp = Time.time;
		DetermineNextFrequency();
	}

    public void PoolableObjectDisable(MMPoolableObject target)
    {
		if(currentObjs.Contains(target.gameObject))
        {
			currentObjs.Remove(target.gameObject);
        }
		target.ExecuteOnDisable.RemoveAllListeners();
	}

    /// <summary>
    /// Determines the next frequency by randomizing a value between the two specified in the inspector.
    /// </summary>
    protected virtual void DetermineNextFrequency()
	{
        //èoåªÇÃÉâÉìÉ_ÉÄê´ÇÕÇ«Ç§Ç∑ÇÈÇ©
        _nextFrequency = UnityEngine.Random.Range(MinFrequency, MaxFrequency);
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
}