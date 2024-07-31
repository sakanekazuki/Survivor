using Naninovel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using static EroSurvivor.GalleryCanvas;

namespace EroSurvivor
{
	// �M�������[�I���L�����o�X
	public class GallerySelectCanvas : MonoBehaviour
	{
		public enum GalleryType
		{
			item,
			story,
			picture
		}

		// �M�������[�̎��
		GalleryType galleryType = GalleryType.item;

		// �A�C�e���L�����o�X
		[SerializeField]
		GameObject itemGalleryCanvas;
		// �X�g�[���[�L�����o�X
		[SerializeField]
		GameObject storyGalleryCanvas;
		// �s�N�`���L�����o�X
		[SerializeField]
		GameObject pictureGalleryCanvas;

		[SerializeField]
		GameObject pictureCanvas;
		Picture pic;

		// �A�C�R����\������X�v���C�g
		[SerializeField]
		Image spriteIcon;

		// ���O��\������e�L�X�g
		[SerializeField]
		/*TMPro.TextMeshProUGUI*/
		LocalizeStringEvent nameTxt;

		// ��������\������e�L�X�g
		[SerializeField]
		/*TMPro.TextMeshProUGUI*/
		LocalizeStringEvent explanationTxt;

		// �A�C�R���z��
		[SerializeField]
		List<Sprite> iconList = new List<Sprite>();

		// �A�C�e����
		[SerializeField]
		List<string> itemNames = new List<string>();

		// �\�����������
		[SerializeField]
		List<string> explanationList = new List<string>();

		[SerializeField]
		SurviveSetting setting;

		[SerializeField]
		Text storyText;
		[SerializeField]
		List<string> pictureList_ja = new List<string>();
		[SerializeField]
		List<string> pictureList_en = new List<string>();

		[SerializeField]
		Text pictureText;
		[SerializeField]
		List<string> storyList_ja = new List<string>();
		[SerializeField]
		List<string> storyList_en = new List<string>();

		private void Start()
		{
			pic = pictureCanvas.GetComponent<Picture>();
		}

		private void OnEnable()
		{
			GalleryChange((int)GalleryType.item);
			Explanation(0);
			StoryTextChange(0);
			PictureTextChange(0);
		}

		//private async void OnDisable()
		//{
		//	var stateManager = Engine.GetService<IStateManager>();
		//	await stateManager.ResetStateAsync();
		//}

		/// <summary>
		/// �M�������[�ύX
		/// </summary>
		/// <param name="type">�ύX����M�������[�̎��</param>
		public void GalleryChange(int type)
		{
			// ���݂̃M�������[�̎��
			galleryType = (GalleryType)type;
			// �Ή�����L�����o�X��\��
			switch (galleryType)
			{
			case GalleryType.item:
				itemGalleryCanvas.SetActive(true);
				storyGalleryCanvas.SetActive(false);
				pictureGalleryCanvas.SetActive(false);
				break;
			case GalleryType.story:
				itemGalleryCanvas.SetActive(false);
				storyGalleryCanvas.SetActive(true);
				pictureGalleryCanvas.SetActive(false);
				break;
			case GalleryType.picture:
				itemGalleryCanvas.SetActive(false);
				storyGalleryCanvas.SetActive(false);
				pictureGalleryCanvas.SetActive(true);
				break;
			}
		}

		/// <summary>
		/// ����M�������[�̑I��
		/// </summary>
		/// <param name="number">�M�������[�̔ԍ�</param>
		public void GallerySelect(/*int number*/string galleryName)
		{
			//GalleryManager.Instance.GalleryDisplay(number);
			GalleryManager.Instance.GalleryDisplay(galleryName);
		}

		/// <summary>
		/// �������\��
		/// </summary>
		/// <param name="number">�A�C�e���̔ԍ�</param>
		public void Explanation(int number)
		{
			spriteIcon.sprite = iconList[number];
			//nameTxt.text = itemNames[number];
			//explanationTxt.text = explanationList[number];
			nameTxt.StringReference.SetReference("GalleryItemName", number.ToString());
			explanationTxt.StringReference.SetReference("GalleryText", number.ToString());
		}

		/// <summary>
		/// �M�������[�I����ʂ���߂�
		/// </summary>
		public void Return()
		{
			// ���̃I�u�W�F�N�g���\���ɂ���
			this.gameObject.SetActive(false);
		}

		public void PictureDisplay(int number)
		{
			pictureCanvas.SetActive(true);
			pic.PictureDisplay(number);
		}

		public void StoryTextChange(int number)
		{
			if (setting.SurviveSettingData.language == 2)//�p��
			{
				storyText.text = pictureList_en[number];
			}
			else
			{
				storyText.text = pictureList_ja[number];
			}
		}

		public void PictureTextChange(int number)
		{
			if (setting.SurviveSettingData.language == 2)//�p��
			{
				pictureText.text = pictureList_en[number];
			}
			else
			{
				pictureText.text = pictureList_ja[number];
			}
		}
	}
}