using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using Naninovel;

namespace EroSurvivor
{
	public struct GalleryEvent
	{
		public Gallery gallery;
		public GalleryEvent(Gallery cg = null)
		{
			this.gallery = cg;
		}
		static GalleryEvent e;
		public static void Trigger(Gallery cg = null)
		{
			e.gallery = cg;
			MMEventManager.TriggerEvent(e);
		}
	}

	public struct FlashFinishEvent
	{
		public bool isFinish;
		public FlashFinishEvent(bool isFinish)
		{
			this.isFinish = isFinish;
		}
		static FlashFinishEvent e;
		public static void Trigger()
		{
			MMEventManager.TriggerEvent(e);
		}
	}

	// �M�������[�L�����o�X
	public class GalleryCanvas : MonoBehaviour, MMEventListener<GalleryEvent>, MMEventListener<FlashFinishEvent>
	{
		//public enum GalleryType
		//{
		//	item,
		//	story,
		//	picture
		//}

		// �M�������[
		[SerializeField]
		Image galleryImage;

		// �r�f�I
		[SerializeField]
		VideoPlayer videoPlayer;

		// �\������M�������[
		Gallery displayGallery;
		// �M�������[�ԍ�
		int cgNumber = 0;

		// �t���b�V��
		[SerializeField]
		GameObject flashSprite;

		// �\�����郁�b�Z�[�W
		[SerializeField]
		TextMeshProUGUI message;

		// true = ����CG�ɐi�ނ��Ƃ��ł���
		bool canNextCG = true;

		//// �M�������[�̎��
		//GalleryType galleryType = GalleryType.item;

		//// �A�C�e���L�����o�X
		//[SerializeField]
		//GameObject itemGalleryCanvas;
		//// �X�g�[���[�L�����o�X
		//[SerializeField]
		//GameObject storyGalleryCanvas;
		//// �s�N�`���L�����o�X
		//[SerializeField]
		//GameObject pictureGalleryCanvas;

		private void Start()
		{
			//flashSprite.SetActive(false);
		}

		//private void Update()
		//{
		//	if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame)
		//	{
		//		StartCoroutine(NextCG());
		//	}
		//}

		private void OnEnable()
		{
			this.MMEventStartListening<GalleryEvent>();
			this.MMEventStartListening<FlashFinishEvent>();

			
		}

		private void OnDisable()
		{
			this.MMEventStopListening<GalleryEvent>();
			this.MMEventStopListening<FlashFinishEvent>();


		}

		

		/// <summary>
		/// �X�v���C�g��\��
		/// </summary>
		/// <param name="sprite">�\������X�v���C�g</param>
		void SpriteDisplay(Sprite sprite)
		{
			galleryImage.sprite = sprite;
			galleryImage.gameObject.SetActive(true);
			videoPlayer.gameObject.SetActive(false);
		}

		/// <summary>
		/// ����\��
		/// </summary>
		/// <param name="clip">�\�����铮��</param>
		void VideoDisplay(VideoClip clip)
		{
			videoPlayer.clip = clip;
			videoPlayer.gameObject.SetActive(true);
			galleryImage.gameObject.SetActive(false);
		}

		/// <summary>
		/// ����CG��
		/// </summary>
		IEnumerator NextCG()
		{
			++cgNumber;
			// �M�������[�I��
			if (displayGallery.gallerys.Count <= cgNumber)
			{
				gameObject.SetActive(false);
				cgNumber = 0;
				yield break;
			}
			if (displayGallery.gallerys.Count == (cgNumber + 1))
			{
				flashSprite.SetActive(true);
				canNextCG = false;
				message.text = "����������\n����������";
			}
			yield return new WaitUntil(() => canNextCG);
			// �X�v���C�g�\��
			if (IsSprite(displayGallery.gallerys[cgNumber]))
			{
				SpriteDisplay(displayGallery.gallerys[cgNumber] as Sprite);
			}
			// ����\��
			else if (IsVideo(displayGallery.gallerys[cgNumber]))
			{
				VideoDisplay(displayGallery.gallerys[cgNumber] as VideoClip);
			}
			else
			{
				Debug.LogError("�摜�ł�����ł�����܂���");
			}
		}

		/// <summary>
		/// �X�v���C�g���ǂ�������
		/// </summary>
		/// <param name="obj">���ʂ���I�u�W�F�N�g</param>
		/// <returns></returns>
		bool IsSprite(Object obj)
		{
			return (obj as Sprite) != null;
		}

		/// <summary>
		/// ���悩�ǂ�������
		/// </summary>
		/// <param name="obj">���ʂ���I�u�W�F�N�g</param>
		/// <returns></returns>
		bool IsVideo(Object obj)
		{
			return (obj as VideoClip) != null;
		}

		public void OnMMEvent(GalleryEvent gallery)
		{
			displayGallery = gallery.gallery;
			// �X�v���C�g�ł���Ή摜�ŕ\��
			if (IsSprite(displayGallery.gallerys[0]))
			{
				SpriteDisplay(displayGallery.gallerys[0] as Sprite);
			}
			// �X�v���C�g�łȂ��ꍇ�����\��
			else if (IsVideo(displayGallery.gallerys[0]))
			{
				VideoDisplay(displayGallery.gallerys[0] as VideoClip);
			}
			// �X�v���C�g�ł�����ł��Ȃ��ꍇ�G���[
			else
			{
				Debug.LogError("Sprite�ł�VideoClip�ł�����܂���");
			}
		}

		public void OnMMEvent(FlashFinishEvent finishEvent)
		{
			canNextCG = true;
		}
	}
}