using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SurviveBackGroundMusic : MMSingleton<SurviveBackGroundMusic>,  MMEventListener<BossStartEvent>, MMEventListener<BossEndEvent>
{

    /// the background music
    [Tooltip("the audio clip to use as background music")]
    public AudioClip SoundClip;
    /// whether or not the music should loop
    [Tooltip("whether or not the music should loop")]
    public bool Loop = true;

    public AudioClip BossSoundClip;
	public AudioClip BossSe;


	public int CurrentBGMID = 9999;

	protected override void Awake()
	{
		base.Awake();
		this.MMEventStartListening<BossStartEvent>();
		this.MMEventStartListening<BossEndEvent>();
	}
    
    protected void Start()
	{
		MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
		options.Loop = Loop;
		options.Location = Vector3.zero;
		options.ID = 9999;
		options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

		MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);

        MMSoundManagerPlayOptions options2 = MMSoundManagerPlayOptions.Default;
        options.Loop = Loop;
        options.Location = Vector3.zero;
        options.ID = 9998;
        options.Fade = true;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

        MMSoundManagerSoundPlayEvent.Trigger(BossSoundClip, options);
		MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, 9998);
    }

    public void OnMMEvent(BossStartEvent eventType)
	{
		MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, 9999);
		DOVirtual.DelayedCall(1f, () => {
            MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Resume, 9998);
        });
		
		CurrentBGMID = 9998;
		//MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
		//      options.Loop = Loop;
		//options.Location = Vector3.zero;
		//options.ID = 9999;
		//options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

		//MMSoundManagerSoundPlayEvent.Trigger(BossSoundClip, options);



		MMSoundManagerPlayOptions optionsSE = MMSoundManagerPlayOptions.Default;
		optionsSE.Loop = false;
		optionsSE.Location = Vector3.zero;
		optionsSE.ID = 0;
		optionsSE.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;

		MMSoundManagerSoundPlayEvent.Trigger(BossSe, optionsSE);
	}
	public void OnMMEvent(BossEndEvent eventType)
	{
		if (SurviveGameManager.GetInstance() != null && SurviveGameManager.GetInstance().Playing)
		{
			MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, 9998);
            DOVirtual.DelayedCall(1f, () => {
				MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Resume, 9999);
            });

            //         MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
            //options.Loop = Loop;
            //options.Location = Vector3.zero;
            //options.ID = 9999;
            //options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;

            //MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
            CurrentBGMID = 9999;
        }

    }
	protected void OnDisable()
	{
		this.MMEventStopListening<BossStartEvent>();
		this.MMEventStopListening<BossEndEvent>();
	}
}
public struct BossStartEvent
{
	static BossStartEvent e;
	public static void Trigger()
	{
		MMEventManager.TriggerEvent(e);
	}
}
public struct BossEndEvent
{
	static BossEndEvent e;
	public static void Trigger()
	{
		MMEventManager.TriggerEvent(e);
	}
}