using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ClickToEndClip : PlayableAsset, ITimelineClipAsset
{
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        return ScriptPlayable<ClickToEndBehaviour>.Create (graph);
    }

    public ClipCaps clipCaps { get; }
}
