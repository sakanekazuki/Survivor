using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{

	public class EroSuvivorCommandBase : MonoBehaviour
	{
		protected int exCmdClearCnt = 0;
		[SerializeField]
		protected List<KeyCode> exCmd = new List<KeyCode>()
	{
		KeyCode.E,KeyCode.R,KeyCode.O,KeyCode.S,KeyCode.A,KeyCode.B,KeyCode.A
	};

		void Update()
		{
			if (Input.anyKeyDown)
			{
				if (exCmdClearCnt < exCmd.Count)
				{
					if (Input.GetKeyDown(exCmd[exCmdClearCnt]))
					{
						exCmdClearCnt++;

						if (exCmdClearCnt >= exCmd.Count)
						{
							Command();
							exCmdClearCnt = 0;
						}
					}
					else
					{
						exCmdClearCnt = 0;
					}
				}
			}
		}

		protected virtual void Command()
		{
			Debug.Log("a");
		}
	}
}