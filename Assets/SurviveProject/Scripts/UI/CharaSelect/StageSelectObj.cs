using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectObj : MonoBehaviour
{
    public Image stageSprite;
    public TMPro.TextMeshProUGUI stageName;
    public TMPro.TextMeshProUGUI coinRate;
    public SurviveMenu surviveMenu;

    StageDatas.StageData StageData;
    StageDatas.StageData EndlessStageData;
    public AudioClip MouseClickSe;
    public void SetData(StageDatas.StageData stageData)
    {
        StageData = stageData;
    }
    public void SetEndlessData(StageDatas.StageData stageData)
    {
        EndlessStageData = stageData;
    }

    public StageDatas.StageData GetData()
    {
        return StageData;
    }
    public StageDatas.StageData GetEndlessData()
    {
        return StageData;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectStage()
    {
        surviveMenu.StageSelectObjPressed(StageData.ID);

        PlaySe(MouseClickSe);

    }
    private void PlaySe(AudioClip ac)
    {
        if (ac == null) return;

        MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
        options.Loop = false;
        options.Location = Vector3.zero;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

        MMSoundManagerSoundPlayEvent.Trigger(ac, options);
    }

}
