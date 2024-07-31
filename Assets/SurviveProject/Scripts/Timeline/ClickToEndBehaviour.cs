using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.Playables;

public class ClickToEndBehaviour : PlayableBehaviour
{
    private PlayableDirector director { get; set; }
    
    public override void OnPlayableCreate(Playable playable)
    {
        director = (playable.GetGraph().GetResolver() as PlayableDirector);
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (SurviveInputManager.Instance.CrouchButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
        {
            var diff = playable.GetDuration() - playable.GetTime();
            director.time += diff;
        }
    }
}