using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	public class NowCharacterSelect : MonoBehaviour
	{
		[SerializeField]
		Color nonSelectColor = Color.white;

		Image image;

		private void Start()
		{
			image = GetComponent<Image>();
			image.color = nonSelectColor;
		}

		public void PointerEnterEvent()
		{
			image.color = Color.white;
		}

		public void PointerExitEvent()
		{
			image.color = nonSelectColor;
		}
	}
}