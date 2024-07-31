using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialBtn : MonoBehaviour
{
	SurviveTouchNavigationManager surviveTouchNavigationManager;
	SurviveTouchNavigationController surviveTouchNavigationController;
	bool isEnabled = false;
	void Start()
	{
		surviveTouchNavigationManager = GameObject.Find("SurviveTouchNavigationManager").GetComponent<SurviveTouchNavigationManager>();
		surviveTouchNavigationController = GetComponent<SurviveTouchNavigationController>();
	}

	private void OnEnable()
	{
		isEnabled = true;
	}

	private void LateUpdate()
	{
		if (isEnabled)
		{
			surviveTouchNavigationManager.SetSelectController(surviveTouchNavigationController);
			isEnabled = false;
		}
	}
}
