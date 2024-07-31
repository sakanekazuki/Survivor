using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviveDamageOnTouch : DamageOnTouch
{

    #region Initialization

    /// <summary>
    /// On Awake we initialize our damage on touch area
    /// </summary>
    protected override void Awake()
    {
        Initialization();
    }

    /// <summary>
    /// OnEnable we set the start time to the current timestamp
    /// </summary>
    protected override void OnEnable()
    {
        _startTime = Time.time;
        _lastPosition = transform.position;
        _lastDamagePosition = transform.position;
    }

    /// <summary>
    /// Initializes ignore list, feedbacks, colliders and grabs components
    /// </summary>
    public override void Initialization()
    {
        InitializeIgnoreList();
        GrabComponents();
        InitalizeGizmos();
        InitializeColliders();
        InitializeFeedbacks();
    }

    /// <summary>
    /// Stores components
    /// </summary>
    protected override void GrabComponents()
    {
        _health = GetComponent<Health>();
        _topDownController = GetComponent<TopDownController>();
        _boxCollider = GetComponent<BoxCollider>();
        _sphereCollider = GetComponent<SphereCollider>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _lastDamagePosition = transform.position;
    }

    /// <summary>
    /// Initializes colliders, setting them as trigger if needed
    /// </summary>
    protected override void InitializeColliders()
    {
        //if(Owner.GetComponent<SurviveEnemy>() != null)
        //{
        //    return;
        //}
        _twoD = _boxCollider2D != null || _circleCollider2D != null;
        if (_boxCollider2D != null)
        {
            SetGizmoOffset(_boxCollider2D.offset);
            _boxCollider2D.isTrigger = true;
        }

        if (_boxCollider != null)
        {
            SetGizmoOffset(_boxCollider.center);
            _boxCollider.isTrigger = true;
        }

        if (_sphereCollider != null)
        {
            SetGizmoOffset(_sphereCollider.center);
            _sphereCollider.isTrigger = true;
        }

        if (_circleCollider2D != null)
        {
            SetGizmoOffset(_circleCollider2D.offset);
            _circleCollider2D.isTrigger = true;
        }
    }

    /// <summary>
    /// Initializes the _ignoredGameObjects list if needed
    /// </summary>
    protected override void InitializeIgnoreList()
    {
        if (_ignoredGameObjects == null) _ignoredGameObjects = new List<GameObject>();
    }

    /// <summary>
    /// Initializes feedbacks
    /// </summary>
    public override void InitializeFeedbacks()
    {
        if (_initializedFeedbacks) return;

        HitDamageableFeedback?.Initialization(this.gameObject);
        HitNonDamageableFeedback?.Initialization(this.gameObject);
        HitAnythingFeedback?.Initialization(this.gameObject);
        _initializedFeedbacks = true;
    }

    /// <summary>
    /// On disable we clear our ignore list
    /// </summary>
    protected override void OnDisable()
    {
        ClearIgnoreList();
    }

    /// <summary>
    /// On validate we ensure our inspector is in sync
    /// </summary>
    protected override void OnValidate()
    {
        TriggerFilter &= AllowedTriggerCallbacks;
    }

    #endregion

    #region Gizmos

    /// <summary>
    /// Initializes gizmo colors & settings
    /// </summary>
    protected override void InitalizeGizmos()
    {
        _gizmosColor = Color.red;
        _gizmosColor.a = 0.25f;
    }

    /// <summary>
    /// A public method letting you (re)define gizmo size
    /// </summary>
    /// <param name="newGizmoSize"></param>
    public override void SetGizmoSize(Vector3 newGizmoSize)
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _boxCollider = GetComponent<BoxCollider>();
        _sphereCollider = GetComponent<SphereCollider>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _gizmoSize = newGizmoSize;
    }

    /// <summary>
    /// A public method letting you specify a gizmo offset
    /// </summary>
    /// <param name="newOffset"></param>
    public override void SetGizmoOffset(Vector3 newOffset)
    {
        _gizmoOffset = newOffset;
    }

    /// <summary>
    /// draws a cube or sphere around the damage area
    /// </summary>
    protected override void OnDrawGizmos()
    {
        Gizmos.color = _gizmosColor;

        if (_boxCollider2D != null)
        {
            if (_boxCollider2D.enabled)
            {
                MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
            }
            else
            {
                MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
            }
        }

        if (_circleCollider2D != null)
        {
            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (_circleCollider2D.enabled)
            {
                Gizmos.DrawSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
            }
            else
            {
                Gizmos.DrawWireSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
            }
        }

        if (_boxCollider != null)
        {
            if (_boxCollider.enabled)
                MMDebug.DrawGizmoCube(transform,
                    _gizmoOffset,
                    _boxCollider.size,
                    false);
            else
                MMDebug.DrawGizmoCube(transform,
                    _gizmoOffset,
                    _boxCollider.size,
                    true);
        }

        if (_sphereCollider != null)
        {
            if (_sphereCollider.enabled)
                Gizmos.DrawSphere(transform.position, _sphereCollider.radius);
            else
                Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
        }
    }

    #endregion

    #region PublicAPIs

    /// <summary>
    /// When knockback is in script direction mode, lets you specify the direction of the knockback
    /// </summary>
    /// <param name="newDirection"></param>
    public override void SetKnockbackScriptDirection(Vector3 newDirection)
    {
        _knockbackScriptDirection = newDirection;
    }

    /// <summary>
    /// When damage direction is in script mode, lets you specify the direction of damage
    /// </summary>
    /// <param name="newDirection"></param>
    public override void SetDamageScriptDirection(Vector3 newDirection)
    {
        _damageDirection = newDirection;
    }

    /// <summary>
    /// Adds the gameobject set in parameters to the ignore list
    /// </summary>
    /// <param name="newIgnoredGameObject">New ignored game object.</param>
    public override void IgnoreGameObject(GameObject newIgnoredGameObject)
    {
        InitializeIgnoreList();
        _ignoredGameObjects.Add(newIgnoredGameObject);
    }

    /// <summary>
    /// Removes the object set in parameters from the ignore list
    /// </summary>
    /// <param name="ignoredGameObject">Ignored game object.</param>
    public override void StopIgnoringObject(GameObject ignoredGameObject)
    {
        if (_ignoredGameObjects != null) _ignoredGameObjects.Remove(ignoredGameObject);
    }

    /// <summary>
    /// Clears the ignore list.
    /// </summary>
    public override void ClearIgnoreList()
    {
        InitializeIgnoreList();
        _ignoredGameObjects.Clear();
    }

    #endregion

    #region Loop

    /// <summary>
    /// During last update, we store the position and velocity of the object
    /// </summary>
    protected override void Update()
    {
        ComputeVelocity();
    }

    /// <summary>
    /// On Late Update we store our position
    /// </summary>
    protected void LateUpdate()
    {
        _positionLastFrame = transform.position;
    }

    /// <summary>
    /// Computes the velocity based on the object's last position
    /// </summary>
    protected override void ComputeVelocity()
    {
        if (Time.deltaTime != 0f)
        {
            _velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

            if (Vector3.Distance(_lastDamagePosition, transform.position) > 0.5f)
            {
                _lastDamagePosition = transform.position;
            }

            _lastPosition = transform.position;
        }
    }

    /// <summary>
    /// Determine the damage direction to pass to the Health Damage method
    /// </summary>
    protected override void DetermineDamageDirection()
    {
        switch (DamageDirectionMode)
        {
            case DamageDirections.BasedOnOwnerPosition:
                if (Owner == null)
                {
                    Owner = gameObject;
                }
                if (_twoD)
                {
                    _damageDirection = _collidingHealth.transform.position - Owner.transform.position;
                    _damageDirection.z = 0;
                }
                else
                {
                    _damageDirection = _collidingHealth.transform.position - Owner.transform.position;
                }
                break;
            case DamageDirections.BasedOnVelocity:
                _damageDirection = transform.position - _lastDamagePosition;
                break;
            case DamageDirections.BasedOnScriptDirection:
                _damageDirection = _damageScriptDirection;
                break;
        }

        _damageDirection = _damageDirection.normalized;
    }

    #endregion

    #region CollisionDetection

    /// <summary>
    /// When a collision with the player is triggered, we give damage to the player and knock it back
    /// </summary>
    /// <param name="collider">what's colliding with the object.</param>
    public override void OnTriggerStay2D(Collider2D collider)
    {
        if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger enter 2D, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>S
    public override void OnTriggerEnter2D(Collider2D collider)
    {
        if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger stay, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>
    public override void OnTriggerStay(Collider collider)
    {
        if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay)) return;
        Colliding(collider.gameObject);
    }

    /// <summary>
    /// On trigger enter, we call our colliding endpoint
    /// </summary>
    /// <param name="collider"></param>
    public override void OnTriggerEnter(Collider collider)
    {
        if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter)) return;
        Colliding(collider.gameObject);
    }

    #endregion


    //ダメージ属性設定
    public enum DamageAttribute
    {
        NONE = 0,
        STUN,
        SLOW,
        PLUS_DAMAGE,
    }
    //複数の時は対応する
    protected DamageAttribute damageAttribute;
    protected float damageAttributeValue;
    public void SetDamageAttribute(DamageAttribute attribute,float value )
    {
        damageAttribute = attribute;
        damageAttributeValue = value;
    }
    /// <summary>
    /// When colliding, we apply the appropriate damage
    /// </summary>
    /// <param name="collider"></param>
    protected override void Colliding(GameObject collider)
    {
        if (!EvaluateAvailability(collider))
        {
            return;
        }

        // cache reset 
        _colliderTopDownController = null;
        _colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

        // if what we're colliding with is damageable
        if (_colliderHealth != null)
        {
            //先にヒット時の処理をするようにする　ダメージ可能かの判定をしたいため
            HitDamageableEvent?.Invoke(_colliderHealth);
            if (_colliderHealth.CurrentHealth > 0)
            {
                OnCollideWithDamageable(_colliderHealth);
            }
        }
        else // if what we're colliding with can't be damaged
        {
            OnCollideWithNonDamageable();
            HitNonDamageableEvent?.Invoke(collider);
        }

        OnAnyCollision(collider);
        HitAnythingEvent?.Invoke(collider);
        HitAnythingFeedback?.PlayFeedbacks(transform.position);
    }

    /// <summary>
    /// Checks whether or not damage should be applied this frame
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    protected override bool EvaluateAvailability(GameObject collider)
    {
        // if we're inactive, we do nothing
        if (!isActiveAndEnabled) { return false; }

        // if the object we're colliding with is part of our ignore list, we do nothing and exit
        if (_ignoredGameObjects.Contains(collider)) { return false; }

        // if what we're colliding with isn't part of the target layers, we do nothing and exit
        if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask)) { return false; }

        // if we're on our first frame, we don't apply damage
        if (Time.time == 0f) { return false; }

        return true;
    }

    /// <summary>
    /// Describes what happens when colliding with a damageable object
    /// </summary>
    /// <param name="health">Health.</param>
    protected override void OnCollideWithDamageable(Health health)
    {
        _collidingHealth = health;
        Guid guid = gameObject.GetComponent<SurviveProjectile>().GUID;
        var shealth = _colliderHealth as SurviveHealth;

        if (shealth != null ? shealth.CanTakeDamageThisFrame(guid): health.CanTakeDamageThisFrame())
        {
            // if what we're colliding with is a TopDownController, we apply a knockback force
            _colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();

            HitDamageableFeedback?.PlayFeedbacks(this.transform.position);

            // we apply the damage to the thing we've collided with
            float randomDamage =
                UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));

            var chara = Owner.GetComponent<SurvivePlayer>();

            bool isCritical = false;
            if (chara != null)
            {
                isCritical =UnityEngine.Random.Range(0f, 100f) < chara.battleParameter.CRITICAL ? true : false;

            }
            ApplyKnockback(randomDamage, TypedDamages);

            DetermineDamageDirection();
            if (shealth == null)
            {
                if (RepeatDamageOverTime)
                {
                    _colliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
                        InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
                        DamageOverTimeInterruptible, RepeatedDamageType);
                }
                else
                {
                    if(gameObject.GetComponent<SurviveProjectile>()!=null && _colliderHealth is SurviveHealth)
                    {
                        (_colliderHealth as SurviveHealth).Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
                      _damageDirection, guid, TypedDamages);
                    }
                    else
                    {
                        _colliderHealth .Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
                        _damageDirection, TypedDamages);
                    }
                   
                }
            }
            else
            {
                if (RepeatDamageOverTime)
                {
                    shealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
                        InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
                        DamageOverTimeInterruptible, RepeatedDamageType);
                }
                else
                {
                    //実際に入るはずのダメージ処理はここ




                    shealth.DamageWithCritical(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
                        _damageDirection, guid, TypedDamages, isCritical, gameObject.GetComponent<SurviveProjectile>().GetIsMain());
                    if (shealth.CurrentHealth > 0)
                    {
                        switch (damageAttribute)
                        {
                            case DamageAttribute.NONE:
                                break;
                            case DamageAttribute.STUN:
                                shealth.ReflectDamageAttribute(damageAttribute, damageAttributeValue);
                                break;
                            case DamageAttribute.SLOW:
                                shealth.ReflectDamageAttribute(damageAttribute, damageAttributeValue);
                                break;
                            case DamageAttribute.PLUS_DAMAGE:
                                shealth.ReflectDamageAttribute(damageAttribute, damageAttributeValue);
                                break;
                        }
                    }
                }
            }
        }

        // we apply self damage
        if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
        {
            SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
        }
    }

    #region Knockback

    /// <summary>
    /// Applies knockback if needed
    /// </summary>
    protected override void ApplyKnockback(float damage, List<TypedDamage> typedDamages)
    {
        if (ShouldApplyKnockback(damage, typedDamages))
        {
            _knockbackForce = DamageCausedKnockbackForce * _colliderHealth.KnockbackForceMultiplier;

            if (_twoD) // if we're in 2D
            {
                ApplyKnockback2D();
            }
            else // if we're in 3D
            {
                ApplyKnockback3D();
            }

            if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
            {
                _colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
            }
        }
    }

    /// <summary>
    /// Determines whether or not knockback should be applied
    /// </summary>
    /// <returns></returns>
    protected override bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
    {
        if (_colliderHealth.ImmuneToKnockbackIfZeroDamage)
        {
            if (_colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0)
            {
                return false;
            }
        }

        return (_colliderTopDownController != null)
               && (DamageCausedKnockbackForce != Vector3.zero)
               && !_colliderHealth.Invulnerable
               && !_colliderHealth.ImmuneToKnockback;
    }

    /// <summary>
    /// Applies knockback if we're in a 2D context
    /// </summary>
    protected override void ApplyKnockback2D()
    {
        switch (DamageCausedKnockbackDirection)
        {
            case KnockbackDirections.BasedOnSpeed:
                var totalVelocity = _colliderTopDownController.Speed + _velocity;
                _knockbackForce = Vector3.RotateTowards(_knockbackForce,
                    totalVelocity.normalized, 10f, 0f);
                break;
            case KnockbackDirections.BasedOnOwnerPosition:
                if (Owner == null)
                {
                    Owner = gameObject;
                }
                _relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                _knockbackForce = Vector3.RotateTowards(_knockbackForce, _relativePosition.normalized, 10f, 0f);
                break;
            case KnockbackDirections.BasedOnDirection:
                var direction = transform.position - _positionLastFrame;
                _knockbackForce = direction * _knockbackForce.magnitude;
                break;
            case KnockbackDirections.BasedOnScriptDirection:
                _knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
                break;
        }
    }

    /// <summary>
    /// Applies knockback if we're in a 3D context
    /// </summary>
    protected override void ApplyKnockback3D()
    {
        switch (DamageCausedKnockbackDirection)
        {
            case KnockbackDirections.BasedOnSpeed:
                var totalVelocity = _colliderTopDownController.Speed + _velocity;
                _knockbackForce = _knockbackForce * totalVelocity.magnitude;
                break;
            case KnockbackDirections.BasedOnOwnerPosition:
                if (Owner == null)
                {
                    Owner = gameObject;
                }
                _relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
                _knockbackForce.x = _relativePosition.normalized.x * _knockbackForce.x;
                _knockbackForce.z = _relativePosition.normalized.z * _knockbackForce.z;
                break;
            case KnockbackDirections.BasedOnDirection:
                var direction = transform.position - _positionLastFrame;
                _knockbackForce = direction * _knockbackForce.magnitude;
                break;
            case KnockbackDirections.BasedOnScriptDirection:
                _knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
                break;
        }
    }

    #endregion


    /// <summary>
    /// Describes what happens when colliding with a non damageable object
    /// </summary>
    protected override void OnCollideWithNonDamageable()
    {
        float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable;
        if (selfDamage > 0)
        {
            SelfDamage(selfDamage);
        }
        HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
    }

    /// <summary>
    /// Describes what could happens when colliding with anything
    /// </summary>
    protected override void OnAnyCollision(GameObject other)
    {
    }

    /// <summary>
    /// Applies damage to itself
    /// </summary>
    /// <param name="damage">Damage.</param>
    protected override void SelfDamage(float damage)
    {
        if (_health != null)
        {
            _damageDirection = Vector3.up;
            _health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
        }

        // if what we're colliding with is a TopDownController, we apply a knockback force
        if (_topDownController != null)
        {
            Vector2 totalVelocity = _colliderTopDownController.Speed + _velocity;
            Vector2 knockbackForce =
                Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

            if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
            {
                _topDownController.AddForce(knockbackForce);
            }
        }
    }
}
