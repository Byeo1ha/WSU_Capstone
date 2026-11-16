using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneSpeaker", menuName = "Cutscene/Speaker")]
public class CutsceneSpeakerData : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite portrait;

    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
}
