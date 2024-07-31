using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
    public class HCGOpenCommand : EroSuvivorCommandBase
    {
		protected override void Command()
		{
			GalleryManager.state.charaUseNum = new List<int> { 10, 10, 10 };
			GalleryManager.Instance.OpenCheck();
		}
	}
}