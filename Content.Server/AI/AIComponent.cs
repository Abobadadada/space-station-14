using Content.Shared.AI;

namespace Content.Server.AI
{
    [RegisterComponent]
    [ComponentReference(typeof(SharedAIComponent))]
    public sealed class AIComponent : SharedAIComponent
    {
    }
}
