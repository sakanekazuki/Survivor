using Cinemachine;
using DG.Tweening;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurviveHealth : Health
{
    [MMInspectorGroup("Survive", true, 3)]

    public DamageFeedback damageFeedback;

    protected Dictionary<string, bool> guidInvulnerables = new Dictionary<string, bool>();

    public int DamageShield = 0;
    public float DamageTaken = 0f;
    public bool Guard60 = false;

    private GameObject GuardEffect;
    #region Initialization

    /// <summary>
    /// On Start, we initialize our health
    /// </summary>
    protected override void Awake()
    {
        Initialization();
        InitializeCurrentHealth();
    }

    /// <summary>
    /// On Start we grab our animator
    /// </summary>
    protected override void Start()
    {
        GrabAnimator();
    }

    /// <summary>
    /// Grabs useful components, enables damage and gets the inital color
    /// </summary>
    public override void Initialization()
    {
        _character = this.gameObject.GetComponentInParent<Character>();

        if (Model != null)
        {
            Model.SetActive(true);
        }

        if (gameObject.GetComponentInParent<Renderer>() != null)
        {
            _renderer = GetComponentInParent<Renderer>();
        }
        if (_character != null)
        {
            _characterMovement = _character.FindAbility<CharacterMovement>();
            if (_character.CharacterModel != null)
            {
                if (_character.CharacterModel.GetComponentInChildren<Renderer>() != null)
                {
                    _renderer = _character.CharacterModel.GetComponentInChildren<Renderer>();
                }
            }
        }
        if (_renderer != null)
        {
            if (UseMaterialPropertyBlocks && (_propertyBlock == null))
            {
                _propertyBlock = new MaterialPropertyBlock();
            }

            if (ResetColorOnRevive)
            {
                if (UseMaterialPropertyBlocks)
                {
                    if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
                    {
                        _hasColorProperty = true;
                        _initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
                    }
                }
                else
                {
                    if (_renderer.material.HasProperty(ColorMaterialPropertyName))
                    {
                        _hasColorProperty = true;
                        _initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
                    }
                }
            }
        }

        _interruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
        _initialLayer = gameObject.layer;

        _autoRespawn = this.gameObject.GetComponentInParent<AutoRespawn>();
        _healthBar = this.gameObject.GetComponentInParent<MMHealthBar>();
        _controller = this.gameObject.GetComponentInParent<TopDownController>();
        _characterController = this.gameObject.GetComponentInParent<CharacterController>();
        _collider2D = this.gameObject.GetComponentInParent<Collider2D>();
        _collider3D = this.gameObject.GetComponentInParent<Collider>();

        DamageMMFeedbacks?.Initialization(this.gameObject);
        DeathMMFeedbacks?.Initialization(this.gameObject);

        StoreInitialPosition();
        _initialized = true;

        DamageEnabled();
    }

    /// <summary>
    /// Grabs the target animator
    /// </summary>
    protected override void GrabAnimator()
    {
        if (TargetAnimator == null)
        {
            BindAnimator();
        }

        if ((TargetAnimator != null) && DisableAnimatorLogs)
        {
            TargetAnimator.logWarnings = false;
        }
    }

    /// <summary>
    /// Finds and binds an animator if possible
    /// </summary>
    protected override void BindAnimator()
    {
        if (_character != null)
        {
            if (_character.CharacterAnimator != null)
            {
                TargetAnimator = _character.CharacterAnimator;
            }
            else
            {
                TargetAnimator = GetComponent<Animator>();
            }
        }
        else
        {
            TargetAnimator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Stores the initial position for further use
    /// </summary>
    public override void StoreInitialPosition()
    {
        _initialPosition = this.transform.position;
    }

    /// <summary>
    /// Initializes health to either initial or current values
    /// </summary>
    public override void InitializeCurrentHealth()
    {
        if (MasterHealth == null)
        {
            SetHealth(InitialHealth);
        }
        else
        {
            CurrentHealth = MasterHealth.CurrentHealth;
        }
    }

    /// <summary>
    /// When the object is enabled (on respawn for example), we restore its initial health levels
    /// </summary>
    protected override void OnEnable()
    {
        if (ResetHealthOnEnable)
        {
            InitializeCurrentHealth();
        }
        if (Model != null)
        {
            Model.SetActive(true);
        }
        DamageEnabled();
    }

    /// <summary>
    /// On Disable, we prevent any delayed destruction from running
    /// </summary>
    protected override void OnDisable()
    {
        CancelInvoke();
        StopAllCoroutines();
    }

    #endregion

    /// <summary>
    /// Returns true if this Health component can be damaged this frame, and false otherwise
    /// </summary>
    /// <returns></returns>
    public override bool CanTakeDamageThisFrame()
    {
        // if the object is invulnerable, we do nothing and exit
        if (Invulnerable || ImmuneToDamage)
        {
            return false;
        }

        if (!this.enabled)
        {
            return false;
        }

        // if we're already below zero, we do nothing and exit
        if ((CurrentHealth <= 0) && (InitialHealth != 0))
        {
            return false;
        }

        return true;
    }
    public bool CanTakeDamageThisFrame(Guid guid)
    {
        string str = guid.ToString();
        bool guidInvulnerable = false;
        if (guidInvulnerables.ContainsKey(str))
        {
            guidInvulnerable = guidInvulnerables[str];
        }

        // if the object is invulnerable, we do nothing and exit
        if (guidInvulnerable/*Invulnerable*/ || ImmuneToDamage)
        {
            return false;
        }

        if (!this.enabled)
        {
            return false;
        }

        // if we're already below zero, we do nothing and exit
        if ((CurrentHealth <= 0) && (InitialHealth != 0))
        {
            return false;
        }

        return true;
    }
    /// <summary>
    /// Called when the object takes damage
    /// </summary>
    /// <param name="damage">The amount of health points that will get lost.</param>
    /// <param name="instigator">The object that caused the damage.</param>
    /// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage - not used anymore, kept to not break retrocompatibility</param>
    /// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
    public override void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
    {
        //ここも判定を追加する
        if (!CanTakeDamageThisFrame())
        {
            return;
        }

        bool damageNot0 = damage > 0;

        damage = ComputeDamageOutput(damage, typedDamages, true);
        damage += Mathf.Floor(damage * DamageTaken);
        damage -= DamageShield;
        if(damageNot0 == true && damage <= 0)
        {
            damage = 1;
        }
        if(plusDamage)
        {
            damage += damage * plusDamageValue;
        }
        if (Guard)
        {
            damage = 0f;
            Guard = false;
            if (GuardEffect != null)
            {
                Destroy(GuardEffect);
            }
        }
        // we decrease the character's health by the damage
        float previousHealth = CurrentHealth;
        if (MasterHealth != null)
        {
            previousHealth = MasterHealth.CurrentHealth;
            MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
        }
        else
        {
            SetHealth(CurrentHealth - damage);
        }

        LastDamage = damage;
        LastDamageDirection = damageDirection;
        if (OnHit != null)
        {
            OnHit();
        }

        // we prevent the character from colliding with Projectiles, Player and Enemies
        if (invincibilityDuration > 0)
        {
            DamageDisabled();
            StartCoroutine(DamageEnabled(invincibilityDuration));
        }

        // we trigger a damage taken event
        MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

        // we update our animator
        if (TargetAnimator != null)
        {
            TargetAnimator.SetTrigger("Damage");
        }

        // we play our feedback
        if (FeedbackIsProportionalToDamage)
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
        }
        else
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
        }

        // we update the health bar
        UpdateHealthBar(true);

        // we process any condition state change
        ComputeCharacterConditionStateChanges(typedDamages);
        ComputeCharacterMovementMultipliers(typedDamages);

        // if health has reached zero we set its health to zero (useful for the healthbar)
        if (MasterHealth != null)
        {
            if (MasterHealth.CurrentHealth <= 0)
            {
                MasterHealth.CurrentHealth = 0;
                MasterHealth.Kill();
            }
        }
        else
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Kill();
            }

        }
    }
    public void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, Guid guid, List<TypedDamage> typedDamages = null)
    {
        //ここも判定を追加する
        if (!CanTakeDamageThisFrame(guid))
        {
            return;
        }

        damage = ComputeDamageOutput(damage, guid, typedDamages, true);
        damage += Mathf.Floor(damage * DamageTaken);
        damage -= DamageShield;
        if(Guard)
        {
            damage = 0f;
            Guard = false;
            if (GuardEffect != null)
            {
                Destroy(GuardEffect);
            }
        }
        // we decrease the character's health by the damage
        float previousHealth = CurrentHealth;
        if (MasterHealth != null)
        {
            previousHealth = MasterHealth.CurrentHealth;
            MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
        }
        else
        {
            SetHealth(CurrentHealth - damage);
        }

        LastDamage = damage;
        LastDamageDirection = damageDirection;
        if (OnHit != null)
        {
            OnHit();
        }

        // we prevent the character from colliding with Projectiles, Player and Enemies
        if (invincibilityDuration > 0)
        {
            DamageDisabled(guid);
            StartCoroutine(DamageEnabled(invincibilityDuration, guid));
        }

        // we trigger a damage taken event
        MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

        // we update our animator
        if (TargetAnimator != null)
        {
            TargetAnimator.SetTrigger("Damage");
        }

        // we play our feedback
        if (FeedbackIsProportionalToDamage)
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
        }
        else
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
        }

        // we update the health bar
        UpdateHealthBar(true);

        // we process any condition state change
        ComputeCharacterConditionStateChanges(typedDamages);
        ComputeCharacterMovementMultipliers(typedDamages);

        // if health has reached zero we set its health to zero (useful for the healthbar)
        if (MasterHealth != null)
        {
            if (MasterHealth.CurrentHealth <= 0)
            {
                MasterHealth.CurrentHealth = 0;
                MasterHealth.Kill();
            }
        }
        else
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Kill();
            }

        }
    }
    public void DamageWithCritical(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, Guid guid, List<TypedDamage> typedDamages = null, bool critical = false, bool isMain = false)
    {
        if (!CanTakeDamageThisFrame(guid))
        {
            return;
        }

        damage = ComputeDamageOutput(damage, guid, typedDamages, true);
        if (critical)
        {
            damage *= 2;

			var brain =Camera.main.GetComponent<CinemachineBrain>();
            if (isMain)
            {
                brain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<MMCinemachineCameraShaker>().ShakeCamera(0.1f, false);
            }
        }
        damage += Mathf.Floor(damage * DamageTaken);


        damage -= DamageShield;

        //敵＋一撃キルアイテム
        var sEnemy = gameObject.MMGetComponentNoAlloc<SurviveEnemy>();
        if (sEnemy != null)
        {
            if ((SurviveLevelManager.GetInstance().Players[0] as SurvivePlayer).KillPower == true)
            {
                damage = CurrentHealth;
            }

        }


        if (Guard)
        {
            damage = 0f;
            Guard = false;
            if (GuardEffect != null)
            {
                Destroy(GuardEffect);
            }
        }
        // we decrease the character's health by the damage
        float previousHealth = CurrentHealth;
        if (MasterHealth != null)
        {
            previousHealth = MasterHealth.CurrentHealth;
            MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
        }
        else
        {
            SetHealth(CurrentHealth - damage);
        }

        LastDamage = damage;
        LastDamageDirection = damageDirection;
        if (OnHit != null)
        {
            OnHit();
        }

        // we prevent the character from colliding with Projectiles, Player and Enemies
        if (invincibilityDuration > 0)
        {
            DamageDisabled(guid);
            StartCoroutine(DamageEnabled(invincibilityDuration, guid));
        }

        // we trigger a damage taken event
        MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

        // we update our animator
        if (TargetAnimator != null)
        {
            TargetAnimator.SetTrigger("Damage");
        }

        // we play our feedback
        if (FeedbackIsProportionalToDamage)
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
        }
        else
        {
            DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
        }
        if (damage > 0)
        {
            damageFeedback?.PlayDamageFeedback(this.transform.position, damage, critical);
        }
        // we update the health bar
        UpdateHealthBar(true);

        // we process any condition state change
        ComputeCharacterConditionStateChanges(typedDamages);
        ComputeCharacterMovementMultipliers(typedDamages);

        // if health has reached zero we set its health to zero (useful for the healthbar)
        if (MasterHealth != null)
        {
            if (MasterHealth.CurrentHealth <= 0)
            {
                MasterHealth.CurrentHealth = 0;
                MasterHealth.Kill();
            }
        }
        else
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Kill();
            }

        }
    }
    /// <summary>
    /// Interrupts all damage over time, regardless of type
    /// </summary>
    public override void InterruptAllDamageOverTime()
    {
        foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
        {
            StopCoroutine(coroutine.DamageOverTimeCoroutine);
        }
    }

    /// <summary>
    /// Interrupts all damage over time of the specified type
    /// </summary>
    /// <param name="damageType"></param>
    public override void InterruptAllDamageOverTimeOfType(DamageType damageType)
    {
        foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
        {
            if (coroutine.DamageOverTimeType == damageType)
            {
                StopCoroutine(coroutine.DamageOverTimeCoroutine);
            }
        }
        TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
    }

    /// <summary>
    /// Applies damage over time, for the specified amount of repeats (which includes the first application of damage, makes it easier to do quick maths in the inspector, and at the specified interval).
    /// Optionally you can decide that your damage is interruptible, in which case, calling InterruptAllDamageOverTime() will stop these from being applied, useful to cure poison for example.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="instigator"></param>
    /// <param name="flickerDuration"></param>
    /// <param name="invincibilityDuration"></param>
    /// <param name="damageDirection"></param>
    /// <param name="typedDamages"></param>
    /// <param name="amountOfRepeats"></param>
    /// <param name="durationBetweenRepeats"></param>
    /// <param name="interruptible"></param>
    public override void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
        float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
        int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
    {
        if (ComputeDamageOutput(damage, typedDamages, false) == 0)
        {
            return;
        }

        InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
        damageOverTime.DamageOverTimeType = damageType;
        damageOverTime.DamageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damage, instigator, flickerDuration,
            invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats,
            interruptible));

        if (interruptible)
        {
            _interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
        }
    }

    /// <summary>
    /// A coroutine used to apply damage over time
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="instigator"></param>
    /// <param name="flickerDuration"></param>
    /// <param name="invincibilityDuration"></param>
    /// <param name="damageDirection"></param>
    /// <param name="typedDamages"></param>
    /// <param name="amountOfRepeats"></param>
    /// <param name="durationBetweenRepeats"></param>
    /// <param name="interruptible"></param>
    /// <param name="damageType"></param>
    /// <returns></returns>
    protected override IEnumerator DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
        float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
        int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
    {
        for (int i = 0; i < amountOfRepeats; i++)
        {
            Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
            yield return MMCoroutine.WaitFor(durationBetweenRepeats);
        }
    }

    /// <summary>
    /// Returns the damage this health should take after processing potential resistances
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    public override float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
    {
        if (Invulnerable || ImmuneToDamage)
        {
            return 0;
        }

        float totalDamage = 0f;
        // we process our damage through our potential resistances
        if (TargetDamageResistanceProcessor != null)
        {
            if (TargetDamageResistanceProcessor.isActiveAndEnabled)
            {
                totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);
            }
        }
        else
        {
            totalDamage = damage;
            if (typedDamages != null)
            {
                foreach (TypedDamage typedDamage in typedDamages)
                {
                    totalDamage += typedDamage.DamageCaused;
                }
            }
        }
        return totalDamage;
    }
    public float ComputeDamageOutput(float damage, Guid guid, List<TypedDamage> typedDamages = null, bool damageApplied = false)
    {
        string str = guid.ToString();
        bool guidInvulnerable = false;
        if (guidInvulnerables.ContainsKey(str))
        {
            guidInvulnerable = guidInvulnerables[str];
        }
        if (guidInvulnerable/*Invulnerable*/ || ImmuneToDamage)
        {
            return 0;
        }

        float totalDamage = 0f;
        // we process our damage through our potential resistances
        if (TargetDamageResistanceProcessor != null)
        {
            if (TargetDamageResistanceProcessor.isActiveAndEnabled)
            {
                totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);
            }
        }
        else
        {
            totalDamage = damage;
            if (typedDamages != null)
            {
                foreach (TypedDamage typedDamage in typedDamages)
                {
                    totalDamage += typedDamage.DamageCaused;
                }
            }
        }
        return totalDamage;
    }

    /// <summary>
    /// Goes through resistances and applies condition state changes if needed
    /// </summary>
    /// <param name="typedDamages"></param>
    protected override void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
    {
        if ((typedDamages == null) || (_character == null))
        {
            return;
        }

        foreach (TypedDamage typedDamage in typedDamages)
        {
            if (typedDamage.ForceCharacterCondition)
            {
                if (TargetDamageResistanceProcessor != null)
                {
                    if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                    {
                        bool checkResistance =
                            TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
                        if (checkResistance)
                        {
                            continue;
                        }
                    }
                }
                _character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);
            }
        }

    }

    /// <summary>
    /// Goes through the resistance list and applies movement multipliers if needed
    /// </summary>
    /// <param name="typedDamages"></param>
    protected override void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
    {
        if ((typedDamages == null) || (_character == null))
        {
            return;
        }

        foreach (TypedDamage typedDamage in typedDamages)
        {
            if (typedDamage.ApplyMovementMultiplier)
            {
                if (TargetDamageResistanceProcessor != null)
                {
                    if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                    {
                        bool checkResistance =
                            TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
                        if (checkResistance)
                        {
                            continue;
                        }
                    }
                }

                _characterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier,
                    typedDamage.MovementMultiplierDuration);
            }
        }

    }


    public void ReflectDamageAttribute(SurviveDamageOnTouch.DamageAttribute damageAttribute, float value)
    {
        switch (damageAttribute)
        {
            case SurviveDamageOnTouch.DamageAttribute.NONE:
                break;
            case SurviveDamageOnTouch.DamageAttribute.STUN:
                if(_character is SurviveEnemy)
                {
                    if((_character as SurviveEnemy).enemyData.PATTERN == 3)
                    {
                        return;
                    }
                }
                _characterMovement?.ApplyMovementMultiplier(0f, value);
                break;
            case SurviveDamageOnTouch.DamageAttribute.SLOW:
                if (_character is SurviveEnemy)
                {
                    if ((_character as SurviveEnemy).enemyData.PATTERN == 3)
                    {
                        return;
                    }
                }
                _characterMovement?.ApplyMovementMultiplier(0.7f, value);
                break;
            case SurviveDamageOnTouch.DamageAttribute.PLUS_DAMAGE:
                plusDamageMaxTime = 5f;
                plusDamageValue = value;
                plusDamage = true;
                plusDamageTime = 0f;
                break;
        }


    }
    /// <summary>
    /// Kills the character, instantiates death effects, handles points, etc
    /// </summary>
    public override void Kill()
    {
        if (ImmuneToDamage)
        {
            return;
        }

        if (_character != null)
        {
            // we set its dead state to true
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
            _character.Reset();

            if (_character.CharacterType == Character.CharacterTypes.Player)
            {
                TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
            }
        }
        SetHealth(0);

        // we prevent further damage
        DamageDisabled();
        //ダメージ表示消す
        //damageFeedback.DeathFeedback();
        DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

        // Adds points if needed.
        if (PointsWhenDestroyed != 0)
        {
            // we send a new points event for the GameManager to catch (and other classes that may listen to it too)
            TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
        }

        if (TargetAnimator != null)
        {
            TargetAnimator.SetTrigger("Death");
        }
        // we make it ignore the collisions from now on
        if (DisableCollisionsOnDeath)
        {
            if (_collider2D != null)
            {
                _collider2D.enabled = false;
            }
            if (_collider3D != null)
            {
                _collider3D.enabled = false;
            }

            // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
            if (_controller != null)
            {
                _controller.CollisionsOff();
            }

            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = false;
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }
            }
        }

        if (ChangeLayerOnDeath)
        {
            gameObject.layer = LayerOnDeath.LayerIndex;
            if (ChangeLayersRecursivelyOnDeath)
            {
                this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
            }
        }

        OnDeath?.Invoke();
        MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

        if (DisableControllerOnDeath && (_controller != null))
        {
            _controller.enabled = false;
        }

        if (DisableControllerOnDeath && (_characterController != null))
        {
            _characterController.enabled = false;
        }

        if (DisableModelOnDeath && (Model != null))
        {
            Model.SetActive(false);
        }

        if (DelayBeforeDestruction > 0f)
        {
            bool useDOTween = false;
            if(_character is SurviveEnemy)
            {
                if((_character as SurviveEnemy).enemyData.ID == SurviveLevelManager.GetInstance().FinalBossID)
                {
                    useDOTween = true;
                }
            }
            if (useDOTween == true)
            {
                DOVirtual.DelayedCall(DelayBeforeDestruction, () => DestroyObject());
            }
            else
            {
                Invoke("DestroyObject", DelayBeforeDestruction);
            }
        }
        else
        {
            // finally we destroy the object
            DestroyObject();
        }
    }

    /// <summary>
    /// Revive this object.
    /// </summary>
    public override void Revive()
    {
        if (!_initialized)
        {
            return;
        }

        if (_collider2D != null)
        {
            _collider2D.enabled = true;
        }
        if (_collider3D != null)
        {
            _collider3D.enabled = true;
        }
        if (DisableChildCollisionsOnDeath)
        {
            foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
            {
                collider.enabled = true;
            }
            foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }
        }
        if (ChangeLayerOnDeath)
        {
            gameObject.layer = _initialLayer;
            if (ChangeLayersRecursivelyOnDeath)
            {
                this.transform.ChangeLayersRecursively(_initialLayer);
            }
        }
        if (_characterController != null)
        {
            _characterController.enabled = true;
        }
        if (_controller != null)
        {
            _controller.enabled = true;
            _controller.CollisionsOn();
            _controller.Reset();
        }
        if (_character != null)
        {
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
        }
        if (ResetColorOnRevive && (_renderer != null))
        {
            if (UseMaterialPropertyBlocks)
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
            else
            {
                _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
            }
        }

        if (RespawnAtInitialLocation)
        {
            transform.position = _initialPosition;
        }
        if (_healthBar != null)
        {
            _healthBar.Initialization();
        }

        Initialization();
        InitializeCurrentHealth();
        OnRevive?.Invoke();
        MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
    }

    /// <summary>
    /// Destroys the object, or tries to, depending on the character's settings
    /// </summary>
    protected override void DestroyObject()
    {
        if (_autoRespawn == null)
        {
            if (DestroyOnDeath)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            _autoRespawn.Kill();
        }
    }

    #region HealthManipulationAPIs


    /// <summary>
    /// Sets the current health to the specified new value, and updates the health bar
    /// </summary>
    /// <param name="newValue"></param>
    public override void SetHealth(float newValue)
    {
        CurrentHealth = newValue;
        UpdateHealthBar(false);
        HealthChangeEvent.Trigger(this, newValue);
    }

    /// <summary>
    /// Called when the character gets health (from a stimpack for example)
    /// </summary>
    /// <param name="health">The health the character gets.</param>
    /// <param name="instigator">The thing that gives the character health.</param>
    public override void ReceiveHealth(float health, GameObject instigator)
    {
        // this function adds health to the character's Health and prevents it to go above MaxHealth.
        if (MasterHealth != null)
        {
            MasterHealth.SetHealth(Mathf.Floor(Mathf.Min(CurrentHealth + health, MaximumHealth)));
        }
        else
        {
            SetHealth(Mathf.Floor(Mathf.Min(CurrentHealth + health, MaximumHealth)));
        }
        UpdateHealthBar(true);
    }

    /// <summary>
    /// Resets the character's health to its max value
    /// </summary>
    public override void ResetHealthToMaxHealth()
    {
        SetHealth(MaximumHealth);
    }

    /// <summary>
    /// Forces a refresh of the character's health bar
    /// </summary>
    public override void UpdateHealthBar(bool show)
    {
        //敵だったらHP表示しない
#if !DEBUG
        var enemy = gameObject.MMGetComponentNoAlloc<SurviveEnemy>();
        if (enemy != null)
        {
            return;
        }
#endif
        if (_healthBar != null)
        {
            _healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
        }

        if (MasterHealth == null)
        {
            if (_character != null)
            {
                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    // We update the health bar
                    if (GUIManager.HasInstance)
                    {
                        GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                    }
                }
            }
        }
    }

