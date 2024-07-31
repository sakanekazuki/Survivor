using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EroSurvivor
{
	// —§‚¿ŠG
	public class Picture : MonoBehaviour
	{
		// •\Ž¦‚·‚é—§‚¿ŠG
		[SerializeField]
		List<GameObject> pictures = new List<GameObject>();

		private void OnEnable()
		{
			foreach (var v in pictures)
			{
				v.gameObject.SetActive(false);
			}
		}

		private void Update()
		{
			if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
			{
				gameObject.SetActive(false);
			}
		}

		public void PictureDisplay(int number)
		{
			pictures[number].SetActive(true);
		}
	}
}