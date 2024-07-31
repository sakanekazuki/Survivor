using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	// キャラクター使用回数
	public class UseCharacterValue : MonoBehaviour
	{
		// キャラクター使用回数
		public int useValue = 0;

		private void Start()
		{
			var characterId = GetComponent<SurvivePlayer>().battleParameter.BaseData.ID;

			useValue = GalleryManager.GetUseNum(characterId - 1);

			//if (useValue <= 10)
			//{
			//	// 使用回数追加
			//	++useValue;
			//	if (useValue == 1)
			//	{
					GalleryManager.StoryOpen(characterId - 1);
					//GalleryManager.PictureOpen(characterId - 1);
			//	}
			//	else if(useValue == 10)
			//	{
			//		GalleryManager.StoryOpen(characterId + 2);
			//	}
			//}
		}
	}
}