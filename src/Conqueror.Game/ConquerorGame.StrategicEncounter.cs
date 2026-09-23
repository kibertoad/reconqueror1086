using Conqueror.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

/// <summary>
/// Host binding for the recovered strategic player/enemy resolver. The core
/// retains source menu codes, unit records, and strict tactical timing; this
/// layer translates only replacement mouse/keyboard events to mapped routes.
/// </summary>
public sealed partial class ConquerorGame
{
    private const int StrategicEncounterWidth = 640;
    private const int StrategicEncounterHeight = 480;
    private OriginalStrategicInteractiveEncounterHoverPresentation _strategicEncounterHover;
    private int _strategicEncounterPlayerScoreModifier;
    private OriginalStrategicInteractiveRetreatConfirmation? _strategicInteractiveRetreatConfirmation;
    private OriginalStrategicPointerEventQueue _strategicPointerEvents = new();
    private OriginalStrategicTemporaryForceEncounter? _strategicPatrolEncounter;

    private void BeginStrategicPatrolEncounter(OriginalStrategicTemporaryForceEncounter encounter)
    {
        _strategicPatrolEncounter = encounter;
        _strategicEncounter = null;
        _strategicInteractiveEncounter = null;
        _strategicEncounterViewport = null;
        _strategicEncounterHover = default;
        _strategicInteractiveRetreatConfirmation = null;
        _strategicPointerEvents = new();
        _strategicEncounterPlayerScoreModifier = 0;
        _screen = Screen.StrategicEncounter;
        _notice = "PATROL CONTACT";
    }

    private void BeginStrategicEncounter(OriginalStrategicPlayerEnemyEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        _strategicEncounter = encounter;
        _strategicInteractiveEncounter = null;
        _strategicEncounterViewport = null;
        _strategicEncounterHover = default;
        _strategicInteractiveRetreatConfirmation = null;
        _strategicPointerEvents = new();
        _strategicEncounterPlayerScoreModifier = _campaign.OriginalStrategicEncounterPlayerScoreModifier(encounter);
        _screen = Screen.StrategicEncounter;
        _notice = "STRATEGIC ARMIES MEET";
    }

    private void UpdateStrategicEncounter(Func<Keys, bool> press, MouseState mouse,
        bool click, bool release, bool controllerRightClick)
    {
        var encounter = _strategicEncounter;
        var patrolEncounter = _strategicPatrolEncounter;
        if (encounter is null && patrolEncounter is null)
        {
            _screen = Screen.Map;
            return;
        }

        if (_strategicInteractiveEncounter is null)
        {
            var entry = EncounterMenuEntryForInput(press, mouse, click);
            if (entry is null) return;
            if (entry.Value.ExitsToAutomaticFallback)
            {
                if (patrolEncounter is not null)
                {
                    var settlement = _campaign.ResolveAutomaticOriginalStrategicPatrolEncounter(
                        patrolEncounter, new HostEncounterRandom());
                    FinishStrategicEncounter(settlement.PatrolCleared ? "PATROL DEFEATED" : "STRATEGIC DEFEAT");
                }
                else
                {
                    var automatic = _campaign.ResolveAutomaticOriginalStrategicEncounter(
                        encounter!, _strategicEncounterPlayerScoreModifier, new HostEncounterRandom());
                    FinishStrategicEncounter(automatic.ResolverResult.PlayerWon ? "STRATEGIC VICTORY" : "STRATEGIC DEFEAT");
                }
                return;
            }

            _strategicInteractiveEncounter = patrolEncounter is not null
                ? _campaign.BeginInteractiveOriginalStrategicPatrolEncounter(
                    patrolEncounter, entry.Value.InteractiveSelectionCode!.Value,
                    StrategicEncounterWidth, StrategicEncounterHeight,
                    TimeSpan.FromSeconds(_presentationSeconds), new HostEncounterRandom())
                : _campaign.BeginInteractiveOriginalStrategicEncounter(
                    encounter!, entry.Value.InteractiveSelectionCode!.Value,
                    StrategicEncounterWidth, StrategicEncounterHeight,
                    TimeSpan.FromSeconds(_presentationSeconds), new HostEncounterRandom());
            _strategicEncounterViewport = OriginalStrategicInteractiveEncounterViewport.ForResolvedDisplay(
                StrategicEncounterWidth, StrategicEncounterHeight,
                StrategicEncounterWidth, StrategicEncounterHeight);
            _strategicPointerEvents = new();
            _notice = "SELECT UNITS, THEN ARM THE FIRST CONTROL";
            return;
        }

        var session = _strategicInteractiveEncounter;
        var viewport = _strategicEncounterViewport
            ?? throw new InvalidOperationException("Strategic encounter viewport is unavailable.");
        var (pointerX, pointerY) = OriginalPoint(mouse);
        CaptureStrategicPointerInput(mouse, click, release, controllerRightClick,
            pointerX, pointerY);
        if (_strategicInteractiveRetreatConfirmation is { } confirmation)
        {
            var accepted = press(Keys.Enter);
            if (!accepted && !press(Keys.Escape)) return;
            _strategicInteractiveRetreatConfirmation = null;
            AdvanceInteractiveStrategicEncounterFrame(
                encounter, patrolEncounter, session, viewport, confirmation.LocalX, confirmation.LocalY,
                inputCode: 3, firstControlConfirmationAccepted: accepted);
            return;
        }

        var input = _strategicPointerEvents.TryDequeue(out var queued)
            ? queued
            : new OriginalStrategicPointerInput(0, pointerX, pointerY);
        var (inputCode, x, y) = input;
        if (inputCode == 3 && session.IsMappedTacticalAdvancementEnabled
            && session.RouteControlStripHit(x, y, viewport.ControlStripMargin, viewport.ViewportHeight)
                == OriginalStrategicInteractiveEncounterControlStripRoute.RetreatConfirmation)
        {
            _strategicInteractiveRetreatConfirmation = new(x, y);
            _notice = "RETREAT? ENTER CONFIRMS, ESC CANCELS";
            return;
        }
        AdvanceInteractiveStrategicEncounterFrame(encounter, patrolEncounter, session, viewport, x, y, inputCode);
    }

