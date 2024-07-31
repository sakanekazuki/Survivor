using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//äÓñ{ìIÇ…íeÇÕÇ±ÇÍÇåpè≥
public class SurviveProjectileAfterEX : SurviveProjectile
{
	protected override void Awake()
	{
		guid = Guid.NewGuid();
		_facingRightInitially = ProjectileIsFacingRight;
		_initialSpeed = Speed;
		_health = GetComponent<Health>();
		_collider = GetComponent<Collider>();
		_collider2D = GetComponent<Collider2D>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_damageOnTouch = GetComponent<DamageOnTouch>();
		_rigidBody = GetComponent<Rigidbody>();
		_rigidBody2D = GetComponent<Rigidbody2D>();
		_initialInvulnerabilityDurationWFS = new WaitForSeconds(InitialInvulnerabilityDuration);
		if (_spriteRenderer != null) { _initialFlipX = _spriteRenderer.flipX; }
		_initialLocalScale = transform.localScale;
	}

	/// <summary>
	/// Handles the projectile's initial invincibility
	/// </summary>
	/// <returns>The invulnerability.</returns>
	protected override IEnumerator InitialInvulnerability()
	{
		if (_damageOnTouch == null) { yield break; }
		if (_weapon == null) { yield break; }

		_damageOnTouch.ClearIgnoreList();
		_damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);
		yield return _initialInvulnerabilityDurationWFS;
		if (DamageOwner)
		{
			_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
		}
	}

	/// <summary>
	/// Initializes the projectile
	/// </summary>
	protected override void Initialization()
	{
		Speed = _initialSpeed;
		ProjectileIsFacingRight = _facingRightInitially;
		if (_spriteRenderer != null) { _spriteRenderer.flipX = _initialFlipX; }
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

		currentTime = 0f;
		afterOn = false;
	}

	/// <summary>
	/// On update(), we move the object based on the level's speed and the object's speed, and apply acceleration
	/// </summary>
	protected override void FixedUpdate()
	{
		base.Update();
		if (_shouldMove)
		{
			Movement();
		}
	}

	public float afterSecond = 0f;
	public float currentTime = 0f;
	public bool afterOn = false;
	/// <summary>
	/// Handles the projectile's movement, every frame
	/// </summary>
	public override void Movement()
	{
		//_movement = Direction * (Speed / 10) * Time.deltaTime;
		////transform.Translate(_movement,Space.World);
		//if (_rigidBody != null)
		//{
		//	_rigidBody.MovePosition(this.transform.position + _movement);
		//}
		//if (_rigidBody2D != null)
		//{
		//	_rigidBody2D.MovePosition(this.transform.position + _movement);
		//}
		//// We apply the acceleration to increase the speed
		//Speed += Acceleration * Time.deltaTime;
		currentTime += Time.deltaTime;
		var sw = SourceWeapon as SurviveWeaponBase;
		var data = sw.GetCurrentData();
		if (data.exWeaponStatuses.Exists(_ => _.exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.IAI2))
		{
			Vector2 _raycastOrigin = new Vector2();


			_raycastOrigin.x = transform.position.x;
			_raycastOrigin.y = transform.position.y;
			var _results = Physics2D.OverlapCircleAll(_raycastOrigin, 5f);
			if (_results != null && _results.Length > 0)
			{
				for (int i = 0; i < _results.Length; i++)
				{
					if (_results[i] == null)
					{
						continue;
					}
					if (_results[i].tag != "Enemy")
					{
						continue;
					}
					_results[i].transform.position = Vector3.MoveTowards(
					 _results[i].transform.position,
					 transform.position,
					 1f * Time.deltaTime);
				}
			}
		}
		if (!afterOn && currentTime >= afterSecond)
        {
	
			if (data != null && data.exWeaponStatuses != null && data.exWeaponStatuses.Count > 0)
			{
				for (int i = 0; i < data.exWeaponStatuses.Count; i++)
				{
					if (data.exWeaponStatuses[i].exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.IAI || data.exWeaponStatuses[i].exWeaponStatus == AbilityData.ExWeaponStatusData.ExWeaponStatus.IAI2)
					{
						var p = this.gameObject.MMGetComponentNoAlloc<SurviveWeaponIaiEX>();
						p.WeaponInputStart();
					}

				}
			}

			afterOn = true;
		}
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

	/// <summary>
	/// Flip the projectile
	/// </summary>
	protected override void Flip()
	{
		if (_spriteRenderer != null)
		{
			_spriteRenderer.flipX = !_spriteRenderer.flipX;
		}
		else
		{
			this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
		}
	}

	/// <summary>
	/// Flip the projectile
	/// </summary>
	protected override void Flip(bool state)
	{
		if (_spriteRenderer != null)
		{
			_spriteRenderer.flipX = state;
		}
		else
		{
			this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
		}
	}

	/// <summary>
	/// Sets the projectile's parent weapon.
	/// </summary>
	/// <param name="newWeapon">New weapon.</param>
	public override void SetWeapon(Weapon newWeapon)
	{
		_weapon = newWeapon;


		var p = this.gameObject.MMGetComponentNoAlloc<SurviveWeaponIaiEX>();
		//p.SetOwner( SourceWeapon.Owner, null);
		p.Initialization();
		p.SetExOwner(SourceWeapon.Owner as SurvivePlayer);
		p.SetParameter((SourceWeapon as SurviveWeaponBase).GetCurrentData(), GameDefine.StatusAttribute.NONE,(SourceWeapon as SurviveWeaponBase).AbilityID);
		p.TimeBetweenUses = (SourceWeapon as SurviveWeaponBase).GetCurrentData().exWeaponStatuses[0].exStatus.Interval;


	}

	/// <summary>
	/// Sets the damage caused by the projectile's DamageOnTouch to the specified value
	/// </summary>
	/// <param name="newDamage"></param>
	public override void SetDamage(float minDamage, float maxDamage)
	{
		if (_damageOnTouch != null)
		{
			_damageOnTouch.MinDamageCaused = minDamage;
			_damageOnTouch.MaxDamageCaused = maxDamage;
		}
	}

	/// <summary>
	/// Sets the projectile's owner.
	/// </summary>
	/// <param name="newOwner">New owner.</param>
	public override void SetOwner(GameObject newOwner)
	{
		_owner = newOwner;
		DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
		if (damageOnTouch != null)
		{
			damageOnTouch.Owner = newOwner;
			damageOnTouch.Owner = newOwner;
			if (!DamageOwner)
			{
				damageOnTouch.ClearIgnoreList();
				damageOnTouch.IgnoreGameObject(newOwner);
			}
		}
	}

	/// <summary>
	/// Returns the current Owner of the projectile
	/// </summary>
	/// <returns></returns>
	public override GameObject GetOwner()
	{
		return _owner;
	}

	/// <summary>
	/// On death, disables colliders and prevents movement
	/// </summary>
	public override void StopAt()
	{
		if (_collider != null)
		{
			_collider.enabled = false;
		}
		if (_collider2D != null)
		{
			_collider2D.enabled = false;
		}

		_shouldMove = false;
	}

	/// <summary>
	/// On death, we stop our projectile
	/// </summary>
	protected override void OnDeath()
	{
		StopAt();
	}

	/// <summary>
	/// On enable, we trigger a short invulnerability
	/// </summary>
	protected override void OnEnable()
	{
		//base.OnEnable();
		Size = GetBounds().extents * 2;
		//if (LifeTime > 0f)
		//{
		//	Invoke("Destroy", LifeTime);
		//}
		ExecuteOnEnable?.Invoke();

		Initialization();
		if (InitialInvulnerabilityDuration > 0)
		{
			StartCoroutine(InitialInvulnerability());
		}

		if (_health != null)
		{
			_health.OnDeath += OnDeath;
		}
	}

	/// <summary>
	/// On disable, we plug our OnDeath method to the health component
	/// </summary>
	protected override void OnDisable()
	{
		base.OnDisable();
		if (_health != null)
		{
			_health.OnDeath -= OnDeath;
		}
	}

}	

