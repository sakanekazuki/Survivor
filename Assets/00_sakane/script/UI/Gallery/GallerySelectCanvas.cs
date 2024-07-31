using Naninovel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using static EroSurvivor.GalleryCanvas;

namespace EroSurvivor
{
	// ギャラリー選択キャンバス
	public class GallerySelectCanvas : MonoBehaviour
	{
		public enum GalleryType
		{
			item,
			story,
			picture
		}

		// ギャラリーの種類
		GalleryType galleryType = GalleryType.item;

		// アイテムキャンバス
		[SerializeField]
		GameObject itemGalleryCanvas;
		// ストーリーキャンバス
		[SerializeField]
		GameObject storyGalleryCanvas;
		// ピクチャキャンバス
		[SerializeField]
		GameObject pictureGalleryCanvas;

		[SerializeField]
		GameObject pictureCanvas;
		Picture pic;

		// アイコンを表示するスプライト
		[SerializeField]
		Image spriteIcon;

		// 名前を表示するテキスト
		[SerializeField]
		/*TMPro.TextMeshProUGUI*/
		LocalizeStringEvent nameTxt;

		// 説明文を表示するテキスト
		[SerializeField]
		/*TMPro.TextMeshProUGUI*/
		LocalizeStringEvent explanationTxt;

		// アイコン配列
		[SerializeField]
		List<Sprite> iconList = new List<Sprite>();

		// アイテム名
		[SerializeField]
		List<string> itemNames = new List<string>();

		// 表示する説明文
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
		/// ギャラリー変更
		/// </summary>
		/// <param name="type">変更するギャラリーの種類</param>
		public void GalleryChange(int type)
		{
			// 現在のギャラリーの種類
			galleryType = (GalleryType)type;
			// 対応するキャンバスを表示
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
		/// 見るギャラリーの選択
		/// </summary>
		/// <param name="number">ギャラリーの番号</param>
		public void GallerySelect(/*int number*/string galleryName)
		{
			//GalleryManager.Instance.GalleryDisplay(number);
			GalleryManager.Instance.GalleryDisplay(galleryName);
		}

		/// <summary>
		/// 説明分表示
		/// </summary>
		/// <param name="number">アイテムの番号</param>
		public void Explanation(int number)
		{
			spriteIcon.sprite = iconList[number];
			//nameTxt.text = itemNames[number];
			//explanationTxt.text = explanationList[number];
			nameTxt.StringReference.SetReference("GalleryItemName", number.ToString());
			explanationTxt.StringReference.SetReference("GalleryText", number.ToString());
		}

		/// <summary>
		/// ギャラリー選択画面から戻る
		/// </summary>
		public void Return()
		{
			// このオブジェクトを非表示にする
			this.gameObject.SetActive(false);
		}

		public void PictureDisplay(int number)
		{
			pictureCanvas.SetActive(true);
			pic.PictureDisplay(number);
		}

		public void StoryTextChange(int number)
		{
			if (setting.SurviveSettingData.language == 2)//英語
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
			if (setting.SurviveSettingData.language == 2)//英語
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