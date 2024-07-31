using UnityEngine;
using TMPro;

namespace EroSurvivor
{
	public class LanguageChanger : MonoBehaviour
	{
		int languageNumber = 0;

		[SerializeField]
		SurviveSetting setting;

		[SerializeField]
		TextMeshProUGUI languageTxt;

		private void Start()
		{
			languageNumber = setting.SurviveSettingData.language - 1;
			Change();
		}

		public void Change()
		{
			languageNumber++;
			languageNumber = (languageNumber % 2) + 2;
			setting.ToggleUpdateLanguage(languageNumber);
			if (languageNumber == 2)
			{
				GalleryManager.LocaleName = "en";
				languageTxt.text = "English";
			}
			else
			{
				GalleryManager.LocaleName = "ja";
				languageTxt.text = "Japanese";
			}
		}
	}
}