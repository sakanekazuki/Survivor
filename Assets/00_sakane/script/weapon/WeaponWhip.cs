using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EroSurvivor
{
	// 鞭
	public class WeaponWhip : SurviveWeaponBase, MMEventListener<TopDownEngineEvent>
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

		[Header("2連攻撃の間の秒数")]
		public float dualSecond = 0.1f;
		[Header("2連攻撃の角度")]
		public float[] DefaultProjectileDirectionDual = new float[] { 30, -30 };
		[Header("2連攻撃の位置")]
		public Vector3[] ProjectileSpawnOffsetDual = new Vector3[] { Vector3.zero, Vector3.zero };

		public MMObjectPooler EXObjectPooler;
		public Vector3 ExSpawnPosition = Vector3.zero;
		public float ExDirection = 0;

		public override void Initialization()
		{
			base.Initialization();

			if (!_poolInitialized)
			{
				if (ObjectPooler == null)
				{
					ObjectPooler = GetComponent<MMObjectPooler>();
				}
				if (ObjectPooler == null)
				{
					Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
					return;
				}
				if (FlipWeaponOnCharacterFlip)
				{
					_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
					_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				}
				_poolInitialized = true;
			}
		}

		public override void WeaponUse()
		{
			base.WeaponUse();

			if (currentData.exWeaponStatuses != null && currentData.exWeaponStatuses.Count > 0)
			{
				for (int i = 0; i < currentData.exWeaponStatuses.Count; i++)
				{
					var status = currentData.exWeaponStatuses[i];
					if (status.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.ShockWave)
					{
						shootCount++;
						if (shootCount >= 10)//回数
						{
							EXDetermineSpawnPosition();
							SpawnEXProjectile(ExSpawnPosition, 0, ProjectilesPerShot, true, i);
							shootCount = 0;
						}
					}
				}

			}
			for (int i = 0; i < ProjectilesPerShot; i++)
			{
				StartCoroutine(SpawnProjectileCoroutine(SpawnPosition, i, ProjectilesPerShot, true));
				PlaySpawnFeedbacks();
			}
		}

		public IEnumerator SpawnProjectileCoroutine(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
		{
			// 時間計測
			for (int i = 0; i < 2; i++)
			{
				DetermineSpawnPosition(i);
				SpawnProjectile(SpawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation, i);

				yield return new WaitForSeconds(dualSecond);
			}

			yield return 0;
		}

		public GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true, int dualIndex = 0)
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
			SetBulletData(projectile as SurviveProjectile);

			var sp = projectile as SurviveProjectile;
			sp.subHitCallBacks.Clear();
			if (sp != null && subDatas.Count > 0)
			{
				foreach (var data in subDatas)
				{
					if (data.active && data.hitCallback != null)
					{
						sp.subHitCallBacks.Add(data.hitCallback);
					}
				}
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

						Vector3 newDirection = spread * transform.right * (Flipped ? -1 : 1);
						var euler = Quaternion.Euler(0, 0, DefaultProjectileDirectionDual[dualIndex]);
						if (Owner.Orientation2D != null)
						{
							projectile.SetDirection(newDirection, euler * transform.rotation, true/*Owner.Orientation2D.IsFacingRight*/);
						}
						else
						{
							projectile.SetDirection(newDirection, euler * transform.rotation, true);
						}
					}
				}

				if (RotateWeaponOnSpread)
				{

					//ブレがあっても元の武器向きは変わらない
					//this.transform.rotation = this.transform.rotation * spread;
				}
			}
			nextGameObject.transform.localEulerAngles = new Vector3(0, dualIndex * 180, 0);

			if (triggerObjectActivation)
			{
				if (nextGameObject.GetComponent<MMPoolableObject>() != null)
				{
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
			}

			if (sp != null && subDatas.Count > 0)
			{
				foreach (var data in subDatas)
				{
					if (data.active && data.shotCallback != null)
					{
						data.shotCallback.Invoke(nextGameObject);
					}
				}
			}

			return (nextGameObject);
		}

		public GameObject SpawnEXProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true, int exIndex = 0)
		{
			/// we get the next object in the pool and make sure it's not null
			GameObject nextGameObject = EXObjectPooler.GetPooledGameObject();

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
			SetExBulletData(projectile as SurviveProjectile, exIndex);
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
						var exD = Quaternion.Euler(0, 0, ExDirection);
						Vector3 newDirection = exD * transform.right * (Flipped ? -1 : 1);
						if (Owner.Orientation2D != null)
						{
							projectile.SetDirection(newDirection, exD * transform.rotation, true/*Owner.Orientation2D.IsFacingRight*/);
						}
						else
						{
							projectile.SetDirection(newDirection, exD * transform.rotation, true);
						}
					}
				}

				if (RotateWeaponOnSpread)
				{

					//ブレがあっても元の武器向きは変わらない
					//this.transform.rotation = this.transform.rotation * spread;
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

		public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
		{
			_projectileSpawnTransform = newSpawnTransform;
		}

		public virtual void DetermineSpawnPosition(int i)
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
				SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffsetDual[i];
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
		public virtual void EXDetermineSpawnPosition()
		{
			if (Flipped)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					ExSpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
				}
				else
				{
					ExSpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
				}
			}
			else
			{
				ExSpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
			}

			if (WeaponUseTransform != null)
			{
				ExSpawnPosition = WeaponUseTransform.position;
			}

			if (SpawnTransforms.Count > 0)
			{
				if (SpawnTransformsMode == SpawnTransformsModes.Random)
				{
					_spawnArrayIndex = Random.Range(0, SpawnTransforms.Count);
					ExSpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
				else
				{
					ExSpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition(0);

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

		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}