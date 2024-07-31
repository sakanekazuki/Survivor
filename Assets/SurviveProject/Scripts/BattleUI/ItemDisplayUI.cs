using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using MoreMountains.InventoryEngine;

public class ItemDisplayUI : MonoBehaviour
{
	// Start is called before the first frame update
	private RectTransform _rt;
	private List<SurviveItemPicker> DisplayPosUiItems = new List<SurviveItemPicker>();
	public GameObject DisplayPosUiBase;
	private List<ItemDisplayObj> DisplayPosUiList = new List<ItemDisplayObj>();
	public Camera _targetCamera;
	private Camera mainCamera;
	void Start()
    {
		mainCamera = Camera.main;
		_rt = this.GetComponent<RectTransform>();
		//DisplayPosUiItems = new List<SurviveItemPicker>();
	}

	// Update is called once per frame
	void Update()
    {
        
    }
	public void SetDisplayItem(SurviveItemPicker itemPicker)
	{
		DisplayPosUiItems.Add(itemPicker);
	}
	public void RemoveDisplayItem(SurviveItemPicker itemPicker)
	{
		if (DisplayPosUiItems.Contains(itemPicker))
		{
			DisplayPosUiItems.Remove(itemPicker);
		}
	}
    private void LateUpdate()
    {
        if(DisplayPosUiItems.Count > 0)
        {
			int count = 0;
			var center = 0.5f * new Vector3(_rt.rect.width, _rt.rect.height);
			//center -=_rt.localPosition;

			for (int i = 0; i < DisplayPosUiItems.Count;i++)
            {
				if(DisplayPosUiList.Count <= count)
                {
					DisplayPosUiList.Add(Instantiate(DisplayPosUiBase, this.transform).GetComponent<ItemDisplayObj>());
				}

				DisplayPosUiList[count].UpdatePosition(center, DisplayPosUiItems[i].gameObject);

				count++;
			}

			if(DisplayPosUiList.Count>= count)
            {
				for (int i = count; i < DisplayPosUiList.Count; i++)
				{
					DisplayPosUiList[i].SetActive(false);
				}

			}
        }
        else
        {
			for (int i = 0; i < DisplayPosUiList.Count; i++)
			{
				DisplayPosUiList[i].SetActive(false);
			}
		}
    }
}
