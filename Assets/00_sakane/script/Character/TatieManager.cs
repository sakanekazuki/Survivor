using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TatieManager : MonoBehaviour
{
	[SerializeField]
	List<GameObject> taties = new List<GameObject>();
	private void Awake()
	{
		foreach (GameObject t in taties)
		{
			t.SetActive(false);
		}
	}

	private void Start()
	{
		taties[SurviveGameManager.Instance.GetComponent<SurviveGameManager>().currentCharacterID - 1].SetActive(true);
	}
}
