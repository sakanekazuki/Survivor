using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
    public class ShopNameChanger : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Text nameTxt;

        [SerializeField]
        List<string> names = new List<string>();

		public void Change(int number)
		{
			nameTxt.text = names[number];
		}
	}
}