    private void CaptureStrategicPointerInput(MouseState mouse, bool click, bool release,
        bool controllerRightClick, int x, int y)
    {
        var timerUnits = OriginalStrategicPointerClock.UnitsAt(
            TimeSpan.FromSeconds(_presentationSeconds));
        _strategicPointerEvents.EnqueueTransitions(click, release,
            mouse.RightButton == ButtonState.Pressed && _lastMouse.RightButton == ButtonState.Released,
            mouse.RightButton == ButtonState.Released && _lastMouse.RightButton == ButtonState.Pressed,
            timerUnits, x, y);
        if (controllerRightClick)
            _strategicPointerEvents.EnqueueControllerDestination(timerUnits, x, y);
    }

    private void AdvanceInteractiveStrategicEncounterFrame(
        OriginalStrategicPlayerEnemyEncounter? encounter,
        OriginalStrategicTemporaryForceEncounter? patrolEncounter,
        OriginalStrategicInteractiveEncounterSession session,
        OriginalStrategicInteractiveEncounterViewport viewport,
        int localX,
        int localY,
        int inputCode,
        bool firstControlConfirmationAccepted = false)
    {
        var result = session.AdvanceMappedFrame(TimeSpan.FromSeconds(_presentationSeconds), inputCode,
            localX, localY, viewport, _strategicEncounterPlayerScoreModifier,
            new HostEncounterRandom(), firstControlConfirmationAccepted);
        _strategicEncounterHover = result.HoverPresentation;
        if (!result.ResolverEnded) return;

        if (patrolEncounter is not null)
        {
            var patrolSettlement = _campaign.ResolveInteractiveOriginalStrategicPatrolEncounter(
                patrolEncounter, session);
            FinishStrategicEncounter(patrolSettlement.PatrolCleared ? "PATROL DEFEATED" : "STRATEGIC DEFEAT");
            return;
        }
        var settlement = _campaign.ResolveInteractiveOriginalStrategicEncounter(encounter!, session);
        FinishStrategicEncounter(settlement.Outcome switch
        {
            OriginalStrategicInteractiveEncounterOutcome.EnemyDefeated => "STRATEGIC VICTORY",
            OriginalStrategicInteractiveEncounterOutcome.PlayerWithdrew => "STRATEGIC WITHDRAWAL",
            _ => "STRATEGIC DEFEAT",
        });
    }

    private OriginalStrategicEncounterMenuEntry? EncounterMenuEntryForInput(
        Func<Keys, bool> press, MouseState mouse, bool click)
    {
        var index = press(Keys.D1) ? 0 : press(Keys.D2) ? 1 : press(Keys.D3) ? 2 : press(Keys.D4) ? 3 : -1;
        if (index >= 0) return OriginalStrategicEncounterMenu.Entries[index];
        if (!click) return null;
        var (x, y) = OriginalPoint(mouse);
        return OriginalStrategicEncounterMenu.FindMappedEntryAt(x, y);
    }

    private void FinishStrategicEncounter(string notice)
    {
        _strategicEncounter = null;
        _strategicPatrolEncounter = null;
        _strategicInteractiveEncounter = null;
        _strategicEncounterViewport = null;
        _strategicEncounterHover = default;
        _strategicEncounterPlayerScoreModifier = 0;
        _strategicInteractiveRetreatConfirmation = null;
        _screen = Screen.Map;
        _notice = notice;
        Autosave();
    }

