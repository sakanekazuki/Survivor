using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameEndUI : MonoBehaviour
{
    public TextMeshProUGUI coin;
    public TextMeshProUGUI deathCount;
    public GameObject GameOver;
    public GameObject GameClear;

    public AudioClip GameOverClip;
    public AudioClip GameClearClip;
    //public GameObject FirstButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetData(bool clear)
    {
        var sgm = SurviveGameManager.GetInstance();
        var slm = SurviveLevelManager.GetInstance();
        var player = slm.Players[0] as SurvivePlayer;
        //EventSystem.current.firstSelectedGameObject = FirstButton;
        if(clear)
        {
            GameOver.SetActive(false);
            GameClear.SetActive(true);


            MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            options.Loop = false;
            options.Location = Vector3.zero;
            options.ID = 0;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

            MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Pause, SurviveBackGroundMusic.Instance.CurrentBGMID);
            MMSoundManagerSoundPlayEvent.Trigger(GameClearClip, options);
        }
        else
        {
            GameOver.SetActive(true);
            GameClear.SetActive(false);


            MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            options.Loop = false;
            options.Location = Vector3.zero;
            options.ID = 0;
            options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

            MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Pause, SurviveBackGroundMusic.Instance.CurrentBGMID);
            MMSoundManagerSoundPlayEvent.Trigger(GameOverClip, options);

        }
        coin.text = player.battleParameter.Coin.ToString();
        deathCount.text = player.battleParameter.EnemyDeathCount.ToString();
    }

}
