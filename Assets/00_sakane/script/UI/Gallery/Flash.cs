using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	// 点滅
	public class Flash : MonoBehaviour
	{
		// 点滅させる画像
		Image flashImage;
		[SerializeField]
		// 点滅速度
		float defaultFlashSpeed;
		// フラッシュ回数
		[SerializeField]
		int maxFlashNumber;
		// フラッシュの加速
		[SerializeField]
		float acceleration;
		[SerializeField]
		float tolerance;

		private void Awake()
		{
			// 画像取得
			flashImage = GetComponent<Image>();
		}

		private void OnEnable()
		{
			flashImage.color = new Color(1, 1, 1, 0);
			StartCoroutine(EFlash());
		}

		IEnumerator EFlash()
		{
			// アルファ値
			var alphaValue = 0f;
			// フラッシュ値
			var flashValue = 0f;
			// 点滅速度
			var flashSpeed = defaultFlashSpeed;
			var flashNumber = 0;

			for (int i = 0; i < maxFlashNumber; ++i)
			{
				while (true)
				{
					yield return null;
					// フラッシュ値変更
					flashValue += flashSpeed;
					// アルファ値変更
					alphaValue = (Mathf.Sin(flashValue) + 1) / 2;
					// アルファ値変更
					flashImage.color = new Color(1, 1, 1, alphaValue);

					if (alphaValue < tolerance)
					{
						++flashNumber;
						break;
					}
				}
			}
			FlashFinishEvent.Trigger();
			gameObject.SetActive(false);
		}
	}
}