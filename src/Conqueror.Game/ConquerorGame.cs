using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed partial class ConquerorGame : Microsoft.Xna.Framework.Game
{
    private sealed record DilemmaAnimation(IReadOnlyList<Texture2D> Frames, int FramesPerChoice);
    private sealed record OriginalAnimation(IReadOnlyList<Texture2D> Frames);
    private sealed class SiegeVisuals(
        DynamixScene scene, int sourceOriginX, int sourceOriginY,
        IReadOnlyDictionary<int, Texture2D> textures,
        IReadOnlyDictionary<int, DynamixSceneTexture> sources,
        byte[] palette,
        DynamixSceneColorMaps? colorMaps,
        DynamixSceneBackdrop? decodedBackdrop,
        Texture2D? backdrop) : IDisposable
    {
        private readonly Dictionary<(int Texture, int ColorMap), Texture2D> _mappedTextures = [];
        public DynamixScene Scene { get; } = scene;
        public int SourceOriginX { get; } = sourceOriginX;
        public int SourceOriginY { get; } = sourceOriginY;
        public IReadOnlyDictionary<int, Texture2D> Textures { get; } = textures;
        public DynamixSceneBackdrop? DecodedBackdrop { get; } = decodedBackdrop;
        public Texture2D? Backdrop { get; } = backdrop;
        public DynamixSceneTexture? SourceFor(int textureIndex) =>
            sources.GetValueOrDefault(textureIndex);

        public Texture2D? TextureFor(int textureIndex, int? colorMapIndex = null)
        {
            if (!Textures.TryGetValue(textureIndex, out var original)) return null;
            if (colorMapIndex is null || colorMaps is null || !sources.TryGetValue(textureIndex, out var source))
                return original;
            var key = (textureIndex, colorMapIndex.Value);
            if (_mappedTextures.TryGetValue(key, out var mapped)) return mapped;
            mapped = new Texture2D(original.GraphicsDevice, source.Width, source.Height, false, SurfaceFormat.Color);
            mapped.SetData(IndexedScenePixels.ToRgba(
                source.Indices, palette, colorMap: colorMaps[colorMapIndex.Value].Span));
            _mappedTextures.Add(key, mapped);
            return mapped;
        }

        public void Dispose()
        {
            foreach (var texture in Textures.Values) texture.Dispose();
            foreach (var texture in _mappedTextures.Values) texture.Dispose();
            Backdrop?.Dispose();
        }
    }
    private const string OriginalCursorAnimationRole = "Interface.Cursor";

    private enum Screen { Title, Movie, OptionsHub, Practice, LoadGame, CharacterOptions, CharacterName, Character, Dilemma, Briefing, Map, Home, WarPlanning, Farm, Village, Inn, InnDialogue, Blacksmith, BlacksmithDialogue, Shop, Tournament, DrogoDemand, FieldBattle, StrategicEncounter, Siege, DragonBattle, Overview, Ending }
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _batch = null!;
    private Texture2D _pixel = null!;
    private OriginalUiFont? _originalUiFont;
    private RenderTarget2D _canvas = null!;
    private Campaign _campaign;
    private Screen _screen = Screen.Title;
    private KeyboardState _last;
    private GamePadState _lastGamePad;
    private MouseState _lastMouse;
    private Vector2 _controllerPointer = new(320, 240);
    private bool _controllerPointerActive;
    private bool _controllerPointerPressed;
    private int _characterOption;
    private int _optionsHubOption;
    private int _pressedOptionsHubOption = -1;
    private int _practiceOption;
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
    private IReadOnlyList<PracticeOption> _practiceOptions = PracticePresentationDefinitions.Options;
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
    private IReadOnlyList<VillageHotspot> _villageHotspots = VillagePresentationDefinitions.Hotspots;
    private IReadOnlyList<VillageSceneDefinition> _villageScenes = [];
    private readonly Dictionary<string, IReadOnlyList<VillageHotspot>> _villageSceneHotspots = new(StringComparer.Ordinal);
    private IReadOnlyList<SceneHotspot> _blacksmithHotspots = BlacksmithPresentationDefinitions.Hotspots;
    private InnPresentationLayout _innLayout = InnPresentationDefinitions.Fallback;
    private InnPatronHotspot? _innPatron;
    private Screen _conversationReturnScreen = Screen.Inn;
    // The original joust presentation's cadence is not recovered. Keep this
    // replacement meter stable at the former 60 Hz visual rate (2 * 60).
    private const double JoustCursorUnitsPerSecond = 120d;
    private double _joustCursor;
    private SiegeSession? _siege;
    private SiegeVisuals? _siegeVisuals;
    private SiegeForegroundTrajectory? _siegeWeaponTrajectory;
    private int _siegeWeaponFrame = -1;
    private SiegeFrameRun _siegeWeaponRun;
    private double _siegeWeaponElapsed;
    private SiegeHitEffect? _siegeHitEffect;
    private bool _showRadar = true;
    private FieldBattleSession? _fieldBattle;
    private OriginalStrategicMapTargetConfirmation? _strategicMapTargetConfirmation;
    private int _strategicRoutePreviewPasses;
    private int _strategicRoutePreviewFrame;
    private OriginalStrategicPlayerEnemyEncounter? _strategicEncounter;
    private OriginalStrategicInteractiveEncounterSession? _strategicInteractiveEncounter;
    private OriginalStrategicInteractiveEncounterViewport? _strategicEncounterViewport;
    private DragonBattleSession? _dragonBattle;
    private SmackerMoviePlayer? _dragonRunMovie;
    private PracticeCombatKind? _activePracticeCombat;
    private bool _drogoCombat;
    private UnitType _selectedUnit = UnitType.Swordsmen;
    // Provisional host cadence for the legacy FieldBattleSession adapter.
    private const double FieldBattleTickSeconds = .45;
    private double _battleTick;
    private string _notice = "";
    private int _selectedLocation;
    private int _shopIndex;
    private int _ladyIndex = 1;
    private int _tournamentOpponent = 2;
    private readonly ImportedContentCatalog _importedContent;
    private readonly ImportedOriginalStrategicResources _originalStrategicResources;
    private ImportedSoundLibrary? _importedSoundLibrary;
    private ImportedDialogueRepository? _importedDialogue;
    private ImportedConversationSession? _conversationSession;
    private DynamixConversationDatabase? _conversationDatabase;
    private DynamixActionTreeDatabase? _conversationActionTrees;
    private IReadOnlyList<int> _conversationInitialVariables = [];
    private WeaponStoreResource? _weaponStore;
    private YouthDilemmaResult? _youthDilemmaResult;
    private SoundEffect? _importedMusic;
    private SoundEffectInstance? _musicInstance;
    private SmackerMoviePlayer? _titleMovie;
    private SmackerMoviePlayer? _eventMovie;
    private readonly Queue<string> _eventMovieQueue = new();
    private Screen _movieReturnScreen = Screen.OptionsHub;
    private CampaignEndReason _presentedEndReason;
    private readonly Dictionary<string, SoundEffect> _originalSounds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> _originalArt = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Texture2D> _conversationPortraits = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte[]> _originalPalettes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, OriginalAnimation> _originalAnimations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DilemmaAnimation?> _dilemmaAnimations = new(StringComparer.OrdinalIgnoreCase);
    private byte[]? _dilemmaPalette;
    private double _presentationSeconds;
    private double _conversationAdvanceAt;
    private bool _hasActiveCampaign;
    private bool _cdMusicEnabled = true;
    private bool _soundEffectsEnabled = true;
    private bool _speechEnabled = true;
    private bool _animationEnabled = true;
    private bool _paused;
    private bool _musicPausedByGame;
    private readonly CampaignSaveSlots _saveSlots;
    private readonly GameSettingsStore _settingsStore;
    private GameSettings _settings = new();

    public ConquerorGame(ImportedContentCatalog importedContent, string? stateRoot = null)
    {
        _importedContent = importedContent ?? throw new ArgumentNullException(nameof(importedContent));
        _originalStrategicResources = new ImportedOriginalStrategicResources(_importedContent);
        _campaign = BindStrategicResources(new Campaign());
        var writableStateRoot = stateRoot ?? AppContext.BaseDirectory;
        _saveSlots = new CampaignSaveSlots(Path.Combine(writableStateRoot, "saves"));
        _settingsStore = new GameSettingsStore(Path.Combine(writableStateRoot, "settings.json"));
        _settings = _settingsStore.Load();
        _cdMusicEnabled = _settings.CdMusic;
        _soundEffectsEnabled = _settings.SoundEffects;
        _speechEnabled = _settings.Speech;
        _animationEnabled = _settings.Animation;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1024,
            PreferredBackBufferHeight = 768,
            HardwareModeSwitch = false,
            IsFullScreen = _settings.Fullscreen
        };
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        TargetElapsedTime = OriginalStrategicHostRuntime.FixedCadence;
        IsMouseVisible = true;
        Window.Title = "ReConqueror A.D. 1086";
    }

    private Campaign BindStrategicResources(Campaign campaign)
    {
        campaign.ConfigureOriginalStrategicResources(_originalStrategicResources);
        _strategicDragonTriggerSuppressed = false;
        return campaign;
    }

    protected override void LoadContent()
    {
        string RequireId(string kind, string suffix) => _importedContent.FindId(kind, suffix)
            ?? throw new InvalidDataException($"Required original {kind} asset '{suffix}' is missing.");
        HatLayout RequireLayout(string suffix)
        {
            var id = RequireId("resource", suffix);
            return _importedContent.DecodeHat(id)
                ?? throw new InvalidDataException($"Required original layout '{suffix}' could not be decoded.");
        }

        _batch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        var fontId = RequireId("indexed-animation", OriginalUiFontDefinition.ResourceSuffix);
        var sourceFont = _importedContent.DecodeCsf(fontId)
            ?? throw new InvalidDataException("Required original CONFONT.CSF could not be decoded.");
        _originalUiFont = OriginalUiFont.Create(GraphicsDevice, sourceFont)
            ?? throw new InvalidDataException("Required original CONFONT.CSF does not contain the expected proportional 256-glyph sheet.");
        _canvas = new RenderTarget2D(GraphicsDevice, PresentationScaling.VirtualWidth,
            PresentationScaling.VirtualHeight, false, SurfaceFormat.Color, DepthFormat.None);
        _importedSoundLibrary = ImportedSoundLibrary.Load(_importedContent);
        _importedDialogue = new ImportedDialogueRepository(_importedContent);
        var weaponStoreId = RequireId("resource", ":weapons.dat");
        _weaponStore = _importedContent.DecodeWeaponStore(weaponStoreId)
            ?? throw new InvalidDataException("Required original WEAPONS.DAT could not be decoded.");
        var optionsLayout = RequireLayout(":gameopts.hat");
        _optionsHubOptions = OptionsHubDefinitions.OptionsFrom(optionsLayout);
        var practiceLayout = RequireLayout(":practice.hat");
        _practiceOptions = PracticePresentationDefinitions.OptionsFrom(practiceLayout);
        var characterLayout = RequireLayout(":cgopts.hat");
        _characterOptions = CharacterCreationDefinitions.OptionsFrom(characterLayout);
        _heraldicColors = CharacterCreationDefinitions.ColorsFrom(characterLayout);
        var pregeneratedLayout = RequireLayout(":pregen.hat");
        _pregeneratedBounds = CharacterCreationDefinitions.PregeneratedFrom(pregeneratedLayout);
        var dilemmaLayout = RequireLayout(":chargen.hat");
        _dilemmaChoiceBounds = YouthDilemmaPresentationDefinitions.ChoicesFrom(dilemmaLayout);
        _dilemmaContinueBounds = YouthDilemmaPresentationDefinitions.ContinueFrom(dilemmaLayout);
        _estateLayout = EstatePresentationDefinitions.From(RequireLayout(":iconmap.hat"));
        _homeHotspots = HomePresentationDefinitions.HotspotsFrom(RequireLayout(":fopts.hat"));
        _villageHotspots = VillagePresentationDefinitions.HotspotsFrom(RequireLayout(":vopts.hat"));
        var villageCatalogId = RequireId("resource", ":village.dat");
        _villageScenes = _importedContent.DecodeVillageSceneCatalog(villageCatalogId)
            ?? throw new InvalidDataException("Required original VILLAGE.DAT could not be decoded.");
        _warPlanningLayout = WarPlanningPresentationDefinitions.From(RequireLayout(":fwarplan.hat"));
        _fiefLayouts = FarmPresentationDefinitions.Layouts.ToDictionary(
            layout => layout.Section,
            layout => FarmPresentationDefinitions.LayoutFrom(
                layout.Section, RequireLayout(layout.LayoutSuffix)));
        _blacksmithHotspots = BlacksmithPresentationDefinitions.HotspotsFrom(RequireLayout(":vsmith.hat"));
        _innLayout = InnPresentationDefinitions.From(RequireLayout(":vinn.hat"));
        foreach (var definition in ImportedArt.Definitions)
        {
            var id = RequireId(definition.Kind, definition.IdSuffix);
            var image = _importedContent.DecodePcx(id) ?? throw new InvalidDataException(
                $"Required original image '{definition.IdSuffix}' could not be decoded.");
            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
            texture.SetData(image.ToRgba());
            _originalArt.Add(definition.Role, texture);
            _originalPalettes.Add(definition.Role, image.PaletteRgb);
            if (definition.Role == "Dilemma.Background") _dilemmaPalette = image.PaletteRgb;
        }
        foreach (var definition in ImportedRawArt.Definitions)
        {
            var id = RequireId("image", definition.IdSuffix);
            var paletteId = RequireId("palette", definition.PaletteIdSuffix);
            var image = _importedContent.DecodeRawIndexedImage(
                id, paletteId, definition.Width, definition.Height) ?? throw new InvalidDataException(
                $"Required original image '{definition.IdSuffix}' could not be decoded.");
            var texture = new Texture2D(GraphicsDevice, image.Width, image.Height, false, SurfaceFormat.Color);
            texture.SetData(image.ToRgba());
            _originalArt.Add(definition.Role, texture);
            _originalPalettes.Add(definition.Role, image.PaletteRgb);
        }
        LoadOriginalConversations();
        foreach (var definition in ImportedAnimations.Definitions)
        {
            var id = RequireId("indexed-animation", definition.IdSuffix);
            byte[]? palette;
            if (definition.PaletteIdSuffix is { } paletteSuffix)
            {
                var paletteId = RequireId("palette", paletteSuffix);
                palette = _importedContent.DecodePalette(paletteId)?.Rgb;
            }
            else
            {
                _originalPalettes.TryGetValue(definition.PaletteArtRole, out palette);
            }
            if (palette is null)
                throw new InvalidDataException($"Required palette for '{definition.IdSuffix}' could not be decoded.");
            var sequence = _importedContent.DecodeCsf(id) ?? throw new InvalidDataException(
                $"Required original animation '{definition.IdSuffix}' could not be decoded.");
            var frames = sequence.Chunks.Select(chunk =>
            {
                var frame = sequence.DecodeFrame(chunk);
                var texture = new Texture2D(GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color);
                texture.SetData(frame.ToRgba(palette));
                return texture;
            }).ToArray();
            _originalAnimations.Add(definition.Role, new OriginalAnimation(frames));
        }
        IsMouseVisible = !_originalAnimations.ContainsKey(OriginalCursorAnimationRole);
        LoadOriginalSounds();
        LoadTitleMovie();
        if (_importedContent.Open("CDDA/TRACK02") is { } music)
        {
            using (music)
            try
            {
                _importedMusic = SoundEffect.FromStream(music);
                _musicInstance = _importedMusic.CreateInstance();
                _musicInstance.IsLooped = true;
                _musicInstance.Volume = _settings.MusicVolume;
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
        _dragonRunMovie?.Dispose();
        ClearSiegeVisuals();
        _musicInstance?.Dispose();
        _importedMusic?.Dispose();
        foreach (var texture in _originalArt.Values) texture.Dispose();
        foreach (var texture in _conversationPortraits.Values) texture.Dispose();
        foreach (var animation in _originalAnimations.Values)
            foreach (var texture in animation.Frames) texture.Dispose();
        foreach (var animation in _dilemmaAnimations.Values.OfType<DilemmaAnimation>())
            foreach (var texture in animation.Frames) texture.Dispose();
        DisposeOriginalSounds();
        _originalUiFont?.Dispose();
        _canvas.Dispose();
        _pixel.Dispose();
        _batch.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _presentationSeconds += gameTime.ElapsedGameTime.TotalSeconds;
        var keys = Keyboard.GetState();
        var gamePad = GamePad.GetState(PlayerIndex.One);
        var mouse = Mouse.GetState();
        var (controllerClick, controllerRelease, controllerRightClick) =
            UpdateControllerPointer(gamePad, mouse, gameTime.ElapsedGameTime.TotalSeconds);
        var controllerContext = ControllerContextFor(_screen);
        bool Press(Keys key) => keys.IsKeyDown(key) && !_last.IsKeyDown(key)
            || ControllerInputBindings.IsPressed(key, controllerContext, gamePad, _lastGamePad);
        var click = mouse.LeftButton == ButtonState.Pressed && _lastMouse.LeftButton == ButtonState.Released
            || controllerClick;
        var release = mouse.LeftButton == ButtonState.Released && _lastMouse.LeftButton == ButtonState.Pressed
            || controllerRelease;
        var rightClick = mouse.RightButton == ButtonState.Pressed && _lastMouse.RightButton == ButtonState.Released
            || controllerRightClick;
        if (Press(Keys.F11)) ToggleFullscreen();
        if (Press(Keys.F10)) ToggleIntegerScaling();
        if (Press(Keys.Pause)) TogglePause();
        if (_paused)
        {
            _last = keys;
            _lastGamePad = gamePad;
            _lastMouse = mouse;
            base.Update(gameTime);
            return;
        }
        if (click && _soundEffectsEnabled) PlayOriginalSound("Interface.Activate");
        var pressAny = keys.GetPressedKeys().Any(key => key is not Keys.Escape and not Keys.F10
            and not Keys.F11 and not Keys.Pause && !_last.IsKeyDown(key))
            || ControllerInputBindings.AnyPressed(gamePad, _lastGamePad);
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
            else if (_screen == Screen.Practice) _screen = Screen.OptionsHub;
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
            else if (_screen == Screen.InnDialogue) _screen = _conversationReturnScreen;
            else if (_screen == Screen.BlacksmithDialogue) _screen = Screen.Blacksmith;
            else if (_screen == Screen.Shop) _screen = Screen.Blacksmith;
            else if (_screen == Screen.FieldBattle && _activePracticeCombat is not null) FinishPracticeCombat("PRACTICE ENDED");
            else if (_screen == Screen.Siege && _activePracticeCombat is not null) FinishPracticeCombat("PRACTICE ENDED");
            else if (_screen == Screen.Siege && _drogoCombat) _notice = "DROGO WILL NOT LET YOU ESCAPE";
            else if (_screen == Screen.DrogoDemand) _notice = "PAY DROGO OR FIGHT HIM";
            else if (_screen == Screen.FieldBattle && _fieldBattle is not null) { _fieldBattle.IssueAll(UnitOrder.Withdraw); _notice = "WITHDRAWAL ORDERED"; }
            else if (_screen == Screen.DragonBattle && _dragonBattle is not null)
            {
                _dragonBattle.Withdraw();
                ResolveDragonBattle();
                _last = keys; _lastGamePad = gamePad; _lastMouse = mouse;
                base.Update(gameTime);
                return;
            }
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
            case Screen.OptionsHub: UpdateOptionsHub(Press, mouse, click, release); break;
            case Screen.Practice: UpdatePractice(Press, mouse, click); break;
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
            case Screen.Village: UpdateVillage(Press, mouse, click); break;
            case Screen.Inn: UpdateInn(Press, mouse, click); break;
            case Screen.InnDialogue: UpdateInnDialogue(Press, mouse, click); break;
            case Screen.Blacksmith: UpdateBlacksmith(Press, mouse, click); break;
            case Screen.BlacksmithDialogue: UpdateBlacksmithDialogue(Press); break;
            case Screen.Shop: UpdateShop(Press, mouse, click); break;
            case Screen.Tournament: UpdateTournament(Press, gameTime); break;
            case Screen.DrogoDemand: UpdateDrogoDemand(Press); break;
            case Screen.FieldBattle: UpdateFieldBattle(Press, gameTime); break;
            case Screen.StrategicEncounter: UpdateStrategicEncounter(Press, mouse, click); break;
            case Screen.Siege: UpdateSiege(Press, mouse, click, gameTime); break;
            case Screen.DragonBattle: UpdateDragonBattle(keys, gamePad, mouse, gameTime); break;
            case Screen.Overview: if (Press(Keys.Enter) || Press(Keys.O)) _screen = _overviewReturnScreen; break;
            case Screen.Ending: if (Press(Keys.Enter)) _screen = Screen.Title; break;
        }
        if (_campaign.State.Victory != VictoryKind.None
            && _screen is not Screen.Title and not Screen.LoadGame and not Screen.Movie)
        {
            _screen = Screen.Ending;
            if (_campaign.State.EndReason == CampaignEndReason.AgeLimit
                && _presentedEndReason != CampaignEndReason.AgeLimit)
            {
                _presentedEndReason = CampaignEndReason.AgeLimit;
                PlayEventMovie("Ending.AgeLimit", Screen.Ending);
            }
        }
        _last = keys;
        _lastGamePad = gamePad;
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

    private void UpdateOptionsHub(Func<Keys, bool> press, MouseState mouse, bool pointerPressed, bool pointerReleased)
    {
        if (press(Keys.Up)) _optionsHubOption = (_optionsHubOption + _optionsHubOptions.Count - 1) % _optionsHubOptions.Count;
        if (press(Keys.Down)) _optionsHubOption = (_optionsHubOption + 1) % _optionsHubOptions.Count;
        if (press(Keys.Left)) AdjustSelectedVolume(-.1f);
        if (press(Keys.Right)) AdjustSelectedVolume(.1f);
        if (press(Keys.R))
        {
            _settings = _settings with { ReducedMotion = !_settings.ReducedMotion };
            _notice = $"REDUCED MOTION {OnOff(_settings.ReducedMotion)}";
            SaveSettings();
        }
        var selected = press(Keys.Enter) ? _optionsHubOption : -1;
        var (x, y) = OriginalPoint(mouse);
        if (pointerPressed)
        {
            _pressedOptionsHubOption = Enumerable.Range(0, _optionsHubOptions.Count)
                .FirstOrDefault(index => _optionsHubOptions[index].OriginalBounds.Contains(x, y), -1);
            if (_pressedOptionsHubOption >= 0) _optionsHubOption = _pressedOptionsHubOption;
        }
        if (pointerReleased)
        {
            if (_pressedOptionsHubOption >= 0
                && _optionsHubOptions[_pressedOptionsHubOption].OriginalBounds.Contains(x, y))
                selected = _pressedOptionsHubOption;
            _pressedOptionsHubOption = -1;
        }
        if (selected >= 0) ActivateOptionsHub(_optionsHubOptions[selected]);
    }

    private void ActivateOptionsHub(OptionsHubOption option)
    {
        _pressedOptionsHubOption = -1;
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
                SaveSettings();
                break;
            case OptionsHubAction.ToggleSoundEffects:
                _soundEffectsEnabled = !_soundEffectsEnabled;
                _notice = $"SOUND EFFECTS {OnOff(_soundEffectsEnabled)}";
                SaveSettings();
                break;
            case OptionsHubAction.ToggleSpeech:
                _speechEnabled = !_speechEnabled;
                _notice = $"DIGITIZED SPEECH {OnOff(_speechEnabled)}";
                SaveSettings();
                break;
            case OptionsHubAction.ToggleAnimation:
                _animationEnabled = !_animationEnabled;
                _notice = $"ANIMATION {OnOff(_animationEnabled)}";
                SaveSettings();
                break;
            case OptionsHubAction.ToggleMidiMusic: _notice = "MIDI MUSIC IS NOT AVAILABLE"; break;
            case OptionsHubAction.Practice:
                _practiceOption = 0;
                _screen = Screen.Practice;
                _notice = "SELECT A PRACTICE EVENT";
                break;
            case OptionsHubAction.Credits: PlayEventMovie("Options.Credits", Screen.OptionsHub); break;
            case OptionsHubAction.Movie: PlayEventMovie("Title.Intro", Screen.OptionsHub); break;
            case OptionsHubAction.Exit: Exit(); break;
            default: throw new ArgumentOutOfRangeException(nameof(option));
        }
    }

    private void UpdatePractice(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        var moved = false;
        if (press(Keys.Up) || press(Keys.Left))
        {
            _practiceOption = (_practiceOption + _practiceOptions.Count - 1) % _practiceOptions.Count;
            moved = true;
        }
        if (press(Keys.Down) || press(Keys.Right))
        {
            _practiceOption = (_practiceOption + 1) % _practiceOptions.Count;
            moved = true;
        }
        if (moved) _notice = _practiceOptions[_practiceOption].Label.ToUpperInvariant();

        var (x, y) = OriginalPoint(mouse);
        var hovered = Enumerable.Range(0, _practiceOptions.Count)
            .FirstOrDefault(index => _practiceOptions[index].OriginalBounds.Contains(x, y), -1);
        if (hovered >= 0)
        {
            _practiceOption = hovered;
            _notice = _practiceOptions[hovered].Label.ToUpperInvariant();
        }

        var selected = press(Keys.Enter) ? _practiceOption : click ? hovered : -1;
        if (selected < 0) return;
        switch (_practiceOptions[selected].Action)
        {
            case PracticeAction.Joust: PlayEventMovie("Practice.Joust", Screen.Practice); break;
            case PracticeAction.Exit: _screen = Screen.OptionsHub; break;
            case PracticeAction.War:
                _activePracticeCombat = PracticeCombatKind.War;
                _fieldBattle = PracticeCombatDefinitions.CreateWar(Environment.TickCount);
                _battleTick = 0;
                _screen = Screen.FieldBattle;
                _notice = "WAR PRACTICE";
                break;
            case PracticeAction.Melee:
                _activePracticeCombat = PracticeCombatKind.Melee;
                var meleeSeed = Environment.TickCount;
                var meleeScene = ImportedSiegeLayouts.ForPracticeMelee(_importedContent, new Random(meleeSeed).Next(3));
                ActivateSiege(meleeScene,
                    PracticeCombatDefinitions.CreateMelee(meleeSeed, meleeScene.Layout));
                _screen = Screen.Siege;
                _notice = "MELEE PRACTICE";
                break;
            case PracticeAction.CastleSkirmish:
                _activePracticeCombat = PracticeCombatKind.CastleSkirmish;
                var castleScene = ImportedSiegeLayouts.ForPracticeCastleSkirmish(_importedContent);
                ActivateSiege(castleScene,
                    PracticeCombatDefinitions.CreateCastleSkirmish(Environment.TickCount, castleScene.Layout));
                _screen = Screen.Siege;
                _notice = "CASTLE SKIRMISH PRACTICE";
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private Screen CampaignScreen() => _campaign.State.YouthDilemmasAnswered < Youth.OriginalPool.StageCount
        && _campaign.State.Player.Age < Balance.StartingAge ? Screen.Dilemma : Screen.Map;

    private static string OnOff(bool enabled) => enabled ? "ON" : "OFF";

    private void ToggleFullscreen()
    {
        _graphics.ToggleFullScreen();
        _notice = _graphics.IsFullScreen ? "FULLSCREEN" : "WINDOWED";
        SaveSettings();
    }

    private void ToggleIntegerScaling()
    {
        _settings = _settings with { IntegerScaling = !_settings.IntegerScaling };
        _notice = $"INTEGER SCALING {OnOff(_settings.IntegerScaling)}";
        SaveSettings();
    }

    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused)
        {
            _titleMovie?.Pause();
            _eventMovie?.Pause();
            _musicPausedByGame = _musicInstance?.State == SoundState.Playing;
            if (_musicPausedByGame) _musicInstance!.Pause();
        }
        else
        {
            _titleMovie?.Resume();
            _eventMovie?.Resume();
            if (_musicPausedByGame) StartMusic();
            _musicPausedByGame = false;
        }
    }

    private void AdjustSelectedVolume(float delta)
    {
        var setting = _optionsHubOptions[_optionsHubOption].Setting;
        switch (setting)
        {
            case OptionsHubSetting.CdMusic:
                _settings = _settings with { MusicVolume = StepVolume(_settings.MusicVolume, delta) };
                if (_musicInstance is not null) _musicInstance.Volume = _settings.MusicVolume;
                _notice = $"CD MUSIC VOLUME {VolumePercent(_settings.MusicVolume)}";
                break;
            case OptionsHubSetting.SoundEffects:
                _settings = _settings with { EffectsVolume = StepVolume(_settings.EffectsVolume, delta) };
                _notice = $"EFFECTS VOLUME {VolumePercent(_settings.EffectsVolume)}";
                break;
            case OptionsHubSetting.Speech:
                _settings = _settings with { SpeechVolume = StepVolume(_settings.SpeechVolume, delta) };
                _titleMovie?.SetVolume(_settings.SpeechVolume);
                _eventMovie?.SetVolume(_settings.SpeechVolume);
                _notice = $"SPEECH/MOVIE VOLUME {VolumePercent(_settings.SpeechVolume)}";
                break;
            default: return;
        }
        SaveSettings();
    }

    private static float StepVolume(float value, float delta) => Math.Clamp(
        MathF.Round((value + delta) * 10) / 10, 0, 1);

    private static string VolumePercent(float value) => $"{MathF.Round(value * 100):0}%";

    private void SaveSettings()
    {
        _settings = _settings with
        {
            CdMusic = _cdMusicEnabled,
            SoundEffects = _soundEffectsEnabled,
            Speech = _speechEnabled,
            Animation = _animationEnabled,
            Fullscreen = _graphics.IsFullScreen,
            IntegerScaling = _settings.IntegerScaling
        };
        _settingsStore.Save(_settings);
    }

    private void UpdateLoadGame(Func<Keys, bool> press, MouseState mouse, bool click)
    {
        if (press(Keys.F8))
        {
            LoadAutosave();
            return;
        }
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
        var inspected = _saveSlots.Inspect(number);
        if (!_saveSlots.TryLoad(number, out var campaign, out var error))
        {
            _notice = error ?? "CAMPAIGN COULD NOT BE LOADED";
            _saveSlotInfo = _saveSlots.Inspect();
            return;
        }

        ApplyLoadedCampaign(campaign!);
        _activeSaveSlot = number;
        _notice = inspected.RecoveredFromBackup
            ? $"PRIMARY SAVE COULD NOT BE USED; SLOT {number} RECOVERED FROM BACKUP"
            : $"CAMPAIGN LOADED FROM SLOT {number}";
    }

    private void LoadAutosave()
    {
        var inspected = _saveSlots.InspectAutosave();
        if (!_saveSlots.TryLoadAutosave(out var campaign, out var error))
        {
            _notice = error ?? "AUTOSAVE COULD NOT BE LOADED";
            return;
        }
        ApplyLoadedCampaign(campaign!);
        _notice = inspected.RecoveredFromBackup
            ? "PRIMARY AUTOSAVE COULD NOT BE USED; RECOVERED FROM BACKUP"
            : "AUTOSAVE LOADED";
    }

    private void ApplyLoadedCampaign(Campaign campaign)
    {
        _campaign = BindStrategicResources(campaign);
        _presentedEndReason = CampaignEndReason.None;
        ResetConversationSession();
        _fiefCheckpoint = null;
        _hasActiveCampaign = true;
        _fieldBattle = null;
        _dragonBattle = null;
        _dragonRunMovie?.Dispose();
        _dragonRunMovie = null;
        _siege = null;
        ClearSiegeVisuals();
        _youthDilemmaResult = null;
        _selectedLocation = _campaign.State.CurrentLocation;
        _screen = CampaignScreen();
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
                var selectedColor = _heraldicColors[_heraldicColor];
                var generatedCampaign = new Campaign(
                    Campaign.NewCustom(NormalizedCharacterName(), seed, selectedColor.Name,
                        selectedColor.OriginalStrategicCharacterColor), seed);
                generatedCampaign.InitializeOriginalStrategicNewGame();
                _campaign = BindStrategicResources(generatedCampaign);
                _presentedEndReason = CampaignEndReason.None;
                ResetConversationSession();
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
        var seed = Environment.TickCount;
        var selectedColor = _heraldicColors[_heraldicColor];
        var pregeneratedCampaign = new Campaign(Campaign.NewFromTemplate(index, selectedColor.Name,
            selectedColor.OriginalStrategicCharacterColor), seed);
        pregeneratedCampaign.InitializeOriginalStrategicNewGame();
        _campaign = BindStrategicResources(pregeneratedCampaign);
        _presentedEndReason = CampaignEndReason.None;
        ResetConversationSession();
        _hasActiveCampaign = true;
        _selectedLocation = 0;
        _screen = Screen.Briefing;
    }

    private string NormalizedCharacterName() => _characterName.Trim() is { Length: > 4 } name ? name : "Sir Custom";
    private (int X, int Y) OriginalPoint(MouseState mouse)
    {
        if (_controllerPointerActive) return ((int)_controllerPointer.X, (int)_controllerPointer.Y);
        return PresentationScaling.ToLogical(mouse.X, mouse.Y, CanvasBounds(), 640, 480);
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
        if (_campaign.State.OriginalStrategicState is { } strategic)
            OriginalStrategicMapCamera.FocusOnGridCell(
                strategic, strategic.PlayerHomeGridX, strategic.PlayerHomeGridY);
        _notice = "YOUR CAMPAIGN BEGINS";
        _screen = Screen.Map;
        Autosave();
    }

    private void Autosave()
    {
        if (_hasActiveCampaign) _saveSlots.SaveAutosave(_campaign);
    }
}
