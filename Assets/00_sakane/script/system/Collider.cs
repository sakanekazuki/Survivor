using UnityEngine;

namespace EroSurvivor
{
	// 衝突判定
	public class Collider : MonoBehaviour
	{
		// コライダーの大きさ
		[SerializeField]
		Vector2 size = Vector2.zero;

		public Vector2 Size
		{
			get
			{
				return size;
			}
		}


	}
}