#endregion

#region DamageDisablingAPIs

    /// <summary>
    /// Prevents the character from taking any damage
    /// </summary>
    public override void DamageDisabled()
    {
        Invulnerable = true;
    }
    public  void DamageDisabled(Guid guid)
    {
        string str = guid.ToString();
        if(guidInvulnerables.ContainsKey(str))
        {
            guidInvulnerables[str] = true;
        }
        else
        {
            guidInvulnerables.Add(str, true);
        }
        //Invulnerable = true;
    }
    /// <summary>
    /// Allows the character to take damage
    /// </summary>
    public override void DamageEnabled()
    {
        Invulnerable = false;
        guidInvulnerables.Clear();
    }

    /// <summary>
    /// makes the character able to take damage again after the specified delay
    /// </summary>
    /// <returns>The layer collision.</returns>
    public override IEnumerator DamageEnabled(float delay)
    {
        yield return new WaitForSeconds(delay);
        Invulnerable = false;
    }
    public IEnumerator DamageEnabled(float delay, Guid guid)
    {
        yield return new WaitForSeconds(delay);
        //Invulnerable = false;

        string str = guid.ToString();
        if (guidInvulnerables.ContainsKey(str))
        {
            guidInvulnerables[str] = false;
        }
        else
        {
            guidInvulnerables.Add(str, false);
        }
    }



    public virtual void ForceKill()
    {
        

        if (_character != null)
        {
            // we set its dead state to true
            //_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
            _character.Reset();

            //if (_character.CharacterType == Character.CharacterTypes.Player)
            //{
            //    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
            //}
        }
        //SetHealth(0);

        // we prevent further damage
        DamageDisabled();
        //ダメージ表示消す
        //damageFeedback.DeathFeedback();
        //DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

       

        //if (TargetAnimator != null)
        //{
        //    TargetAnimator.SetTrigger("Death");
        //}
        // we make it ignore the collisions from now on
        if (DisableCollisionsOnDeath)
        {
            if (_collider2D != null)
            {
                _collider2D.enabled = false;
            }
            if (_collider3D != null)
            {
                _collider3D.enabled = false;
            }

            // if we have a controller, removes collisions, restores parameters for a potential respawn, and applies a death force
            if (_controller != null)
            {
                _controller.CollisionsOff();
            }

            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = false;
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }
            }
        }

        if (ChangeLayerOnDeath)
        {
            gameObject.layer = LayerOnDeath.LayerIndex;
            if (ChangeLayersRecursivelyOnDeath)
            {
                this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
            }
        }

        //OnDeath?.Invoke();
        //MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

        if (DisableControllerOnDeath && (_controller != null))
        {
            _controller.enabled = false;
        }

        if (DisableControllerOnDeath && (_characterController != null))
        {
            _characterController.enabled = false;
        }

        if (DisableModelOnDeath && (Model != null))
        {
            Model.SetActive(false);
        }

        //if (DelayBeforeDestruction > 0f)
        //{
        //    Invoke("DestroyObject", DelayBeforeDestruction);
        //}
        //else
        {
            // finally we destroy the object
            DestroyObject();
        }
    }
    //最初から発動するようにするため
    private float currentGuardTime = 60f;
    private bool Guard = false;
    public void GuardUpdate()
    {
        if (!Guard)
        {
            currentGuardTime += Time.deltaTime;
            if (Guard60)
            {
                if(currentGuardTime >= 60f)
                {
                    Guard = true;
                    currentGuardTime = 0f;
                    GuardEffect = GameObject.Instantiate(SurviveGameManager.GetInstance().ShieldEffect, gameObject.transform);
                }
            }
        }
    }


    bool plusDamage = false;
    float plusDamageValue = 0f;
    float plusDamageTime = 0f;
    float plusDamageMaxTime = 0f;

    public void FixedUpdate()
    {
        if (plusDamage)
        {
            plusDamageTime += Time.deltaTime;
            if(plusDamageTime >= plusDamageMaxTime)
            {
                plusDamage = false;
            }
        }
    }


#endregion
}
