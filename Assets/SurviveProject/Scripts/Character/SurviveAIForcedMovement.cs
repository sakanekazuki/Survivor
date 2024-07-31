using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This Action will make the Character patrol along the defined path (see the MMPath inspector for that) until it hits a wall or a hole while following a path.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/SurviveAIForcedMovement")]
	//[RequireComponent(typeof(MMPath))]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	//[RequireComponent(typeof(CharacterMovement))]
	public class SurviveAIForcedMovement : AIAction
	{
		[Header("Obstacle Detection")]
		/// If set to true, the agent will change direction when hitting an obstacle
		[Tooltip("If set to true, the agent will change direction when hitting an obstacle")]
		public bool ChangeDirectionOnObstacle = true;
		/// the layermask to look for obstacles on
		[Tooltip("the layermask to look for obstacles on")]
		public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("the frequency (in seconds) at which to check for obstacles")]
		public float ObstaclesCheckFrequency = 1f;
		/// the coordinates of the last patrol point
		public Vector3 LastReachedPatrolPoint { get; set; }

		// private stuff
		protected TopDownController _controller;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected CharacterMovement _characterMovement;
		protected Health _health;
		protected Vector2 _direction;
		protected Vector2 _startPosition;
		protected Vector3 _initialScale;
		protected MMPath _mmPath;
		protected float _lastObstacleDetectionTimestamp = 0f;

		protected int _currentIndex = 0;
		protected int _indexLastFrame = -1;
		protected float _waitingDelay = 0f;


		public float BackTime = 0f;
		private float _backTimeCount = 0f;


        public float ToTargetTime = 0f;
        private float _toTargetTimeCount = 0f;
        public float StopTime = 0f;
        private float _stopTimeCount = 0f;


		public float SetToTargetTime = 0f;
        private float _toSetToTargetTimeCount = 0f;

        public class ForcedMovementData
        {
			public Vector3 direction;
			public MOVE_PATTERN movePattern;
			public float duration;
			public float speed;
		}
		ForcedMovementData forcedMovement = null;
		public enum MOVE_PATTERN
		{
			//直線
			STRAIGHT,
			//直線ループ
			STRAIGHT_LOOP,
			//グラフにそう
        }

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		protected virtual void InitializePatrol()
		{
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_health = _character?.CharacterHealth;
			_mmPath = this.gameObject.GetComponentInParent<MMPath>();
			// initialize the start position
			_startPosition = transform.position;
			// initialize the direction
			_direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			_initialScale = transform.localScale;
			_currentIndex = 0;
			_indexLastFrame = -1;
			_waitingDelay = 0;
			_initialized = true;
		}
		

		public void SetData(ForcedMovementData forcedMovementData )
        {

			InitializePatrol();

			forcedMovement = forcedMovementData;
			_characterMovement.WalkSpeed = forcedMovement.speed;


		}
		public ForcedMovementData GetData()
		{


			return forcedMovement;


		}
		/// <summary>
		/// On PerformAction we patrol
		/// </summary>
		public override void PerformAction()
		{
			if (forcedMovement != null)
			{
				Move();
			}
		}

		/// <summary>
		/// This method initiates all the required checks and moves the character
		/// </summary>
		protected virtual void Move()
		{
			_waitingDelay -= Time.deltaTime;

			if (_character == null)
			{
				return;
			}
			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
				|| (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}

			if (_waitingDelay > 0)
			{
				_characterMovement.SetHorizontalMovement(0f);
				_characterMovement.SetVerticalMovement(0f);
				return;
			}

			if(StopTime > 0f)
			{
                _stopTimeCount += Time.deltaTime;
                if (_stopTimeCount < StopTime)
                {
                    _characterMovement.SetHorizontalMovement(0f);
                    _characterMovement.SetVerticalMovement(0f);
                    return;
                }
            }

			// moves the agent in its current direction
			//CheckForObstacles();

			//_currentIndex = _mmPath.CurrentIndex();
			//if (_currentIndex != _indexLastFrame)
			//{
			//	LastReachedPatrolPoint = _mmPath.CurrentPoint();
			//	_waitingDelay = _mmPath.PathElements[_currentIndex].Delay;
			//}

			//_direction = _mmPath.CurrentPoint() - this.transform.position;
			//_direction = _direction.normalized;

			//_direction =  Quaternion.LookRotation(forcedMovement.direction, Vector3.up) * Vector3.right;
			
			if(SetToTargetTime > 0f)
			{
                _toSetToTargetTimeCount += Time.deltaTime;
                if (_toSetToTargetTimeCount >= SetToTargetTime)
                {
                    forcedMovement.direction =(_brain.Target.position - this.transform.position).normalized;
					_toSetToTargetTimeCount = 0f;
                }
            }
			if(ToTargetTime > 0f)
			{
                _toTargetTimeCount += Time.deltaTime;
                if (_toTargetTimeCount >= ToTargetTime)
                {
					ToTargetMove();
					return;
                }
            }
			if (BackTime > 0f)
			{
				_backTimeCount += Time.deltaTime;
				if(_backTimeCount < BackTime)
				{
                    _characterMovement.SetHorizontalMovement(forcedMovement.direction.x);
                    _characterMovement.SetVerticalMovement(forcedMovement.direction.y);
				}
				else
				{
                    _characterMovement.SetHorizontalMovement(-forcedMovement.direction.x);
                    _characterMovement.SetVerticalMovement(-forcedMovement.direction.y);

                }
                if (_backTimeCount >= BackTime * 2f)
				{
					_backTimeCount = 0f;
                }

            }
			else
			{

				_characterMovement.SetHorizontalMovement(forcedMovement.direction.x);
				_characterMovement.SetVerticalMovement(forcedMovement.direction.y);
			}
			//_indexLastFrame = _currentIndex;
		}


        protected void ToTargetMove()
        {
            if (_brain.Target == null)
            {
                return;
            }

            {
                _direction = (_brain.Target.position - this.transform.position).normalized;
                _characterMovement.SetMovement(_direction);
            }

        }

        /// <summary>
        /// Draws bounds gizmos
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			//if (_mmPath == null)
			//{
			//	return;
			//}
			//Gizmos.color = MMColors.IndianRed;
			//Gizmos.DrawLine(this.transform.position, _mmPath.CurrentPoint());
		}

		/// <summary>
		/// When exiting the state we reset our movement
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}

		/// <summary>
		/// Checks for a wall and changes direction if it meets one
		/// </summary>
		protected virtual void CheckForObstacles()
		{
			if (!ChangeDirectionOnObstacle)
			{
				return;
			}

			if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
			{
				return;
			}

			RaycastHit2D raycast = MMDebug.RayCast(_controller.ColliderCenter, _direction, 1f, ObstaclesLayerMask, MMColors.Gold, true);

			// if the agent is colliding with something, make it turn around
			if (raycast)
			{
				ChangeDirection();
			}

			_lastObstacleDetectionTimestamp = Time.time;
		}

		/// <summary>
		/// Changes the current movement direction
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = -_direction;
			_mmPath.ChangeDirection();
		}

		/// <summary>
		/// When reviving we make sure our directions are properly setup
		/// </summary>
		protected virtual void OnRevive()
		{
			if (!_initialized)
			{
				return;
			}

			if (_orientation2D != null)
			{
				_direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			}

			InitializePatrol();
		}

		/// <summary>
		/// On enable we start listening for OnRevive events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = this.gameObject.GetComponent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRevive;
			}
		}

		/// <summary>
		/// On disable we stop listening for OnRevive events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}
	}
}