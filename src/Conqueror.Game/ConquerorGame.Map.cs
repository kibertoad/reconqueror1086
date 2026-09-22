using Conqueror.Core;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void UpdateMap(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        UpdateOriginalStrategicMapRuntime();
        if (UpdateOriginalStrategicMapInput(mouse, click)) return;
        if (_campaign.State.PendingDrogoEncounter)
        {
            _screen = Screen.DrogoDemand;
            return;
        }
        if (_campaign.HasPendingFieldBattle)
        {
            BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED");
            return;
        }
        if (press(Keys.Left) || press(Keys.Up)) SelectMapLocation(-1);
        if (press(Keys.Right) || press(Keys.Down)) SelectMapLocation(1);
        for (var armyIndex = 0; armyIndex < Player.ArmyDivisionLimit; armyIndex++)
            if (press(Keys.D1 + armyIndex)) _warPlanningArmyIndex = armyIndex;
        if (press(Keys.Enter)) TravelToSelectedLocation();
        if (press(Keys.H) && _campaign.State.CurrentLocation == 0) _screen = Screen.Home;
        if (press(Keys.V)) _screen = Screen.Village;
        if (press(Keys.T) && _campaign.IsTournamentHere) _screen = Screen.Tournament;
        if (press(Keys.O)) EnterOverview(Screen.Map);
        if (press(Keys.S) && _campaign.StartSiege(_selectedLocation))
        {
            var importedScene = ImportedSiegeLayouts.ForCampaignLocation(_importedContent, _selectedLocation);
            if (importedScene is null)
                throw new InvalidOperationException("A campaign siege has no mapped original scene.");
            ActivateSiege(importedScene, _campaign.CreateSiege(importedScene.Layout));
            _showRadar = true;
            _screen = Screen.Siege;
        }
        if (press(Keys.A))
        {
            var armyName = _campaign.State.Player.ArmyNameAt(_warPlanningArmyIndex);
            _notice = _campaign.DispatchArmy(_warPlanningArmyIndex, _selectedLocation)
                ? $"{armyName.ToUpperInvariant()} MARCHES TO {World.Locations[_selectedLocation].Name.ToUpperInvariant()}"
                : $"{armyName.ToUpperInvariant()} CANNOT TAKE THAT ORDER";
        }
        if (press(Keys.B))
        {
            if (_campaign.JoinedArmyTotalHere == 0) _notice = "NO JOINED ARMY IS PRESENT";
            else if (_campaign.CanStartFieldBattle) BeginFieldBattle("YOU CHALLENGE THE GARRISON");
            else _notice = "THERE IS NO HOSTILE FIELD ARMY HERE";
        }
        if (press(Keys.P)) _notice = _campaign.SendSpy(_selectedLocation)
            ? $"SPY REPORTS {_campaign.GarrisonAt(_selectedLocation)} SOLDIERS AT {World.Locations[_selectedLocation].Name}"
            : "SPY NOT SENT (NEED HOSTILE CASTLE AND 80S)";
        if (press(Keys.C)) _notice = "TO CLAIM THE CROWN, TRAVEL TO LONDON AND PRESS S TO BESIEGE IT";
        if (press(Keys.D)) BeginDragonChallenge();
        if (press(Keys.E))
        {
            var previousReport = _campaign.State.LatestSpyReport;
            _campaign.AdvanceDays(_campaign.State.DaySpeed);
            ShowNewSpyReport(previousReport);
            Autosave();
        }
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.DaySpeed = Math.Min(15, _campaign.State.DaySpeed + 1);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.DaySpeed = Math.Max(1, _campaign.State.DaySpeed - 1);
        if (!click || !_originalArt.ContainsKey("Estate.Shell")) return;

        var (x, y) = OriginalPoint(mouse);
        if (_estateLayout.InsetMap.Contains(x, y))
        {
            var selected = EstatePresentationDefinitions.LocationAt(_estateLayout.InsetMap, x, y);
            if (_campaign.CanRevealLocation(selected)) _selectedLocation = selected;
            else _notice = "THE DRAGON'S LAIR HAS NOT YET BEEN DISCOVERED";
            _estatePanel = EstatePanel.Map;
            return;
        }
        var control = _estateLayout.Controls.FirstOrDefault(item => item.Bounds.Contains(x, y));
        if (control is not null) ActivateEstateControl(control.Action);
    }

    /// <summary>
    /// Advances the source-shaped strategic state at the same explicit fixed
    /// cadence used by the MonoGame host. This replaces the original map
    /// loop's processor-rate scheduler without tying simulation work to
    /// drawing or pointer frequency.
    /// </summary>
    private void UpdateOriginalStrategicMapRuntime()
    {
        var previousReport = _campaign.State.LatestSpyReport;
        var pass = OriginalStrategicHostRuntime.AdvanceFixedPass(_campaign,
            playerEncounterHandoffActive: _strategicEncounter is not null);
        if (pass?.SpyReport is not null) ShowNewSpyReport(previousReport);
        if (pass?.Encounters.Count > 0)
            BeginStrategicEncounter(pass.Encounters[0]);
    }

    /// <summary>
    /// Keeps the executable-mapped map camera and pointer path ahead of the
    /// dated destination adapter. MonoGame's fixed update is the explicit,
    /// processor-independent cadence for edge scrolling; the original's
    /// unrestricted main-loop frequency is not reproduced.
    /// </summary>
    private bool UpdateOriginalStrategicMapInput(MouseState mouse, bool click)
    {
        if (_campaign.State.OriginalStrategicState is not { } strategic) return false;

        var (x, y) = OriginalPoint(mouse);
        OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, x, y);
        if (!click || x < OriginalStrategicMapTerrainRendering.ViewportLeft
            || x > OriginalStrategicMapTerrainRendering.ViewportRight
            || y < OriginalStrategicMapTerrainRendering.ViewportTop
            || y > OriginalStrategicMapTerrainRendering.ViewportBottom)
            return false;

        var noDivisionTargets = Enumerable.Repeat(
            new OriginalStrategicPlayerTarget(false, 0, 0),
            OriginalStrategicMovement.PlayerDivisionTargetCount).ToArray();
        var result = OriginalStrategicMapCommands.DispatchRawPointer(
            strategic, _originalStrategicResources, noDivisionTargets, x, y,
            targetConfirmed: false);
        if (result.Hit.EnemySlot is not null || result.Hit.DivisionSlot is not null)
            _notice = "TARGET CONFIRMATION IS NOT YET AVAILABLE";
        else if (result.Command.RouteLimitReached)
            _notice = "ROUTE LIMIT REACHED";
        return true;
    }

    private void SelectMapLocation(int direction)
    {
        do _selectedLocation = (_selectedLocation + World.Locations.Length + direction) % World.Locations.Length;
        while (!_campaign.CanRevealLocation(_selectedLocation));
    }

    private void TravelToSelectedLocation()
    {
        if (!_campaign.CanTravelTo(_selectedLocation))
        {
            _notice = "THE DRAGON'S LAIR HAS NOT YET BEEN DISCOVERED";
            return;
        }
        var previousReport = _campaign.State.LatestSpyReport;
        var days = _campaign.TravelTo(_selectedLocation);
        _notice = days == 0 ? $"ALREADY AT {World.Locations[_selectedLocation].Name}" : $"TRAVELLED {days} DAYS TO {World.Locations[_selectedLocation].Name}";
        ShowNewSpyReport(previousReport);
        if (days > 0) Autosave();
        if (_campaign.HasPendingFieldBattle) { BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED"); return; }
        if (days > 0 && World.Locations[_selectedLocation].Kind == LocationKind.DragonLair)
        {
            _dragonBattle = _campaign.BeginDragonBattle();
            if (_dragonBattle is null)
            {
                _notice = "YOU REACH THE LAIR, BUT CANNOT CHALLENGE THE DRAGON WITHOUT MIGHTY STRENGTH, ARMOR, SHIELD, AND LANCE";
                PlayEventMovie("Travel.DragonLair", Screen.Map);
                return;
            }

            _notice = _dragonBattle.LastMessage.ToUpperInvariant();
            _screen = Screen.DragonBattle;
            PlayEventMovie("Travel.DragonLair", Screen.DragonBattle);
        }
    }

    private void ShowNewSpyReport(StrategicSpyReport? previousReport)
    {
        var report = _campaign.State.LatestSpyReport;
        if (report is null || report == previousReport) return;
        var location = report.LocationName ?? World.Locations[report.Location].Name;
        _notice = $"SPY REPORT—{location.ToUpperInvariant()}: " +
            $"{report.Swordsmen} SWORDSMEN, {report.Halberdiers} HALBERDIERS, {report.Knights} KNIGHTS MOVING";
    }

    private void BeginDragonChallenge()
    {
        _dragonBattle = _campaign.BeginDragonBattle();
        if (_dragonBattle is not null) { _screen = Screen.DragonBattle; _notice = _dragonBattle.LastMessage.ToUpperInvariant(); }
        else _notice = "DRAGON CHALLENGE REQUIRES ITS LOCATION, MIGHTY STRENGTH, ARMOR, SHIELD, AND LANCE";
    }
}
