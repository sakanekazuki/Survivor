using EroSurvivor;

namespace Naninovel
{
	[CommandAlias("loadfinish")]
	public class loadFinish : Command
	{
		public override async UniTask ExecuteAsync(AsyncToken asyncToken = default)
		{
			GalleryManager.Instance.LoadingScreenDisable();
			await UniTask.DelayFrame(0);
		}
	}
}