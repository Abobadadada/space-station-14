﻿using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Timing;

namespace Content.Shared.Cabinet;

public abstract class SharedItemCabinetSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ItemCabinetComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<ItemCabinetComponent, ComponentHandleState>(OnHandleState);

        SubscribeLocalEvent<ItemCabinetComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<ItemCabinetComponent, ComponentRemove>(OnComponentRemove);
        SubscribeLocalEvent<ItemCabinetComponent, ComponentStartup>(OnComponentStartup);

        SubscribeLocalEvent<ItemCabinetComponent, ActivateInWorldEvent>(OnActivateInWorld);
        SubscribeLocalEvent<ItemCabinetComponent, GetVerbsEvent<AlternativeVerb>>(AddToggleOpenVerb);

        SubscribeLocalEvent<ItemCabinetComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<ItemCabinetComponent, EntRemovedFromContainerMessage>(OnContainerModified);

        SubscribeLocalEvent<ItemCabinetComponent, LockToggleAttemptEvent>(OnLockToggleAttempt);
    }

    private void OnGetState(EntityUid uid, ItemCabinetComponent component, ref ComponentGetState args)
    {
        args.State = new ItemCabinetComponentState(component.DoorSound,
            component.Opened,
            component.OpenState,
            component.ClosedState);
    }

    private void OnHandleState(EntityUid uid, ItemCabinetComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ItemCabinetComponentState state)
            return;
        component.DoorSound = state.DoorSound;
        component.Opened = state.Opened;
        component.OpenState = state.OpenState;
        component.ClosedState = state.ClosedState;
    }

    private void OnComponentInit(EntityUid uid, ItemCabinetComponent cabinet, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, "ItemCabinet", cabinet.CabinetSlot);
    }

    private void OnComponentRemove(EntityUid uid, ItemCabinetComponent cabinet, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, cabinet.CabinetSlot);
    }

    private void OnComponentStartup(EntityUid uid, ItemCabinetComponent cabinet, ComponentStartup args)
    {
        UpdateAppearance(uid, cabinet);
        _itemSlots.SetLock(uid, cabinet.CabinetSlot, !cabinet.Opened);
    }

    protected virtual void UpdateAppearance(EntityUid uid, ItemCabinetComponent? cabinet = null)
    {
        // we don't fuck with appearance data, and instead just manually update the sprite on the client
    }

    private void OnContainerModified(EntityUid uid, ItemCabinetComponent cabinet, ContainerModifiedMessage args)
    {
        if (!cabinet.Initialized)
            return;

        if (args.Container.ID == cabinet.CabinetSlot.ID)
            UpdateAppearance(uid, cabinet);
    }

    private void OnLockToggleAttempt(EntityUid uid, ItemCabinetComponent cabinet, ref LockToggleAttemptEvent args)
    {
        // Cannot lock or unlock while open.
        if (cabinet.Opened)
            args.Cancelled = true;
    }

    private void AddToggleOpenVerb(EntityUid uid, ItemCabinetComponent cabinet, GetVerbsEvent<AlternativeVerb> args)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract)
            return;

        if (TryComp<LockComponent>(uid, out var lockComponent) && lockComponent.Locked)
            return;

        // Toggle open verb
        AlternativeVerb toggleVerb = new()
        {
            Act = () => ToggleItemCabinet(uid, args.User, cabinet)
        };
        if (cabinet.Opened)
        {
            toggleVerb.Text = Loc.GetString("verb-common-close");
            toggleVerb.IconTexture = "/Textures/Interface/VerbIcons/close.svg.192dpi.png";
        }
        else
        {
            toggleVerb.Text = Loc.GetString("verb-common-open");
            toggleVerb.IconTexture = "/Textures/Interface/VerbIcons/open.svg.192dpi.png";
        }
        args.Verbs.Add(toggleVerb);
    }

    private void OnActivateInWorld(EntityUid uid, ItemCabinetComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ToggleItemCabinet(uid, args.User, comp);
    }

    /// <summary>
    ///     Toggles the ItemCabinet's state.
    /// </summary>
    public void ToggleItemCabinet(EntityUid uid, EntityUid? user = null, ItemCabinetComponent? cabinet = null)
    {
        if (!Resolve(uid, ref cabinet))
            return;

        if (TryComp<LockComponent>(uid, out var lockComponent) && lockComponent.Locked)
            return;

        cabinet.Opened = !cabinet.Opened;
        Dirty(cabinet);
        _itemSlots.SetLock(uid, cabinet.CabinetSlot, !cabinet.Opened);

        if (_timing.IsFirstTimePredicted)
        {
            UpdateAppearance(uid, cabinet);
            _audio.PlayPredicted(cabinet.DoorSound, uid, user, AudioParams.Default.WithVariation(0.15f));
        }
    }
}
