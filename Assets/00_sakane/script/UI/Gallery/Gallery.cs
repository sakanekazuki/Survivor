using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	// �M�������[�̏��
	[System.Serializable]
	public class Gallery
	{
		// true = �{���\
		public bool isViewable = false;

		// �摜�Ɠ���z��
		public List<Object> gallerys = new List<Object>();

		// �\������
		public float displayTime = 0;
	}
}