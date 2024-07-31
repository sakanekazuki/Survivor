using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




//基本的に弾はこれを継承
public class SurviveKunai : SurviveProjectile
{

	/// <summary>
	/// On awake, we store the initial speed of the object 
	/// </summary>
	protected override void Awake()
	{
		base.Awake();
		
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


	//　現在の角度
	private float angle;
	//　回転するスピード
	private float rotateSpeed = 180f;
	//　ターゲットからの距離
	public Vector3 distanceFromTarget = new Vector3(5f, 0f, 0f);

	// Update is called once per frame

	public void SetAngle(float _angle)
    {
		angle = _angle;

	}
	void UpdateTest()
	{

		//　ユニットの位置 = ターゲットの位置 ＋ ターゲットから見たユニットの角度 ×　ターゲットからの距離
		transform.position = _owner.transform.position + Quaternion.Euler(0f, 0f, angle) * distanceFromTarget;
		//　ユニット自身の角度 = ターゲットから見たユニットの方向の角度を計算しそれをユニットの角度に設定する
		//transform.rotation = Quaternion.LookRotation(transform.position - new Vector3(_owner.transform.position.x, _owner.transform.position.y, transform.position.z), Vector3.forward);
		transform.localEulerAngles = new Vector3(0, 0, angle);
		//　ユニットの角度を変更
		angle += Speed * Time.deltaTime;
		//　角度を0～360度の間で繰り返す
		angle = Mathf.Repeat(angle, 360f);
	}
	/// <summary>
	/// Handles the projectile's movement, every frame
	/// </summary>
	public override void Movement()
	{
		UpdateTest();
		//float distance = 2;
		//float followRate = 1;
		//transform.position = Vector3.Lerp(transform.position, _owner.transform.position +
		//(transform.position - _owner.transform.position).normalized * distance, followRate);

		//Vector3 _axis = Vector3.forward;
		////_movement = Direction * (Speed / 10) * Time.deltaTime;
		//var _center = _owner.transform.position;
		////距離調整 距離指定

		//         var tr = transform;
		//// 回転のクォータニオン作成
		//var angleAxis = Quaternion.AngleAxis(360 / Speed * Time.deltaTime, _axis);

		//// 円運動の位置計算
		//var pos = tr.position;

		//pos -= _center;
		//pos = angleAxis * pos;
		//pos += _center;

		////tr.position = pos;
		//_rigidBody2D.MovePosition(pos);
		//// 向き更新
		////if (_updateRotation)
		//{
		//_rigidBody2D.MoveRotation(angleAxis);
		//	//tr.rotation = tr.rotation * angleAxis;
		//}

		//transform.RotateAround(
		//	_owner.transform.position,
		//	_axis,
		//	360f /Speed  * Time.deltaTime
		//);
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
		base.OnEnable();

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

	public DamageOnTouch GetDamageOnTouch()
    {
		return _damageOnTouch == null? GetComponent<DamageOnTouch>():_damageOnTouch;
    }
	public Health GetHealth()
	{
		return _health == null ? GetComponent<Health>() : _health;
	}
}	

