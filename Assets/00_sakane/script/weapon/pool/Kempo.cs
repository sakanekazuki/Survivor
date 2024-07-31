using System.Collections;
using UnityEngine;

namespace EroSurvivor
{
	public class Kempo : SurviveProjectile
	{
		protected override void Awake()
		{
			base.Awake();
		}

		protected override IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null)
			{
				yield break;
			}
			if (_weapon == null)
			{
				yield break;
			}

			_damageOnTouch.ClearIgnoreList();
			_damageOnTouch.IgnoreGameObject(_weapon.gameObject);
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.gameObject);
			}
		}

		protected override void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = _initialFlipX;
			}
			transform.localScale = _initialLocalScale;
			_shouldMove = true;
			_damageOnTouch?.InitializeFeedbacks();

			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

		protected override void FixedUpdate()
		{
			base.Update();
			if (_shouldMove)
			{
				Movement();
			}
		}

		public override void Movement()
		{

		}

		public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			
		}
	}
}