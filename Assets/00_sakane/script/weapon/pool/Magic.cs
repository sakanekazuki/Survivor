using System.Collections;
using UnityEngine;

namespace EroSurvivor
{
	public class Magic : SurviveProjectile
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

		/// <summary>
		/// ˆÚ“®
		/// </summary>
		public override void Movement()
		{
			_movement = Direction * (Speed * 0.1f) * Time.deltaTime;
			//transform.Translate(_movement,Space.World);
			if (_rigidBody != null)
			{
				_rigidBody.MovePosition(this.transform.position + _movement);
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.MovePosition(this.transform.position + _movement);
			}
			Speed += Acceleration * Time.deltaTime;
		}

		/// <summary>
		/// Sets the projectile's direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		/// <param name="newRotation">New rotation.</param>
		/// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
		public override void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			_spawnerIsFacingRight = spawnerIsFacingRight;

			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}

			if (_damageOnTouch != null)
			{
				_damageOnTouch.SetKnockbackScriptDirection(newDirection);
			}

			if (FaceMovement)
			{
				switch (MovementVector)
				{
				case MovementVectors.Forward:
					transform.forward = newDirection;
					break;
				case MovementVectors.Right:
					transform.right = newDirection;
					break;
				case MovementVectors.Up:
					transform.up = newDirection;
					break;
				}
			}
		}
	}
}