using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindingInput : MonoBehaviour
{
    //PlayerInput�R���|�[�l���g������Q�[���I�u�W�F�N�g
    [SerializeField]
    private PlayerInput _pInput;

    //���o�C���f�B���O���̃��b�Z�[�W�\���e�L�X�g�B�A�N�e�B�u��Ԃ̉ۂɎg�p�B
    [SerializeField]
    private GameObject _rebindingMessage;
    //���o�C���f�B���O���J�n����{�^���B�A�N�e�B�u��Ԃ̉ۂɎg�p�B
    [SerializeField]
    private GameObject _rebindingButton;
    //���o�C���f�B���O�J�n�{�^���̃e�L�X�g�B�L�[����\���B
    [SerializeField]
    private Text _bindingName;

    //���o�C���f�B���O������InputAction���ځB�����Map:Player��Action:Fire���g�p���Ă��܂��B
    [SerializeField]
    private InputActionReference _action;
    //���o�C���f�B���O�������R���g���[���̃C���f�b�N�X
    private string rebindingIndex;

    private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
    public void Start()
    {
        //�A�N�V����(Fire)�{�^������������ƃf�o�b�O��Fire�ƕ\��
        _pInput.actions["Jump"].performed += _ => Debug.Log("Jump");

        //���łɃ��o�C���f�B���O�������Ƃ�����ꍇ�̓V�[���ǂݍ��ݎ��ɕύX�B
        string rebinds = PlayerPrefs.GetString("rebindSample");

        if (!string.IsNullOrEmpty(rebinds))
        {
            //���o�C���f�B���O��Ԃ����[�h
            _pInput.actions.LoadBindingOverridesFromJson(rebinds);

            //�o�C���f�B���O�����擾
            int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);
            _bindingName.text = InputControlPath.ToHumanReadableString(
                _action.action.bindings[bindingIndex].effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice);
        }


    }

    public void StartRebinding()
    {
        //�{�^���������A����Ƀ��o�C���f�B���O���̃��b�Z�[�W��\��
        _rebindingButton.SetActive(false);
        _rebindingMessage.SetActive(true);

        //�{�^�����䒆�̕\��
        //�{�^���̌�쓮��h�����߁A���������A�N�V�����}�b�v�ɐ؂�ւ�
        _pInput.SwitchCurrentActionMap("Select");

        //Fire�{�^���̃��o�C���f�B���O�J�n
        _rebindingOperation = _action.action.PerformInteractiveRebinding()
            .WithTargetBinding(_action.action.GetBindingIndexForControl(_action.action.controls[0]))
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindComplete())
            .Start();
    }

    public void RebindComplete()
    {
        //fire�A�N�V������1�Ԗڂ̃R���g���[��(�o�C���f�B���O�����R���g���[��)�̃C���f�b�N�X���擾
        int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);

        //�o�C���f�B���O�����L�[�̖��̂��擾����
        _bindingName.text = InputControlPath.ToHumanReadableString(
            _action.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        _rebindingOperation.Dispose();

        //��ʂ�ʏ�ɖ߂�
        _rebindingButton.SetActive(true);
        _rebindingMessage.SetActive(false);

        //���o�C���f�B���O���͋�̃A�N�V�����}�b�v�������̂Œʏ�̃A�N�V�����}�b�v�ɐ؂�ւ�
        _pInput.SwitchCurrentActionMap("PlayerControls");

        //���o�C���f�B���O�����L�[��ۑ�(�V�[���J�n���ɓǂݍ��ނ���)
        PlayerPrefs.SetString("rebindSample", _pInput.actions.SaveBindingOverridesAsJson());
    }

    public void StartRebindingGamepad()
    {
        //�{�^���������A����Ƀ��o�C���f�B���O���̃��b�Z�[�W��\��
        _rebindingButton.SetActive(false);
        _rebindingMessage.SetActive(true);

        //�{�^�����䒆�̕\��
        //�{�^���̌�쓮��h�����߁A���������A�N�V�����}�b�v�ɐ؂�ւ�
        _pInput.SwitchCurrentActionMap("Select");

        //Fire�{�^���̃��o�C���f�B���O�J�n
        _rebindingOperation = _action.action.PerformInteractiveRebinding()
            .WithTargetBinding(_action.action.GetBindingIndexForControl(_action.action.controls[1]))
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation => RebindCompleteGamepad())
            .Start();
    }

    public void RebindCompleteGamepad()
    {
        //fire�A�N�V������1�Ԗڂ̃R���g���[��(�o�C���f�B���O�����R���g���[��)�̃C���f�b�N�X���擾
        int bindingIndex = _action.action.GetBindingIndexForControl(_action.action.controls[0]);

        //�o�C���f�B���O�����L�[�̖��̂��擾����
        _bindingName.text = InputControlPath.ToHumanReadableString(
            _action.action.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        _rebindingOperation.Dispose();

        //��ʂ�ʏ�ɖ߂�
        _rebindingButton.SetActive(true);
        _rebindingMessage.SetActive(false);

        //���o�C���f�B���O���͋�̃A�N�V�����}�b�v�������̂Œʏ�̃A�N�V�����}�b�v�ɐ؂�ւ�
        _pInput.SwitchCurrentActionMap("PlayerControls");

        //���o�C���f�B���O�����L�[��ۑ�(�V�[���J�n���ɓǂݍ��ނ���)
        PlayerPrefs.SetString("rebindSample", _pInput.actions.SaveBindingOverridesAsJson());
    }

}
