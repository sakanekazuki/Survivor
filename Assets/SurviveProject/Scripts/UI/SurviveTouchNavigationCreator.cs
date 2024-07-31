using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using static SurviveTouchNavigationController;

public class SurviveTouchNavigationCreator : MonoBehaviour
{
    public enum SurviveTouchNavigationCreatorType { 
        Grid,
        OnOff
    }
    [SerializeField] SurviveTouchNavigationCreatorType surviveTouchNavigationCreatorType = SurviveTouchNavigationCreatorType.Grid;

    [Header("---��Grid�ݒ�----------------------")]
    //GridLayout��Content�ɃX�N���v�g���琶��
    [SerializeField] int GridNumX = 0;
    [SerializeField] float GridLineHight = 0;//LayoutGroup�����v�Z�ŋ��߂Ă��������ǎ��}���x�^��
    [SerializeField] float GridLineHightMax = 0;
    [SerializeField] SurviveTouchNavigationController FrontController;
    [SerializeField] SurviveTouchNavigationController AfterController;
    [SerializeField] List<GameObject> TargetObjs;
    [SerializeField] string[] SelectImgObjPath;
    [SerializeField] Transform GridContent;
    [SerializeField] string[] SurviveTouchButtonObjPath;

    [Header("---��OnOff�ݒ�----------------------")]
    [SerializeField] SurviveTouchNavigationController[] TargetNavigationController;
    public enum TargetNavigationControllerKey
    {
        Up,
        Down,
        Right,
        Left
    }
    [SerializeField] TargetNavigationControllerKey[] targetNavigationControllerKey;
    [SerializeField] SurviveTouchNavigationController OnNavigationController;
    [SerializeField] SurviveTouchNavigationController OffNavigationController;
    private bool UpdateCheckOn = false;

    private void LateUpdate()
    {
        switch (surviveTouchNavigationCreatorType)
        {
            case SurviveTouchNavigationCreatorType.OnOff:
                if (UpdateCheckOn != OnNavigationController.gameObject.activeSelf)
                {
                    ResetOnOffController();
                }
                break;
        }
    }

    void Awake()
    {
        switch (surviveTouchNavigationCreatorType)
        {
            case SurviveTouchNavigationCreatorType.Grid:
                CreateGridController(TargetObjs);
                break;
            case SurviveTouchNavigationCreatorType.OnOff:
                ResetOnOffController();
                break;
        }
    }

    public void CreateGridController(List<GameObject> gameObjects)
    {
        Dictionary<int, SurviveTouchNavigationController> controllers = new Dictionary<int, SurviveTouchNavigationController>();
        int max = (gameObjects.Count - 1);
        for (int i=0; i<=max; i++)
        {
            SurviveTouchNavigationController controller = gameObjects[i].AddComponent<SurviveTouchNavigationController>();
            controller.navigationType = NavigationType.ShopScrollContent;
            controllers.Add(i, controller);

            //�I���摜
            GameObject _selectImg = null;
            if (SelectImgObjPath.Length > 0)
            {
                _selectImg = GetSearchChildObj(gameObjects[i], SelectImgObjPath);
            }
            controller.SelectMarkImage = _selectImg;

            //SurviveTouchButton
            SurviveTouchButton stb = gameObjects[i].GetComponent<SurviveTouchButton>();
            if (stb == null && SurviveTouchButtonObjPath.Length > 0)
            {
                GameObject _obj = GetSearchChildObj(gameObjects[i], SurviveTouchButtonObjPath);
                if (_obj != null)
                {
                    stb = _obj.GetComponent<SurviveTouchButton>();
                }
            }
            controller.thisSelectable = stb;

            //�V���b�v�p
            controller.thisShopItem = gameObjects[i].GetComponent<SurviveShopItem>();
        }

        for (int i = 0; i <= max; i++)
        {
            SurviveTouchNavigationController controller = controllers[i];

            controller.ScrollLineY = GridLineHight * Mathf.FloorToInt(i / GridNumX);
            controller.ScrollLineYMax = GridLineHightMax;
            controller.ScrollContent = GridContent;

            //up
            if (i == 0)
            {
                //Grid�O��
                controller.upSelectable = FrontController;
                FrontController.downSelectable = controller;
            }
            else if (1 <= i && i < GridNumX)
            {
                //�ŏ���1�ԖڂɈ�U���
                controller.upSelectable = controllers[0];
            }
            else
            {
                //1���
                int index = i - GridNumX;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.upSelectable = controllers[index];
            }

            //down
            if (i == max)
            {
                //Grid�O��
                controller.downSelectable = AfterController;
                AfterController.upSelectable = controller;
            }
            else if (i >= (Mathf.FloorToInt(max / GridNumX) * GridNumX))
            {
                //�ŉ���͍Ō�Ɉ�U���
                controller.downSelectable = controllers[max];
            }
            else
            {
                //1��
                int index = i + GridNumX;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.downSelectable = controllers[index];
            }


            //right
            if (i == max || ((i % GridNumX) == (GridNumX - 1)))
            {
                //���[�ɖ߂�
                int index = Mathf.FloorToInt(i / GridNumX) * GridNumX;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.rightSelectable = controllers[index];
            }
            else
            {
                //�E��
                int index = i + 1;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.rightSelectable = controllers[i + 1];
            }

            //left
            if ((i % GridNumX) == 0)
            {
                //�E�[�ɂ���
                int index = (Mathf.FloorToInt(i / GridNumX) + 1) * GridNumX - 1;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.leftSelectable = controllers[index];
            }
            else
            {
                //���ւ�
                int index = i - 1;
                if (index >= max) index = max;
                if (index <= 0) index = 0;
                controller.leftSelectable = controllers[i - 1];
            }

        }
    }


    //�q�v�f�̃I�u�W�F�N�g�擾
    private GameObject GetSearchChildObj(GameObject parent=null, string[] path=null, int depth=0)
    {
        GameObject obj = parent;

        if (path.Length <= depth)
        {
            return obj;
        }


        foreach (Transform child in parent.transform)
        {
            if (child.name == path[depth])
            {
                obj = GetSearchChildObj(child.gameObject, path, (depth + 1));
            }
        }

        return obj;
    }









    private void ResetOnOffController()
    {
        UpdateCheckOn = OnNavigationController.gameObject.activeSelf;

        foreach (TargetNavigationControllerKey key in targetNavigationControllerKey)
        {
            foreach (SurviveTouchNavigationController target in TargetNavigationController)
            {
                switch (key)
                {
                    case TargetNavigationControllerKey.Up:
                        target.upSelectable = (OnNavigationController.gameObject.activeSelf) ? OnNavigationController : OffNavigationController;
                        break;
                    case TargetNavigationControllerKey.Down:
                        target.downSelectable = (OnNavigationController.gameObject.activeSelf) ? OnNavigationController : OffNavigationController;
                        break;
                    case TargetNavigationControllerKey.Right:
                        target.rightSelectable = (OnNavigationController.gameObject.activeSelf) ? OnNavigationController : OffNavigationController;
                        break;
                    case TargetNavigationControllerKey.Left:
                        target.leftSelectable = (OnNavigationController.gameObject.activeSelf) ? OnNavigationController : OffNavigationController;
                        break;
                }
            }
        }

    }
}
