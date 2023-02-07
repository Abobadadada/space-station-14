using Content.Server.Construction.Components;
using Content.Server.Destructible;
using Content.Server.Interaction;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.AI;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using JetBrains.Annotations;

namespace Content.Server.AI
{
    [UsedImplicitly]
    public sealed class AISystem : SharedAISystem
    {

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AIComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        }

        void OnInteractionAttempt(EntityUid uid, AIComponent component, InteractionAttemptEvent args)
        {
            if (args.Target == null) return; //checking for entity
            var target = args.Target.Value;

            if (!HasComp<DestructibleComponent>(target)) //if entity is not construction no interaction
                args.Cancel();

            if (!this.IsPowered(target, EntityManager)) //if no power no interaction
                args.Cancel();
        }
    }
}
