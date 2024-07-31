using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using MoreMountains.InventoryEngine;
using UnityEngine.UI;

public class ItemDisplayObj : MonoBehaviour
{
	// Start is called before the first frame update
	private RectTransform _rt;
	private List<SurviveItemPicker> DisplayPosUiItems = new List<SurviveItemPicker>();
	public GameObject DisplayPosUiBase;
	private List<GameObject> DisplayPosUiList = new List<GameObject>();
	public Camera _targetCamera;
	private Camera mainCamera;
	public Image image;
	public Canvas canvas;
	



	private void Awake()
    {
		mainCamera = Camera.main;
		_rt = this.GetComponent<RectTransform>();
	}
    void Start()
    {
	
		//DisplayPosUiItems = new List<SurviveItemPicker>();
	}

	// Update is called once per frame
	void Update()
    {
        
    }
	public void UpdatePosition(Vector3 center, GameObject target)
	{
		float canvasScale = transform.root.localScale.z;
		var canvasRect = canvas.GetComponent<RectTransform>();
		Vector2 canvasCenter = 0.5f * new Vector3(canvasRect.rect.width, canvasRect.rect.height);

		Vector3 pos;

		var screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, target.transform.position);
		Vector2 localPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPos, _targetCamera, out localPos);
		pos = localPos;
		if (pos.z < 0f)
		{
			pos.x = -pos.x;
			pos.y = -pos.y;

			if (Mathf.Approximately(pos.y, 0f))
			{
				pos.y = -center.y;
			}
		}

		var halfSize = 0.5f * canvasScale * _rt.sizeDelta;
		float d = Mathf.Max(
			Mathf.Abs(pos.x / (center.x - halfSize.x)),
			Mathf.Abs(pos.y / (center.y - halfSize.y))
		);
		float d2 = Mathf.Max(
			Mathf.Abs(pos.x / (canvasCenter.x - halfSize.x)),
			Mathf.Abs(pos.y / (canvasCenter.y - halfSize.y))
		);
		
		bool isOffscreen = (pos.z < 0f || d2 > 1f);
		image.enabled = true;
		if (isOffscreen)
		{
			pos.x /= d;
			pos.y /= d;

			_rt.anchoredPosition = pos / canvasScale;

			//å≥Ç™Å™å¸Ç´Ç»ÇÃÇ≈90ìxâÒì]
			image.rectTransform.eulerAngles = new Vector3(
				0f, 0f,
				Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg - 90f
			);
		}
        else
        {
			_rt.anchoredPosition = pos + new Vector3(0f, halfSize.y + 120f, 0f);
			image.rectTransform.eulerAngles = new Vector3(
				0f, 0f,
				180f
			);
		}


		if (isOffscreen)
		{
			
		}
	}

	public void SetActive(bool active)
    {
		image.enabled = active;
	}
}
