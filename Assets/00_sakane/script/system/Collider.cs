using UnityEngine;

namespace EroSurvivor
{
	// �Փ˔���
	public class Collider : MonoBehaviour
	{
		// �R���C�_�[�̑傫��
		[SerializeField]
		Vector2 size = Vector2.zero;

		public Vector2 Size
		{
			get
			{
				return size;
			}
		}


	}
}