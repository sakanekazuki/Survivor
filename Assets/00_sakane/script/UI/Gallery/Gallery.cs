using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	// ギャラリーの状態
	[System.Serializable]
	public class Gallery
	{
		// true = 閲覧可能
		public bool isViewable = false;

		// 画像と動画配列
		public List<Object> gallerys = new List<Object>();

		// 表示時間
		public float displayTime = 0;
	}
}