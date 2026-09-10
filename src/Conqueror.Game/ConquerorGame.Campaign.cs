using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void UpdateHome(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.F)) EnterFiefManagement(FarmPresentationDefinitions.Section.Farm);
        if (press(Keys.V)) EnterFiefManagement(FarmPresentationDefinitions.Section.Village);
        if (press(Keys.Enter) || press(Keys.H)) _screen = Screen.Map;
        if (click) ActivateSceneHotspot(HitSceneHotspot(_homeHotspots, mouse));
    }

    private void UpdateWarPlanning(Func<Keys, bool> press, MouseState mouse, bool click, bool rightClick)
    {
        var player = _campaign.State.Player;
        if (_editingWarPlanningArmyName)
        {
            var name = player.ArmyNameAt(_warPlanningArmyIndex);
            if (press(Keys.Back) && name.Length > 0) player.SetArmyName(_warPlanningArmyIndex, name[..^1]);
            if (press(Keys.Space) && name.Length < 16 && !name.EndsWith(' ')) player.SetArmyName(_warPlanningArmyIndex, name + " ");
            for (var value = (int)Keys.A; value <= (int)Keys.Z && player.ArmyNameAt(_warPlanningArmyIndex).Length < 16; value++)
                if (press((Keys)value)) player.SetArmyName(_warPlanningArmyIndex,
                    player.ArmyNameAt(_warPlanningArmyIndex) + (char)('A' + value - (int)Keys.A));
            if (press(Keys.Enter)) { _editingWarPlanningArmyName = false; return; }
        }
        else if (press(Keys.Enter)) { CommitWarPlanning(); return; }
        if (press(Keys.Up)) _warPlanningArmyIndex = Math.Max(0, _warPlanningArmyIndex - 1);
        if (press(Keys.Down)) _warPlanningArmyIndex = Math.Min(WarPlanningPresentationDefinitions.ArmyCount - 1, _warPlanningArmyIndex + 1);
        if (!click && !rightClick) return;

        var (x, y) = OriginalPoint(mouse);
        if (click && _warPlanningLayout.ArmyName.Contains(x, y))
        {
            _editingWarPlanningArmyName = true;
            return;
        }
        _editingWarPlanningArmyName = false;
        if (click && _warPlanningLayout.Okay.Contains(x, y)) { CommitWarPlanning(); return; }
        if (click && _warPlanningLayout.Cancel.Contains(x, y)) { CancelWarPlanning(); return; }
        var army = Enumerable.Range(0, _warPlanningLayout.ArmyButtons.Count)
            .FirstOrDefault(index => _warPlanningLayout.ArmyButtons[index].Contains(x, y), -1);
        if (army >= 0)
        {
            _warPlanningArmyIndex = army;
            return;
        }
        var unitRow = Enumerable.Range(0, _warPlanningLayout.UnitRows.Count)
            .FirstOrDefault(index => _warPlanningLayout.UnitRows[index].Contains(x, y), -1);
        if (unitRow >= 0)
        {
            var unit = Enum.GetValues<UnitType>()[unitRow];
            _notice = _campaign.AdjustArmyCompany(_warPlanningArmyIndex, unit, rightClick ? -1 : 1)
                ? $"{_campaign.State.Player.ArmyNameAt(_warPlanningArmyIndex).ToUpperInvariant()}: {unit.ToString().ToUpperInvariant()} UPDATED"
                : "THAT COMPANY CHANGE IS NOT AVAILABLE";
            return;
        }
        if (click && _warPlanningLayout.FieldArmy.Contains(x, y))
        {
            _notice = _campaign.FieldArmy(_warPlanningArmyIndex)
                ? $"{_campaign.State.Player.ArmyNameAt(_warPlanningArmyIndex).ToUpperInvariant()} FIELDED"
                : "THAT ARMY CANNOT BE FIELDED";
            return;
        }
        if (click && _warPlanningLayout.SendSpy.Contains(x, y))
        {
            _notice = _campaign.AssignSpy() ? "SPY ASSIGNED" : "YOU CANNOT ASSIGN ANOTHER SPY";
            return;
        }
        if (click && _warPlanningLayout.Membership.Contains(x, y))
            _notice = _campaign.ToggleArmyMembership(_warPlanningArmyIndex)
                ? "ARMY MEMBERSHIP UPDATED"
                : "YOU CANNOT JOIN THAT ARMY";
    }

    private void UpdateFarm(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        var layout = _fiefLayouts[_fiefSection];
        var entries = FarmPresentationDefinitions.EntriesFor(_fiefSection);
        var maximumOffset = Math.Max(0, entries.Count - layout.Rows.Count);
        if (press(Keys.Up)) _fiefRowOffset = Math.Max(0, _fiefRowOffset - 1);
        if (press(Keys.Down)) _fiefRowOffset = Math.Min(maximumOffset, _fiefRowOffset + 1);
        var command = FarmPresentationDefinitions.CommandsFor(_fiefSection).FirstOrDefault(item => press(item.Key));
        if (command is not null) ActivateFarmAction(command.Action);
        if (!click) return;
        var (x, y) = OriginalPoint(mouse);
        var row = Enumerable.Range(0, layout.Rows.Count).FirstOrDefault(index => layout.Rows[index].Contains(x, y), -1);
        if (row >= 0 && row + _fiefRowOffset < entries.Count && entries[row + _fiefRowOffset].Action is { } action)
        {
            ActivateFarmAction(action);
            return;
        }
        switch (FarmPresentationDefinitions.FooterActionAt(layout, x, y))
        {
            case FarmPresentationDefinitions.FooterAction.Okay: CommitFiefManagement(); break;
            case FarmPresentationDefinitions.FooterAction.Cancel: CancelFiefManagement(); break;
            case FarmPresentationDefinitions.FooterAction.FullScreen: _graphics.ToggleFullScreen(); break;
        }
    }

    private void EnterFiefManagement(FarmPresentationDefinitions.Section section)
    {
        _fiefSection = section;
        _fiefRowOffset = 0;
        _fiefCheckpoint = FiefManagementCheckpoint.Capture(_campaign.State);
        _screen = Screen.Farm;
    }

    private void CommitFiefManagement()
    {
        _fiefCheckpoint = null;
        _screen = Screen.Home;
    }

    private void CancelFiefManagement()
    {
        _fiefCheckpoint?.Restore(_campaign.State);
        _fiefCheckpoint = null;
        _screen = Screen.Home;
    }

    private void ActivateFarmAction(FarmAction action)
    {
        switch (action)
        {
            case BuildFarmAction build: _campaign.Build(build.Building); break;
            case PlantFarmAction plant: _campaign.Plant(plant.Crop); break;
            case DevelopForestFarmAction forest: _campaign.DevelopForest(forest.Industry); break;
            case RecruitFarmAction recruit: _campaign.Recruit(recruit.Unit); break;
            case LeaveFarmAction: CommitFiefManagement(); break;
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void UpdateVillage(Func<Keys, bool> press)
    {
        if (press(Keys.B)) _notice = _campaign.Borrow(200) ? "BORROWED 200S AT 50% INTEREST" : "LOAN REFUSED";
        if (press(Keys.D)) _campaign.Donate();
        if (press(Keys.C)) _campaign.Build("Church");
        if (press(Keys.A)) _notice = _campaign.BuyEquipment("Spiked Mace") ? "BOUGHT AND EQUIPPED SPIKED MACE" : "PURCHASE REFUSED";
        if (press(Keys.N)) _notice = _campaign.BuyEquipment("Norman Shield") ? "BOUGHT AND EQUIPPED NORMAN SHIELD" : "PURCHASE REFUSED";
        if (press(Keys.W)) _notice = _campaign.BuyEquipment("War Helm") ? "BOUGHT AND EQUIPPED WAR HELM" : "PURCHASE REFUSED";
        if (press(Keys.K)) _screen = Screen.Blacksmith;
        if (press(Keys.I)) { _notice = ""; _screen = Screen.Inn; }
        if (press(Keys.P) && !StartChurchConversation()) _notice = "ORIGINAL PARISH CONVERSATION DATA IS NOT INSTALLED";
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.Player.Home.TaxRate = Math.Min(100, _campaign.State.Player.Home.TaxRate + 5);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.Player.Home.TaxRate = Math.Max(0, _campaign.State.Player.Home.TaxRate - 5);
        if (press(Keys.Enter) || press(Keys.V)) _screen = Screen.Map;
    }

    private void UpdateInn(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.I) || press(Keys.V) || press(Keys.Enter))
        {
            _screen = Screen.Village;
            return;
        }
        if (!click) return;
        var (x, y) = OriginalPoint(mouse);
        _innPatron = _innLayout.Patrons.FirstOrDefault(patron => patron.Bounds.Contains(x, y));
        if (_innPatron is not null)
        {
            _notice = "";
            StartConversation(_innPatron, Screen.Inn);
        }
        else if (_innLayout.ExitBounds.Contains(x, y))
            _screen = Screen.Village;
    }

    private void UpdateInnDialogue(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.I) || press(Keys.V))
        {
            _screen = _conversationReturnScreen;
            return;
        }
        var node = _conversationSession?.CurrentNode;
        if (node is null)
        {
            if (press(Keys.Enter)) _screen = _conversationReturnScreen;
            return;
        }
        if (node.Responses.Count == 0)
        {
            if (_presentationSeconds >= _conversationAdvanceAt || press(Keys.Enter)) ContinueConversation();
            return;
        }
        var selected = Enumerable.Range(0, node.Responses.Count)
            .FirstOrDefault(index => press(Keys.D1 + index), -1);
        if (selected < 0 && click)
        {
            var (x, y) = OriginalPoint(mouse);
            selected = Enumerable.Range(0, node.Responses.Count)
                .FirstOrDefault(index => ConversationPresentationDefinitions.ResponseBounds(index).Contains(x, y), -1);
        }
        if (selected >= 0) ChooseConversationResponse(selected);
    }

    private void ChooseConversationResponse(int index)
    {
        if (_conversationSession?.ChooseResponse(index, Random.Shared.Next) != true)
        {
            _screen = _conversationReturnScreen;
            return;
        }
        OriginalConversationBindings.SynchronizeTournamentState(_campaign.State);
        ScheduleAutomaticConversationAdvance();
    }

    private void ContinueConversation()
    {
        if (_conversationSession?.Continue(Random.Shared.Next) != true)
        {
            _screen = _conversationReturnScreen;
            return;
        }
        OriginalConversationBindings.SynchronizeTournamentState(_campaign.State);
        ScheduleAutomaticConversationAdvance();
    }

    private bool StartConversation(InnPatronHotspot speaker, Screen returnScreen)
    {
        if (_conversationSession?.Start(speaker.ConversationRootNodeId, Random.Shared.Next) != true) return false;
        _innPatron = speaker;
        _conversationReturnScreen = returnScreen;
        OriginalConversationBindings.SynchronizeTournamentState(_campaign.State);
        ScheduleAutomaticConversationAdvance();
        _screen = Screen.InnDialogue;
        return true;
    }

    private void ScheduleAutomaticConversationAdvance() => _conversationAdvanceAt =
        _conversationSession?.CurrentNode?.Responses.Count == 0 ? _presentationSeconds + 2 : double.PositiveInfinity;

    private void UpdateBlacksmith(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.Enter))
        {
            if (!StartBlacksmithConversation()) _screen = Screen.BlacksmithDialogue;
            return;
        }
        if (press(Keys.B))
        {
            _shopIndex = 0;
            _screen = Screen.Shop;
            return;
        }
        if (click) ActivateSceneHotspot(HitSceneHotspot(_blacksmithHotspots, mouse));
    }

    private void UpdateBlacksmithDialogue(Func<Keys, bool> press)
    {
        var command = BlacksmithDialoguePresentationDefinitions.Commands
            .FirstOrDefault(item => item.Keys.Any(press));
        if (command is null) return;
        switch (command.Action)
        {
            case BlacksmithDialogueAction.Shop: _shopIndex = 0; _screen = Screen.Shop; break;
            case BlacksmithDialogueAction.Return: _screen = Screen.Blacksmith; break;
            default: throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    private bool StartBlacksmithConversation()
    {
        var speaker = new InnPatronHotspot(-1, BlacksmithDialoguePresentationDefinitions.Speaker,
            "Blacksmith.Portrait", ":blacksmi.pcc",
            BlacksmithDialoguePresentationDefinitions.OriginalConversationRootNodeId, new UiBounds(0, 0, 0, 0));
        return StartConversation(speaker, Screen.Blacksmith);
    }

    private bool StartChurchConversation()
    {
        var speaker = new InnPatronHotspot(-1, ChurchConversationPresentationDefinitions.Speaker,
            "", ChurchConversationPresentationDefinitions.PortraitSuffix,
            ChurchConversationPresentationDefinitions.RootNodeIdFor(_campaign.State.CurrentLocation),
            new UiBounds(0, 0, 0, 0));
        return StartConversation(speaker, Screen.Village);
    }

    private SceneHotspot? HitSceneHotspot(IReadOnlyList<SceneHotspot> hotspots, MouseState mouse)
    {
        var (x, y) = OriginalPoint(mouse);
        return hotspots.FirstOrDefault(hotspot => hotspot.Bounds.Contains(x, y));
    }

    private void ActivateSceneHotspot(SceneHotspot? hotspot)
    {
        if (hotspot is null) return;
        switch (hotspot.Action)
        {
            case SceneNavigationAction.Overview: EnterOverview(Screen.Home); break;
            case SceneNavigationAction.Castle: EnterFiefManagement(FarmPresentationDefinitions.Section.Castle); break;
            case SceneNavigationAction.Farm: EnterFiefManagement(FarmPresentationDefinitions.Section.Farm); break;
            case SceneNavigationAction.Village: EnterFiefManagement(FarmPresentationDefinitions.Section.Village); break;
            case SceneNavigationAction.Forest: EnterFiefManagement(FarmPresentationDefinitions.Section.Forest); break;
            case SceneNavigationAction.WarPlanning: EnterWarPlanning(); break;
            case SceneNavigationAction.Exit: _screen = Screen.Map; break;
            case SceneNavigationAction.Jump: _notice = "JUMP ACTION REQUIRES EXECUTABLE CONFIRMATION"; break;
            case SceneNavigationAction.Map: _estatePanel = EstatePanel.Map; _screen = Screen.Map; break;
            case SceneNavigationAction.Orders: _estatePanel = EstatePanel.Orders; _screen = Screen.Map; break;
            case SceneNavigationAction.BlacksmithDialogue:
                if (!StartBlacksmithConversation()) _screen = Screen.BlacksmithDialogue;
                break;
            case SceneNavigationAction.Shop: _shopIndex = 0; _screen = Screen.Shop; break;
            default: throw new ArgumentOutOfRangeException(nameof(hotspot));
        }
    }

    private void EnterOverview(Screen returnScreen)
    {
        _overviewReturnScreen = returnScreen;
        _screen = Screen.Overview;
    }

    private void EnterWarPlanning()
    {
        _campaign.State.Player.EnsureArmyRoster();
        _warPlanningCheckpoint = new WarPlanningCheckpoint(_campaign);
        _warPlanningArmyIndex = 0;
        _editingWarPlanningArmyName = false;
        _screen = Screen.WarPlanning;
    }

    private void CommitWarPlanning()
    {
        var player = _campaign.State.Player;
        for (var index = 0; index < Player.ArmyDivisionLimit; index++)
        {
            var name = player.ArmyNameAt(index).Trim();
            player.SetArmyName(index, name.Length == 0 ? $"Army {index + 1}" : name);
        }
        _editingWarPlanningArmyName = false;
        _warPlanningCheckpoint = null;
        _screen = Screen.Home;
        _notice = "WAR PLANS CONFIRMED";
    }

    private void CancelWarPlanning()
    {
        _warPlanningCheckpoint?.Restore(_campaign);
        _editingWarPlanningArmyName = false;
        _warPlanningCheckpoint = null;
        _screen = Screen.Home;
        _notice = "WAR PLANS CANCELLED";
    }

    private void UpdateShop(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        var stock = Balance.StoreEquipment;
        if (press(Keys.Up) || press(Keys.Left)) _shopIndex = (_shopIndex + stock.Length - 1) % stock.Length;
        if (press(Keys.Down) || press(Keys.Right)) _shopIndex = (_shopIndex + 1) % stock.Length;
        var item = stock[_shopIndex];
        if (press(Keys.B)) _notice = _campaign.BuyEquipment(item.Name) ? $"BOUGHT {item.Name}" : "PURCHASE REFUSED";
        if (press(Keys.S)) _notice = _campaign.SellEquipment(item.Name) ? $"SOLD {item.Name}" : "YOU DO NOT OWN THAT ITEM";
        if (press(Keys.Enter)) _screen = Screen.Blacksmith;
        if (!click) return;

        var (x, y) = OriginalPoint(mouse);
        var control = ShopPresentationDefinitions.Controls.FirstOrDefault(control => control.Bounds.Contains(x, y));
        if (control is not null) ActivateShopControl(control.Action, stock);
    }

    private void ActivateShopControl(ShopControlAction action, EquipmentBalance[] stock)
    {
        switch (action)
        {
            case ShopControlAction.Previous: _shopIndex = (_shopIndex + stock.Length - 1) % stock.Length; break;
            case ShopControlAction.Next: _shopIndex = (_shopIndex + 1) % stock.Length; break;
            case ShopControlAction.View: ViewShopItem(stock[_shopIndex]); break;
            case ShopControlAction.Transaction: TransactShopItem(stock[_shopIndex]); break;
            case ShopControlAction.Exit: _screen = Screen.Blacksmith; break;
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
    }

    private void ViewShopItem(EquipmentBalance item)
    {
        var imported = ImportedStoreEntry(item);
        if (imported?.HasMovie == true) PlayEventMovie('/' + imported.MovieFile, Screen.Shop);
        else _notice = "NO ITEM VIEW IS AVAILABLE";
    }

    private void TransactShopItem(EquipmentBalance item)
    {
        if (_campaign.State.Player.Inventory.Items.Contains(item.Name))
            _notice = _campaign.SellEquipment(item.Name) ? $"SOLD {item.Name}" : "SALE REFUSED";
        else
            _notice = _campaign.BuyEquipment(item.Name) ? $"BOUGHT {item.Name}" : "PURCHASE REFUSED";
    }

    private void UpdateTournament(Func<Keys, bool> press)
    {
        _joustCursor = (_joustCursor + 2) % 200;
        if (press(Keys.Up)) _ladyIndex = (_ladyIndex + Balance.Courtships.Length - 1) % Balance.Courtships.Length;
        if (press(Keys.Down)) _ladyIndex = (_ladyIndex + 1) % Balance.Courtships.Length;
        if (press(Keys.Left)) _tournamentOpponent = (_tournamentOpponent + Balance.TournamentOpponents.Length - 1) % Balance.TournamentOpponents.Length;
        if (press(Keys.Right)) _tournamentOpponent = (_tournamentOpponent + 1) % Balance.TournamentOpponents.Length;
        if (press(Keys.C))
        {
            var lady = Balance.Courtships[_ladyIndex].Name;
            var accepted = _campaign.RequestColors(lady);
            if (accepted) OriginalConversationBindings.RecordLadyColors(_campaign.State, lady);
            _notice = accepted ? $"WEARING {lady}'S COLORS" : "YOUR REQUEST IS REFUSED";
        }
        if (press(Keys.I)) StartTournamentConversation();
        if (press(Keys.Space))
        {
            var lady = _campaign.State.Player.LadyColors;
            var before = _campaign.State.JoustsThisTournament;
            var won = _campaign.Joust(Math.Abs(100 - _joustCursor), _tournamentOpponent);
            if (_campaign.State.JoustsThisTournament > before)
                OriginalConversationBindings.RecordJoustResult(_campaign.State, lady, won);
            _notice = won ? "A CLEAN STRIKE - YOU WIN THE WAGER" : "YOU MISS, CANNOT PAY, OR THE LISTS ARE CLOSED";
        }
        if (press(Keys.K)) _notice = _campaign.TournamentSkirmish(_tournamentOpponent) is { } result ? result.Summary : "THE SKIRMISH IS UNAVAILABLE";
        if (press(Keys.Enter) || press(Keys.T)) _screen = Screen.Map;
    }

    private void StartTournamentConversation()
    {
        var definition = TournamentConversationDefinitions.For(Balance.Courtships[_ladyIndex].Name);
        var speaker = new InnPatronHotspot(-1, definition.Lady, "", definition.PortraitSuffix,
            definition.RootNodeId, new UiBounds(0, 0, 0, 0));
        if (!StartConversation(speaker, Screen.Tournament))
            _notice = "ORIGINAL TOURNAMENT CONVERSATION DATA IS NOT INSTALLED";
    }

    private void UpdateSiege(Func<Keys, bool> press, GameTime gameTime)
    {
        if (_siege is null)
        {
            _screen = _activePracticeCombat is null ? Screen.Map : Screen.Practice;
            _activePracticeCombat = null;
            return;
        }
        AdvanceSiegeForeground(gameTime.ElapsedGameTime.TotalSeconds);
        var healthBeforeInput = _siege.Health;
        _siege.AdvanceDoorAnimations(_animationEnabled ? gameTime.ElapsedGameTime.TotalSeconds : SiegeSession.DoorOpeningSeconds);
        _siege.AdvanceEnemyAnimations(_animationEnabled ? gameTime.ElapsedGameTime.TotalSeconds : 1);
        if (press(Keys.W)) _siege.Move(true); if (press(Keys.S)) _siege.Move(false);
        if (press(Keys.A)) _siege.TurnLeft(); if (press(Keys.D)) _siege.TurnRight();
        if (press(Keys.E)) _siege.Interact();
        if (press(Keys.Space))
        {
            var enemyHealthBeforeAttack = LivingSiegeEnemyHealth();
            var attackFrames = SiegeCombatPresentation.AttackFramesFor(_campaign.State.Player.Inventory.Weapon);
            _siege.Attack();
            StartSiegeWeapon(attackFrames);
            if (LivingSiegeEnemyHealth() < enemyHealthBeforeAttack) StartSiegeImpact();
        }
        if (press(Keys.X))
        {
            var enemyHealthBeforeShot = LivingSiegeEnemyHealth();
            if (_siege.Shoot() == SiegeAction.Shot)
            {
                StartSiegeWeapon(SiegeCombatPresentation.CrossbowAttack);
                if (LivingSiegeEnemyHealth() < enemyHealthBeforeShot) StartSiegeImpact();
            }
        }
        if (_siege.Health < healthBeforeInput) StartSiegeBlood();
        if (press(Keys.M)) _showRadar = !_showRadar;
        _notice = _siege.LastMessage;
        if (press(Keys.R))
        {
            if (_drogoCombat) { _notice = "DROGO WILL NOT LET YOU ESCAPE"; return; }
            if (_activePracticeCombat is not null) { FinishPracticeCombat("PRACTICE ENDED"); return; }
            _campaign.FinishSiege(_siege); var lost = _campaign.Retreat(); _siege = null; ClearSiegeVisuals(); _screen = Screen.Map; _notice = $"RETREATED - {lost} SOLDIERS LOST"; Autosave(); return;
        }
        if (_siege.Won)
        {
            if (_drogoCombat) FinishDrogoCombat();
            else if (_activePracticeCombat is not null) FinishPracticeCombat("PRACTICE WON");
            else
            {
                _campaign.FinishSiege(_siege);
                var crowned = _campaign.State.Victory == VictoryKind.Crown;
                _siege = null;
                ClearSiegeVisuals();
                _screen = crowned ? Screen.Ending : Screen.Map;
                _notice = crowned ? "YOU HAVE DEFEATED WILLIAM AND USURPED THE THRONE" : "THE CASTLE IS YOURS";
                Autosave();
                if (crowned) PlayEventMovie("Ending.CrownVictory", Screen.Ending);
            }
        }
        else if (_siege.Defeated)
        {
            if (_drogoCombat) FinishDrogoCombat();
            else if (_activePracticeCombat is not null) FinishPracticeCombat("PRACTICE LOST");
            else { _campaign.FinishSiege(_siege); _siege = null; ClearSiegeVisuals(); _screen = Screen.Map; _notice = "YOU ARE CARRIED FROM THE CASTLE"; Autosave(); }
        }
    }

    private void UpdateDrogoDemand(Func<Keys, bool> press)
    {
        if (!_campaign.State.PendingDrogoEncounter) { _screen = Screen.Map; return; }
        if (press(Keys.P))
        {
            _notice = _campaign.PayDrogo() ? "DROGO ACCEPTS THE DEBT PAYMENT" : "YOU CANNOT PAY WHAT YOU OWE";
            if (!_campaign.State.PendingDrogoEncounter) { _screen = Screen.Map; Autosave(); }
        }
        if (!press(Keys.F)) return;
        ClearSiegeVisuals();
        _siege = _campaign.CreateDrogoBattle();
        _drogoCombat = true;
        _showRadar = false;
        _notice = "DROGO PREPARES TO FIGHT";
        _screen = Screen.Siege;
    }

    private void FinishDrogoCombat()
    {
        if (_siege is null) return;
        var won = _siege.Won;
        _campaign.FinishDrogoBattle(_siege);
        _siege = null;
        _drogoCombat = false;
        ClearSiegeVisuals();
        _screen = won ? Screen.Map : Screen.Ending;
        _notice = won ? "DROGO IS DEAD - THE MONEYLENDER WILL NOT RETURN" : "DROGO HAS KILLED YOU";
        Autosave();
    }

    private void StartSiegeWeapon(SiegeFrameRun run)
    {
        _siegeWeaponFrame = _animationEnabled ? run.Start : run.EndExclusive - 1;
        _siegeWeaponEnd = run.EndExclusive;
        _siegeWeaponElapsed = 0;
    }

    private void StartSiegeBlood()
    {
        _siegeBloodFrame = _animationEnabled
            ? SiegeCombatPresentation.PlayerBlood.Start
            : SiegeCombatPresentation.PlayerBlood.EndExclusive - 1;
        _siegeBloodElapsed = 0;
    }

    private void StartSiegeImpact()
    {
        _siegeImpactFrame = _animationEnabled
            ? SiegeCombatPresentation.EnemyBlood.Start
            : SiegeCombatPresentation.EnemyBlood.EndExclusive - 1;
        _siegeImpactElapsed = 0;
    }

    private int LivingSiegeEnemyHealth() =>
        _siege?.Enemies.Where(enemy => enemy.Health > 0).Sum(enemy => enemy.Health) ?? 0;

    private void AdvanceSiegeForeground(double elapsedSeconds)
    {
        AdvanceSiegeRun(ref _siegeWeaponFrame, _siegeWeaponEnd, ref _siegeWeaponElapsed, elapsedSeconds);
        AdvanceSiegeRun(ref _siegeBloodFrame, SiegeCombatPresentation.PlayerBlood.EndExclusive,
            ref _siegeBloodElapsed, elapsedSeconds);
        AdvanceSiegeRun(ref _siegeImpactFrame, SiegeCombatPresentation.EnemyBlood.EndExclusive,
            ref _siegeImpactElapsed, elapsedSeconds);
    }

    private void AdvanceSiegeRun(ref int frame, int endExclusive, ref double elapsed, double elapsedSeconds)
    {
        if (frame < 0) return;
        if (!_animationEnabled)
        {
            frame = -1;
            elapsed = 0;
            return;
        }
        elapsed += elapsedSeconds;
        while (elapsed >= SiegeCombatPresentation.FrameSeconds && frame >= 0)
        {
            elapsed -= SiegeCombatPresentation.FrameSeconds;
            if (++frame >= endExclusive) frame = -1;
        }
    }

    private void UpdateFieldBattle(Func<Keys, bool> press, GameTime gameTime)
    {
        if (_fieldBattle is null)
        {
            _screen = _activePracticeCombat is null ? Screen.Map : Screen.Practice;
            _activePracticeCombat = null;
            return;
        }
        var unitTypes = Enum.GetValues<UnitType>();
        if (press(Keys.Left)) _selectedUnit = unitTypes[((int)_selectedUnit + unitTypes.Length - 1) % unitTypes.Length];
        if (press(Keys.Right)) _selectedUnit = unitTypes[((int)_selectedUnit + 1) % unitTypes.Length];
        if (press(Keys.D1)) _selectedUnit = UnitType.Swordsmen; if (press(Keys.D2)) _selectedUnit = UnitType.Halberdiers; if (press(Keys.D3)) _selectedUnit = UnitType.Knights;
        if (press(Keys.H)) _fieldBattle.Issue(_selectedUnit, UnitOrder.Hold);
        if (press(Keys.A)) _fieldBattle.Issue(_selectedUnit, UnitOrder.Advance);
        if (press(Keys.Q)) _fieldBattle.Issue(_selectedUnit, UnitOrder.FlankLeft);
        if (press(Keys.E)) _fieldBattle.Issue(_selectedUnit, UnitOrder.FlankRight);
        if (press(Keys.R)) _fieldBattle.Issue(_selectedUnit, UnitOrder.Withdraw);
        if (press(Keys.C)) _fieldBattle.IssueAll(UnitOrder.Captains);
        if (press(Keys.W)) _fieldBattle.IssueAll(UnitOrder.Withdraw);
        _battleTick += gameTime.ElapsedGameTime.TotalSeconds;
        if (_battleTick >= .45) { _battleTick = 0; _fieldBattle.Tick(); }
        _notice = _fieldBattle.LastMessage;
        if (_fieldBattle.Outcome != FieldBattleOutcome.InProgress)
        {
            if (_activePracticeCombat is not null)
            {
                var result = _fieldBattle.Outcome;
                FinishPracticeCombat($"WAR PRACTICE: {result}".ToUpperInvariant());
                return;
            }
            var outcome = _campaign.FinishFieldBattle(_fieldBattle); _fieldBattle = null; _screen = Screen.Map; _notice = $"FIELD BATTLE: {outcome}"; Autosave();
        }
    }

    private void FinishPracticeCombat(string notice)
    {
        _fieldBattle = null;
        _siege = null;
        ClearSiegeVisuals();
        _activePracticeCombat = null;
        _screen = Screen.Practice;
        _notice = notice;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_canvas);
        GraphicsDevice.Clear(new Color(20, 25, 20));
        _batch.Begin(samplerState: SamplerState.PointClamp);
        switch (_screen)
        {
            case Screen.Title: DrawTitle(); break; case Screen.Movie: DrawEventMovie(); break; case Screen.OptionsHub: DrawOptionsHub(); break; case Screen.Practice: DrawPractice(); break; case Screen.LoadGame: DrawLoadGame(); break; case Screen.CharacterOptions: DrawCharacterOptions(); break; case Screen.CharacterName: DrawCharacterName(); break; case Screen.Character: DrawCharacter(); break; case Screen.Dilemma: DrawDilemma(); break; case Screen.Briefing: DrawCampaignBriefing(); break; case Screen.Map: DrawMap(); break;
            case Screen.Home: DrawHome(); break; case Screen.WarPlanning: DrawWarPlanning(); break; case Screen.Farm: DrawFarm(); break; case Screen.Village: DrawVillage(); break; case Screen.Inn: DrawInn(); break; case Screen.InnDialogue: DrawInnDialogue(); break; case Screen.Blacksmith: DrawBlacksmith(); break; case Screen.BlacksmithDialogue: DrawBlacksmithDialogue(); break; case Screen.Shop: DrawShop(); break; case Screen.Tournament: DrawTournament(); break; case Screen.FieldBattle: DrawFieldBattle(); break;
            case Screen.DrogoDemand: DrawDrogoDemand(); break; case Screen.Siege: DrawSiege(); break; case Screen.Overview: DrawOverview(); break; case Screen.Ending: DrawEnding(); break;
            case Screen.DragonBattle: DrawDragonBattle(); break;
        }
        if (_screen is not Screen.Title and not Screen.Movie and not Screen.LoadGame
            and not Screen.Character and not Screen.Dilemma and not Screen.Briefing and not Screen.Inn and not Screen.InnDialogue)
            DrawText(_notice, 24, 730, Color.Gold, 2);
        if (_paused)
        {
            Fill(new Rectangle(0, 0, PresentationScaling.VirtualWidth, PresentationScaling.VirtualHeight),
                new Color(0, 0, 0, 170));
            DrawText("PAUSED", 405, 350, Color.White, 4);
            DrawText("PRESS PAUSE TO CONTINUE", 325, 405, Color.Wheat, 2);
        }
        if (_screen != Screen.Movie && !(_screen == Screen.Title && _titleMovie is { IsComplete: false }))
            DrawOriginalCursor();
        _batch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);
        _batch.Begin(samplerState: SamplerState.PointClamp);
        _batch.Draw(_canvas, CanvasDestination(), Color.White);
        _batch.End();
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _titleMovie?.Dispose();
            _eventMovie?.Dispose();
            _musicInstance?.Dispose();
            _importedMusic?.Dispose();
            DisposeOriginalSounds();
        }
        base.Dispose(disposing);
    }
}
