using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class SurviveTouchNavigationManager : MonoBehaviour
{
    private SurviveTouchNavigationController SelectSurviveTouchNavigationController;

    private int longTapCntTime = 20;
    private int longTapCnt = 0;
    private int longTapIntervalTime = 3;
    private int longTapInterval = 0;
    private bool longTap = false;

    public SurviveTouchNavigationController PointaLastNavigation;

    private float OnStickMagni = 0.5f;

    public void SetSelectController(SurviveTouchNavigationController _select, bool isKey=false)
    {
        if (_select != null && SelectSurviveTouchNavigationController != null && _select.gameObject == SelectSurviveTouchNavigationController.gameObject)
        {
            return;
        }
        if (_select.thisSelectable != null && !_select.thisSelectable.Interactable)
        {
            return;
        }

        if (SelectSurviveTouchNavigationController != null)
        {
            SelectSurviveTouchNavigationController.ExitPointer();
        }

        SelectSurviveTouchNavigationController = _select;
        SelectSurviveTouchNavigationController.SelectThisSelectable(isKey);

#if UNITY_EDITOR
        //Debug.Log($"SetSelectController:{_select.GetRootPath()}");
#endif


        if (SelectSurviveTouchNavigationController.navigationType == SurviveTouchNavigationController.NavigationType.ShopScrollContent)
        {
            //スクロール動かす
            if (CheckScrollMoveKey())
            {
                StartCoroutine(MoveScrollContent(SelectSurviveTouchNavigationController.ScrollContent, SelectSurviveTouchNavigationController.ScrollLineY, SelectSurviveTouchNavigationController.ScrollLineYMax));
            }
        }
    }

    public void Update()
    {
        if (SelectSurviveTouchNavigationController == null) return;

        switch (SelectSurviveTouchNavigationController.navigationType)
        {
            case SurviveTouchNavigationController.NavigationType.Slider:

                string checkKeyName = "";

                if (CheckLeftTypeKey() && CheckRightTypeKey())
                {
                    //同時押しは動かさないようにしたい
                }
                else
                {
                    if (CheckLeftTypeKey())
                    {
                        checkKeyName = "0";
                        if (CheckLongTapMethod())
                        {
                            SelectSurviveTouchNavigationController.SelectLeftSelectable();
                        }
                    }
                    if (CheckRightTypeKey())
                    {
                        checkKeyName = "1";
                        if (CheckLongTapMethod())
                        {
                            SelectSurviveTouchNavigationController.SelectRightSelectable();
                        }
                    }
                }
                if (checkKeyName == "")
                {
                    longTapCnt = 0;
                    longTapInterval = 0;
                    if (longTap)
                    {
                        longTap = false;
                        SelectSurviveTouchNavigationController.OnPointerUpAction();

                    }
                }


                if (InputButton(KeyCode.UpArrow) || InputButton(KeyCode.W))
                {
                    SelectSurviveTouchNavigationController.SelectUpSelectable();
                }
                else if (InputButton(KeyCode.DownArrow) || InputButton(KeyCode.S))
                {
                    SelectSurviveTouchNavigationController.SelectDownSelectable();
                }

                break;
            default:
                if (InputButton(KeyCode.LeftArrow) || InputButton(KeyCode.A))
                {
                    SelectSurviveTouchNavigationController.SelectLeftSelectable();
                }
                else if (InputButton(KeyCode.RightArrow) || InputButton(KeyCode.D))
                {
                    SelectSurviveTouchNavigationController.SelectRightSelectable();
                }
                else if (InputButton(KeyCode.UpArrow) || InputButton(KeyCode.W))
                {
                    SelectSurviveTouchNavigationController.SelectUpSelectable();
                }
                else if (InputButton(KeyCode.DownArrow) || InputButton(KeyCode.S))
                {
                    SelectSurviveTouchNavigationController.SelectDownSelectable();
                }
                else if (InputButton(KeyCode.Return) || InputButton(KeyCode.Space))
                {
                    SelectSurviveTouchNavigationController.SelectSubmit();
                }

                break;

        }

    }




    private bool CheckLongTapMethod()
    {
        bool b = false;
        if (longTapCnt == 0 || longTapCnt >= longTapCntTime)
        {
            if (longTapInterval == 0)
            {
                b = true;
                longTap = true;
            }
            else
            {
                b = false;
            }
            longTapInterval++;
            if (longTapInterval >= longTapIntervalTime) longTapInterval = 0;
        }
        else
        {
            b = false;
        }
        longTapCnt++;

        return b;
    }
    private bool CheckLeftTypeKey()
    {
        return (InputButton(KeyCode.LeftArrow, false) || InputButton(KeyCode.A, false));
    }
    private bool CheckRightTypeKey()
    {
        return (InputButton(KeyCode.RightArrow, false) || InputButton(KeyCode.D, false) || InputButton(KeyCode.Return, false) || InputButton(KeyCode.Space, false));
    }
    private bool CheckScrollMoveKey()
    {
        return (
            Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) ||
            Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) 
            || CheckStickMovement(InputManager.Instance.PrimaryMovement.y, OnStickMagni)
            || CheckStickMovement(InputManager.Instance.PrimaryMovement.y, -OnStickMagni)
            || CheckStickMovement(InputManager.Instance.PrimaryMovement.x, OnStickMagni)
            || CheckStickMovement(InputManager.Instance.PrimaryMovement.x, -OnStickMagni)
            );
    }




    IEnumerator MoveScrollContent(Transform content, float targetY, float targetYMax)
    {
        if (content == null) yield break;

        Vector3 pos = content.transform.localPosition;
        for (int i=0; i<10; i++)
        {
            pos.y = pos.y + ((targetY - pos.y) * 0.5f);
            if (pos.y >= targetYMax)
            {
                pos.y = targetYMax;
                content.transform.localPosition = pos;
                yield break;
            }
            content.transform.localPosition = pos;
            yield return null;
        }
        pos.y = targetY;
        content.transform.localPosition = pos;

        yield break;
    }


    private bool InputButton(KeyCode keyCode, bool down = true)
    {
        bool b = false;
        switch (keyCode)
        {
            case KeyCode.UpArrow:
            case KeyCode.W:
                b = ((down ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode)) || StickMovement("up", InputManager.Instance.PrimaryMovement.y, OnStickMagni, down));
                break;
            case KeyCode.DownArrow:
            case KeyCode.S:
                b = ((down ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode)) || StickMovement("down", InputManager.Instance.PrimaryMovement.y, -OnStickMagni, down));
                break;
            case KeyCode.RightArrow:
            case KeyCode.D:
                b = ((down ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode)) || StickMovement("right", InputManager.Instance.PrimaryMovement.x, OnStickMagni, down));
                break;
            case KeyCode.LeftArrow:
            case KeyCode.A:
                b = ((down ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode)) || StickMovement("left", InputManager.Instance.PrimaryMovement.x, -OnStickMagni, down));
                break;
            case KeyCode.Return:
            case KeyCode.Space:
                b = ((down ? Input.GetKeyDown(keyCode) : Input.GetKey(keyCode)) || (SurviveInputManager.Instance as SurviveInputManager).SubmitButton.State.CurrentState == MMInput.ButtonStates.ButtonDown);

                break;
        }
        return b;
    }


    private Dictionary<string, bool> InputStick = new Dictionary<string, bool>();
    private bool StickMovement(string keyName, float magni, float maxmagni, bool down)
    {
        bool b = false;

        bool _InputStick = false;
        if (!InputStick.ContainsKey(keyName))
        {
            InputStick.Add(keyName, false);
        }
        _InputStick = InputStick[keyName];

        if (!_InputStick && CheckStickMovement(magni, maxmagni))
        {
            InputStick[keyName] = true;
            b = true;
        }
        else if (_InputStick)
        {
            if (!CheckStickMovement(magni, maxmagni) || down == false)
            {
                InputStick[keyName] = false;
            }
        }


        return b;
    }
    private bool CheckStickMovement(float magni, float maxmagni)
    {
        return (maxmagni > 0 ? magni >= maxmagni : magni <= maxmagni);
    }


}
