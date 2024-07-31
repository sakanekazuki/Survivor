using Naninovel;
using UnityEngine;

// テスト
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
	/// naninovel開始
	/// </summary>
	[ContextMenu("initial")]
	public async void NaniInitial()
	{
		await Naninovel.RuntimeInitializer.InitializeAsync();
		var player = Engine.GetService<IScriptPlayer>();
		await player.PreloadAndPlayAsync("meifan_002");
	}

	/// <summary>
	/// naninovel終了
	/// </summary>
	[ContextMenu("final")]
	public void NaniFinal()
	{
		Naninovel.Engine.Destroy();
	}

	/// <summary>
	/// CG表示
	/// </summary>
	[ContextMenu("CGGalleryShow")]
	public void CGGalleryShow()
	{
		Naninovel.Engine.GetService<Naninovel.IUIManager>().GetUI<Naninovel.UI.ICGGalleryUI>().Show();
	}
}