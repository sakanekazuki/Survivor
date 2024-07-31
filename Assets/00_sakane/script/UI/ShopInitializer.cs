using UnityEngine;

namespace EroSurvivor
{
	public class ShopInitializer : MonoBehaviour
	{
		Vector3 initPos = Vector3.zero;

		private void Awake()
		{
			initPos = transform.position;
		}

		private void OnEnable()
		{
			transform.position = initPos;
		}
	}
}