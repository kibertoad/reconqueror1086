using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed class ConquerorGame : Microsoft.Xna.Framework.Game
{
    private sealed record DilemmaAnimation(IReadOnlyList<Texture2D> Frames, int FramesPerChoice);
    private sealed record OriginalAnimation(IReadOnlyList<Texture2D> Frames);

    private enum Screen { Title, OptionsHub, LoadGame, CharacterOptions, CharacterName, Character, Dilemma, Map, Home, Farm, Village, Blacksmith, Shop, Tournament, FieldBattle, Siege, Overview, Ending }
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _batch = null!;
    private Texture2D _pixel = null!;
    private Campaign _campaign = new();
    private Screen _screen = Screen.Title;
    private KeyboardState _last;
    private MouseState _lastMouse;
    private int _characterOption;
    private int _optionsHubOption;
    private int _characterTemplate;
    private int _loadSlot;
    private int _activeSaveSlot = 1;
    private Screen _loadReturnScreen = Screen.Title;
    private IReadOnlyList<CampaignSaveSlot> _saveSlotInfo = [];
    private int _heraldicColor = 1;
    private string _characterName = "Sir ";
    private IReadOnlyList<CharacterCreationOption> _characterOptions = CharacterCreationDefinitions.Options;
    private IReadOnlyList<OptionsHubOption> _optionsHubOptions = OptionsHubDefinitions.Options;
    private IReadOnlyList<HeraldicColorOption> _heraldicColors = CharacterCreationDefinitions.HeraldicColors;
    private IReadOnlyList<UiBounds> _pregeneratedBounds = CharacterCreationDefinitions.PregeneratedCharacters;
    private IReadOnlyList<UiBounds> _dilemmaChoiceBounds = YouthDilemmaPresentationDefinitions.Choices;
    private UiBounds _dilemmaContinueBounds = YouthDilemmaPresentationDefinitions.Continue;
    private UiBounds _blacksmithBounds = BlacksmithPresentationDefinitions.Blacksmith;
    private int _joustCursor;
    private SiegeSession? _siege;
    private bool _showRadar = true;
    private FieldBattleSession? _fieldBattle;
    private UnitType _selectedUnit = UnitType.Swordsmen;
    private double _battleTick;
    private string _notice = "";
    private int _selectedLocation;
    private int _shopIndex;
    private int _ladyIndex = 1;
    private int _tournamentOpponent = 2;
    private ImportedContentCatalog? _importedContent;
    private ImportedDialogueRepository? _importedDialogue;
    private WeaponStoreResource? _weaponStore;
    private YouthDilemmaResult? _youthDilemmaResult;
    private SoundEffect? _importedMusic;
    private SoundEffectInstance? _musicInstance;
    private readonly Dictionary<string, Texture2D> _originalArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte[]> _originalPalettes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, OriginalAnimation> _originalAnimations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DilemmaAnimation?> _dilemmaAnimations = new(StringComparer.OrdinalIgnoreCase);
    private byte[]? _dilemmaPalette;
    private double _presentationSeconds;
    private bool _hasActiveCampaign;
    private bool _cdMusicEnabled = true;
    private bool _soundEffectsEnabled = true;
    private bool _speechEnabled = true;
    private bool _animationEnabled = true;
    private readonly CampaignSaveSlots _saveSlots = new(Path.Combine(AppContext.BaseDirectory, "saves"));

    public ConquerorGame()
    {
        _graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1024, PreferredBackBufferHeight = 768 };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Conqueror: A.D. 1086 - MonoGame Reimplementation";
    }

    protected override void LoadContent()
    {
        _batch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _importedContent = ImportedContentCatalog.Discover();
        _importedDialogue = _importedContent is null ? null : new ImportedDialogueRepository(_importedContent);
        var weaponStoreId = _importedContent?.FindId("resource", ":weapons.dat");
        _weaponStore = weaponStoreId is null ? null : _importedContent?.DecodeWeaponStore(weaponStoreId);
        var optionsLayoutId = _importedContent?.FindId("resource", ":gameopts.hat");
        var optionsLayout = optionsLayoutId is null ? null : _importedContent?.DecodeHat(optionsLayoutId);
        _optionsHubOptions = OptionsHubDefinitions.OptionsFrom(optionsLayout);
        var characterLayoutId = _importedContent?.FindId("resource", ":cgopts.hat");
        var characterLayout = characterLayoutId is null ? null : _importedContent?.DecodeHat(characterLayoutId);
        _characterOptions = CharacterCreationDefinitions.OptionsFrom(characterLayout);
        _heraldicColors = CharacterCreationDefinitions.ColorsFrom(characterLayout);
        var pregeneratedLayoutId = _importedContent?.FindId("resource", ":pregen.hat");
        var pregeneratedLayout = pregeneratedLayoutId is null ? null : _importedContent?.DecodeHat(pregeneratedLayoutId);
        _pregeneratedBounds = CharacterCreationDefinitions.PregeneratedFrom(pregeneratedLayout);
        var dilemmaLayoutId = _importedContent?.FindId("resource", ":chargen.hat");
        var dilemmaLayout = dilemmaLayoutId is null ? null : _importedContent?.DecodeHat(dilemmaLayoutId);
        _dilemmaChoiceBounds = YouthDilemmaPresentationDefinitions.ChoicesFrom(dilemmaLayout);
        _dilemmaContinueBounds = YouthDilemmaPresentationDefinitions.ContinueFrom(dilemmaLayout);
        var blacksmithLayoutId = _importedContent?.FindId("resource", ":vsmith.hat");
        var blacksmithLayout = blacksmithLayoutId is null ? null : _importedContent?.DecodeHat(blacksmithLayoutId);
        _blacksmithBounds = BlacksmithPresentationDefinitions.BlacksmithFrom(blacksmithLayout);
        foreach (var definition in ImportedArt.Definitions)
        {
            var id = _importedContent?.FindId(definition.Kind, definition.IdSuffix);
            if (id is null || _importedContent?.DecodePcx(id) is not { } image) continue;
            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
            texture.SetData(image.ToRgba());
            _originalArt.Add(definition.Role, texture);
            _originalPalettes.Add(definition.Role, image.PaletteRgb);
            if (definition.Role == "Dilemma.Background") _dilemmaPalette = image.PaletteRgb;
        }
        foreach (var definition in ImportedAnimations.Definitions)
        {
            var id = _importedContent?.FindId("indexed-animation", definition.IdSuffix);
            if (id is null || _importedContent?.DecodeCsf(id) is not { } sequence
                || !_originalPalettes.TryGetValue(definition.PaletteArtRole, out var palette)) continue;
            var frames = sequence.Chunks.Select(chunk =>
            {
                var frame = sequence.DecodeFrame(chunk);
                var texture = new Texture2D(GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color);
                texture.SetData(frame.ToRgba(palette));
                return texture;
            }).ToArray();
            _originalAnimations.Add(definition.Role, new OriginalAnimation(frames));
        }
        if (_importedContent?.Open("CDDA/TRACK02") is { } music)
        {
            using (music)
            try
            {
                _importedMusic = SoundEffect.FromStream(music);
                _musicInstance = _importedMusic.CreateInstance();
                _musicInstance.IsLooped = true;
                _musicInstance.Volume = .35f;
                _musicInstance.Play();
            }
            catch (Exception error) when (error is InvalidDataException or NotSupportedException)
            {
                _notice = "IMPORTED AUDIO COULD NOT BE PLAYED";
            }
        }
    }

    protected override void UnloadContent()
    {
        _musicInstance?.Dispose();
        _importedMusic?.Dispose();
        foreach (var texture in _originalArt.Values) texture.Dispose();
        foreach (var animation in _originalAnimations.Values)
            foreach (var texture in animation.Frames) texture.Dispose();
        foreach (var animation in _dilemmaAnimations.Values.OfType<DilemmaAnimation>())
            foreach (var texture in animation.Frames) texture.Dispose();
        _pixel.Dispose();
        _batch.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _presentationSeconds += gameTime.ElapsedGameTime.TotalSeconds;
        var keys = Keyboard.GetState();
        var mouse = Mouse.GetState();
        bool Press(Keys key) => keys.IsKeyDown(key) && !_last.IsKeyDown(key);
        var click = mouse.LeftButton == ButtonState.Pressed && _lastMouse.LeftButton == ButtonState.Released;
        var pressAny = keys.GetPressedKeys().Any(key => key != Keys.Escape && !_last.IsKeyDown(key));
        if (Press(Keys.F5) && _screen is not Screen.Title and not Screen.LoadGame)
        {
            _saveSlots.Save(_campaign, _activeSaveSlot);
            _notice = $"CAMPAIGN SAVED IN SLOT {_activeSaveSlot}";
        }
        if (Press(Keys.F9)) OpenLoadGame();
        if (Press(Keys.Escape))
        {
            if (_screen == Screen.Title) Exit();
            else if (_screen == Screen.OptionsHub) _screen = Screen.Title;
            else if (_screen == Screen.LoadGame) ResumeFromLoadGame();
            else if (_screen == Screen.CharacterName) _screen = Screen.CharacterOptions;
            else if (_screen == Screen.CharacterOptions) _screen = Screen.Title;
            else if (_screen == Screen.Character) _screen = Screen.CharacterOptions;
            else if (_screen == Screen.Farm) _screen = Screen.Home;
            else if (_screen == Screen.Blacksmith) _screen = Screen.Village;
            else if (_screen == Screen.Shop) _screen = Screen.Blacksmith;
            else if (_screen == Screen.FieldBattle && _fieldBattle is not null) { _fieldBattle.IssueAll(UnitOrder.Withdraw); _notice = "WITHDRAWAL ORDERED"; }
            else { if (_screen == Screen.Siege && _siege is not null) _campaign.FinishSiege(_siege); _screen = Screen.Map; }
        }

        switch (_screen)
        {
            case Screen.Title:
                if (pressAny || click) _screen = Screen.OptionsHub;
                break;
            case Screen.OptionsHub: UpdateOptionsHub(Press, mouse, click); break;
            case Screen.LoadGame: UpdateLoadGame(Press, mouse, click); break;
            case Screen.CharacterOptions: UpdateCharacterOptions(Press, mouse, click); break;
            case Screen.CharacterName: UpdateCharacterName(Press); break;
            case Screen.Character:
                UpdatePregeneratedCharacters(Press, mouse, click);
                break;
            case Screen.Dilemma: UpdateDilemma(Press, mouse, click); break;
            case Screen.Map: UpdateMap(Press); break;
            case Screen.Home: UpdateHome(Press); break;
            case Screen.Farm: UpdateFarm(Press); break;
            case Screen.Village: UpdateVillage(Press); break;
            case Screen.Blacksmith: UpdateBlacksmith(Press, mouse, click); break;
            case Screen.Shop: UpdateShop(Press, mouse, click); break;
            case Screen.Tournament: UpdateTournament(Press); break;
            case Screen.FieldBattle: UpdateFieldBattle(Press, gameTime); break;
            case Screen.Siege: UpdateSiege(Press); break;
            case Screen.Overview: if (Press(Keys.Enter) || Press(Keys.O)) _screen = Screen.Map; break;
            case Screen.Ending: if (Press(Keys.Enter)) _screen = Screen.Title; break;
        }
        if (_campaign.State.Victory != VictoryKind.None && _screen is not Screen.Title and not Screen.LoadGame) _screen = Screen.Ending;
        _last = keys;
        _lastMouse = mouse;
        base.Update(gameTime);
    }

    private void OpenLoadGame()
    {
        if (_screen != Screen.LoadGame) _loadReturnScreen = _screen;
        _saveSlotInfo = _saveSlots.Inspect();
        _loadSlot = Math.Clamp(_activeSaveSlot - 1, 0, CampaignSaveSlots.SlotCount - 1);
        _screen = Screen.LoadGame;
        _notice = "SELECT A SAVED CAMPAIGN OR RESUME";
    }

    private void UpdateOptionsHub(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.Up)) _optionsHubOption = (_optionsHubOption + _optionsHubOptions.Count - 1) % _optionsHubOptions.Count;
        if (press(Keys.Down)) _optionsHubOption = (_optionsHubOption + 1) % _optionsHubOptions.Count;
        var selected = press(Keys.Enter) ? _optionsHubOption : -1;
        if (selected < 0 && click)
        {
            var (x, y) = OriginalPoint(mouse);
            selected = Enumerable.Range(0, _optionsHubOptions.Count)
                .FirstOrDefault(index => _optionsHubOptions[index].OriginalBounds.Contains(x, y), -1);
        }
        if (selected >= 0) ActivateOptionsHub(_optionsHubOptions[selected]);
    }

    private void ActivateOptionsHub(OptionsHubOption option)
    {
        if (option.RequiresCampaign && !_hasActiveCampaign)
        {
            _notice = $"{option.Label.ToUpperInvariant()} REQUIRES AN ACTIVE CAMPAIGN";
            return;
        }

        switch (option.Action)
        {
            case OptionsHubAction.NewGame: _screen = Screen.CharacterOptions; break;
            case OptionsHubAction.Load: OpenLoadGame(); break;
            case OptionsHubAction.Save:
                _saveSlots.Save(_campaign, _activeSaveSlot);
                _notice = $"CAMPAIGN SAVED IN SLOT {_activeSaveSlot}";
                break;
            case OptionsHubAction.Resume: _screen = CampaignScreen(); break;
            case OptionsHubAction.ToggleCdMusic:
                _cdMusicEnabled = !_cdMusicEnabled;
                if (_cdMusicEnabled) _musicInstance?.Resume(); else _musicInstance?.Pause();
                _notice = $"CD MUSIC {OnOff(_cdMusicEnabled)}";
                break;
            case OptionsHubAction.ToggleSoundEffects:
                _soundEffectsEnabled = !_soundEffectsEnabled;
                _notice = $"SOUND EFFECTS {OnOff(_soundEffectsEnabled)}";
                break;
            case OptionsHubAction.ToggleSpeech:
                _speechEnabled = !_speechEnabled;
                _notice = $"DIGITIZED SPEECH {OnOff(_speechEnabled)}";
                break;
            case OptionsHubAction.ToggleAnimation:
                _animationEnabled = !_animationEnabled;
                _notice = $"ANIMATION {OnOff(_animationEnabled)}";
                break;
            case OptionsHubAction.ToggleMidiMusic: _notice = "MIDI MUSIC IS NOT AVAILABLE"; break;
            case OptionsHubAction.Practice: _notice = "PRACTICE MODE IS NOT IMPLEMENTED YET"; break;
            case OptionsHubAction.Credits: _notice = "CREDITS PRESENTATION IS NOT IMPLEMENTED YET"; break;
            case OptionsHubAction.Movie: _notice = "MOVIE PLAYBACK IS NOT IMPLEMENTED YET"; break;
            case OptionsHubAction.Exit: Exit(); break;
            default: throw new ArgumentOutOfRangeException(nameof(option));
        }
    }

    private Screen CampaignScreen() => _campaign.State.YouthDilemmasAnswered < Youth.OriginalPool.StageCount
        && _campaign.State.Player.Age < Balance.StartingAge ? Screen.Dilemma : Screen.Map;

    private static string OnOff(bool enabled) => enabled ? "ON" : "OFF";

    private void UpdateLoadGame(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.R))
        {
            ResumeFromLoadGame();
            return;
        }
        if (press(Keys.Up)) _loadSlot = (_loadSlot + CampaignSaveSlots.SlotCount - 1) % CampaignSaveSlots.SlotCount;
        if (press(Keys.Down)) _loadSlot = (_loadSlot + 1) % CampaignSaveSlots.SlotCount;
        for (var i = 0; i < CampaignSaveSlots.SlotCount; i++)
            if (press(Keys.D1 + i)) LoadSlot(i + 1);
        if (press(Keys.Enter)) LoadSlot(_loadSlot + 1);
        if (!click) return;

        var (x, y) = OriginalPoint(mouse);
        if (LoadGameDefinitions.Resume.Contains(x, y))
        {
            ResumeFromLoadGame();
            return;
        }
        for (var i = 0; i < LoadGameDefinitions.Slots.Count; i++)
            if (LoadGameDefinitions.Slots[i].Contains(x, y))
            {
                _loadSlot = i;
                LoadSlot(i + 1);
                return;
            }
    }

    private void LoadSlot(int number)
    {
        if (!_saveSlots.TryLoad(number, out var campaign, out var error))
        {
            _notice = error ?? "CAMPAIGN COULD NOT BE LOADED";
            _saveSlotInfo = _saveSlots.Inspect();
            return;
        }

        _campaign = campaign!;
        _hasActiveCampaign = true;
        _fieldBattle = null;
        _siege = null;
        _youthDilemmaResult = null;
        _activeSaveSlot = number;
        _selectedLocation = _campaign.State.CurrentLocation;
        _screen = CampaignScreen();
        _notice = $"CAMPAIGN LOADED FROM SLOT {number}";
    }

    private void ResumeFromLoadGame()
    {
        _screen = _loadReturnScreen;
        _notice = "";
    }

    private void UpdateCharacterOptions(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        var options = _characterOptions;
        if (press(Keys.Up)) _characterOption = (_characterOption + options.Count - 1) % options.Count;
        if (press(Keys.Down)) _characterOption = (_characterOption + 1) % options.Count;
        for (var i = 0; i < options.Count; i++) if (press(Keys.D1 + i)) ActivateCharacterOption(options[i].Action);
        if (press(Keys.Enter)) ActivateCharacterOption(options[_characterOption].Action);
        if (!click) return;
        var (x, y) = OriginalPoint(mouse);
        for (var i = 0; i < _heraldicColors.Count; i++)
            if (_heraldicColors[i].OriginalBounds.Contains(x, y))
            {
                _heraldicColor = i;
                _characterOption = 1;
                _notice = $"HERALDIC COLOR: {_heraldicColors[i].Name}";
                return;
            }
        for (var i = 0; i < options.Count; i++)
            if (options[i].OriginalBounds.Contains(x, y)) { _characterOption = i; ActivateCharacterOption(options[i].Action); break; }
    }

    private void ActivateCharacterOption(CharacterCreationAction action)
    {
        switch (action)
        {
            case CharacterCreationAction.ChooseName:
                _screen = Screen.CharacterName;
                break;
            case CharacterCreationAction.ChooseColor:
                _heraldicColor = (_heraldicColor + 1) % _heraldicColors.Count;
                _notice = $"HERALDIC COLOR: {_heraldicColors[_heraldicColor].Name}";
                break;
            case CharacterCreationAction.GenerateNew:
                var seed = Environment.TickCount;
                _campaign = new Campaign(Campaign.NewCustom(NormalizedCharacterName(), seed, _heraldicColors[_heraldicColor].Name), seed);
                _hasActiveCampaign = true;
                _youthDilemmaResult = null;
                _screen = Screen.Dilemma;
                break;
            case CharacterCreationAction.ChoosePregenerated:
                _screen = Screen.Character;
                break;
        }
    }

    private void UpdateCharacterName(Func<Keys, bool> press)
    {
        if (press(Keys.Back) && _characterName.Length > 4) _characterName = _characterName[..^1];
        if (press(Keys.Space) && _characterName.Length < 24 && !_characterName.EndsWith(' ')) _characterName += ' ';
        for (var value = (int)Keys.A; value <= (int)Keys.Z && _characterName.Length < 24; value++)
            if (press((Keys)value)) _characterName += ((char)('A' + value - (int)Keys.A)).ToString();
        if (press(Keys.Enter)) _screen = Screen.CharacterOptions;
    }

    private void UpdatePregeneratedCharacters(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.Left)) _characterTemplate = (_characterTemplate + Balance.Templates.Length - 1) % Balance.Templates.Length;
        if (press(Keys.Right)) _characterTemplate = (_characterTemplate + 1) % Balance.Templates.Length;
        if (press(Keys.Up) || press(Keys.Down)) _characterTemplate = (_characterTemplate + 3) % Balance.Templates.Length;
        for (var i = 0; i < Balance.Templates.Length; i++) if (press(Keys.D1 + i)) StartPregeneratedCharacter(i);
        if (press(Keys.Enter)) StartPregeneratedCharacter(_characterTemplate);
        if (!click) return;
        var (x, y) = OriginalPoint(mouse);
        for (var i = 0; i < _pregeneratedBounds.Count; i++)
            if (_pregeneratedBounds[i].Contains(x, y)) { _characterTemplate = i; StartPregeneratedCharacter(i); break; }
    }

    private void StartPregeneratedCharacter(int index)
    {
        _campaign = new Campaign(Campaign.NewFromTemplate(index));
        _hasActiveCampaign = true;
        _screen = Screen.Map;
    }

    private string NormalizedCharacterName() => _characterName.Trim() is { Length: > 4 } name ? name : "Sir Custom";
    private (int X, int Y) OriginalPoint(MouseState mouse) =>
        (mouse.X * 640 / Math.Max(1, GraphicsDevice.Viewport.Width), mouse.Y * 480 / Math.Max(1, GraphicsDevice.Viewport.Height));

    private void UpdateMap(Func<Keys, bool> press)
    {
        if (_campaign.HasPendingFieldBattle)
        {
            BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED");
            return;
        }
        if (press(Keys.Left) || press(Keys.Up)) _selectedLocation = (_selectedLocation + World.Locations.Length - 1) % World.Locations.Length;
        if (press(Keys.Right) || press(Keys.Down)) _selectedLocation = (_selectedLocation + 1) % World.Locations.Length;
        if (press(Keys.Enter))
        {
            var days = _campaign.TravelTo(_selectedLocation);
            _notice = days == 0 ? $"ALREADY AT {World.Locations[_selectedLocation].Name}" : $"TRAVELLED {days} DAYS TO {World.Locations[_selectedLocation].Name}";
            if (_campaign.HasPendingFieldBattle) { BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED"); return; }
        }
        if (press(Keys.H) && _campaign.State.CurrentLocation == 0) _screen = Screen.Home;
        if (press(Keys.V)) _screen = Screen.Village;
        if (press(Keys.T) && _campaign.IsTournamentHere) _screen = Screen.Tournament;
        if (press(Keys.O)) _screen = Screen.Overview;
        if (press(Keys.S) && _campaign.StartSiege(_selectedLocation)) { _siege = _campaign.CreateSiege(); _showRadar = true; _screen = Screen.Siege; }
        if (press(Keys.B))
        {
            if (_campaign.State.Player.Army.Total == 0) _notice = "YOU HAVE NO ARMY";
            else if (_campaign.CanStartFieldBattle) BeginFieldBattle("YOU CHALLENGE THE GARRISON");
            else _notice = "THERE IS NO HOSTILE FIELD ARMY HERE";
        }
        if (press(Keys.P)) _notice = _campaign.SendSpy(_selectedLocation)
            ? $"SPY REPORTS {_campaign.GarrisonAt(_selectedLocation)} SOLDIERS AT {World.Locations[_selectedLocation].Name}"
            : "SPY NOT SENT (NEED HOSTILE CASTLE AND 80S)";
        if (press(Keys.C)) _notice = "TO CLAIM THE CROWN, TRAVEL TO LONDON AND PRESS S TO BESIEGE IT";
        if (press(Keys.D)) _campaign.AttemptDragon();
        if (press(Keys.E)) _campaign.AdvanceDays(_campaign.State.DaySpeed);
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.DaySpeed = Math.Min(15, _campaign.State.DaySpeed + 1);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.DaySpeed = Math.Max(1, _campaign.State.DaySpeed - 1);
    }

    private void BeginFieldBattle(string notice)
    {
        _fieldBattle = _campaign.CreateFieldBattle();
        _battleTick = 0;
        _screen = Screen.FieldBattle;
        _notice = notice;
    }

    private void UpdateDilemma(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (_youthDilemmaResult is not null)
        {
            var (x, y) = OriginalPoint(mouse);
            if (press(Keys.Enter) || press(Keys.Space) || click && _dilemmaContinueBounds.Contains(x, y))
            {
                _youthDilemmaResult = null;
                CompleteYouthIfReady();
            }
            return;
        }

        var selectedChoice = Enumerable.Range(0, _dilemmaChoiceBounds.Count)
            .FirstOrDefault(index => press(Keys.D1 + index), -1);
        if (selectedChoice < 0 && click)
        {
            var (x, y) = OriginalPoint(mouse);
            selectedChoice = Enumerable.Range(0, _dilemmaChoiceBounds.Count)
                .FirstOrDefault(index => _dilemmaChoiceBounds[index].Contains(x, y), -1);
        }
        if (selectedChoice < 0) return;

        var imported = CurrentImportedDilemma();
        if (imported is not null) _youthDilemmaResult = _campaign.AnswerDilemma(imported, selectedChoice);
        else _campaign.AnswerDilemma(selectedChoice);
        if (_youthDilemmaResult is null) CompleteYouthIfReady();
    }

    private YouthDilemmaDefinition? CurrentImportedDilemma() =>
        _importedDialogue?.GetPlayableDilemma(_campaign.CurrentYouthDilemmaNumber);

    private void CompleteYouthIfReady()
    {
        if (_campaign.State.YouthDilemmasAnswered < Youth.OriginalPool.StageCount) return;
        _selectedLocation = 0;
        _screen = Screen.Map;
        _notice = "YOUR YEARS OF TRAINING ARE COMPLETE";
    }

    private void UpdateHome(Func<Keys, bool> press)
    {
        if (press(Keys.F)) _screen = Screen.Farm;
        if (press(Keys.V)) _screen = Screen.Village;
        if (press(Keys.Enter) || press(Keys.H)) _screen = Screen.Map;
    }

    private void UpdateFarm(Func<Keys, bool> press)
    {
        if (press(Keys.D1)) _campaign.Build("Steward"); if (press(Keys.D2)) _campaign.Build("Beadle");
        if (press(Keys.D3)) _campaign.Build("Priest"); if (press(Keys.D4)) _campaign.Build("Servant Room");
        if (press(Keys.D5)) _campaign.Build("House"); if (press(Keys.D6)) _campaign.Build("Monastery");
        if (press(Keys.D7)) _campaign.Plant(CropType.Beans); if (press(Keys.D8)) _campaign.Plant(CropType.Vegetables);
        if (press(Keys.Z)) _campaign.Plant(CropType.Grain); if (press(Keys.X)) _campaign.Plant(CropType.Fruit);
        if (press(Keys.D9)) _campaign.DevelopForest(ForestIndustry.Timber); if (press(Keys.G)) _campaign.DevelopForest(ForestIndustry.GoldMine);
        if (press(Keys.I)) _campaign.DevelopForest(ForestIndustry.IronMine); if (press(Keys.C)) _campaign.DevelopForest(ForestIndustry.CoalMine); if (press(Keys.S)) _campaign.DevelopForest(ForestIndustry.SilverMine);
        if (press(Keys.Q)) _campaign.Recruit(UnitType.Swordsmen); if (press(Keys.W)) _campaign.Recruit(UnitType.Halberdiers); if (press(Keys.R)) _campaign.Recruit(UnitType.Knights);
        if (press(Keys.Enter) || press(Keys.H)) _screen = Screen.Home;
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
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.Player.Home.TaxRate = Math.Min(100, _campaign.State.Player.Home.TaxRate + 5);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.Player.Home.TaxRate = Math.Max(0, _campaign.State.Player.Home.TaxRate - 5);
        if (press(Keys.Enter) || press(Keys.V)) _screen = Screen.Map;
    }

    private void UpdateBlacksmith(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.Enter) || press(Keys.B))
        {
            _shopIndex = 0;
            _screen = Screen.Shop;
            return;
        }
        if (!click) return;
        var (x, y) = OriginalPoint(mouse);
        if (_blacksmithBounds.Contains(x, y))
        {
            _shopIndex = 0;
            _screen = Screen.Shop;
        }
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
        _notice = imported?.HasMovie == true ? "ITEM MOVIE PLAYBACK IS NOT IMPLEMENTED YET" : "NO ITEM VIEW IS AVAILABLE";
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
        if (press(Keys.C)) _notice = _campaign.RequestColors(Balance.Courtships[_ladyIndex].Name) ? $"WEARING {Balance.Courtships[_ladyIndex].Name}'S COLORS" : "YOUR REQUEST IS REFUSED";
        if (press(Keys.Space)) _notice = _campaign.Joust(Math.Abs(100 - _joustCursor), _tournamentOpponent) ? "A CLEAN STRIKE - YOU WIN THE WAGER" : "YOU MISS, CANNOT PAY, OR THE LISTS ARE CLOSED";
        if (press(Keys.K)) _notice = _campaign.TournamentSkirmish(_tournamentOpponent) is { } result ? result.Summary : "THE SKIRMISH IS UNAVAILABLE";
        if (press(Keys.Enter) || press(Keys.T)) _screen = Screen.Map;
    }

    private void UpdateSiege(Func<Keys, bool> press)
    {
        if (_siege is null) { _screen = Screen.Map; return; }
        if (press(Keys.W)) _siege.Move(true); if (press(Keys.S)) _siege.Move(false);
        if (press(Keys.A)) _siege.TurnLeft(); if (press(Keys.D)) _siege.TurnRight();
        if (press(Keys.E)) _siege.Interact(); if (press(Keys.Space)) _siege.Attack(); if (press(Keys.X)) _siege.Shoot();
        if (press(Keys.M)) _showRadar = !_showRadar;
        _notice = _siege.LastMessage;
        if (press(Keys.R))
        {
            _campaign.FinishSiege(_siege); var lost = _campaign.Retreat(); _siege = null; _screen = Screen.Map; _notice = $"RETREATED - {lost} SOLDIERS LOST"; return;
        }
        if (_siege.Won) { _campaign.FinishSiege(_siege); _siege = null; _screen = Screen.Map; _notice = "THE CASTLE IS YOURS"; }
        else if (_siege.Defeated) { _campaign.FinishSiege(_siege); _siege = null; _screen = Screen.Map; _notice = "YOU ARE CARRIED FROM THE CASTLE"; }
    }

    private void UpdateFieldBattle(Func<Keys, bool> press, GameTime gameTime)
    {
        if (_fieldBattle is null) { _screen = Screen.Map; return; }
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
            var outcome = _campaign.FinishFieldBattle(_fieldBattle); _fieldBattle = null; _screen = Screen.Map; _notice = $"FIELD BATTLE: {outcome}";
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(20, 25, 20));
        _batch.Begin(samplerState: SamplerState.PointClamp);
        switch (_screen)
        {
            case Screen.Title: DrawTitle(); break; case Screen.OptionsHub: DrawOptionsHub(); break; case Screen.LoadGame: DrawLoadGame(); break; case Screen.CharacterOptions: DrawCharacterOptions(); break; case Screen.CharacterName: DrawCharacterName(); break; case Screen.Character: DrawCharacter(); break; case Screen.Dilemma: DrawDilemma(); break; case Screen.Map: DrawMap(); break;
            case Screen.Home: DrawHome(); break; case Screen.Farm: DrawFarm(); break; case Screen.Village: DrawVillage(); break; case Screen.Blacksmith: DrawBlacksmith(); break; case Screen.Shop: DrawShop(); break; case Screen.Tournament: DrawTournament(); break; case Screen.FieldBattle: DrawFieldBattle(); break;
            case Screen.Siege: DrawSiege(); break; case Screen.Overview: DrawOverview(); break; case Screen.Ending: DrawEnding(); break;
        }
        if (_screen is not Screen.Title and not Screen.LoadGame and not Screen.Character and not Screen.Dilemma) DrawText(_notice, 24, 730, Color.Gold, 2);
        _batch.End();
        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _musicInstance?.Dispose();
            _importedMusic?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void DrawTitle()
    {
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

        foreach (var option in _optionsHubOptions.Where(option => option.Setting.HasValue))
        {
            var frameIndex = SettingEnabled(option.Setting!.Value)
                ? OptionsHubDefinitions.EnabledStatusFrame
                : OptionsHubDefinitions.DisabledStatusFrame;
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
            var details = info.IsValid ? $"{info.PlayerName}   {info.CampaignDate:dd MMM yyyy}" : info.PlayerName;
            DrawText(details.ToUpperInvariant(), bounds.X + 90, bounds.Y + 10, info.IsValid ? Color.White : Color.LightGray, 2, bounds.Width - 110);
            if (i == _loadSlot) DrawOutline(bounds, Color.Gold, 3);
        }

        if (!original) DrawText("ESC OR R  RESUME", 150, 640, Color.LightGreen, 2);
        DrawText("ARROWS/1-5 LOAD   ESC RESUME", 250, 710, Color.Wheat, 2);
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

    private void DrawDilemmaAnimation(YouthDilemmaDefinition dilemma)
    {
        var animation = GetDilemmaAnimation(dilemma.SceneFile);
        if (animation is null) return;
        var localFrame = _animationEnabled ? (int)(_presentationSeconds * 5) % animation.FramesPerChoice : 0;
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
        if (!DrawOriginal("Map.England", new Rectangle(0, 0, 760, 768)))
            Fill(new Rectangle(0, 0, 760, 768), new Color(71, 92, 58));
        for (var i = 0; i < World.Locations.Length; i++)
        {
            var place = World.Locations[i];
            var owned = i == 0 || _campaign.State.ConqueredLocations.Contains(i);
            var color = i == _selectedLocation ? Color.Gold : owned ? Color.LightGreen : place.Kind == LocationKind.DragonLair ? Color.DarkRed : Color.White;
            Fill(new Rectangle(place.X, place.Y, i == _selectedLocation ? 13 : 8, i == _selectedLocation ? 13 : 8), color);
            if (i == _selectedLocation) DrawText(place.Name, Math.Max(5, place.X - 30), Math.Max(5, place.Y - 25), Color.Gold, 1);
        }
        var current = World.Locations[_campaign.State.CurrentLocation];
        Fill(new Rectangle(current.X - 5, current.Y - 5, 18, 18), Color.Red);
        Fill(new Rectangle(760, 0, 264, 768), new Color(47, 36, 28));
        var p = _campaign.State.Player; var f = p.Home;
        var selected = World.Locations[_selectedLocation];
        var intel = _campaign.HasGarrisonIntel(_selectedLocation) ? _campaign.GarrisonAt(_selectedLocation).ToString() : "UNKNOWN";
        DrawText("ENGLAND", 820, 25, Color.Gold, 3); DrawText($"{_campaign.State.Date:DD MMM YYYY}", 785, 80, Color.White, 2);
        DrawText(p.Name, 780, 125, Color.Wheat, 2, 230); DrawText($"AGE {p.Age}   WEALTH {p.Wealth}S", 780, 180, Color.White, 2);
        DrawText($"FIEFS {p.Fiefs}  VILLAGES {p.Villages}", 780, 215, Color.White, 2); DrawText($"ARMY {p.Army.Total}  FAME {p.Fame}", 780, 250, Color.White, 2);
        DrawText($"PRODUCTIVITY {f.Productivity()}%", 780, 285, Color.White, 2);
        var tournament = World.Locations[World.TournamentIndex(_campaign.State.Date)].Name;
        DrawText($"AT {current.Name}", 780, 320, Color.Gold, 2, 230);
        DrawText($"TARGET {selected.Name} GARRISON {intel}", 780, 350, Color.Wheat, 2, 230);
        DrawText("ARROWS SELECT ENTER TRAVEL", 780, 395, Color.LightGreen, 2, 230); DrawText("H HOME   V VILLAGE", 780, 440, Color.LightGreen, 2);
        DrawText("T TOURNAMENT O OVERVIEW", 780, 475, Color.LightGreen, 2, 230);
        DrawText("S SIEGE   B FIELD BATTLE", 780, 520, Color.LightGreen, 2, 230); DrawText("P SPY 80S   D DRAGON", 780, 555, Color.LightGreen, 2, 230);
        DrawText($"TOURNAMENT {tournament}", 780, 595, Color.Wheat, 2, 230); DrawText("E ADVANCE  PLUS MINUS SPEED", 780, 635, Color.LightGreen, 2, 230);
        DrawText("F5 SAVE  F9 LOAD", 780, 690, Color.Gold, 2);
    }

    private void DrawHome()
    {
        var original = DrawOriginal("Home.Office", new Rectangle(0, 0, 1024, 768));
        if (!original) DrawPanel("CASTLE OFFICE", "MANAGE YOUR FIEF OR RETURN TO THE ROAD");
        DrawText("F  FARM MANAGEMENT    V  VILLAGE    ENTER  MAP", 80, 715, Color.Wheat, 2);
    }

    private void DrawFarm()
    {
        var f = _campaign.State.Player.Home; var p = _campaign.State.Player;
        var original = DrawOriginal("Farm.Management", new Rectangle(0, 0, 1024, 768));
        if (!original) DrawPanel("FARM MANAGEMENT", $"WEALTH {p.Wealth}S  POPULATION {f.Population}  PRODUCTIVITY {f.Productivity()}%");
        else DrawFarmTerrain(FarmPresentationDefinitions.Terrain);

        var accountColor = original ? Color.Black : Color.White;
        DrawText("FIEF ACCOUNTS", 55, 70, accountColor, 3);
        DrawText($"WEALTH          {p.Wealth}", 55, 120, accountColor, 2);
        DrawText($"POPULATION      {f.Population}", 55, 155, accountColor, 2);
        DrawText($"SERFS AVAILABLE {f.AvailableSerfs}", 55, 190, accountColor, 2);
        DrawText($"PRODUCTIVITY    {f.Productivity()}%", 55, 225, accountColor, 2);
        DrawText($"HOUSES          {f.Houses}", 55, 260, accountColor, 2);
        DrawText("1-6 STAFF/BUILD   7/8/Z/X CROPS", 55, 545, Color.Wheat, 2, 500);
        DrawText("9/G/I/C/S FOREST   Q/W/R RECRUIT", 55, 585, Color.Wheat, 2, 500);
        DrawText("ENTER OR ESC  OFFICE", 55, 710, Color.Gold, 2);
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
        DrawText($"PLUS MINUS TAX RATE  {_campaign.State.Player.Home.TaxRate}%", 100, 455, Color.Wheat, 2);
        DrawText("ENTER RETURN TO MAP", 100, 520, Color.LightGreen);
    }

    private void DrawBlacksmith()
    {
        if (!DrawOriginal("Blacksmith.Workshop", new Rectangle(0, 0, 1024, 768)))
            DrawPanel("THE BLACKSMITH", "SELECT THE SMITH TO BROWSE HIS WARES");
        DrawOutline(ScaleBounds(_blacksmithBounds), Color.Gold, 2);
        DrawText("ENTER OR B  BROWSE WARES    ESC  VILLAGE", 120, 715, Color.Wheat, 2);
    }

    private void DrawShop()
    {
        var stock = Balance.StoreEquipment;
        var p = _campaign.State.Player;
        var item = stock[_shopIndex];
        if (DrawOriginal("Shop.Inventory", new Rectangle(0, 0, 1024, 768)))
        {
            var imported = ImportedStoreEntry(item);
            DrawStoreItem(imported);
            DrawShopOverlay(ShopPresentationDefinitions.ViewOverlay(imported?.HasMovie == true));
            DrawShopOverlay(ShopPresentationDefinitions.TransactionOverlay(p.Inventory.Items.Contains(item.Name)));
            var descriptionBounds = ScaleBounds(ShopPresentationDefinitions.Description);
            var fallback = item.Power > 0 ? $"{item.Name} - fighting power {item.Power}." : $"{item.Name} - armor protection {item.Armor}.";
            DrawText(imported?.Description ?? fallback, descriptionBounds.X, descriptionBounds.Y, Color.Black, 2, descriptionBounds.Width);
            var wealthBounds = ScaleBounds(ShopPresentationDefinitions.Wealth);
            var priceBounds = ScaleBounds(ShopPresentationDefinitions.Price);
            DrawText($"WEALTH\n{p.Wealth}", wealthBounds.X, wealthBounds.Y, Color.White, 2);
            DrawText($"BUY\n{imported?.Price ?? item.BuyPrice}", priceBounds.X, priceBounds.Y, Color.White, 2);
            return;
        }

        DrawPanel("THE BLACKSMITH", $"WEALTH {p.Wealth}S   B BUY   S SELL FOR 75%   ENTER LEAVE");
        var first = Math.Clamp(_shopIndex - 3, 0, Math.Max(0, stock.Length - 8));
        for (var row = 0; row < 8 && first + row < stock.Length; row++)
        {
            var index = first + row; var rowItem = stock[index]; var owned = p.Inventory.Items.Contains(rowItem.Name);
            var details = rowItem.Power > 0 ? $"POWER {rowItem.Power}" : $"ARMOR {rowItem.Armor}";
            DrawText($"{(index == _shopIndex ? ">" : " ")} {rowItem.Name}  {rowItem.BuyPrice}S  {details} {(owned ? "OWNED" : "")}", 85, 180 + row * 58, index == _shopIndex ? Color.Gold : owned ? Color.LightGreen : Color.White, 2);
        }
        DrawText($"EQUIPPED: {p.Inventory.Weapon} / {p.Inventory.Armor} / {p.Inventory.Shield} / {p.Inventory.Helm}", 75, 660, Color.Wheat, 2, 870);
    }

    private WeaponStoreEntry? ImportedStoreEntry(EquipmentBalance item) => item.OriginalStoreRecord is { } record
        && _weaponStore?.Entries.ElementAtOrDefault(record) is { } imported
        ? imported
        : null;

    private void DrawStoreItem(WeaponStoreEntry? item)
    {
        if (item is null || !_originalAnimations.TryGetValue("Shop.Items", out var animation)
            || item.ImageFrame >= animation.Frames.Count) return;
        var frame = animation.Frames[item.ImageFrame];
        _batch.Draw(frame, ScaleBounds(ShopPresentationDefinitions.ItemBounds(frame.Width, frame.Height)), Color.White);
    }

    private void DrawShopOverlay(ShopOverlay overlay)
    {
        if (!_originalAnimations.TryGetValue("Shop.Controls", out var animation)
            || overlay.Frame >= animation.Frames.Count) return;
        var frame = animation.Frames[overlay.Frame];
        _batch.Draw(frame, ScaleBounds(ShopPresentationDefinitions.OverlayBounds(overlay, frame.Width, frame.Height)), Color.White);
    }

    private void DrawTournament()
    {
        DrawPanel("THE TOURNAMENT", "JOUST UP TO THREE TIMES, SKIRMISH, WAGER, AND COURT THE LADIES");
        Fill(new Rectangle(110, 310, 800, 12), Color.DarkGoldenrod); Fill(new Rectangle(500, 275, 12, 80), Color.Gold);
        Fill(new Rectangle(110 + _joustCursor * 4, 290, 8, 52), Color.White);
        DrawText("PRESS SPACE WHEN THE LANCE MEETS THE GOLD MARK", 170, 390, Color.White, 2);
        DrawText($"JOUSTS {_campaign.State.JoustsThisTournament}/3", 400, 445, Color.Gold, 2);
        var opponent = Balance.TournamentOpponents[_tournamentOpponent];
        if (_originalArt.TryGetValue($"Tournament.{opponent.Name}", out var opponentPortrait))
        {
            Fill(new Rectangle(816, 150, 154, 166), Color.Gold);
            _batch.Draw(opponentPortrait, new Rectangle(820, 154, 146, 158), Color.White);
        }
        DrawText($"LEFT RIGHT OPPONENT: {opponent.Name}   WAGER {opponent.Wager}S", 150, 470, Color.LightGreen, 2);
        var lady = Balance.Courtships[_ladyIndex];
        var wins = _campaign.State.Player.CourtshipWins.GetValueOrDefault(lady.Name);
        DrawText($"UP DOWN LADY: {lady.Name}   WINS {wins}   C REQUEST COLORS", 150, 515, lady.CourtAble ? Color.Wheat : Color.Gray, 2);
        DrawText($"COLORS: {_campaign.State.Player.LadyColors ?? "NONE"}", 150, 555, Color.Gold, 2);
        DrawText("SPACE JOUST   K SKIRMISH   ENTER LEAVE", 250, 610, Color.LightGreen, 2);
    }

    private void DrawSiege()
    {
        if (_siege is null) return;
        Fill(new Rectangle(0, 0, 1024, 768), new Color(22, 19, 18));
        for (var i = 0; i < 16; i++)
        {
            var shade = (byte)(35 + i * 5); var width = 1024 - i * 55; var height = 768 - i * 38;
            Fill(new Rectangle((1024 - width) / 2, (768 - height) / 2, width, 18), new Color(shade, shade, shade));
        }
        Fill(new Rectangle(395, 170, 235, 430), new Color(70, 58, 48));
        var distance = _siege.VisibleEnemyDistance();
        if (distance > 0)
        {
            var size = Math.Max(35, 180 / distance); var enemy = _siege.Enemies.FirstOrDefault(x => Math.Abs(x.X - _siege.PlayerX) + Math.Abs(x.Y - _siege.PlayerY) == distance);
            Fill(new Rectangle(512 - size / 2, 410 - size, size, size * 2), enemy?.Champion == true ? Color.DarkRed : new Color(120, 75, 50));
            Fill(new Rectangle(512 - size / 3, 390 - size, size * 2 / 3, size * 2 / 3), Color.Gray);
        }
        DrawText($"HEALTH {_siege.Health}/{_siege.MaxHealth}  ENEMIES {_siege.Enemies.Count}  ALLIES {_siege.AlliesAlive}", 25, 25, Color.White, 2);
        DrawText($"FACING {_siege.Facing}  ARMOR {_siege.ArmorRating()}  GOLD FOUND {_siege.GoldFound}", 25, 55, Color.Wheat, 2);
        if (_showRadar) DrawRadar(_siege);
        DrawText("W/S MOVE  A/D TURN  E OPEN  SPACE SWING  X CROSSBOW", 130, 655, Color.Gold, 2);
        DrawText("M RADAR  R RETREAT", 380, 685, Color.Gold, 2);
    }

    private void DrawFieldBattle()
    {
        if (_fieldBattle is null) return;
        Fill(new Rectangle(0, 0, 1024, 768), new Color(75, 95, 52));
        Fill(new Rectangle(45, 75, 934, 500), new Color(96, 116, 66));
        for (var x = 0; x <= FieldBattleSession.Rules.Width; x++) Fill(new Rectangle(45 + x * 66, 75, 1, 500), new Color(70, 85, 50));
        for (var y = 0; y <= FieldBattleSession.Rules.Height; y++) Fill(new Rectangle(45, 75 + y * 55, 934, 1), new Color(70, 85, 50));
        foreach (var squad in _fieldBattle.Squads.Where(x => x.Count > 0))
        {
            var x = 55 + squad.X * 66; var y = 85 + squad.Y * 55;
            var color = squad.Friendly ? squad.Type == _selectedUnit ? Color.Gold : Color.RoyalBlue : Color.DarkRed;
            Fill(new Rectangle(x, y, 48, 38), color);
            DrawText($"{squad.Count}", x + 8, y + 10, Color.White, 2);
        }
        DrawText($"FIELD BATTLE - TICK {_fieldBattle.TickNumber}", 55, 20, Color.Gold, 3);
        DrawText($"SELECTED {_selectedUnit}: 1 SWORDS  2 HALBERDS  3 KNIGHTS", 60, 600, Color.White, 2);
        DrawText("A ADVANCE  H HOLD  Q/E FLANK  R WITHDRAW", 60, 635, Color.Wheat, 2);
        DrawText("C CAPTAINS CONTROL ALL  W WITHDRAW ALL", 60, 670, Color.Wheat, 2);
    }

    private void DrawRadar(SiegeSession siege)
    {
        const int scale = 9; const int ox = 880; const int oy = 85;
        Fill(new Rectangle(ox - 8, oy - 8, siege.Width * scale + 16, siege.Height * scale + 16), new Color(10, 10, 10, 220));
        for (var x = 0; x < siege.Width; x++) for (var y = 0; y < siege.Height; y++)
        {
            var tile = siege.TileAt(x, y);
            var color = tile switch { SiegeTile.Wall => Color.Gray, SiegeTile.Door => Color.SaddleBrown, SiegeTile.SecretDoor => Color.DarkSlateGray, SiegeTile.Barrel => Color.Green, SiegeTile.Treasure => Color.Gold, _ => new Color(35, 35, 35) };
            Fill(new Rectangle(ox + x * scale, oy + y * scale, scale - 1, scale - 1), color);
        }
        foreach (var enemy in siege.Enemies) Fill(new Rectangle(ox + enemy.X * scale, oy + enemy.Y * scale, scale - 1, scale - 1), enemy.Champion ? Color.Magenta : Color.Red);
        Fill(new Rectangle(ox + siege.PlayerX * scale, oy + siege.PlayerY * scale, scale - 1, scale - 1), Color.Cyan);
    }

    private void DrawOverview()
    {
        var p = _campaign.State.Player; var s = p.Stats;
        DrawPanel("PERSONAL OVERVIEW", p.Name);
        DrawText($"STRENGTH {s.Strength} {s.DescribeStrength}", 100, 190, Color.White); DrawText($"DEXTERITY {s.Dexterity} {s.DescribeDexterity}", 100, 240, Color.White);
        DrawText($"PIETY {s.Piety} {s.DescribePiety}", 100, 290, Color.White); DrawText($"STAMINA {s.Stamina} {s.DescribeStamina}", 100, 340, Color.White);
        DrawText($"HONOR {s.Honor} {s.DescribeHonor}", 100, 390, Color.White); DrawText($"SWORD EXPERIENCE {p.SwordExperience}", 100, 460, Color.Wheat);
        DrawText($"LANCE EXPERIENCE {p.LanceExperience}", 100, 510, Color.Wheat); DrawText($"WIFE {p.Wife ?? "NONE"}", 100, 560, Color.Wheat);
        DrawText($"TOURNAMENT PROFIT {p.TournamentWinnings}S  CONQUEST SPOILS {p.ConquestWinnings}S", 100, 610, Color.Gold, 2);
        DrawText("ENTER RETURN", 740, 670, Color.LightGreen, 2);
    }

    private void DrawEnding()
    {
        var victory = _campaign.State.Victory;
        Fill(new Rectangle(0, 0, 1024, 768), victory == VictoryKind.Defeat ? Color.Black : new Color(50, 30, 15));
        DrawText(victory == VictoryKind.Crown ? "KING OF ENGLAND" : victory == VictoryKind.Dragon ? "SLAYER OF THE DRAGON" : "YOUR TIME HAS PASSED", 150, 260, victory == VictoryKind.Defeat ? Color.Gray : Color.Gold, 4, 760);
        DrawText("PRESS ENTER", 410, 520, Color.White, 2);
    }

    private void DrawPanel(string title, string subtitle)
    {
        Fill(new Rectangle(0, 0, 1024, 768), new Color(40, 32, 25)); Fill(new Rectangle(30, 30, 964, 708), new Color(85, 63, 39)); Fill(new Rectangle(45, 45, 934, 678), new Color(25, 29, 25));
        DrawText(title, 70, 70, Color.Gold, 4); DrawText(subtitle, 70, 125, Color.Wheat, 2, 850);
    }

    private void Fill(Rectangle rectangle, Color color) => _batch.Draw(_pixel, rectangle, color);
    private static Rectangle ScaleBounds(UiBounds bounds) => new(bounds.X * 1024 / 640, bounds.Y * 768 / 480, bounds.Width * 1024 / 640, bounds.Height * 768 / 480);
    private void DrawOutline(Rectangle rectangle, Color color, int thickness)
    {
        Fill(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        Fill(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }
    private bool DrawOriginal(string role, Rectangle destination)
    {
        if (!_originalArt.TryGetValue(role, out var texture)) return false;
        _batch.Draw(texture, destination, Color.White);
        return true;
    }
    private void DrawText(string text, int x, int y, Color color, int scale = 3, int wrap = 0) => PixelFont.Draw(_batch, _pixel, text, new Vector2(x, y), color, scale, wrap);
}
