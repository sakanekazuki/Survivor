using System;
using UnityEngine;

namespace EroSurvivor
{
	// シングルトンクラス
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		static T instance = null;
		public static T Instance
		{
			get
			{
				// nullなら生成
				if (instance == null)
				{
					Type t = typeof(T);
					instance = (T)FindObjectOfType(t);
					// 生成に失敗したらnullを返す
					if (instance == null)
					{
						Debug.LogError("オブジェクトが存在しません");
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