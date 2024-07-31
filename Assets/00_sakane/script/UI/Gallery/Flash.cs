using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	// �_��
	public class Flash : MonoBehaviour
	{
		// �_�ł�����摜
		Image flashImage;
		[SerializeField]
		// �_�ő��x
		float defaultFlashSpeed;
		// �t���b�V����
		[SerializeField]
		int maxFlashNumber;
		// �t���b�V���̉���
		[SerializeField]
		float acceleration;
		[SerializeField]
		float tolerance;

		private void Awake()
		{
			// �摜�擾
			flashImage = GetComponent<Image>();
		}

		private void OnEnable()
		{
			flashImage.color = new Color(1, 1, 1, 0);
			StartCoroutine(EFlash());
		}

		IEnumerator EFlash()
		{
			// �A���t�@�l
			var alphaValue = 0f;
			// �t���b�V���l
			var flashValue = 0f;
			// �_�ő��x
			var flashSpeed = defaultFlashSpeed;
			var flashNumber = 0;

			for (int i = 0; i < maxFlashNumber; ++i)
			{
				while (true)
				{
					yield return null;
					// �t���b�V���l�ύX
					flashValue += flashSpeed;
					// �A���t�@�l�ύX
					alphaValue = (Mathf.Sin(flashValue) + 1) / 2;
					// �A���t�@�l�ύX
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