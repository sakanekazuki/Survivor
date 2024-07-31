using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	public class NameTextChange : MonoBehaviour
	{
		TMPro.TextMeshProUGUI nameTxt;

		private void Awake()
		{
			nameTxt = GetComponent<TMPro.TextMeshProUGUI>();
		}

		public void Change(string name)
		{
			nameTxt.text = name;
		}

		private void OnDisable()
		{
			nameTxt.text = "rui";
		}
	}
}