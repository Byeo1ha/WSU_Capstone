using UnityEngine;
using VContainer;
using VContainer.Unity;

public class CutsceneLifetimeScope : LifetimeScope
{
    [SerializeField] private CutsceneDialogue dialogue;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(dialogue);
    }
}
