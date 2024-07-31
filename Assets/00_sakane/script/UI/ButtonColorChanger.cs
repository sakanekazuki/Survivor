using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	public class ButtonColorChanger : MonoBehaviour
	{
		[SerializeField]
		Color enterColor;
		[SerializeField]
		Color exitColor;

		Image changeImage;

		private void Awake()
		{
			changeImage = GetComponent<Image>();
		}

		public void PointerEnter()
		{
			changeImage.color = enterColor;
		}

		public void PointerExit()
		{
			changeImage.color = exitColor;
		}
	}
}