using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �Q�[�����[���@
/// ����
/// 
/// 
/// </summary>
public class SurviveRuleManager
{

    //���݂̎���
    private float CurrentTime = 0f;

    //-1�͖����i�ǂ�������Ō��E�͍��ׂ��ł͂��邪�j ��U1��
    private float MaxTime = 60f;

    public bool GameEnd = false;

    
    public float GetCurrentTime()
    {
        return CurrentTime;
    }
    public void ResetCurrentTime()
    {
        CurrentTime = 0f;
    }
    public int GetCurrentSecond()
    {
        return (int)CurrentTime;
    }

#if DEBUG || UNITY_EDITOR
    public void AddCurrentTime(int time)
    {
        CurrentTime += time;
    }
#endif



    public void TimeUpdate()
    {
        //�}�l�[�W���[���Ƃ��ŌĂяo��
        CurrentTime += Time.deltaTime;
    }


    public bool TimeEnd()
    {
        return CurrentTime >= MaxTime;
    }

    //�Ƃ肠�������Ԑݒ肾��
    public void Initialize(float time)
    {
        MaxTime = time;
        CurrentTime = 0f;
        GameEnd = false;



        // ���ʍĐݒ�����������āc
        // �C���F�u���S�v���u���g���C�v�ŃQ�[���X�^�[�g���ASE�̉��ʂ��f�t�H���g�ɖ߂�
        GameObject.Find("SettingManager")?.GetComponent<SurviveSetting>().InitSoundVolumes();
    }
}
