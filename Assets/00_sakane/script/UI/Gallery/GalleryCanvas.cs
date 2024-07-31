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

	// ギャラリーキャンバス
	public class GalleryCanvas : MonoBehaviour, MMEventListener<GalleryEvent>, MMEventListener<FlashFinishEvent>
	{
		//public enum GalleryType
		//{
		//	item,
		//	story,
		//	picture
		//}

		// ギャラリー
		[SerializeField]
		Image galleryImage;

		// ビデオ
		[SerializeField]
		VideoPlayer videoPlayer;

		// 表示するギャラリー
		Gallery displayGallery;
		// ギャラリー番号
		int cgNumber = 0;

		// フラッシュ
		[SerializeField]
		GameObject flashSprite;

		// 表示するメッセージ
		[SerializeField]
		TextMeshProUGUI message;

		// true = 次のCGに進むことができる
		bool canNextCG = true;

		//// ギャラリーの種類
		//GalleryType galleryType = GalleryType.item;

		//// アイテムキャンバス
		//[SerializeField]
		//GameObject itemGalleryCanvas;
		//// ストーリーキャンバス
		//[SerializeField]
		//GameObject storyGalleryCanvas;
		//// ピクチャキャンバス
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
		/// スプライトを表示
		/// </summary>
		/// <param name="sprite">表示するスプライト</param>
		void SpriteDisplay(Sprite sprite)
		{
			galleryImage.sprite = sprite;
			galleryImage.gameObject.SetActive(true);
			videoPlayer.gameObject.SetActive(false);
		}

		/// <summary>
		/// 動画表示
		/// </summary>
		/// <param name="clip">表示する動画</param>
		void VideoDisplay(VideoClip clip)
		{
			videoPlayer.clip = clip;
			videoPlayer.gameObject.SetActive(true);
			galleryImage.gameObject.SetActive(false);
		}

		/// <summary>
		/// 次のCGに
		/// </summary>
		IEnumerator NextCG()
		{
			++cgNumber;
			// ギャラリー終了
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
				message.text = "いいいいい\nいいいいい";
			}
			yield return new WaitUntil(() => canNextCG);
			// スプライト表示
			if (IsSprite(displayGallery.gallerys[cgNumber]))
			{
				SpriteDisplay(displayGallery.gallerys[cgNumber] as Sprite);
			}
			// 動画表示
			else if (IsVideo(displayGallery.gallerys[cgNumber]))
			{
				VideoDisplay(displayGallery.gallerys[cgNumber] as VideoClip);
			}
			else
			{
				Debug.LogError("画像でも動画でもありません");
			}
		}

		/// <summary>
		/// スプライトかどうか判別
		/// </summary>
		/// <param name="obj">判別するオブジェクト</param>
		/// <returns></returns>
		bool IsSprite(Object obj)
		{
			return (obj as Sprite) != null;
		}

		/// <summary>
		/// 動画かどうか判別
		/// </summary>
		/// <param name="obj">判別するオブジェクト</param>
		/// <returns></returns>
		bool IsVideo(Object obj)
		{
			return (obj as VideoClip) != null;
		}

		public void OnMMEvent(GalleryEvent gallery)
		{
			displayGallery = gallery.gallery;
			// スプライトであれば画像で表示
			if (IsSprite(displayGallery.gallerys[0]))
			{
				SpriteDisplay(displayGallery.gallerys[0] as Sprite);
			}
			// スプライトでない場合動画を表示
			else if (IsVideo(displayGallery.gallerys[0]))
			{
				VideoDisplay(displayGallery.gallerys[0] as VideoClip);
			}
			// スプライトでも動画でもない場合エラー
			else
			{
				Debug.LogError("SpriteでもVideoClipでもありません");
			}
		}

		public void OnMMEvent(FlashFinishEvent finishEvent)
		{
			canNextCG = true;
		}
	}
}