using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindingInput : MonoBehaviour
{
    //PlayerInputコンポーネントがあるゲームオブジェクト
    [SerializeField]
    private PlayerInput _pInput;

    //リバインディング中のメッセージ表示テキスト。アクティブ状態の可否に使用。
    [SerializeField]
    private GameObject _rebindingMessage;
    //リバインディングを開始するボタン。アクティブ状態の可否に使用。
    [SerializeField]
    private GameObject _rebindingButton;
    //リバインディング開始ボタンのテキスト。キー名を表示。
    [SerializeField]
    private Text _bindingName;

    //リバインディングしたいInputAction項目。今回はMap:PlayerのAction:Fireを使用しています。
    [SerializeField]
    private InputActionReference _action;
    //リバインディングしたいコントロールのインデックス
    private string rebindingIndex;

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    public void Start()
    {
        //アクション(Fire)ボタンを押下するとデバッグにFireと表示
        _pInput.actions["Jump"].performed += _ => Debug.Log("Jump");

        //すでにリバインディングしたことがある場合はシーン読み込み時に変更。
        string rebinds = PlayerPrefs.GetString("rebindSample");

        if (!string.IsNullOrEmpty(rebinds))
        {
            //リバインディング状態をロード
            _pInput.actions.LoadBindingOverridesFromJson(rebinds);

            //バインディング名を取得
            int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);
            _bindingName.text = InputControlPath.ToHumanReadableString(
                _action.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }


    }

    public void StartRebinding()
    {
        //ボタンを消し、代わりにリバインディング中のメッセージを表示
        _rebindingButton.SetActive(false);
        _rebindingMessage.SetActive(true);

        //ボタン制御中の表示
        //ボタンの誤作動を防ぐため、何も無いアクションマップに切り替え
        _pInput.SwitchCurrentActionMap("Select");

        //Fireボタンのリバインディング開始
        _rebindingOperation = _action.action.PerformInteractiveRebinding()
            .WithTargetBinding(_action.action.GetBindingIndexForControl(_action.action.controls[0]))
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindComplete())
            .Start();
    }

    public void RebindComplete()
    {
        //fireアクションの1番目のコントロール(バインディングしたコントロール)のインデックスを取得
        int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);

        //バインディングしたキーの名称を取得する
        _bindingName.text = InputControlPath.ToHumanReadableString(
            _action.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        _rebindingOperation.Dispose();

        //画面を通常に戻す
        _rebindingButton.SetActive(true);
        _rebindingMessage.SetActive(false);

        //リバインディング時は空のアクションマップだったので通常のアクションマップに切り替え
        _pInput.SwitchCurrentActionMap("PlayerControls");

        //リバインディングしたキーを保存(シーン開始時に読み込むため)
        PlayerPrefs.SetString("rebindSample", _pInput.actions.SaveBindingOverridesAsJson());
    }

    public void StartRebindingGamepad()
    {
        //ボタンを消し、代わりにリバインディング中のメッセージを表示
        _rebindingButton.SetActive(false);
        _rebindingMessage.SetActive(true);

        //ボタン制御中の表示
        //ボタンの誤作動を防ぐため、何も無いアクションマップに切り替え
        _pInput.SwitchCurrentActionMap("Select");

        //Fireボタンのリバインディング開始
        _rebindingOperation = _action.action.PerformInteractiveRebinding()
            .WithTargetBinding(_action.action.GetBindingIndexForControl(_action.action.controls[1]))
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindCompleteGamepad())
            .Start();
    }

    public void RebindCompleteGamepad()
    {
        //fireアクションの1番目のコントロール(バインディングしたコントロール)のインデックスを取得
        int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);

        //バインディングしたキーの名称を取得する
        _bindingName.text = InputControlPath.ToHumanReadableString(
            _action.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        _rebindingOperation.Dispose();

        //画面を通常に戻す
        _rebindingButton.SetActive(true);
        _rebindingMessage.SetActive(false);

        //リバインディング時は空のアクションマップだったので通常のアクションマップに切り替え
        _pInput.SwitchCurrentActionMap("PlayerControls");

        //リバインディングしたキーを保存(シーン開始時に読み込むため)
        PlayerPrefs.SetString("rebindSample", _pInput.actions.SaveBindingOverridesAsJson());
    }

}
