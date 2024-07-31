using UnityEngine;

namespace Naninovel
{
	[CommandAlias("endadv")]
	public class endadv : Command
	{
		public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
		{
			// Naninovel入力を無効にします。
			var inputManager = Engine.GetService<IInputManager>();
			inputManager.ProcessInput = false;
			//Debug.Log(inputManager.ProcessInput.GetType().Name + " = " + inputManager.ProcessInput);

			// スクリプトプレーヤーを停止。
			var scriptPlayer = Engine.GetService<IScriptPlayer>();
			scriptPlayer.Stop();
			//Debug.Log(scriptPlayer.GetType().Name + " = " + scriptPlayer);

			// ステートをリセット。
			var stateManager = Engine.GetService<IStateManager>();
			await stateManager.ResetStateAsync();
			//Debug.Log(stateManager.GetType().Name + " = " + stateManager);

			// カメラを切り替え。
			var naniCamera = Engine.GetService<ICameraManager>().Camera;
			naniCamera.enabled = false;
			//Debug.Log(naniCamera.GetType().Name + " = " + naniCamera.name);

			//色々消す
			GameObject AdvRenderTexture = GameObject.Find("AdvRenderTexture");
			//Debug.Log(AdvRenderTexture.gameObject.name + " = " + AdvRenderTexture);
			if (AdvRenderTexture != null) AdvRenderTexture.SetActive(false);

			GameObject AdvCanvas = GameObject.Find("AdvCanvas");
			//Debug.Log(AdvCanvas.gameObject.name + " = " + AdvCanvas);
			if (AdvCanvas != null) AdvCanvas.SetActive(false);

			Engine.Destroy();
			//Debug.LogError("Naninovel終了");

			EroSurvivor.GalleryManager.Instance.StoryFinish();
		}
	}
}
