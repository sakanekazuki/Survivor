using UnityEngine;

namespace EroSurvivor
{
	public class LoadImageRotator : MonoBehaviour
	{
		[SerializeField]
		Vector3 rotSpeed = Vector3.one;

		private void FixedUpdate()
		{
			transform.Rotate(rotSpeed);
		}
	}
}