    private void DrawStrategicEncounter()
    {
        if (!DrawOriginal(OriginalStrategicInteractiveEncounterPresentation.BackgroundArtRole,
                new Rectangle(0, 0, 1024, 728)))
            throw new InvalidOperationException("Strategic encounter requires its verified original battlefield art.");
        if (_strategicInteractiveEncounter is null)
        {
            DrawStrategicEncounterMenu();
            return;
        }

        var session = _strategicInteractiveEncounter;
        var viewport = _strategicEncounterViewport
            ?? throw new InvalidOperationException("Strategic encounter viewport is unavailable.");
        if (!_originalAnimations.TryGetValue(OriginalStrategicInteractiveEncounterPresentation.UnitAnimationRole,
                out var animation))
            throw new InvalidOperationException("Strategic encounter requires its verified original unit animation.");
        if (animation.Frames.Count <= OriginalStrategicInteractiveEncounterPresentation.ControlStripFrame)
            throw new InvalidOperationException("Strategic encounter unit animation is missing its required control frames.");

        foreach (var draw in OriginalStrategicInteractiveEncounterPresentation.UnitDrawsFor(
                     session.Units, viewport.HorizontalOffset, viewport.VerticalOffset))
            _batch.Draw(animation.Frames[draw.Frame], ScaleBounds(new UiBounds(
                draw.X, draw.Y, animation.Frames[draw.Frame].Width, animation.Frames[draw.Frame].Height)), Color.White);
        foreach (var draw in OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayDrawsFor(
                     session.Units, session.SelectedUnitIndices, viewport.HorizontalOffset, viewport.VerticalOffset))
            _batch.Draw(animation.Frames[draw.Frame], ScaleBounds(new UiBounds(
                draw.X, draw.Y, animation.Frames[draw.Frame].Width, animation.Frames[draw.Frame].Height)), Color.White);

        var controlFrame = session.IsMappedTacticalAdvancementEnabled
            ? OriginalStrategicInteractiveEncounterPresentation.PendingFirstControlFrame
            : OriginalStrategicInteractiveEncounterPresentation.ControlStripFrame;
        var (controlX, controlY) = session.IsMappedTacticalAdvancementEnabled
            ? OriginalStrategicInteractiveEncounterPresentation.PendingFirstControlDrawPositionFor(
                viewport.ControlStripMargin, viewport.ViewportHeight)
            : OriginalStrategicInteractiveEncounterPresentation.ControlStripDrawPositionFor(
                viewport.ControlStripMargin, viewport.ViewportHeight);
        _batch.Draw(animation.Frames[controlFrame], ScaleBounds(new UiBounds(controlX, controlY,
            animation.Frames[controlFrame].Width, animation.Frames[controlFrame].Height)), Color.White);

        DrawText("STRATEGIC ENCOUNTER", 32, 18, Color.Gold, 2);
        DrawText(session.IsMappedTacticalAdvancementEnabled
            ? "TACTICAL ORDERS ACTIVE" : "CLICK THE FIRST CONTROL TO BEGIN", 32, 46, Color.White, 2);
        if (_strategicInteractiveRetreatConfirmation is not null)
            DrawText("RETREAT? ENTER CONFIRMS, ESC CANCELS", 32, 74, Color.Gold, 2);
        else
            DrawText("LEFT SELECT   RIGHT ORDER", 32, 74, Color.White, 2);
        DrawStrategicEncounterHover(viewport);
    }

    private void DrawStrategicEncounterHover(OriginalStrategicInteractiveEncounterViewport viewport)
    {
        var text = _strategicEncounterHover.Text;
        if (text.Length == 0) return;
        var panel = OriginalStrategicInteractiveEncounterPresentation.HoverStatusPanelBoundsFor(
            viewport.ControlStripMargin, viewport.ViewportHeight);
        var bounds = ScaleBounds(new UiBounds(panel.X, panel.Y, panel.Width, panel.Height));
        DrawText(text, bounds.X, bounds.Y, Color.White, 1, bounds.Width);
    }

    private void DrawStrategicEncounterMenu()
    {
        DrawText("STRATEGIC ENCOUNTER", 32, 18, Color.Gold, 2);
        DrawText("CHOOSE A MAPPED DEPLOYMENT", 32, 46, Color.White, 2);
        for (var index = 0; index < OriginalStrategicEncounterMenu.InteractiveSelectionCount; index++)
        {
            var entry = OriginalStrategicEncounterMenu.Entries[index];
            var bounds = ScaleBounds(new UiBounds(entry.X, entry.Y, entry.Width, entry.Height));
            DrawOutline(bounds, Color.Gold, 2);
            DrawText($"{index + 1}  OPTION {index + 1}", bounds.X + 12, bounds.Y + 12, Color.White, 2);
        }
        var fallback = OriginalStrategicEncounterMenu.Entries[OriginalStrategicEncounterMenu.ExitRegionIndex];
        var fallbackBounds = ScaleBounds(new UiBounds(fallback.X, fallback.Y, fallback.Width, fallback.Height));
        DrawOutline(fallbackBounds, Color.Wheat, 2);
        DrawText("AUTOMATIC", fallbackBounds.X + 14, fallbackBounds.Y + 12, Color.White, 2);
    }

    private sealed class HostEncounterRandom : IOriginalStrategicEncounterRandom
    {
        public int NextRaw() => Random.Shared.Next(int.MaxValue);
    }

    private readonly record struct OriginalStrategicInteractiveRetreatConfirmation(int LocalX, int LocalY);
}
