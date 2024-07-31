using Naninovel;
using UnityEngine;

// �e�X�g
public class NaninovelTest : MonoBehaviour
{
	[SerializeField]
	bool isNaninovelStart = false;

	//ScriptPlayer player = null;

	private void Start()
	{
		if (isNaninovelStart)
		{
			NaniInitial();
		}
	}

	/// <summary>
	/// naninovel�J�n
	/// </summary>
	[ContextMenu("initial")]
	public async void NaniInitial()
	{
		await Naninovel.RuntimeInitializer.InitializeAsync();
		var player = Engine.GetService<IScriptPlayer>();
		await player.PreloadAndPlayAsync("meifan_002");
	}

	/// <summary>
	/// naninovel�I��
	/// </summary>
	[ContextMenu("final")]
	public void NaniFinal()
	{
		Naninovel.Engine.Destroy();
	}

	/// <summary>
	/// CG�\��
	/// </summary>
	[ContextMenu("CGGalleryShow")]
	public void CGGalleryShow()
	{
		Naninovel.Engine.GetService<Naninovel.IUIManager>().GetUI<Naninovel.UI.ICGGalleryUI>().Show();
	}
}