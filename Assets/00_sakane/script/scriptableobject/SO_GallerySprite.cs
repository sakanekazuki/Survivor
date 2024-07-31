using System.Collections.Generic;
using UnityEngine;

namespace EroSurvivor
{
	// �M�������[�摜��ۑ����郋�N���v�^�u���I�u�W�F�N�g
	[CreateAssetMenu(fileName = "GallerySprite", menuName = "EroSurvivor/GallerySprite")]
	public class SO_GallerySprite : ScriptableObject
	{
		// �M�������[�X�v���C�g
		public List<Gallery> gallerys = new List<Gallery>();

		[SerializeField]
		Gallery gallery;
	}
}