using UnityEngine;

namespace Naninovel
{
	[CommandAlias("endadv")]
	public class endadv : Command
	{
		public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
		{
			// Naninovel���͂𖳌��ɂ��܂��B
			var inputManager = Engine.GetService<IInputManager>();
			inputManager.ProcessInput = false;
			//Debug.Log(inputManager.ProcessInput.GetType().Name + " = " + inputManager.ProcessInput);

			// �X�N���v�g�v���[���[���~�B
			var scriptPlayer = Engine.GetService<IScriptPlayer>();
			scriptPlayer.Stop();
			//Debug.Log(scriptPlayer.GetType().Name + " = " + scriptPlayer);

			// �X�e�[�g�����Z�b�g�B
			var stateManager = Engine.GetService<IStateManager>();
			await stateManager.ResetStateAsync();
			//Debug.Log(stateManager.GetType().Name + " = " + stateManager);

			// �J������؂�ւ��B
			var naniCamera = Engine.GetService<ICameraManager>().Camera;
			naniCamera.enabled = false;
			//Debug.Log(naniCamera.GetType().Name + " = " + naniCamera.name);

			//�F�X����
			GameObject AdvRenderTexture = GameObject.Find("AdvRenderTexture");
			//Debug.Log(AdvRenderTexture.gameObject.name + " = " + AdvRenderTexture);
			if (AdvRenderTexture != null) AdvRenderTexture.SetActive(false);

			GameObject AdvCanvas = GameObject.Find("AdvCanvas");
			//Debug.Log(AdvCanvas.gameObject.name + " = " + AdvCanvas);
			if (AdvCanvas != null) AdvCanvas.SetActive(false);

			Engine.Destroy();
			//Debug.LogError("Naninovel�I��");

			EroSurvivor.GalleryManager.Instance.StoryFinish();
		}
	}
}
