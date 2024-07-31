using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// A feedback to bind Unity events to and trigger them when played
    /// </summary>
    [AddComponentMenu("")]
    [FeedbackHelp("DamageFeedback")]
    [FeedbackPath("Events/DamageFeedback")]
    public class DamageFeedback : MMFeedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        /// sets the inspector color for this feedback
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
#endif

        [Header("Events")]
        /// the events to trigger when the feedback is played
        //[Tooltip("the events to trigger when the feedback is played")]
        //public UnityEvent PlayEvents;
        /// the events to trigger when the feedback is stopped
        [Tooltip("the events to trigger when the feedback is stopped")]
        public UnityEvent StopEvents;
        /// the events to trigger when the feedback is initialized
        [Tooltip("the events to trigger when the feedback is initialized")]
        public UnityEvent InitializationEvents;
        /// the events to trigger when the feedback is reset
        [Tooltip("the events to trigger when the feedback is reset")]
        public UnityEvent ResetEvents;


        public DamageUI damageUI;
        private DamageUI currentdamageUI;
        /// <summary>
        /// On init, triggers the init events
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(GameObject owner)
        {
            base.CustomInitialization(owner);
            if(currentdamageUI != null)
            {
                Destroy(currentdamageUI.gameObject);
            }
            if (Active && (InitializationEvents != null))
            {
                InitializationEvents.Invoke();
            }
        }

        /// <summary>
        /// On Play, triggers the play events
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            //currentdamageUI = GameObject.Instantiate(damageUI, Owner.transform);
            //currentdamageUI.Play(feedbacksIntensity);
        }

        public void PlayDamageFeedback(Vector3 position, float feedbacksIntensity = 1.0f, bool critical=false)
        {
            currentdamageUI = GameObject.Instantiate(damageUI);
            currentdamageUI.transform.position = Owner.transform.position;
            currentdamageUI.Play(feedbacksIntensity, critical);
        }

        public void DeathFeedback()
        {
            if (currentdamageUI != null)
            {
                Destroy(currentdamageUI.gameObject);
            }
        }

        /// <summary>
        /// On Stop, triggers the stop events
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized || (StopEvents == null))
            {
                return;
            }
            StopEvents.Invoke();
        }

        /// <summary>
        /// On reset, triggers the reset events
        /// </summary>
        protected override void CustomReset()
        {
            if (!Active || !FeedbackTypeAuthorized || (ResetEvents == null))
            {
                return;
            }
            base.CustomReset();
            ResetEvents.Invoke();
        }
    }
}