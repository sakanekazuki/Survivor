using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// A class to load scenes using a loading screen instead of just the default API
	/// This class used to be known as LoadingSceneManager, and has now been renamed to MMSceneLoadingManager for consistency
	/// </summary>
	public class SurviveSceneLoadingManager : MMSceneLoadingManager
	{


		//こっち使う　なるべくロード画面も機能はそのまま使う感じに
		public static string SurviveLoadingScreenSceneName = "SurviveLoadingScreen";


		/// <summary>
		/// Call this static method to load a scene from anywhere
		/// </summary>
		/// <param name="sceneToLoad">Level name.</param>
		public static void LoadScene(string sceneToLoad)
		{
			_sceneToLoad = sceneToLoad;					
			Application.backgroundLoadingPriority = ThreadPriority.High;
			if (SurviveLoadingScreenSceneName != null)
			{
				LoadingSceneEvent.Trigger(sceneToLoad, LoadingStatus.LoadStarted);
				SceneManager.LoadScene(SurviveLoadingScreenSceneName);
			}
		}

		/// <summary>
		/// Call this static method to load a scene from anywhere
		/// </summary>
		/// <param name="sceneToLoad">Level name.</param>
		public static void LoadScene(string sceneToLoad, string loadingSceneName)
		{
			_sceneToLoad = sceneToLoad;					
			Application.backgroundLoadingPriority = ThreadPriority.High;
			SceneManager.LoadScene(loadingSceneName);
		}

		/// <summary>
		/// On Start(), we start loading the new level asynchronously
		/// </summary>
		protected virtual void Start()
		{
			_tween = new MMTweenType(MMTween.MMTweenCurve.EaseOutCubic);
			_progressBarImage = LoadingProgressBar.GetComponent<Image>();
            
			_loadingTextValue =LoadingText.text;
			if (!string.IsNullOrEmpty(_sceneToLoad))
			{
				StartCoroutine(LoadAsynchronously());
			}        
		}

		/// <summary>
		/// Every frame, we fill the bar smoothly according to loading progress
		/// </summary>
		protected virtual void Update()
		{
			Time.timeScale = 1f;
			_progressBarImage.fillAmount = MMMaths.Approach(_progressBarImage.fillAmount,_fillTarget,Time.deltaTime*ProgressBarSpeed);
		}

		/// <summary>
		/// Loads the scene to load asynchronously.
		/// </summary>
		protected virtual IEnumerator LoadAsynchronously() 
		{
			// we setup our various visual elements
			LoadingSetup();

			// we fade from black
			MMFadeOutEvent.Trigger(StartFadeDuration, _tween);
			yield return new WaitForSeconds(StartFadeDuration);
            
			// we start loading the scene
			_asyncOperation = SceneManager.LoadSceneAsync(_sceneToLoad,LoadSceneMode.Single );
			_asyncOperation.allowSceneActivation = false;

			// while the scene loads, we assign its progress to a target that we'll use to fill the progress bar smoothly
			while (_asyncOperation.progress < 0.9f) 
			{
				_fillTarget = _asyncOperation.progress;
				yield return null;
			}
			// when the load is close to the end (it'll never reach it), we set it to 100%
			_fillTarget = 1f;

			// we wait for the bar to be visually filled to continue
			while (_progressBarImage.fillAmount != _fillTarget)
			{
				yield return null;
			}

			// the load is now complete, we replace the bar with the complete animation
			LoadingComplete();
			yield return new WaitForSeconds(LoadCompleteDelay);

			// we fade to black
			MMFadeInEvent.Trigger(ExitFadeDuration, _tween);
			yield return new WaitForSeconds(ExitFadeDuration);

			// we switch to the new scene
			_asyncOperation.allowSceneActivation = true;
			LoadingSceneEvent.Trigger(_sceneToLoad, LoadingStatus.LoadTransitionComplete);
		}

		/// <summary>
		/// Sets up all visual elements, fades from black at the start
		/// </summary>
		protected virtual void LoadingSetup() 
		{
			LoadingCompleteAnimation.alpha=0;
			_progressBarImage.fillAmount = 0f;
			LoadingText.text = _loadingTextValue;
		}

		/// <summary>
		/// Triggered when the actual loading is done, replaces the progress bar with the complete animation
		/// </summary>
		protected virtual void LoadingComplete() 
		{
			LoadingSceneEvent.Trigger(_sceneToLoad, LoadingStatus.InterpolatedLoadProgressComplete);
			LoadingCompleteAnimation.gameObject.SetActive(true);
			StartCoroutine(MMFade.FadeCanvasGroup(LoadingProgressBar,0.1f,0f));
			StartCoroutine(MMFade.FadeCanvasGroup(LoadingAnimation,0.1f,0f));
			StartCoroutine(MMFade.FadeCanvasGroup(LoadingCompleteAnimation,0.1f,1f));
		}
	}
}