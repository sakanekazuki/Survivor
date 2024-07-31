using System;
using UnityEngine;

namespace EroSurvivor
{
	// �V���O���g���N���X
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		static T instance = null;
		public static T Instance
		{
			get
			{
				// null�Ȃ琶��
				if (instance == null)
				{
					Type t = typeof(T);
					instance = (T)FindObjectOfType(t);
					// �����Ɏ��s������null��Ԃ�
					if (instance == null)
					{
						Debug.LogError("�I�u�W�F�N�g�����݂��܂���");
						return null;
					}
				}
				return instance;
			}
		}

		private void Awake()
		{
			instance = this as T;
		}

		private void OnDestroy()
		{
			instance = null;
		}

		protected virtual void Update()
		{

		}
	}
}