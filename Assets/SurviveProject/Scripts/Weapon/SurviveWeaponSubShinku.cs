using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A weapon class aimed specifically at allowing the creation of various projectile weapons, from shotgun to machine gun, via plasma gun or rocket launcher
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/SurviveWeaponSubShinku")]
	public class SurviveWeaponSubShinku : SurviveWeaponBase, MMEventListener<TopDownEngineEvent>, MMEventListener<MainWeaponShotEvent>
	{
		[MMInspectorGroup("Projectiles", true, 22)]
		/// the offset position at which the projectile will spawn
		[Tooltip("the offset position at which the projectile will spawn")]
		public Vector3 ProjectileSpawnOffset = Vector3.zero;
		/// in the absence of a character owner, the default direction of the projectiles
		[Tooltip("in the absence of a character owner, the default direction of the projectiles")]
		public Vector3 DefaultProjectileDirection = Vector3.forward;
		/// the number of projectiles to spawn per shot
		[Tooltip("the number of projectiles to spawn per shot")]
		public int ProjectilesPerShot = 1;

		[Header("Spawn Transforms")]
		/// a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy
		[Tooltip("a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy")]
		public List<Transform> SpawnTransforms = new List<Transform>();
		/// a list of modes the spawn transforms can operate on
		public enum SpawnTransformsModes { Random, Sequential }
		/// the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot
		[Tooltip("the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot")]
		public SpawnTransformsModes SpawnTransformsMode = SpawnTransformsModes.Sequential;

		[Header("Spread")]
		/// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
		[Tooltip("the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile")]
		public Vector3 Spread = Vector3.zero;
		/// whether or not the weapon should rotate to align with the spread angle
		[Tooltip("whether or not the weapon should rotate to align with the spread angle")]
		public bool RotateWeaponOnSpread = false;
		/// whether or not the spread should be random (if not it'll be equally distributed)
		[Tooltip("whether or not the spread should be random (if not it'll be equally distributed)")]
		public bool RandomSpread = true;
		/// the projectile's spawn position
		[MMReadOnly]
		[Tooltip("the projectile's spawn position")]
		public Vector3 SpawnPosition = Vector3.zero;

		/// the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object
		[Tooltip("the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object")]
		public MMObjectPooler ObjectPooler;

		[Header("Spawn Feedbacks")]
		public List<MMFeedbacks> SpawnFeedbacks = new List<MMFeedbacks>();

		protected Vector3 _flippedProjectileSpawnOffset;
		protected Vector3 _randomSpreadDirection;
		protected bool _poolInitialized = false;
		protected Transform _projectileSpawnTransform;
		protected int _spawnArrayIndex = 0;

		[MMInspectorButton("TestShoot")]
		/// a button to test the shoot method
		public bool TestShootButton;

		public bool isEx = false;
		public int exIndex = 0;



		public override void SetParameter(AbilityData.WeaponLevelData data, GameDefine.StatusAttribute attribute, int id)
		{
			currentData = data;
			currentAttribute = attribute;
			AbilityID = id;

			if (IsBurstShot)
			{
				BurstLength = currentData.Num + (Owner != null ? (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID) : 0);
				//バーストの間隔の設定は未定
			}
			else
			{
				//バーストではないパターンはこれ系
				(this as SurviveWeaponSubShinku).ProjectilesPerShot = currentData.Num + (Owner != null ? (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID) : 0);
			}
			//var PermanentEffects = (Owner as SurvivePlayer).CalcPermanentDataStatus();

			////最低限のスピードは持たせる　要デバッグ
			//TimeBetweenUses = Mathf.Max(currentData.Interval - (currentData.Interval * PermanentEffects[(int)SurviveProgressManager.PermanentEffect.AttackInterval] / 100f), currentData.Interval*0.1f);

			foreach (var ws in (Owner as SurvivePlayer).skillSets)
			{
				foreach (var w in ws.surviveWeaponBases)
				{
					if (w.main)
					{
						if(ws.AbilityID == 4)
						{
							currentData.Speed *= 1.5f;
						}
						break;
					}
				}
			}
		}
		public override void UpdateParameter()
		{
			//キャラステータス変更に合わせて更新入れたり

			if (IsBurstShot)
			{
				BurstLength = currentData.Num + (Owner != null ? (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID) : 0);
				//バーストの間隔の設定は未定
			}
			else
			{
				//バーストではないパターンはこれ系
				(this as SurviveWeaponSubShinku).ProjectilesPerShot = currentData.Num + (Owner != null ? (Owner as SurvivePlayer).battleParameter.GetThrowNum(AbilityID) : 0);
			}
			var PermanentEffects = (Owner as SurvivePlayer).CalcPermanentDataStatus();

			//最低限のスピードは持たせる　要デバッグ
			TimeBetweenUses = Mathf.Max(currentData.Interval * (Owner as SurvivePlayer).battleParameter.GetSkillInterval(main), currentData.Interval * 0.1f);

		}


		/// <summary>
		/// A test method that triggers the weapon
		/// </summary>
		protected virtual void TestShoot()
		{
			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				WeaponInputStart();
			}
			else
			{
				WeaponInputStop();
			}
		}

		/// <summary>
		/// Initialize this weapon
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			//_weaponAim = GetComponent<WeaponAim>();
			IsBurstShot = false;


			if (!_poolInitialized)
			{
				//if (ObjectPooler == null)
				//{
				//	ObjectPooler = GetComponent<MMObjectPooler>();
				//}
				//if (ObjectPooler == null)
				//{
				//	Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
				//	return;
				//}
				//if (FlipWeaponOnCharacterFlip)
				//{
				//	_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
				//	_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				//}
				_poolInitialized = true;
			}
		}

		/// <summary>
		/// Called everytime the weapon is used
		/// </summary>
		public override void WeaponUse()
		{
			//base.WeaponUse();

			//DetermineSpawnPosition();

			//for (int i = 0; i < ProjectilesPerShot; i++)
			//{
			//	SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
			//	PlaySpawnFeedbacks();
			//}
		}

		/// <summary>
		/// Spawns a new object and positions/resizes it
		/// </summary>
		public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true, GameObject target = null)
		{
			/// we get the next object in the pool and make sure it's not null
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

			// mandatory checks
			if (nextGameObject == null) { return null; }
			if (nextGameObject.GetComponent<MMPoolableObject>() == null)
			{
				throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
			}
			// we position the object
			nextGameObject.transform.position = spawnPosition;
			if (_projectileSpawnTransform != null)
			{
				nextGameObject.transform.position = _projectileSpawnTransform.position;
			}
			// we set its direction

			Projectile projectile = nextGameObject.GetComponent<Projectile>();
			if (projectile != null)
			{
				projectile.SetWeapon(this);
				if (Owner != null)
				{
					projectile.SetOwner(Owner.gameObject);
				}



			}
			// we activate the object
			nextGameObject.gameObject.SetActive(true);
			//ダメージのブレに関してどうするか　　検討（多分特に扱わない）
			//武器パラメータ、キャラ、キャラ強化、等参照
			//一旦ダメージだけ反映してみる(キャラステそのまま反映)
			if (!isEx)
			{
				SetBulletData(projectile as SurviveProjectile);
			}
			else
			{
				SetExBulletData(projectile as SurviveProjectile, exIndex);
			}
			if (projectile != null)
			{
				if (RandomSpread)
				{
					_randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
					_randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
					_randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
				}
				else
				{
					if (totalProjectiles > 1)
					{
						_randomSpreadDirection.x = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
						_randomSpreadDirection.y = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
						_randomSpreadDirection.z = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
					}
					else
					{
						_randomSpreadDirection = Vector3.zero;
					}
				}

				Quaternion spread = Quaternion.Euler(_randomSpreadDirection);

				if (Owner == null)
				{
					projectile.SetDirection(spread * transform.rotation * DefaultProjectileDirection, transform.rotation, true);
				}
				else
				{
					if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D) // if we're in 3D
					{
						projectile.SetDirection(spread * transform.forward, transform.rotation, true);
					}
					else // if we're in 2D
					{
						Vector3 newDirection = (spread * transform.right) * (Flipped ? -1 : 1);
						if (Owner.Orientation2D != null)
						{
							projectile.SetDirection(newDirection, spread * transform.rotation, true/*Owner.Orientation2D.IsFacingRight*/);
						}
						else
						{
							projectile.SetDirection(newDirection, spread * transform.rotation, true);
						}
					}
				}

				if (RotateWeaponOnSpread)
				{

					//ブレがあっても元の武器向きは変わらない
					//this.transform.rotation = this.transform.rotation * spread;
				}

				if (projectile is SurviveBurn)
				{
					(projectile as SurviveBurn).SetTarget(target.GetComponent<Health>());
				}
				else
				{
					projectile.transform.position = target.transform.position;
				}
			}

			if (triggerObjectActivation)
			{
				if (nextGameObject.GetComponent<MMPoolableObject>() != null)
				{
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
			}

			return (nextGameObject);
		}

		/// <summary>
		/// This method is in charge of playing feedbacks on projectile spawn
		/// </summary>
		protected virtual void PlaySpawnFeedbacks()
		{
			if (SpawnFeedbacks.Count > 0)
			{
				SpawnFeedbacks[_spawnArrayIndex]?.PlayFeedbacks();
			}

			_spawnArrayIndex++;
			if (_spawnArrayIndex >= SpawnTransforms.Count)
			{
				_spawnArrayIndex = 0;
			}
		}

		/// <summary>
		/// Sets a forced projectile spawn position
		/// </summary>
		/// <param name="newSpawnTransform"></param>
		public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
		{
			_projectileSpawnTransform = newSpawnTransform;
		}

		/// <summary>
		/// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
		/// </summary>
		public virtual void DetermineSpawnPosition()
		{
			if (Flipped)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					SpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
				}
				else
				{
					SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
				}
			}
			else
			{
				SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
			}

			if (WeaponUseTransform != null)
			{
				SpawnPosition = WeaponUseTransform.position;
			}

			if (SpawnTransforms.Count > 0)
			{
				if (SpawnTransformsMode == SpawnTransformsModes.Random)
				{
					_spawnArrayIndex = Random.Range(0, SpawnTransforms.Count);
					SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
				else
				{
					SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
			}
		}

		/// <summary>
		/// When the weapon is selected, draws a circle at the spawn's position
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
		}

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
			case TopDownEngineEventTypes.LevelStart:
				_poolInitialized = false;
				Initialization();
				break;
			}
		}

		public override void CaseWeaponIdle()
		{
			//ResetMovementMultiplier();
		}

		public override void CaseWeaponStart()
		{
			//if (DelayBeforeUse > 0)
			//{
			//	_delayBeforeUseCounter = DelayBeforeUse;
			//	WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			//}
			//else
			//{
			//	StartCoroutine(ShootRequestCo());
			//}
		}
		public override void CaseWeaponUse()
		{
			//WeaponUse();
			//_delayBetweenUsesCounter = TimeBetweenUses;
			//WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}
		int mainWeaponId = 0;

		[Serializable]
		public class WeaponEffectData
		{
			public int WeaponID;
			[SerializeField]
			public List<ObjectWithName> Effects;

			[Serializable]
			public class ObjectWithName
			{
				public GameObject Effect;
				public string Name;
			}
		}
		[SerializeField]
		public List<WeaponEffectData> WeaponEffectDatas = new List<WeaponEffectData>();

		public void WeaponUseStart(GameObject baseObj)
		{
			if (mainWeaponId == 0)
			{
				foreach (var ws in (Owner as SurvivePlayer).skillSets)
				{
					foreach (var w in ws.surviveWeaponBases)
					{
						if (w.main)
						{
							mainWeaponId = ws.AbilityID;
							break;
						}
					}
				}
				if (mainWeaponId == 0)
				{
					return;
				}



			}

			mainShotCount++;
			if (mainShotCount >= currentData.Interval)
			{
				mainShotCount = 0;
				StartCoroutine(WeaponUse(baseObj));
				//if (WeaponBase != null)
				//{
				//	if (WeaponBase.subDatas.Exists(_ => _.weaponID == ID.ToString()))
				//	{
				//		WeaponBase.subDatas.Find(_ => _.weaponID == ID.ToString()).active = false;
				//	}
				//}
			}

		}
		public IEnumerator WeaponUse(GameObject target)
		{
			base.WeaponUse();

			//DetermineSpawnPosition();
			for (int i = 0; i < ProjectilesPerShot; i++)
			{
				CopyObject(target);
				PlaySpawnFeedbacks();

				yield return new WaitForSeconds(BurstTimeBetweenShots);
			}


		}


		public void CopyObject(GameObject target)
		{


			if (WeaponEffectDatas.Exists(_ => _.WeaponID == mainWeaponId))
			{
				var data = WeaponEffectDatas.Find(_ => _.WeaponID == mainWeaponId);
				var name = target.name;
				if (name.Contains("-"))
				{
					name = name.Substring(0, name.IndexOf("-"));

				}
				var effect = data.Effects.Find(_ => _.Name == name);
				GameObject obj = Instantiate(target, target.transform.parent);
				if (mainWeaponId == 4)
				{
					obj.GetComponentInChildren<SpriteRenderer>().sprite = null;
					var col2D = obj.GetComponent<BoxCollider2D>();
					col2D.offset = new Vector2(0, 0.3f);
					col2D.size = new Vector2(1.7f, 2);
				}
				obj.SetActive(true);
				obj.name = name;
				var baseEffectList = obj.GetComponentsInChildren<ParticleSystem>();
				foreach (var e in baseEffectList)
				{
					e.gameObject.SetActive(false);
				}
				GameObject neweffect = Instantiate(effect.Effect, obj.transform);

				//neweffect.transform.localPosition = Vector3.zero;
				if (mainWeaponId == 4)
				{
					neweffect.transform.localScale = new Vector3(0.5f, 0.5f, 1);
				}
				var proj = obj.GetComponent<SurviveProjectile>();
				proj.subHitCallBacks.Clear();
				SetBulletData(proj);
				proj.Direction = target.transform.right;
				proj.GetDamageOnTouch().InvincibilityDuration = currentData.Time;
				proj.ExecuteOnDisable.AddListener(() =>
				{

					//EX判定
					if (currentData.exWeaponStatuses != null && currentData.exWeaponStatuses.Exists(_ => _.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.SHINKU6))
						CopyObject6(obj);
					GameObject.Destroy(proj.gameObject);


				});
			}

		}

		//6方向コピー

		public void CopyObject6(GameObject target)
		{
			for (int i = 0; i < 6; i++)
			{
				if (WeaponEffectDatas.Exists(_ => _.WeaponID == mainWeaponId))
				{
					var data = WeaponEffectDatas.Find(_ => _.WeaponID == mainWeaponId);
					var name = target.name;
					if (name.Contains("-"))
					{
						name = name.Substring(0, name.IndexOf("-"));

					}
					var effect = data.Effects.Find(_ => _.Name == name);
					GameObject obj = Instantiate(target, target.transform.parent);
					obj.SetActive(true);
					var baseEffectList = obj.GetComponentsInChildren<ParticleSystem>();
					foreach (var e in baseEffectList)
					{
						e.gameObject.SetActive(false);
					}
					GameObject neweffect = Instantiate(effect.Effect, obj.transform);

					//neweffect.transform.localPosition = Vector3.zero;

					var proj = obj.GetComponent<SurviveProjectile>();
					proj.subHitCallBacks.Clear();
					SetExBulletData(proj, 0);
					var euler = Quaternion.Euler(0, 0, (float)(60 * i));
					Vector3 newDirection = obj.transform.right;
					proj.SetDirection(euler * newDirection, euler * obj.transform.rotation, true/*Owner.Orientation2D.IsFacingRight*/);
					//proj.transform.rotation = Quaternion.Euler(0f, 0f, 360 / (i+1));
					proj.GetDamageOnTouch().InvincibilityDuration = currentData.Time;
					proj.ExecuteOnDisable.AddListener(() =>
					{


						GameObject.Destroy(proj.gameObject);


					});
					PlaySpawnFeedbacks();
				}
			}


		}


		int mainShotCount = 0;
		int mainShotMax = 1;
		SurviveWeaponBase WeaponBase = null;

		public void OnMMEvent(MainWeaponShotEvent mainShotEvent)
		{
			if (WeaponBase == null)
			{
				WeaponBase = mainShotEvent.WeaponBase;
			}
			if (WeaponBase.subDatas.Exists(_ => _.weaponID == ID.ToString()))
			{
				WeaponBase.subDatas.Find(_ => _.weaponID == ID.ToString()).active = true;
			}
			else
			{
				WeaponBase.subDatas.Add(
					new SubData() { active = true, shotCallback = WeaponUseStart, weaponID = ID.ToString() }
					);
			}
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
			this.MMEventStartListening<MainWeaponShotEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
			this.MMEventStopListening<MainWeaponShotEvent>();
		}

		protected override void SetBulletData(SurviveProjectile projectile)
		{
			base.SetBulletData(projectile);



		}
		protected override void SetExBulletData(SurviveProjectile projectile, int id)
		{
			base.SetExBulletData(projectile, id);
		}
	}
}