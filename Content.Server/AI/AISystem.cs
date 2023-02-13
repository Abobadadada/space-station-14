<<<<<<< HEAD
using Content.Server.Construction.Components;
using Content.Server.Interaction;
using Content.Server.MachineLinking.Components;
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

        private void OnInteractionAttempt(EntityUid uid, AIComponent component, InteractionAttemptEvent args)
        {
            //We need to check for power(and also if its an APC, because its not powered)
            if (args.Target == null) return;
            var target = args.Target.Value;

            if (!this.IsPowered(target, EntityManager) && !HasComp<ApcComponent>(target)
                && !HasComp<SignalSwitchComponent>(target))
                args.Cancel();
        }
    }
}
=======
>>>>>>> parent of 6bbdd64e7 (da merge)
