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
        var movie = CreateMovie(roleOrSuffix);
        if (movie is null) return;
        _eventMovie?.Dispose();
        _eventMovie = movie;
        _movieReturnScreen = returnScreen;
        if (_musicInstance?.State == SoundState.Playing) _musicInstance.Pause();
        _screen = Screen.Movie;
    }

    private void FinishEventMovie()
    {
        _eventMovie?.Skip();
        _eventMovie?.Dispose();
        _eventMovie = null;
        _screen = _movieReturnScreen;
        StartMusic();
    }

    private void StartMusic()
    {
        if (!_cdMusicEnabled || _titleMovie is { IsComplete: false }
            || _eventMovie is { IsComplete: false } || _musicInstance is null) return;
        if (_musicInstance.State == SoundState.Paused) _musicInstance.Resume();
        else if (_musicInstance.State == SoundState.Stopped) _musicInstance.Play();
    }

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

    private void DrawTitle()
    {
        if (_titleMovie is { IsComplete: false })
        {
            _batch.Draw(_titleMovie.Texture, new Rectangle(0, 0, 1024, 768), Color.White);
            return;
        }
        var hasTitle = DrawOriginal("Title.Background", new Rectangle(0, 0, 1024, 768));
        if (!hasTitle)
        {
            Fill(new Rectangle(0, 0, 1024, 768), new Color(39, 50, 35));
            Fill(new Rectangle(90, 90, 844, 590), new Color(91, 63, 38));
            Fill(new Rectangle(110, 110, 804, 550), new Color(25, 30, 24));
            DrawText("CONQUEROR", 250, 190, Color.Gold, 7); DrawText("A.D. 1086", 350, 265, Color.Wheat, 4);
        }
        if (!hasTitle) DrawText("PRESS ANY KEY", 405, 600, Color.White, 2);
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
        _batch.Draw(_eventMovie.Texture, new Rectangle(0, 0, 1024, 768), Color.White);
    }

    private void DrawOptionsHub()
    {
        var original = DrawOriginal("Options.Background", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("OPTIONS", "NEW GAME, LOAD, SETTINGS, OR RESUME");
            for (var i = 0; i < _optionsHubOptions.Count; i++)
            {
                var option = _optionsHubOptions[i];
                var unavailable = option.RequiresCampaign && !_hasActiveCampaign;
                DrawText($"{i + 1,2}  {option.Label}", 110, 180 + i * 35,
                    unavailable ? Color.Gray : i == _optionsHubOption ? Color.Gold : Color.White, 2);
            }
        }
        else
        {
            DrawOptionsWidgets();
            var selected = _optionsHubOptions[_optionsHubOption];
            DrawOutline(ScaleBounds(selected.OriginalBounds),
                selected.RequiresCampaign && !_hasActiveCampaign ? Color.Gray : Color.Gold, 3);
        }
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
        var original = DrawOriginal("Practice.Background", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("PRACTICE", "SELECT A TRAINING EVENT");
            for (var index = 0; index < _practiceOptions.Count; index++)
                DrawText($"{index + 1}  {_practiceOptions[index].Label}", 220, 235 + index * 60,
                    index == _practiceOption ? Color.Gold : Color.White, 3);
            return;
        }

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
        var original = DrawOriginal("Load.Background", new Rectangle(0, 0, 1024, 768));
        if (!original) DrawPanel("LOAD GAME", "SELECT ONE OF FIVE CAMPAIGN SLOTS");

        for (var i = 0; i < CampaignSaveSlots.SlotCount; i++)
        {
            var bounds = original ? ScaleBounds(LoadGameDefinitions.Slots[i]) : new Rectangle(150, 190 + i * 82, 724, 55);
            var info = i < _saveSlotInfo.Count ? _saveSlotInfo[i] : new CampaignSaveSlot(i + 1, false, false, "EMPTY", null, null);
            var details = info.IsValid
                ? $"{info.PlayerName}   {info.CampaignDate:dd MMM yyyy}{(info.RecoveredFromBackup ? "   BACKUP" : "")}"
                : info.PlayerName;
            DrawText(details.ToUpperInvariant(), bounds.X + 90, bounds.Y + 10, info.IsValid ? Color.White : Color.LightGray, 2, bounds.Width - 110);
            if (i == _loadSlot) DrawOutline(bounds, Color.Gold, 3);
        }

        if (!original) DrawText("ESC OR R  RESUME", 150, 640, Color.LightGreen, 2);
        if (_loadSlot < _saveSlotInfo.Count && !string.IsNullOrEmpty(_saveSlotInfo[_loadSlot].Error))
            DrawText(_saveSlotInfo[_loadSlot].Error!, 120, 675, Color.Orange, 2, 780);
        DrawText("ARROWS/1-5 LOAD   F8 AUTOSAVE   ESC RESUME", 190, 710, Color.Wheat, 2);
        if (!string.IsNullOrEmpty(_notice)) DrawText(_notice, 250, 740, Color.Gold, 2);
    }

    private void DrawCharacterOptions()
    {
        var original = DrawOriginal("Character.Options", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("CREATE YOUR CHARACTER", "CHOOSE HOW YOUR CONQUEROR WILL BEGIN");
            for (var i = 0; i < _characterOptions.Count; i++)
                DrawText($"{i + 1}  {_characterOptions[i].Label}", 170, 220 + i * 80, i == _characterOption ? Color.Gold : Color.White, 2);
        }
        else
        {
            DrawOutline(ScaleBounds(_characterOptions[_characterOption].OriginalBounds), Color.Gold, 3);
            DrawOutline(ScaleBounds(_heraldicColors[_heraldicColor].OriginalBounds), Color.Gold, 2);
        }
        if (!string.IsNullOrEmpty(_notice)) DrawText(_notice, 25, 700, Color.Gold, 2);
    }

    private void DrawCharacterName()
    {
        if (!DrawOriginal("Character.Options", new Rectangle(0, 0, 1024, 768)))
            DrawPanel("CHOOSE CHARACTER NAME", "TYPE A NAME AND PRESS ENTER");
        var entry = ScaleBounds(CharacterCreationDefinitions.NameEntryBounds);
        Fill(entry, new Color(10, 10, 10, 225));
        DrawOutline(entry, Color.Gold, 2);
        DrawText(_characterName + "_", entry.X + 12, entry.Y + 14, Color.White, 2);
    }

    private void DrawCharacter()
    {
        if (DrawOriginal("Character.Pregenerated", new Rectangle(0, 0, 1024, 768)))
        {
            DrawOutline(ScaleBounds(_pregeneratedBounds[_characterTemplate]), Color.Gold, 3);
            DrawText("1-6 OR ARROWS/ENTER   ESC BACK", 25, 735, Color.Wheat, 2);
            return;
        }
        DrawPanel("CHOOSE YOUR KNIGHT", "THE ROAD FROM BOYHOOD TO LORDSHIP BEGINS");
        for (var i = 0; i < Balance.Templates.Length; i++)
        {
            var t = Balance.Templates[i];
            DrawText($"{i + 1}  {t.Name}   STR {t.Stats.Strength} DEX {t.Stats.Dexterity} PIE {t.Stats.Piety} STA {t.Stats.Stamina} HON {t.Stats.Honor}  {t.Wealth}S", 80, 180 + i * 62, i == _characterTemplate ? Color.Gold : Color.White, 2);
        }
        DrawText("0  CUSTOM ROLLED CHARACTER", 80, 575, Color.LightGreen, 2);
    }

    private void DrawDilemma()
    {
        var index = _campaign.State.YouthDilemmasAnswered;
        var stats = _campaign.State.Player.Stats;
        var original = DrawOriginal("Dilemma.Background", new Rectangle(0, 0, 1024, 768));
        var imported = _youthDilemmaResult is null ? CurrentImportedDilemma() : null;
        if (original && imported is not null) DrawDilemmaAnimation(imported);
        if (_youthDilemmaResult is { } result)
        {
            if (!original) DrawPanel($"YOUTH - {result.Outcome.ToString().ToUpperInvariant()}", $"DILEMMA {result.DilemmaNumber}");
            DrawText(result.Text, original ? 84 : 90, original ? 330 : 190, original ? Color.Black : Color.Wheat, 2, original ? 850 : 820);
            if (!original) DrawText("ENTER OR SPACE TO CONTINUE", 220, 520, Color.LightGreen, 2);
        }
        else if (imported is not null)
        {
            if (!original) DrawPanel($"YOUTH - AGE {imported.Age}", imported.Title);
            DrawText(imported.Prompt, original ? 84 : 90, original ? 330 : 170, original ? Color.Black : Color.Wheat, 2, original ? 850 : 820);
            if (!original)
            {
                for (var i = 0; i < imported.Choices.Count; i++)
                    DrawText($"{i + 1}  {imported.Choices[i].Text}", 110, 285 + i * 75, Color.White, 2, 800);
            }
        }
        else
        {
            var dilemma = Youth.Dilemmas[Math.Min(index, Youth.Dilemmas.Length - 1)];
            DrawPanel($"YOUTH - YEAR {index + 1} OF {Youth.OriginalPool.StageCount}", dilemma.Title);
            DrawText(dilemma.Prompt, 90, 190, Color.Wheat, 2, 820);
            for (var i = 0; i < dilemma.Choices.Length; i++) DrawText($"{i + 1}  {dilemma.Choices[i].Text}", 110, 300 + i * 75, Color.White);
        }
        if (original)
        {
            int[] values = [stats.Strength, stats.Dexterity, stats.Piety, stats.Stamina, stats.Honor, _campaign.State.Player.Wealth, _campaign.State.Player.Age];
            for (var i = 0; i < values.Length; i++) DrawText(values[i].ToString(), 320, 103 + i * 27, Color.Black, 2);
        }
        else DrawText($"STR {stats.Strength}  DEX {stats.Dexterity}  INT {stats.Intelligence}  PIETY {stats.Piety}  STAMINA {stats.Stamina}  HONOR {stats.Honor}", 70, 590, Color.Gold, 2);
    }

    private void DrawCampaignBriefing()
    {
        if (DrawOriginal("Campaign.Briefing", new Rectangle(0, 0, 1024, 768))) return;
        DrawPanel("YOUR CAMPAIGN", "TWO ROADS TO GLORY LIE BEFORE YOU");
        DrawText("Raise an army, conquer rival holdings, and challenge the crown in London.",
            105, 230, Color.Wheat, 2, 810);
        DrawText("Or strengthen your estate, compete at tournaments, and seek the path to the dragon.",
            105, 355, Color.Wheat, 2, 810);
        DrawText("PRESS ANY KEY TO BEGIN", 310, 585, Color.LightGreen, 2);
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
        if (DrawOriginal("Estate.Shell", new Rectangle(0, 0, 1024, 768)))
        {
            DrawEstateMap();
            return;
        }

        DrawFallbackMap();
    }

    private void DrawEstateMap()
    {
        DrawEstateTerrain();
        DrawEstateMarkers();
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

    private void DrawEstateTerrain()
    {
        var viewport = ScaleBounds(_estateLayout.MainViewport);
        Fill(viewport, new Color(113, 107, 72));
        var terrain = EstatePresentationDefinitions.TerrainFor(_campaign.State.Player.Home);
        var atlasDefinition = EstatePresentationDefinitions.AtlasFor(_campaign.State.Date);
        _originalAnimations.TryGetValue(atlasDefinition.Role, out var atlas);
        for (var index = 0; index < terrain.Count; index++)
        {
            var frameIndex = EstatePresentationDefinitions.TileFrames[terrain[index]];
            if (atlas is not null && frameIndex < atlas.Frames.Count)
            {
                _batch.Draw(atlas.Frames[frameIndex],
                    ScaleBounds(EstatePresentationDefinitions.TileSpriteBounds(_estateLayout.MainViewport, index)), Color.White);
                continue;
            }
            var style = EstatePresentationDefinitions.TerrainStyles[terrain[index]];
            var tile = ScaleBounds(EstatePresentationDefinitions.TileBounds(_estateLayout.MainViewport, index));
            FillDiamond(tile, new Color(40, 67, 32));
            var inner = new Rectangle(tile.X + 3, tile.Y + 3, Math.Max(1, tile.Width - 6), Math.Max(1, tile.Height - 6));
            FillDiamond(inner, new Color(style.Red, style.Green, style.Blue));
            if (terrain[index] == EstateTerrainKind.Forest)
            {
                Fill(new Rectangle(tile.Center.X - 2, tile.Center.Y - 9, 4, 18), new Color(25, 62, 29));
                Fill(new Rectangle(tile.Center.X - 10, tile.Center.Y - 3, 20, 5), new Color(31, 84, 35));
            }
            else if (terrain[index] == EstateTerrainKind.Settlement)
            {
                Fill(new Rectangle(tile.Center.X - 8, tile.Center.Y - 8, 16, 14), new Color(95, 72, 45));
                Fill(new Rectangle(tile.Center.X - 11, tile.Center.Y - 10, 22, 5), new Color(64, 48, 35));
            }
        }
    }

    private void FillDiamond(Rectangle bounds, Color color)
    {
        var center = bounds.X + bounds.Width / 2;
        for (var row = 0; row < bounds.Height; row++)
        {
            var distance = Math.Abs(row * 2 + 1 - bounds.Height);
            var width = Math.Max(1, bounds.Width * (bounds.Height - distance) / bounds.Height);
            Fill(new Rectangle(center - width / 2, bounds.Y + row, width, 1), color);
        }
    }

    private void DrawEstateMarkers()
    {
        for (var index = 0; index < World.Locations.Length; index++)
        {
            if (!_campaign.CanRevealLocation(index)) continue;
            var point = EstatePresentationDefinitions.InsetPoint(_estateLayout.InsetMap, World.Locations[index]);
            var marker = ScaleBounds(new UiBounds(point.X - 2, point.Y - 2, index == _selectedLocation ? 6 : 4, index == _selectedLocation ? 6 : 4));
            var owned = index == 0 || _campaign.State.ConqueredLocations.Contains(index);
            Fill(marker, index == _campaign.State.CurrentLocation ? Color.Red : index == _selectedLocation ? Color.Gold : owned ? Color.LightGreen : Color.White);
        }
        DrawArmyMarkers((armyIndex, x, y) =>
        {
            var point = EstatePresentationDefinitions.InsetPoint(_estateLayout.InsetMap, x, y);
            var marker = ScaleBounds(new UiBounds(point.X - 3 + armyIndex, point.Y - 3, 5, 5));
            Fill(marker, armyIndex == _warPlanningArmyIndex ? Color.Gold : Color.Cyan);
        });
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

    private void DrawFallbackMap()
    {
        if (!DrawOriginal("Map.England", new Rectangle(0, 0, 760, 768)))
            Fill(new Rectangle(0, 0, 760, 768), new Color(71, 92, 58));
        for (var i = 0; i < World.Locations.Length; i++)
        {
            if (!_campaign.CanRevealLocation(i)) continue;
            var place = World.Locations[i];
            var owned = i == 0 || _campaign.State.ConqueredLocations.Contains(i);
            var color = i == _selectedLocation ? Color.Gold : owned ? Color.LightGreen : place.Kind == LocationKind.DragonLair ? Color.DarkRed : Color.White;
            Fill(new Rectangle(place.X, place.Y, i == _selectedLocation ? 13 : 8, i == _selectedLocation ? 13 : 8), color);
            if (i == _selectedLocation) DrawText(place.Name, Math.Max(5, place.X - 30), Math.Max(5, place.Y - 25), Color.Gold, 1);
        }
        var current = World.Locations[_campaign.State.CurrentLocation];
        Fill(new Rectangle(current.X - 5, current.Y - 5, 18, 18), Color.Red);
        DrawArmyMarkers((armyIndex, x, y) =>
            Fill(new Rectangle(x - 4 + armyIndex * 2, y - 4, 8, 8),
                armyIndex == _warPlanningArmyIndex ? Color.Gold : Color.Cyan));
        Fill(new Rectangle(760, 0, 264, 768), new Color(47, 36, 28));
        var p = _campaign.State.Player; var f = p.Home;
        var selected = World.Locations[_selectedLocation];
        var intel = _campaign.HasGarrisonIntel(_selectedLocation) ? _campaign.GarrisonAt(_selectedLocation).ToString() : "UNKNOWN";
        DrawText("ENGLAND", 820, 25, Color.Gold, 3); DrawText($"{_campaign.State.Date:DD MMM YYYY}", 785, 80, Color.White, 2);
        DrawText(p.Name, 780, 125, Color.Wheat, 2, 230); DrawText($"AGE {p.Age}   WEALTH {p.Wealth}S", 780, 180, Color.White, 2);
        DrawText($"FIEFS {p.Fiefs}  VILLAGES {p.Villages}", 780, 215, Color.White, 2); DrawText($"ARMIES {p.TotalArmyPopulation}  FAME {p.Fame}", 780, 250, Color.White, 2);
        DrawText($"PRODUCTIVITY {f.Productivity()}%", 780, 285, Color.White, 2);
        var tournament = World.Locations[World.TournamentIndex(_campaign.State.Date)].Name;
        DrawText($"AT {current.Name}", 780, 320, Color.Gold, 2, 230);
        DrawText($"TARGET {selected.Name} GARRISON {intel}", 780, 350, Color.Wheat, 2, 230);
        DrawText("ARROWS SELECT ENTER TRAVEL", 780, 395, Color.LightGreen, 2, 230); DrawText("H HOME   V VILLAGE", 780, 440, Color.LightGreen, 2);
        DrawText("T TOURNAMENT O OVERVIEW", 780, 475, Color.LightGreen, 2, 230);
        DrawText("S SIEGE   B FIELD BATTLE", 780, 520, Color.LightGreen, 2, 230); DrawText("1-5 ARMY  A DISPATCH", 780, 555, Color.LightGreen, 2, 230);
        DrawText("P SPY 80S   D DRAGON", 780, 585, Color.LightGreen, 2, 230);
        DrawText($"TOURNAMENT {tournament}", 780, 615, Color.Wheat, 2, 230); DrawText("E ADVANCE  PLUS MINUS SPEED", 780, 650, Color.LightGreen, 2, 230);
        DrawText("F5 SAVE  F9 LOAD", 780, 690, Color.Gold, 2);
    }

    private void DrawArmyMarkers(Action<int, int, int> draw)
    {
        var player = _campaign.State.Player;
        player.EnsureArmyRoster();
        for (var armyIndex = 0; armyIndex < Player.ArmyDivisionLimit; armyIndex++)
        {
            if (!player.ArmyIsFielded(armyIndex) || player.ArmyAt(armyIndex).Total == 0) continue;
            var order = _campaign.ArmyOrderAt(armyIndex);
            var origin = World.Locations[order?.Origin ?? player.ArmyLocationAt(armyIndex)];
            if (order is null) { draw(armyIndex, origin.X, origin.Y); continue; }
            var destination = World.Locations[order.Destination];
            var duration = Math.Max(1, (order.Arrives - order.Departed).TotalDays);
            var progress = Math.Clamp((_campaign.State.Date - order.Departed).TotalDays / duration, 0, 1);
            draw(armyIndex,
                (int)Math.Round(origin.X + (destination.X - origin.X) * progress),
                (int)Math.Round(origin.Y + (destination.Y - origin.Y) * progress));
        }
    }

    private void DrawHome()
    {
        var original = DrawOriginal("Home.Office", new Rectangle(0, 0, 1024, 768));
        if (!original) DrawPanel("CASTLE OFFICE", "MANAGE YOUR FIEF OR RETURN TO THE ROAD");
        if (original) DrawSceneHoverLabel(_homeHotspots, HomePresentationDefinitions.HoverLabelBounds);
        else DrawText("F  FARM MANAGEMENT    V  VILLAGE    ENTER  MAP", 80, 715, Color.Wheat, 2);
    }

    private void DrawWarPlanning()
    {
        var original = DrawOriginal("Home.WarPlanning", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("WAR PLANNING", "ORIGINAL COMMAND DISPATCH IS STILL UNDER INVESTIGATION");
            DrawText("ENTER OR ESC  RETURN TO OFFICE", 610, 720, Color.Wheat, 2, 390);
        }
        else DrawWarPlanningControls();
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
        DrawWarPlanningFrame(animation, player.Wealth >= Balance.Strategy.SpyCost
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
        var f = _campaign.State.Player.Home; var p = _campaign.State.Player;
        var layout = _fiefLayouts[_fiefSection];
        var original = DrawOriginal("Farm.Management", new Rectangle(0, 0, 1024, 768));
        if (!original) DrawPanel(layout.Title, $"WEALTH {p.Wealth}S  POPULATION {f.Population}  PRODUCTIVITY {f.Productivity()}%");
        else DrawFarmTerrain(layout.Terrain);

        var accountColor = original ? Color.Black : Color.White;
        DrawText(layout.Title, 55, 70, accountColor, 3);
        if (original)
        {
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
            return;
        }
        DrawText($"WEALTH          {p.Wealth}", 55, 120, accountColor, 2);
        DrawText($"POPULATION      {f.Population}", 55, 155, accountColor, 2);
        DrawText($"SERFS AVAILABLE {f.AvailableSerfs}", 55, 190, accountColor, 2);
        DrawText($"PRODUCTIVITY    {f.Productivity()}%", 55, 225, accountColor, 2);
        DrawText($"HOUSES          {f.Houses}", 55, 260, accountColor, 2);
        var helpRows = FarmPresentationDefinitions.HelpRowsFor(_fiefSection);
        for (var row = 0; row < helpRows.Count; row++)
            DrawText(helpRows[row], 55, 525 + row * 36,
                row == helpRows.Count - 1 ? Color.Gold : Color.Wheat, 1, 900);
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
        DrawPanel("THE VILLAGE", "VISIT THE MONEYLENDER, CHURCH, BLACKSMITH, AND INN");
        DrawText("B  BORROW 200S (300S DUE AT HARVEST)", 100, 220, Color.White); DrawText("D  DONATE 15S (+1 PIETY)", 100, 280, Color.White);
        DrawText("C  BUILD CHURCH 100S (+15% PRODUCTIVITY)", 100, 340, Color.White); DrawText("K  ENTER BLACKSMITH (ALL WEAPONS AND ARMOR)", 100, 400, Color.LightGreen, 2);
        DrawText("I  ENTER INN   P  VISIT PARISH", 100, 435, Color.LightGreen, 2);
        DrawText($"PLUS MINUS TAX RATE  {_campaign.State.Player.Home.TaxRate}%", 100, 475, Color.Wheat, 2);
        DrawText("ENTER RETURN TO MAP", 100, 535, Color.LightGreen);
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
