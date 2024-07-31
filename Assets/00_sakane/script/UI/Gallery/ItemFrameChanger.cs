using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	public class ItemFrameChanger : MonoBehaviour
	{
		[SerializeField]
		Sprite enterSprite;
		[SerializeField]
		Sprite exitSprite;
		Image changeImage;

		private void Awake()
		{
			changeImage = GetComponent<Image>();
		}

		private void OnDisable()
		{
			changeImage.sprite = exitSprite;
		}

		public void PointerEnterEvent()
		{
			changeImage.sprite = enterSprite;
		}

		public void PointerExitEvent()
		{
			changeImage.sprite = exitSprite;
		}
	}
}