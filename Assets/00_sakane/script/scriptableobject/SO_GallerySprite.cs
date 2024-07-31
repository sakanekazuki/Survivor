using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	// ギャラリー画像を保存するルクリプタブルオブジェクト
	[CreateAssetMenu(fileName = "GallerySprite", menuName = "EroSurvivor/GallerySprite")]
	public class SO_GallerySprite : ScriptableObject
	{
		// ギャラリースプライト
		public List<Gallery> gallerys = new List<Gallery>();

		[SerializeField]
		Gallery gallery;
	}
}