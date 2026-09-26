using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void LoadOriginalSounds()
    {
        foreach (var bankDefinitions in ImportedSounds.Definitions.GroupBy(
            sound => sound.IdSuffix, StringComparer.OrdinalIgnoreCase))
        {
            var bankId = _importedContent?.FindId("sound-bank", bankDefinitions.Key);
            if (bankId is null) continue;
            foreach (var definition in bankDefinitions)
            {
                if (_importedSoundLibrary?.Find(bankId, definition.SampleIndex) is not { } sample) continue;
                _originalSounds.Add(definition.Role, new SoundEffect(
                    sample.Pcm16LittleEndian, sample.SampleRate, AudioChannels.Mono));
            }
        }
    }

    private void LoadOriginalConversations()
    {
        var bodyId = _importedContent?.FindId("resource", ":all.cbf");
        var indexId = _importedContent?.FindId("resource", ":all.cif");
        if (bodyId is null || indexId is null
            || _importedContent?.DecodeConversations(bodyId, indexId) is not { } database) return;
        _conversationDatabase = database;
        var actionBodyId = _importedContent.FindId("resource", ":all.tmb");
        var actionIndexId = _importedContent.FindId("resource", ":all.tmi");
        var variableId = _importedContent.FindId("resource", ":all.vtb");
        _conversationActionTrees = actionBodyId is not null && actionIndexId is not null
            ? _importedContent.DecodeActionTrees(actionBodyId, actionIndexId) : null;
        _conversationInitialVariables = variableId is not null
            ? _importedContent.DecodeVariableTable(variableId)?.InitialValues ?? [] : [];
        ResetConversationSession();
        foreach (var portraitFile in database.Nodes.Values
            .Select(node => node.PortraitFile)
            .OfType<string>()
            .Where(file => Path.GetFileName(file) == file
                && (file.EndsWith(".PCC", StringComparison.OrdinalIgnoreCase)
                    || file.EndsWith(".PCX", StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var id = _importedContent.FindId("image", ":" + portraitFile);
            if (id is null || _importedContent.DecodePcx(id) is not { } image) continue;
            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
            texture.SetData(image.ToRgba());
            _conversationPortraits.Add(portraitFile, texture);
        }
    }

    private void ResetConversationSession()
    {
        if (_conversationDatabase is null)
        {
            _conversationSession = null;
            return;
        }
        var state = new ImportedConversationActionState(_campaign.State);
        state.Initialize(_conversationInitialVariables);
        var actions = _conversationActionTrees is null ? null : new DynamixActionInterpreter(_conversationActionTrees, state);
        _conversationSession = new ImportedConversationSession(_conversationDatabase, actions);
    }

    private void LoadTitleMovie()
    {
        _titleMovie = CreateMovie("Title.Intro", reportFailure: false);
    }

    private SmackerMoviePlayer? CreateMovie(string roleOrSuffix, bool reportFailure = true)
    {
        var suffix = ImportedMovies.Definitions.FirstOrDefault(movie => movie.Role == roleOrSuffix)?.IdSuffix
            ?? roleOrSuffix;
        var id = _importedContent?.FindId("movie", suffix);
        if (id is null || _importedContent?.Open(id) is not { } source)
        {
            if (reportFailure) _notice = "ORIGINAL MOVIE IS NOT INSTALLED";
            return null;
        }
        try
        {
            return new SmackerMoviePlayer(GraphicsDevice, source, _settings.SpeechVolume);
        }
        catch (Exception error) when (error is InvalidDataException or NotSupportedException or IOException)
        {
            source.Dispose();
            if (reportFailure) _notice = "ORIGINAL MOVIE COULD NOT BE PLAYED";
            return null;
        }
    }

    private void PlayEventMovie(string roleOrSuffix, Screen returnScreen)
    {
        PlayEventMovieSequence([roleOrSuffix], returnScreen);
    }

    private void PlayEventMovieSequence(IEnumerable<string> rolesOrSuffixes, Screen returnScreen)
    {
        _eventMovieQueue.Clear();
        foreach (var roleOrSuffix in rolesOrSuffixes) _eventMovieQueue.Enqueue(roleOrSuffix);
        _movieReturnScreen = returnScreen;
        PlayNextEventMovie();
    }

    private void PlayNextEventMovie()
    {
        _eventMovie?.Dispose();
        _eventMovie = null;
        DisposePracticeJoustLances();
        _activeEventMovieRole = null;
        while (_eventMovieQueue.TryDequeue(out var roleOrSuffix))
        {
            var movie = CreateMovie(roleOrSuffix);
            if (movie is null) continue;
            if (roleOrSuffix == "Practice.Joust")
            {
                try { LoadPracticeJoustLances(movie); }
                catch { movie.Dispose(); throw; }
            }
            _eventMovie = movie;
            _activeEventMovieRole = roleOrSuffix;
            if (roleOrSuffix == "Practice.Joust")
            {
                _practiceJoustLance = new();
                _practiceJoustTrial = new();
                _practiceJoustResult = null;
                _practiceJoustLastFrame = -1;
            }
            if (_musicInstance?.State == SoundState.Playing) _musicInstance.Pause();
            _screen = Screen.Movie;
            return;
        }

        _screen = _movieReturnScreen;
        StartMusic();
    }

    private void FinishEventMovie()
    {
        _eventMovie?.Skip();
        _eventMovie?.Dispose();
        _eventMovie = null;
        PlayNextEventMovie();
    }

    // PLACEHOLDER: RULE-SOUND-004. The original plays track 2 for the opening, 3 for the title, 4 in
    // field battles, 5 in game options and 6 for the credits; the rebuild loops track 2 everywhere.
    private void StartMusic()
    {
        if (!_cdMusicEnabled || _titleMovie is { IsComplete: false }
            || _eventMovie is { IsComplete: false } || _musicInstance is null) return;
        if (_musicInstance.State == SoundState.Paused) _musicInstance.Resume();
        else if (_musicInstance.State == SoundState.Stopped) _musicInstance.Play();
    }

    // PLACEHOLDER: RULE-SOUND-002. One host voice per call with no ten-voice limit, and the effects
    // volume in place of the caller's level.
    private void PlayOriginalSound(string role)
    {
        if (_originalSounds.TryGetValue(role, out var sound))
            sound.Play(_settings.EffectsVolume, pitch: 0, pan: 0);
    }

    private void DisposeOriginalSounds()
    {
        foreach (var sound in _originalSounds.Values) sound.Dispose();
        _originalSounds.Clear();
    }

    // SCR-UI-001.
    private void DrawTitle()
    {
        if (_titleMovie is { IsComplete: false })
        {
            _batch.Draw(_titleMovie.Texture, new Rectangle(0, 0, 1024, 768), Color.White);
            return;
        }
        if (!DrawOriginal("Title.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Title screen requires its verified original background art.");
    }

    private void DrawEventMovie()
    {
        if (_eventMovie is null)
        {
            Fill(new Rectangle(0, 0, 1024, 768), Color.Black);
            return;
        }
        if (_movieReturnScreen == Screen.Shop)
        {
            DrawShop();
            _batch.Draw(_eventMovie.Texture, ScaleBounds(ShopPresentationDefinitions.ItemBounds(
                _eventMovie.Texture.Width, _eventMovie.Texture.Height)), Color.White);
            return;
        }
        if (_movieReturnScreen == Screen.Map && _activeEventMovieRole?.StartsWith("Season.", StringComparison.Ordinal) == true)
        {
            DrawMap();
            _batch.Draw(_eventMovie.Texture, ScaleBounds(
                OriginalStrategicTerrainPresentation.TransitionMovieBounds(
                    _eventMovie.Texture.Width, _eventMovie.Texture.Height)), Color.White);
            return;
        }
        if (_activeEventMovieRole == "Practice.Joust")
        {
            Fill(new Rectangle(0, 0, 1024, 768), Color.Black);
            _batch.Draw(_eventMovie.Texture, new Rectangle(0, 144, 1024, 480), Color.White);
            if (_practiceJoustLances is not { Count: 25 } lances)
                throw new InvalidOperationException("Joust practice requires the imported lance foreground.");
            var frame = lances[_practiceJoustLance.Frame];
            if (OriginalPracticeJoustLancePresentation.Clip(
                    _practiceJoustLance.X, _practiceJoustLance.Y,
                    frame.Width, frame.Height) is { } blit)
                _batch.Draw(frame, ScaleBounds(blit.Destination),
                    new Rectangle(blit.Source.X, blit.Source.Y,
                        blit.Source.Width, blit.Source.Height), Color.White);
            DrawText("MOVE MOUSE TO AIM THE LANCE   ESC TO LEAVE", 55, 670, Color.White, 2);
            return;
        }
        _batch.Draw(_eventMovie.Texture, new Rectangle(0, 0, 1024, 768), Color.White);
    }

    private void DrawOptionsHub()
    {
        if (!DrawOriginal("Options.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Options hub requires its verified original background art.");
        DrawOptionsWidgets();
        var selected = _optionsHubOptions[_optionsHubOption];
        DrawOutline(ScaleBounds(selected.OriginalBounds),
            selected.RequiresCampaign && !_hasActiveCampaign ? Color.Gray : Color.Gold, 3);
        if (_notice.Length > 0) DrawText(_notice, 30, 730, Color.Gold, 2, 960);
    }

    private void DrawOptionsWidgets()
    {
        if (!_originalAnimations.TryGetValue("Options.Widgets", out var animation)
            || animation.Frames.Count <= OptionsHubDefinitions.ResumeFrame) return;

        foreach (var indexed in _optionsHubOptions.Select((option, index) => (option, index))
                     .Where(indexed => indexed.option.Setting.HasValue))
        {
            var option = indexed.option;
            var pressed = indexed.index == _pressedOptionsHubOption
                && (Mouse.GetState().LeftButton == ButtonState.Pressed || _controllerPointerPressed);
            var frameIndex = OptionsHubDefinitions.StatusFrame(SettingEnabled(option.Setting!.Value), pressed);
            var frame = animation.Frames[frameIndex];
            var bounds = OptionsHubDefinitions.StatusBounds(option, frame.Width, frame.Height);
            _batch.Draw(frame, ScaleBounds(bounds), Color.White);
        }

        if (_hasActiveCampaign)
        {
            var resume = _optionsHubOptions.Single(option => option.Action == OptionsHubAction.Resume);
            var frame = animation.Frames[OptionsHubDefinitions.ResumeFrame];
            _batch.Draw(frame, ScaleBounds(resume.OriginalBounds), Color.White);
        }
    }

    private void DrawPractice()
    {
        if (!DrawOriginal("Practice.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Practice menu requires its verified original background art.");
        DrawPracticeJoustResult();
    }

    private bool SettingEnabled(OptionsHubSetting setting) => setting switch
    {
        OptionsHubSetting.CdMusic => _cdMusicEnabled,
        OptionsHubSetting.MidiMusic => false,
        OptionsHubSetting.SoundEffects => _soundEffectsEnabled,
        OptionsHubSetting.Speech => _speechEnabled,
        OptionsHubSetting.Animation => _animationEnabled,
        _ => throw new ArgumentOutOfRangeException(nameof(setting))
    };

    private void DrawLoadGame()
    {
        if (!DrawOriginal("Load.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Load-game screen requires its verified original background art.");

        for (var i = 0; i < CampaignSaveSlots.SlotCount; i++)
        {
            var bounds = ScaleBounds(LoadGameDefinitions.Slots[i]);
            var info = i < _saveSlotInfo.Count ? _saveSlotInfo[i] : new CampaignSaveSlot(i + 1, false, false, "EMPTY", null, null);
            var details = info.IsValid
                ? $"{info.PlayerName}   {info.CampaignDate:dd MMM yyyy}{(info.RecoveredFromBackup ? "   BACKUP" : "")}"
                : info.PlayerName;
            DrawText(details.ToUpperInvariant(), bounds.X + 90, bounds.Y + 10, info.IsValid ? Color.White : Color.LightGray, 2, bounds.Width - 110);
            if (i == _loadSlot) DrawOutline(bounds, Color.Gold, 3);
        }

        if (_loadSlot < _saveSlotInfo.Count && !string.IsNullOrEmpty(_saveSlotInfo[_loadSlot].Error))
            DrawText(_saveSlotInfo[_loadSlot].Error!, 120, 675, Color.Orange, 2, 780);
        DrawText("ARROWS/1-5 LOAD   F8 AUTOSAVE   ESC RESUME", 190, 710, Color.Wheat, 2);
        if (!string.IsNullOrEmpty(_notice)) DrawText(_notice, 250, 740, Color.Gold, 2);
    }

    private void DrawCharacterOptions()
    {
        if (!DrawOriginal("Character.Options", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Character creation requires its verified original background art.");
        DrawOutline(ScaleBounds(_characterOptions[_characterOption].OriginalBounds), Color.Gold, 3);
        DrawOutline(ScaleBounds(_heraldicColors[_heraldicColor].OriginalBounds), Color.Gold, 2);
        if (!string.IsNullOrEmpty(_notice)) DrawText(_notice, 25, 700, Color.Gold, 2);
    }

    private void DrawCharacterName()
    {
        if (!DrawOriginal("Character.Options", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Character naming requires its verified original background art.");
        var entry = ScaleBounds(CharacterCreationDefinitions.NameEntryBounds);
        Fill(entry, new Color(10, 10, 10, 225));
        DrawOutline(entry, Color.Gold, 2);
        DrawText(_characterName + "_", entry.X + 12, entry.Y + 14, Color.White, 2);
    }

    private void DrawCharacter()
    {
        if (!DrawOriginal("Character.Pregenerated", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Pre-generated character selection requires its verified original background art.");
        DrawOutline(ScaleBounds(_pregeneratedBounds[_characterTemplate]), Color.Gold, 3);
        DrawText("1-6 OR ARROWS/ENTER   ESC BACK", 25, 735, Color.Wheat, 2);
    }

    private void DrawDilemma()
    {
        var stats = _campaign.State.Player.Stats;
        if (!DrawOriginal("Dilemma.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Youth dilemma screen requires its verified original background art.");
        var imported = _youthDilemmaResult is null ? CurrentImportedDilemma() : null;
        if (imported is not null) DrawDilemmaAnimation(imported);
        if (_youthDilemmaResult is { } result)
        {
            DrawText(result.Text, 84, 330, Color.Black, 2, 850);
        }
        else if (imported is not null)
        {
            DrawText(imported.Prompt, 84, 330, Color.Black, 2, 850);
        }
        else
        {
            throw new InvalidOperationException("Youth dilemma screen requires its verified original dialogue data.");
        }
        int[] values = [stats.Strength, stats.Dexterity, stats.Piety, stats.Stamina, stats.Honor, _campaign.State.Player.Wealth, _campaign.State.Player.Age];
        for (var i = 0; i < values.Length; i++) DrawText(values[i].ToString(), 320, 103 + i * 27, Color.Black, 2);
    }

    private void DrawCampaignBriefing()
    {
        if (!DrawOriginal("Campaign.Briefing", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Campaign briefing requires its verified original background art.");
    }

    private void DrawDilemmaAnimation(YouthDilemmaDefinition dilemma)
    {
        var animation = GetDilemmaAnimation(dilemma.SceneFile);
        if (animation is null) return;
        var localFrame = _animationEnabled && !_settings.ReducedMotion
            ? (int)(_presentationSeconds * 5) % animation.FramesPerChoice : 0;
        for (var choice = 0; choice < YouthDilemmaPresentationDefinitions.ChoiceCount; choice++)
            _batch.Draw(animation.Frames[choice * animation.FramesPerChoice + localFrame],
                ScaleBounds(_dilemmaChoiceBounds[choice]), Color.White);
    }

    private DilemmaAnimation? GetDilemmaAnimation(string? sceneFile)
    {
        if (sceneFile is null || _dilemmaPalette is null || _importedContent is null) return null;
        if (_dilemmaAnimations.TryGetValue(sceneFile, out var cached)) return cached;
        var id = _importedContent.FindId("indexed-animation", $":{sceneFile}");
        var sequence = id is null ? null : _importedContent.DecodeCsf(id);
        if (sequence is null || sequence.Chunks.Count == 0
            || sequence.Chunks.Count % YouthDilemmaPresentationDefinitions.ChoiceCount != 0)
            return _dilemmaAnimations[sceneFile] = null;

        var textures = new List<Texture2D>(sequence.Chunks.Count);
        foreach (var chunk in sequence.Chunks)
        {
            var frame = sequence.DecodeFrame(chunk);
            var texture = new Texture2D(GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color);
            texture.SetData(frame.ToRgba(_dilemmaPalette));
            textures.Add(texture);
        }
        return _dilemmaAnimations[sceneFile] = new DilemmaAnimation(textures,
            sequence.Chunks.Count / YouthDilemmaPresentationDefinitions.ChoiceCount);
    }

    private void DrawMap()
    {
        if (!DrawOriginal("Estate.Shell", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Estate map requires its verified original shell art.");
        DrawEstateMap();
    }

    private void DrawEstateMap()
    {
        var strategic = _campaign.State.OriginalStrategicState
            ?? throw new InvalidOperationException("Estate map requires original strategic state.");
        DrawOriginalStrategicTerrain(strategic);
        DrawOriginalStrategicMarkers(strategic);
        DrawEstateInformation();

        foreach (var control in _estateLayout.Controls)
        {
            var bounds = ScaleBounds(control.Bounds);
            if (control.Action is EstateControlAction.Home or EstateControlAction.Village)
                DrawText(control.Label, bounds.X + 8, bounds.Y + 7, Color.Red, 2, bounds.Width - 16);
            else if (control.Panel == _estatePanel)
                DrawOutline(bounds, Color.Gold, 2);
        }

        var footer = ScaleBounds(_estateLayout.FooterStatus);
        DrawText(World.Locations[_selectedLocation].Name.ToUpperInvariant(), footer.X + 8, footer.Y + 7, Color.Wheat, 2, footer.Width - 16);
    }

    private void DrawOriginalStrategicTerrain(OriginalStrategicCampaignState strategic)
    {
        var role = OriginalStrategicTerrainPresentation.AtlasRoleFor(strategic.TerrainProfile);
        if (!_originalAnimations.TryGetValue(role, out var atlas))
            throw new InvalidOperationException("Estate map requires its imported strategic terrain atlas.");

        foreach (var blit in OriginalStrategicTerrainPresentation.BuildBlits(
                     strategic, _originalStrategicResources, atlas.Frames.Count))
        {
            _batch.Draw(atlas.Frames[blit.FrameIndex], ScaleBounds(blit.Destination),
                new Rectangle(blit.Source.X, blit.Source.Y, blit.Source.Width, blit.Source.Height),
                Color.White);
        }
    }

    private void DrawOriginalStrategicMarkers(OriginalStrategicCampaignState strategic)
    {
        if (!_originalAnimations.TryGetValue("Strategic.Map.Markers", out var markers)
            || markers.Frames.Count == 0)
            throw new InvalidOperationException("Estate map requires its imported strategic markers.");

        var firstFrame = markers.Frames[0];
        foreach (var frame in markers.Frames)
            if (frame.Width != firstFrame.Width || frame.Height != firstFrame.Height)
                throw new InvalidDataException("Strategic marker frames must use one source dimension.");

        foreach (var blit in OriginalStrategicMarkerPresentation.BuildBlits(
                     strategic, _campaign.State.Player.OriginalStrategicCharacterColor,
                     markers.Frames.Count, firstFrame.Width, firstFrame.Height))
        {
            _batch.Draw(markers.Frames[blit.FrameIndex], ScaleBounds(blit.Destination),
                new Rectangle(blit.Source.X, blit.Source.Y, blit.Source.Width, blit.Source.Height),
                Color.White);
        }

        foreach (var blit in OriginalStrategicMarkerPresentation.BuildMovementBlits(
                     strategic, markers.Frames.Count, firstFrame.Width, firstFrame.Height))
        {
            _batch.Draw(markers.Frames[blit.FrameIndex], ScaleBounds(blit.Destination),
                new Rectangle(blit.Source.X, blit.Source.Y, blit.Source.Width, blit.Source.Height),
                Color.White);
        }

        foreach (var blit in OriginalStrategicMarkerPresentation.BuildTemporaryForceBlits(
                     strategic, markers.Frames.Count, firstFrame.Width, firstFrame.Height))
        {
            _batch.Draw(markers.Frames[blit.FrameIndex], ScaleBounds(blit.Destination),
                new Rectangle(blit.Source.X, blit.Source.Y, blit.Source.Width, blit.Source.Height),
                Color.White);
        }

        if (!_originalAnimations.TryGetValue("Strategic.Map.MarkerOverlay", out var routeMarkers)
            || routeMarkers.Frames.Count == 0)
            throw new InvalidOperationException("Estate map requires its imported route markers.");
        var firstRouteFrame = routeMarkers.Frames[0];
        foreach (var frame in routeMarkers.Frames)
            if (frame.Width != firstRouteFrame.Width || frame.Height != firstRouteFrame.Height)
                throw new InvalidDataException("Strategic route marker frames must use one source dimension.");

        foreach (var blit in OriginalStrategicMarkerPresentation.BuildRoutePreviewBlits(
                     strategic, _strategicRoutePreviewFrame, routeMarkers.Frames.Count,
                     firstRouteFrame.Width, firstRouteFrame.Height))
        {
            _batch.Draw(routeMarkers.Frames[blit.FrameIndex], ScaleBounds(blit.Destination),
                new Rectangle(blit.Source.X, blit.Source.Y, blit.Source.Width, blit.Source.Height),
                Color.White);
        }
    }

    private void DrawEstateInformation()
    {
        var panel = ScaleBounds(_estateLayout.InformationPanel);
        var player = _campaign.State.Player;
        var selected = World.Locations[_selectedLocation];
        var intel = _campaign.HasGarrisonIntel(_selectedLocation) ? _campaign.GarrisonAt(_selectedLocation).ToString() : "UNKNOWN";
        DrawText(selected.Name, panel.X + 12, panel.Y + 20, Color.White, 2, panel.Width - 24);
        switch (_estatePanel)
        {
            case EstatePanel.Map:
                DrawText($"GARRISON {intel}\nDISTANCE {World.TravelDays(_campaign.State.CurrentLocation, _selectedLocation)} DAYS", panel.X + 12, panel.Y + 58, Color.Wheat, 2, panel.Width - 24);
                break;
            case EstatePanel.Orders:
                var order = _campaign.ArmyOrderAt(_warPlanningArmyIndex);
                var army = player.ArmyAt(_warPlanningArmyIndex);
                var armyStatus = order is null
                    ? $"AT {World.Locations[player.ArmyLocationAt(_warPlanningArmyIndex)].Name}"
                    : $"TO {World.Locations[order.Destination].Name}\nARRIVES {order.Arrives:MMM d}";
                DrawText($"{_warPlanningArmyIndex + 1} {player.ArmyNameAt(_warPlanningArmyIndex)} ({army.Total})\n{armyStatus}\n1-5 SELECT  A DISPATCH\nS SIEGE  B BATTLE",
                    panel.X + 12, panel.Y + 48, Color.Wheat, 2, panel.Width - 24);
                break;
            case EstatePanel.Help:
                DrawText("ARROWS SELECT\n+/- CHANGE SPEED\nE ADVANCE TIME\nF5 SAVE  F9 LOAD", panel.X + 12, panel.Y + 58, Color.Wheat, 2, panel.Width - 24);
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        DrawText($"{_campaign.State.Date:MMM d, yyyy}\nTIME {_campaign.State.DaySpeed}X\nWEALTH {player.Wealth}\nCENSUS {player.Home.Population}",
            panel.X + 12, panel.Y + panel.Height - 138, Color.White, 2, panel.Width - 24);
        if (!string.IsNullOrEmpty(_notice)) DrawText(_notice, panel.X + 12, panel.Y + panel.Height - 34, Color.Gold, 1, panel.Width - 24);
    }

    private void DrawHome()
    {
        if (!DrawOriginal("Home.Office", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Castle office requires its verified original background art.");
        DrawSceneHoverLabel(_homeHotspots, HomePresentationDefinitions.HoverLabelBounds);
    }

    private void DrawWarPlanning()
    {
        if (!DrawOriginal("Home.WarPlanning", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("War planning requires its verified original background art.");
        DrawWarPlanningControls();
    }

    private void DrawWarPlanningControls()
    {
        if (!_originalAnimations.TryGetValue("Home.WarPlanning.Controls", out var animation)
            || animation.Frames.Count <= WarPlanningPresentationDefinitions.SendSpyUnavailableFrame) return;

        for (var index = 0; index < _warPlanningLayout.ArmyButtons.Count; index++)
        {
            var available = _campaign.State.Player.ArmyLocationAt(index) == 0;
            DrawWarPlanningFrame(animation,
                WarPlanningPresentationDefinitions.ArmyFrame(index, index == _warPlanningArmyIndex, available),
                _warPlanningLayout.ArmyButtons[index]);
        }
        var player = _campaign.State.Player;
        var selectedArmy = player.ArmyAt(_warPlanningArmyIndex);
        var hasArmy = selectedArmy.Total > 0;
        var editableAtHome = player.ArmyLocationAt(_warPlanningArmyIndex) == 0;
        DrawWarPlanningFrame(animation, hasArmy && editableAtHome
            ? WarPlanningPresentationDefinitions.FieldArmyFrame
            : WarPlanningPresentationDefinitions.FieldArmyUnavailableFrame, _warPlanningLayout.FieldArmy);
        DrawWarPlanningFrame(animation, player.ActiveSpies == 0 && player.Wealth >= Balance.Strategy.SpyCost
            ? WarPlanningPresentationDefinitions.SendSpyFrame
            : WarPlanningPresentationDefinitions.SendSpyUnavailableFrame, _warPlanningLayout.SendSpy);
        DrawWarPlanningFrame(animation, !hasArmy
            ? WarPlanningPresentationDefinitions.MembershipUnavailableFrame
            : player.JoinedArmyIndex == _warPlanningArmyIndex
                ? WarPlanningPresentationDefinitions.LeaveFrame
                : WarPlanningPresentationDefinitions.JoinFrame,
            _warPlanningLayout.Membership);

        var unitTypes = Enum.GetValues<UnitType>();
        for (var index = 0; index < _warPlanningLayout.UnitRows.Count && index < unitTypes.Length; index++)
        {
            var unit = unitTypes[index];
            var bounds = ScaleBounds(_warPlanningLayout.UnitRows[index]);
            DrawText($"{unit,-18} {selectedArmy.Units[unit],4}", bounds.X + 4, bounds.Y + 2,
                editableAtHome ? Color.SaddleBrown : Color.Gray, 2, bounds.Width - 8);
        }
        var armyName = ScaleBounds(_warPlanningLayout.ArmyName);
        DrawText(player.ArmyNameAt(_warPlanningArmyIndex) + (_editingWarPlanningArmyName ? "_" : ""), armyName.X + 4, armyName.Y + 3,
            Color.SaddleBrown, 2, armyName.Width - 8);
        DrawText($"TOTAL POPULATION       {player.Home.Population}", 180, 355, Color.SaddleBrown, 2);
        DrawText($"TOTAL SERFS AVAILABLE  {player.AvailableSerfs}", 180, 380, Color.SaddleBrown, 2);
        DrawText($"TOTAL MONTHLY COST     {WarPlanningMonthlyCost(player)}", 180, 405, Color.SaddleBrown, 2);
        DrawText($"TOTAL WEALTH           {player.Wealth}", 180, 430, Color.SaddleBrown, 2);
        DrawText($"ACTIVE SPIES           {player.ActiveSpies}", 680, 355, Color.SaddleBrown, 2, 280);
    }

    private static int WarPlanningMonthlyCost(Player player)
    {
        var productivity = player.Home.Productivity(false);
        return Enumerable.Range(0, Player.ArmyDivisionLimit).Sum(index => player.ArmyAt(index).Units.Sum(pair =>
            pair.Value * Balance.ScaleFrom50(Balance.Units[pair.Key].UpkeepAt50,
                Balance.Units[pair.Key].UpkeepAt100, productivity)));
    }

    private void DrawWarPlanningFrame(OriginalAnimation animation, int frameIndex, UiBounds bounds)
    {
        var frame = animation.Frames[frameIndex];
        _batch.Draw(frame, ScaleBounds(bounds), Color.White);
    }

    private void DrawFarm()
    {
        var layout = _fiefLayouts[_fiefSection];
        if (!DrawOriginal("Farm.Management", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Farm management requires its verified original background art.");
        DrawFarmTerrain(layout.Terrain);
        var accountColor = Color.Black;
        DrawText(layout.Title, 55, 70, accountColor, 3);
        var entries = FarmPresentationDefinitions.EntriesFor(_fiefSection);
        for (var row = 0; row < layout.Rows.Count && row + _fiefRowOffset < entries.Count; row++)
        {
            var entry = entries[row + _fiefRowOffset];
            var bounds = ScaleBounds(layout.Rows[row]);
            var value = FiefEntryValue(entry);
            DrawText(value.Length == 0 ? entry.Label : $"{entry.Label,-22} {value}",
                bounds.X + 4, bounds.Y + 2, accountColor, 1, bounds.Width - 8);
        }
        var okay = ScaleBounds(layout.Okay);
        var cancel = ScaleBounds(layout.Cancel);
        DrawText("OK", okay.X + 4, okay.Y + 3, Color.White, 1, okay.Width - 8);
        DrawText("CANCEL", cancel.X + 4, cancel.Y + 3, Color.White, 1, cancel.Width - 8);
        if (entries.Count > layout.Rows.Count)
            DrawText($"UP/DOWN  {_fiefRowOffset + 1}-{Math.Min(entries.Count, _fiefRowOffset + layout.Rows.Count)} OF {entries.Count}", 400, 680, Color.Wheat, 1);
    }

    private string FiefEntryValue(FarmPresentationDefinitions.Entry entry)
    {
        var fief = _campaign.State.Player.Home;
        return entry.Action switch
        {
            BuildFarmAction { Building: BuildingKind.House } => fief.Houses.ToString(),
            BuildFarmAction build => fief.Has(build.Building) ? "YES" : "NO",
            PlantFarmAction plant => fief.Crops[plant.Crop].ToString(),
            DevelopForestFarmAction forest => fief.Forest[forest.Industry].ToString(),
            _ => ""
        };
    }

    private void DrawFarmTerrain(UiBounds originalBounds)
    {
        var bounds = ScaleBounds(originalBounds);
        Fill(bounds, new Color(91, 115, 63));
        const int columns = 14;
        const int rows = 22;
        var cellWidth = Math.Max(1, bounds.Width / columns);
        var cellHeight = Math.Max(1, bounds.Height / rows);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var x = bounds.X + column * cellWidth + (row % 2) * 2;
                var y = bounds.Y + row * cellHeight;
                Fill(new Rectangle(x, y, 2, Math.Max(3, cellHeight - 3)), new Color(25, 82, 35));
            }
    }

    private void DrawVillage()
    {
        var scene = OriginalVillageScenePresentation.SceneForNewGameHome(_campaign.State, _villageScenes);
        if (scene is not null)
        {
            DrawVillageSceneBackground(scene, new Rectangle(0, 0, 1024, 768));
        }
        else if (!DrawOriginal("Village.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Village screen requires its verified original background art.");
        DrawVillageHoverLabel();
        if (_notice.Length > 0) DrawText(_notice, 24, 730, Color.Gold, 2, 976);
    }

    private void DrawVillageSceneBackground(VillageSceneDefinition scene, Rectangle destination)
    {
        var role = "Village.Scene." + scene.BackgroundName;
        if (!_originalArt.TryGetValue(role, out var texture))
        {
            var id = _importedContent.FindId("image", ":" + scene.BackgroundName)
                ?? throw new InvalidDataException($"Required original village image '{scene.BackgroundName}' is missing.");
            var image = _importedContent.DecodePcx(id)
                ?? throw new InvalidDataException($"Required original village image '{scene.BackgroundName}' could not be decoded.");
            texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
            texture.SetData(image.ToRgba());
            _originalArt.Add(role, texture);
        }
        _batch.Draw(texture, destination, Color.White);
    }

    private void DrawVillageHoverLabel()
    {
        var point = OriginalPoint(_lastMouse);
        var hotspot = CurrentVillageHotspots().FirstOrDefault(item => item.Bounds.Contains(point.X, point.Y));
        if (hotspot is null) return;
        var bounds = ScaleBounds(VillagePresentationDefinitions.HoverLabelBounds);
        DrawText(hotspot.HoverLabel, bounds.X + 8, bounds.Y + 6, Color.Wheat, 2, bounds.Width - 16);
    }

    private void DrawDrogoDemand()
    {
        var player = _campaign.State.Player;
        DrawPanel("DROGO HAS COME TO COLLECT", $"THE MONEYLENDER DEMANDS THE {player.Debt} SHILLINGS YOU OWE");
        DrawOriginal("Encounter.Drogo", new Rectangle(660, 185, 293, 305));
        DrawText($"WEALTH {player.Wealth}S", 100, 245, Color.Wheat, 2);
        DrawText("P  PAY THE DEBT", 100, 335, player.Wealth >= player.Debt ? Color.LightGreen : Color.Gray, 2);
        DrawText("F  REFUSE AND FIGHT DROGO", 100, 395, Color.IndianRed, 2);
        DrawText("THIS FIGHT IS TO THE DEATH", 100, 475, Color.Gold, 2);
    }
}
