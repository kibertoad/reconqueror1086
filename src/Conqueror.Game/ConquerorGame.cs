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
    private const string OriginalCursorRole = "Interface.Cursor";
    private const int OriginalDefaultCursorFrame = 0;

    private enum Screen { Title, Movie, OptionsHub, LoadGame, CharacterOptions, CharacterName, Character, Dilemma, Briefing, Map, Home, WarPlanning, Farm, Village, Inn, InnDialogue, Blacksmith, BlacksmithDialogue, Shop, Tournament, FieldBattle, Siege, Overview, Ending }
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
    private Screen _overviewReturnScreen = Screen.Home;
    private IReadOnlyList<CampaignSaveSlot> _saveSlotInfo = [];
    private int _heraldicColor = 1;
    private string _characterName = "Sir ";
    private IReadOnlyList<CharacterCreationOption> _characterOptions = CharacterCreationDefinitions.Options;
    private IReadOnlyList<OptionsHubOption> _optionsHubOptions = OptionsHubDefinitions.Options;
    private IReadOnlyList<HeraldicColorOption> _heraldicColors = CharacterCreationDefinitions.HeraldicColors;
    private IReadOnlyList<UiBounds> _pregeneratedBounds = CharacterCreationDefinitions.PregeneratedCharacters;
    private IReadOnlyList<UiBounds> _dilemmaChoiceBounds = YouthDilemmaPresentationDefinitions.Choices;
    private UiBounds _dilemmaContinueBounds = YouthDilemmaPresentationDefinitions.Continue;
    private EstateLayout _estateLayout = EstatePresentationDefinitions.Fallback;
    private WarPlanningPresentationDefinitions.Layout _warPlanningLayout = WarPlanningPresentationDefinitions.Fallback;
    private EstatePanel _estatePanel = EstatePanel.Map;
    private FarmPresentationDefinitions.Section _fiefSection = FarmPresentationDefinitions.Section.Farm;
    private FiefManagementCheckpoint? _fiefCheckpoint;
    private WarPlanningCheckpoint? _warPlanningCheckpoint;
    private int _fiefRowOffset;
    private int _warPlanningArmyIndex;
    private bool _editingWarPlanningArmyName;
    private IReadOnlyDictionary<FarmPresentationDefinitions.Section, FarmPresentationDefinitions.Layout> _fiefLayouts =
        FarmPresentationDefinitions.Layouts.ToDictionary(layout => layout.Section);
    private IReadOnlyList<SceneHotspot> _homeHotspots = HomePresentationDefinitions.Hotspots;
    private IReadOnlyList<SceneHotspot> _blacksmithHotspots = BlacksmithPresentationDefinitions.Hotspots;
    private InnPresentationLayout _innLayout = InnPresentationDefinitions.Fallback;
    private InnPatronHotspot? _innPatron;
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
    private ImportedSoundLibrary? _importedSoundLibrary;
    private ImportedDialogueRepository? _importedDialogue;
    private WeaponStoreResource? _weaponStore;
    private YouthDilemmaResult? _youthDilemmaResult;
    private SoundEffect? _importedMusic;
    private SoundEffectInstance? _musicInstance;
    private SmackerMoviePlayer? _titleMovie;
    private SmackerMoviePlayer? _eventMovie;
    private Screen _movieReturnScreen = Screen.OptionsHub;
    private readonly Dictionary<string, SoundEffect> _originalSounds = new(StringComparer.OrdinalIgnoreCase);
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
        _importedSoundLibrary = _importedContent is null ? null : ImportedSoundLibrary.Load(_importedContent);
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
        var estateLayoutId = _importedContent?.FindId("resource", ":iconmap.hat");
        _estateLayout = EstatePresentationDefinitions.From(estateLayoutId is null ? null : _importedContent?.DecodeHat(estateLayoutId));
        var homeLayoutId = _importedContent?.FindId("resource", ":fopts.hat");
        _homeHotspots = HomePresentationDefinitions.HotspotsFrom(homeLayoutId is null ? null : _importedContent?.DecodeHat(homeLayoutId));
        var warPlanningLayoutId = _importedContent?.FindId("resource", ":fwarplan.hat");
        _warPlanningLayout = WarPlanningPresentationDefinitions.From(
            warPlanningLayoutId is null ? null : _importedContent?.DecodeHat(warPlanningLayoutId));
        _fiefLayouts = FarmPresentationDefinitions.Layouts.ToDictionary(
            layout => layout.Section,
            layout =>
            {
                var id = _importedContent?.FindId("resource", layout.LayoutSuffix);
                return FarmPresentationDefinitions.LayoutFrom(layout.Section,
                    id is null ? null : _importedContent?.DecodeHat(id));
            });
        var blacksmithLayoutId = _importedContent?.FindId("resource", ":vsmith.hat");
        var blacksmithLayout = blacksmithLayoutId is null ? null : _importedContent?.DecodeHat(blacksmithLayoutId);
        _blacksmithHotspots = BlacksmithPresentationDefinitions.HotspotsFrom(blacksmithLayout);
        var innLayoutId = _importedContent?.FindId("resource", ":vinn.hat");
        _innLayout = InnPresentationDefinitions.From(
            innLayoutId is null ? null : _importedContent?.DecodeHat(innLayoutId));
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
        IsMouseVisible = !_originalAnimations.ContainsKey(OriginalCursorRole);
        LoadOriginalSounds();
        LoadTitleMovie();
        if (_importedContent?.Open("CDDA/TRACK02") is { } music)
        {
            using (music)
            try
            {
                _importedMusic = SoundEffect.FromStream(music);
                _musicInstance = _importedMusic.CreateInstance();
                _musicInstance.IsLooped = true;
                _musicInstance.Volume = .35f;
                StartMusic();
            }
            catch (Exception error) when (error is InvalidDataException or NotSupportedException)
            {
                _notice = "IMPORTED AUDIO COULD NOT BE PLAYED";
            }
        }
    }

    protected override void UnloadContent()
    {
        _titleMovie?.Dispose();
        _eventMovie?.Dispose();
        _musicInstance?.Dispose();
        _importedMusic?.Dispose();
        foreach (var texture in _originalArt.Values) texture.Dispose();
        foreach (var animation in _originalAnimations.Values)
            foreach (var texture in animation.Frames) texture.Dispose();
        foreach (var animation in _dilemmaAnimations.Values.OfType<DilemmaAnimation>())
            foreach (var texture in animation.Frames) texture.Dispose();
        DisposeOriginalSounds();
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
        var rightClick = mouse.RightButton == ButtonState.Pressed && _lastMouse.RightButton == ButtonState.Released;
        if (click && _soundEffectsEnabled) PlayOriginalSound("Interface.Activate");
        var pressAny = keys.GetPressedKeys().Any(key => key != Keys.Escape && !_last.IsKeyDown(key));
        if (Press(Keys.F5) && _screen is Screen.Farm or Screen.WarPlanning)
        {
            _notice = "CONFIRM OR CANCEL PENDING MANAGEMENT CHANGES BEFORE SAVING";
        }
        else if (Press(Keys.F5) && _screen is not Screen.Title and not Screen.LoadGame)
        {
            _saveSlots.Save(_campaign, _activeSaveSlot);
            _notice = $"CAMPAIGN SAVED IN SLOT {_activeSaveSlot}";
        }
        if (Press(Keys.F9)) OpenLoadGame();
        if (Press(Keys.Escape))
        {
            if (_screen == Screen.Title) Exit();
            else if (_screen == Screen.Movie) FinishEventMovie();
            else if (_screen == Screen.OptionsHub) _screen = Screen.Title;
            else if (_screen == Screen.LoadGame) ResumeFromLoadGame();
            else if (_screen == Screen.CharacterName) _screen = Screen.CharacterOptions;
            else if (_screen == Screen.CharacterOptions) _screen = Screen.Title;
            else if (_screen == Screen.Character) _screen = Screen.CharacterOptions;
            else if (_screen == Screen.Briefing) EnterCampaignMap();
            else if (_screen == Screen.Farm) CancelFiefManagement();
            else if (_screen == Screen.WarPlanning) CancelWarPlanning();
            else if (_screen == Screen.Overview) _screen = _overviewReturnScreen;
            else if (_screen == Screen.Blacksmith) _screen = Screen.Village;
            else if (_screen == Screen.Inn) _screen = Screen.Village;
            else if (_screen == Screen.InnDialogue) _screen = Screen.Inn;
            else if (_screen == Screen.BlacksmithDialogue) _screen = Screen.Blacksmith;
            else if (_screen == Screen.Shop) _screen = Screen.Blacksmith;
            else if (_screen == Screen.FieldBattle && _fieldBattle is not null) { _fieldBattle.IssueAll(UnitOrder.Withdraw); _notice = "WITHDRAWAL ORDERED"; }
            else { if (_screen == Screen.Siege && _siege is not null) _campaign.FinishSiege(_siege); _screen = Screen.Map; }
        }

        switch (_screen)
        {
            case Screen.Title:
                if (_titleMovie is { IsComplete: false })
                {
                    _titleMovie.Update(gameTime.ElapsedGameTime);
                    if (_titleMovie.IsComplete) StartMusic();
                }
                if (pressAny || click)
                {
                    _titleMovie?.Skip();
                    StartMusic();
                    _screen = Screen.OptionsHub;
                }
                break;
            case Screen.Movie:
                if (_eventMovie is null) FinishEventMovie();
                else
                {
                    _eventMovie.Update(gameTime.ElapsedGameTime);
                    if (pressAny || click || _eventMovie.IsComplete) FinishEventMovie();
                }
                break;
            case Screen.OptionsHub: UpdateOptionsHub(Press, mouse, click); break;
            case Screen.LoadGame: UpdateLoadGame(Press, mouse, click); break;
            case Screen.CharacterOptions: UpdateCharacterOptions(Press, mouse, click); break;
            case Screen.CharacterName: UpdateCharacterName(Press); break;
            case Screen.Character:
                UpdatePregeneratedCharacters(Press, mouse, click);
                break;
            case Screen.Dilemma: UpdateDilemma(Press, mouse, click); break;
            case Screen.Briefing: if (pressAny || click) EnterCampaignMap(); break;
            case Screen.Map: UpdateMap(Press, mouse, click); break;
            case Screen.Home: UpdateHome(Press, mouse, click); break;
            case Screen.WarPlanning: UpdateWarPlanning(Press, mouse, click, rightClick); break;
            case Screen.Farm: UpdateFarm(Press, mouse, click); break;
            case Screen.Village: UpdateVillage(Press); break;
            case Screen.Inn: UpdateInn(Press, mouse, click); break;
            case Screen.InnDialogue: UpdateInnDialogue(Press); break;
            case Screen.Blacksmith: UpdateBlacksmith(Press, mouse, click); break;
            case Screen.BlacksmithDialogue: UpdateBlacksmithDialogue(Press); break;
            case Screen.Shop: UpdateShop(Press, mouse, click); break;
            case Screen.Tournament: UpdateTournament(Press); break;
            case Screen.FieldBattle: UpdateFieldBattle(Press, gameTime); break;
            case Screen.Siege: UpdateSiege(Press); break;
            case Screen.Overview: if (Press(Keys.Enter) || Press(Keys.O)) _screen = _overviewReturnScreen; break;
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
                if (_cdMusicEnabled) StartMusic();
                else if (_musicInstance?.State == SoundState.Playing) _musicInstance.Pause();
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
            case OptionsHubAction.Credits: PlayEventMovie("Options.Credits", Screen.OptionsHub); break;
            case OptionsHubAction.Movie: PlayEventMovie("Title.Intro", Screen.OptionsHub); break;
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
        _fiefCheckpoint = null;
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
        _selectedLocation = 0;
        _screen = Screen.Briefing;
    }

    private string NormalizedCharacterName() => _characterName.Trim() is { Length: > 4 } name ? name : "Sir Custom";
    private (int X, int Y) OriginalPoint(MouseState mouse) =>
        (mouse.X * 640 / Math.Max(1, GraphicsDevice.Viewport.Width), mouse.Y * 480 / Math.Max(1, GraphicsDevice.Viewport.Height));

    private void UpdateMap(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (_campaign.HasPendingFieldBattle)
        {
            BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED");
            return;
        }
        if (press(Keys.Left) || press(Keys.Up)) _selectedLocation = (_selectedLocation + World.Locations.Length - 1) % World.Locations.Length;
        if (press(Keys.Right) || press(Keys.Down)) _selectedLocation = (_selectedLocation + 1) % World.Locations.Length;
        for (var armyIndex = 0; armyIndex < Player.ArmyDivisionLimit; armyIndex++)
            if (press(Keys.D1 + armyIndex)) _warPlanningArmyIndex = armyIndex;
        if (press(Keys.Enter))
        {
            var days = _campaign.TravelTo(_selectedLocation);
            _notice = days == 0 ? $"ALREADY AT {World.Locations[_selectedLocation].Name}" : $"TRAVELLED {days} DAYS TO {World.Locations[_selectedLocation].Name}";
            if (_campaign.HasPendingFieldBattle) { BeginFieldBattle("YOUR ARMY HAS BEEN INTERCEPTED"); return; }
        }
        if (press(Keys.H) && _campaign.State.CurrentLocation == 0) _screen = Screen.Home;
        if (press(Keys.V)) _screen = Screen.Village;
        if (press(Keys.T) && _campaign.IsTournamentHere) _screen = Screen.Tournament;
        if (press(Keys.O)) EnterOverview(Screen.Map);
        if (press(Keys.S) && _campaign.StartSiege(_selectedLocation)) { _siege = _campaign.CreateSiege(); _showRadar = true; _screen = Screen.Siege; }
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
        if (press(Keys.D)) _campaign.AttemptDragon();
        if (press(Keys.E)) _campaign.AdvanceDays(_campaign.State.DaySpeed);
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.DaySpeed = Math.Min(15, _campaign.State.DaySpeed + 1);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.DaySpeed = Math.Max(1, _campaign.State.DaySpeed - 1);
        if (!click || !_originalArt.ContainsKey("Estate.Shell")) return;

        var (x, y) = OriginalPoint(mouse);
        if (_estateLayout.InsetMap.Contains(x, y))
        {
            _selectedLocation = EstatePresentationDefinitions.LocationAt(_estateLayout.InsetMap, x, y);
            _estatePanel = EstatePanel.Map;
            return;
        }
        var control = _estateLayout.Controls.FirstOrDefault(item => item.Bounds.Contains(x, y));
        if (control is not null) ActivateEstateControl(control.Action);
    }

    private void ActivateEstateControl(EstateControlAction action)
    {
        switch (action)
        {
            case EstateControlAction.Map: _estatePanel = EstatePanel.Map; break;
            case EstateControlAction.Orders: _estatePanel = EstatePanel.Orders; break;
            case EstateControlAction.Help: _estatePanel = EstatePanel.Help; break;
            case EstateControlAction.Home:
                if (_campaign.State.CurrentLocation == 0) _screen = Screen.Home;
                else _notice = "HOME IS AVAILABLE AT YOUR ESTATE";
                break;
            case EstateControlAction.Village: _screen = Screen.Village; break;
            default: throw new ArgumentOutOfRangeException(nameof(action));
        }
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
        _notice = "YOUR YEARS OF TRAINING ARE COMPLETE";
        _screen = Screen.Briefing;
    }

    private void EnterCampaignMap()
    {
        _selectedLocation = _campaign.State.CurrentLocation;
        _notice = "YOUR CAMPAIGN BEGINS";
        _screen = Screen.Map;
    }

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
            _screen = Screen.InnDialogue;
        }
        else if (_innLayout.ExitBounds.Contains(x, y))
            _screen = Screen.Village;
    }

    private void UpdateInnDialogue(Func<Keys, bool> press)
    {
        if (press(Keys.Enter) || press(Keys.I) || press(Keys.V)) _screen = Screen.Inn;
    }

    private void UpdateBlacksmith(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.Enter))
        {
            _screen = Screen.BlacksmithDialogue;
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
            case SceneNavigationAction.BlacksmithDialogue: _screen = Screen.BlacksmithDialogue; break;
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
            case Screen.Title: DrawTitle(); break; case Screen.Movie: DrawEventMovie(); break; case Screen.OptionsHub: DrawOptionsHub(); break; case Screen.LoadGame: DrawLoadGame(); break; case Screen.CharacterOptions: DrawCharacterOptions(); break; case Screen.CharacterName: DrawCharacterName(); break; case Screen.Character: DrawCharacter(); break; case Screen.Dilemma: DrawDilemma(); break; case Screen.Briefing: DrawCampaignBriefing(); break; case Screen.Map: DrawMap(); break;
            case Screen.Home: DrawHome(); break; case Screen.WarPlanning: DrawWarPlanning(); break; case Screen.Farm: DrawFarm(); break; case Screen.Village: DrawVillage(); break; case Screen.Inn: DrawInn(); break; case Screen.InnDialogue: DrawInnDialogue(); break; case Screen.Blacksmith: DrawBlacksmith(); break; case Screen.BlacksmithDialogue: DrawBlacksmithDialogue(); break; case Screen.Shop: DrawShop(); break; case Screen.Tournament: DrawTournament(); break; case Screen.FieldBattle: DrawFieldBattle(); break;
            case Screen.Siege: DrawSiege(); break; case Screen.Overview: DrawOverview(); break; case Screen.Ending: DrawEnding(); break;
        }
        if (_screen is not Screen.Title and not Screen.Movie and not Screen.LoadGame
            and not Screen.Character and not Screen.Dilemma and not Screen.Briefing and not Screen.Inn and not Screen.InnDialogue)
            DrawText(_notice, 24, 730, Color.Gold, 2);
        if (_screen != Screen.Movie && !(_screen == Screen.Title && _titleMovie is { IsComplete: false }))
            DrawOriginalCursor();
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
            return new SmackerMoviePlayer(GraphicsDevice, source);
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
        if (_originalSounds.TryGetValue(role, out var sound)) sound.Play();
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
        DrawText("I  ENTER INN", 100, 435, Color.LightGreen, 2);
        DrawText($"PLUS MINUS TAX RATE  {_campaign.State.Player.Home.TaxRate}%", 100, 455, Color.Wheat, 2);
        DrawText("ENTER RETURN TO MAP", 100, 520, Color.LightGreen);
    }

    private void DrawInn()
    {
        var original = DrawOriginal("Village.Inn", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("THE INN", "TRAVELLERS AND LOCAL PATRONS GATHER HERE");
            DrawText("PATRON CONVERSATIONS ARE NOT YET AVAILABLE", 155, 340, Color.Wheat, 2);
            DrawText("ENTER, I, OR V  RETURN TO VILLAGE", 185, 570, Color.LightGreen, 2);
            return;
        }

        var (x, y) = OriginalPoint(_lastMouse);
        var patron = _innLayout.Patrons.FirstOrDefault(candidate => candidate.Bounds.Contains(x, y));
        var label = patron?.Name ?? (_innLayout.ExitBounds.Contains(x, y) ? "EXIT" : "");
        if (label.Length == 0) return;
        var footer = ScaleBounds(_innLayout.HoverLabelBounds);
        DrawText(label, footer.X + 10, footer.Y + 8, Color.Wheat, 2, footer.Width - 20);
    }

    private void DrawInnDialogue()
    {
        var patron = _innPatron;
        if (patron is null)
        {
            _screen = Screen.Inn;
            return;
        }

        if (DrawOriginal("Dialogue.Frame", new Rectangle(0, 0, 1024, 768)))
        {
            DrawOriginal(patron.PortraitRole, ScaleBounds(BlacksmithDialoguePresentationDefinitions.Portrait));
            DrawText(patron.Name, 155, 385, Color.White, 2, 260);
            DrawText("Conversation text has not yet been recovered.", 435, 65, Color.White, 2, 520);
            DrawText("ENTER  RETURN TO THE INN", 75, 500, Color.Cyan, 2, 850);
            return;
        }

        DrawPanel(patron.Name.ToUpperInvariant(), "CONVERSATION TEXT HAS NOT YET BEEN RECOVERED");
        DrawText("ENTER  RETURN TO THE INN", 100, 300, Color.LightGreen, 2);
    }

    private void DrawBlacksmith()
    {
        var original = DrawOriginal("Blacksmith.Workshop", new Rectangle(0, 0, 1024, 768));
        if (!original)
            DrawPanel("THE BLACKSMITH", "SELECT THE SMITH TO BROWSE HIS WARES");
        if (original) DrawSceneHoverLabel(_blacksmithHotspots, BlacksmithPresentationDefinitions.HoverLabelBounds);
        else DrawText("ENTER OR B  BROWSE WARES    ESC  VILLAGE", 120, 715, Color.Wheat, 2);
    }

    private void DrawBlacksmithDialogue()
    {
        if (DrawOriginal("Dialogue.Frame", new Rectangle(0, 0, 1024, 768)))
        {
            DrawOriginal("Blacksmith.Portrait", ScaleBounds(BlacksmithDialoguePresentationDefinitions.Portrait));
            DrawText(BlacksmithDialoguePresentationDefinitions.Speaker, 155, 385, Color.White, 2, 260);
            DrawText(BlacksmithDialoguePresentationDefinitions.FallbackPrompt, 435, 65, Color.White, 2, 520);
            for (var index = 0; index < BlacksmithDialoguePresentationDefinitions.Commands.Count; index++)
                DrawText(BlacksmithDialoguePresentationDefinitions.Commands[index].Label, 75, 500 + index * 55, Color.Cyan, 2, 850);
            return;
        }

        DrawPanel(BlacksmithDialoguePresentationDefinitions.Speaker, BlacksmithDialoguePresentationDefinitions.FallbackPrompt.ToUpperInvariant());
        for (var index = 0; index < BlacksmithDialoguePresentationDefinitions.Commands.Count; index++)
            DrawText(BlacksmithDialoguePresentationDefinitions.Commands[index].Label, 100, 260 + index * 60, Color.LightGreen, 2);
    }

    private void DrawSceneHoverLabel(IReadOnlyList<SceneHotspot> hotspots, UiBounds labelBounds)
    {
        var hotspot = HitSceneHotspot(hotspots, _lastMouse);
        if (hotspot is null) return;
        var bounds = ScaleBounds(labelBounds);
        DrawText(hotspot.HoverLabel, bounds.X + 8, bounds.Y + 6, Color.Wheat, 2, bounds.Width - 16);
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
        if (DrawOriginal("Home.Overview", new Rectangle(0, 0, 1024, 768)))
        {
            DrawText(p.Name, 325, 78, Color.Wheat, 2, 360);
            DrawText($"{p.Age}", 130, 61, Color.Wheat, 2);
            DrawText($"{p.Home.Population}", 175, 237, Color.Wheat, 2);
            DrawText($"{p.Wealth}", 710, 157, Color.Wheat, 2);
            DrawText($"{s.Strength}", 710, 200, Color.Wheat, 2);
            DrawText($"{s.Piety}", 710, 239, Color.Wheat, 2);
            DrawText($"{s.Honor}", 710, 278, Color.Wheat, 2);
            return;
        }
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
    private void DrawOriginalCursor()
    {
        if (!_originalAnimations.TryGetValue(OriginalCursorRole, out var cursor)
            || cursor.Frames.Count <= OriginalDefaultCursorFrame) return;
        var frame = cursor.Frames[OriginalDefaultCursorFrame];
        var mouse = Mouse.GetState();
        _batch.Draw(frame, new Rectangle(mouse.X, mouse.Y,
            frame.Width * 1024 / 640, frame.Height * 768 / 480), Color.White);
    }
    private void DrawText(string text, int x, int y, Color color, int scale = 3, int wrap = 0) => PixelFont.Draw(_batch, _pixel, text, new Vector2(x, y), color, scale, wrap);
}
