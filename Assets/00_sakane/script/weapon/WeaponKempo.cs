using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using UnityEngine;

namespace EroSurvivor
{
	public class WeaponKempo : SurviveWeaponProjectile, MMEventListener<TopDownEngineEvent>
	{
		public override void Initialization()
		{
			base.Initialization();
		}

		/// <summary>
		/// 時間で武器を使う際に呼ばれる
		/// </summary>
		public override void WeaponUse()
		{
			base.WeaponUse();
		}

		/// <summary>
		/// 新しいオブジェクトの生成
		/// </summary>
		/// <param name="spawnPosition">生成する位置</param>
		/// <param name="projectileIndex">生成する番号</param>
		/// <param name="totalProjectiles">生成する数</param>
		/// <param name="triggerObjectActivation"></param>
		/// <returns></returns>
		public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
		{
			// 蓄えてあるオブジェクトを取得して、nullでないことを確認
			var nextGameObject = ObjectPooler.GetPooledGameObject();

			if (nextGameObject == null)
			{
				return null;
			}
			if (nextGameObject.GetComponent<MMPoolableObject>() == null)
			{
				throw new Exception(gameObject.name + "はPoolableObjectを持たないオブジェクトを生成しようとしています。");
			}
			// 位置設定
			nextGameObject.transform.position = spawnPosition;
			if (_projectileSpawnTransform != null)
			{
				nextGameObject.transform.position = _projectileSpawnTransform.position;
			}

			// 方向設定

			var projectile = nextGameObject.GetComponent<Projectile>();
			if (projectile != null)
			{
				projectile.SetWeapon(this);
				if (Owner != null)
				{
					projectile.SetOwner(Owner.gameObject);
				}
			}

			// オブジェクトをアクティブ状態にする
			nextGameObject.gameObject.SetActive(true);

			if (!isEx)
			{
				SetBulletData(projectile as SurviveProjectile);
			}
			else
			{
				SetExBulletData(projectile as SurviveProjectile, exIndex);
			}
			if (currentData.exWeaponStatuses != null && currentData.exWeaponStatuses.Exists(_ => _.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.OukaEx))
			{

				if (Owner.MovementState.CurrentState == CharacterStates.MovementStates.Idle)
				{
					//ダメージと範囲再計算
					float damage = (Owner as SurvivePlayer).battleParameter.ATTACK * ((currentData.DamageRate / 100f) + 0.5f);
					damage = Mathf.Floor(damage);//繰り上げ
					projectile.SetDamage(damage, damage);

					var size = currentData.Size * ((Owner as SurvivePlayer).battleParameter.GetSkillArea(main) + 0.75f);
					projectile.transform.localScale = new Vector3(size, size, 1);

				}
				else /*if (Owner.MovementState.CurrentState == CharacterStates.MovementStates.Walking)*/
				{
					//CD再計算　移動中HP自然回復は別途付与
					TimeBetweenUses = Mathf.Max(currentData.Interval * ((Owner as SurvivePlayer).battleParameter.GetSkillInterval(main) - 0.2f), currentData.Interval * 0.1f);

				}
			}

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
			}

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

			return nextGameObject;
		}
	}
}