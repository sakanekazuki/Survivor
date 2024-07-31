using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace EroSurvivor
{
	public class TitleLaungageChanger : MonoBehaviour
	{
		[SerializeField]
		Text texts;

		[SerializeField]
		List<string> jas = new List<string>();

		[SerializeField]
		List<string> ens = new List<string>();

		[SerializeField]
		SurviveSetting setting;

		private void OnEnable()
		{
			NameChange(0);
		}

		public void NameChange(int number)
		{
			if (setting?.SurviveSettingData?.language == 2)
			{
				texts.text = ens[number];
			}
			else if (setting?.SurviveSettingData?.language != 2)
			{
				texts.text = jas[number];
			}
		}
	}
}