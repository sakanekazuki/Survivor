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
	[AddComponentMenu("TopDown Engine/Weapons/SurviveWeaponIAI")]
	public class SurviveWeaponIAI : SurviveWeaponProjectile
	{
		
		/// <summary>
		/// Initialize this weapon
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
		
		}

		/// <summary>
		/// Called everytime the weapon is used
		/// </summary>
		public override void WeaponUse()
		{
			base.WeaponUse();

			
		}

		/// <summary>
		/// Spawns a new object and positions/resizes it
		/// </summary>
		public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
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
			var sp = projectile.gameObject.MMGetComponentNoAlloc<SurviveWeaponIaiEX>();
			sp.subDatas = subDatas;
			//var sp = projectile as SurviveProjectile;
			//sp.subHitCallBacks.Clear();
			//if (sp != null && subDatas.Count > 0)
			//{
			//	foreach (var data in subDatas)
			//	{
			//		if (data.active && data.hitCallback != null)
			//		{
			//			sp.subHitCallBacks.Add(data.hitCallback);
			//		}
			//	}
			//}
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
			}

			if (triggerObjectActivation)
			{
				if (nextGameObject.GetComponent<MMPoolableObject>() != null)
				{
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
			}


			//if (sp != null && subDatas.Count > 0)
			//{
			//	foreach (var data in subDatas)
			//	{
			//		if (data.active && data.shotCallback != null)
			//		{
			//			data.shotCallback.Invoke(nextGameObject);
			//		}
			//	}
			//}

			return (nextGameObject);
		}



	}
}