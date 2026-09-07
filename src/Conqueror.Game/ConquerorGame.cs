using Conqueror.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed class ConquerorGame : Microsoft.Xna.Framework.Game
{
    private enum Screen { Title, Character, Dilemma, Map, Home, Village, Shop, Tournament, FieldBattle, Siege, Overview, Ending }
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _batch = null!;
    private Texture2D _pixel = null!;
    private Campaign _campaign = new();
    private Screen _screen = Screen.Title;
    private KeyboardState _last;
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
    private SoundEffect? _importedMusic;
    private SoundEffectInstance? _musicInstance;
    private readonly string _savePath = Path.Combine(AppContext.BaseDirectory, "saves", "campaign.json");

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

    protected override void Update(GameTime gameTime)
    {
        var keys = Keyboard.GetState();
        bool Press(Keys key) => keys.IsKeyDown(key) && !_last.IsKeyDown(key);
        if (Press(Keys.F5) && _screen != Screen.Title) { _campaign.Save(_savePath); _notice = "CAMPAIGN SAVED"; }
        if (Press(Keys.F9) && File.Exists(_savePath)) { _campaign = Campaign.Load(_savePath); _selectedLocation = _campaign.State.CurrentLocation; _screen = Screen.Map; _notice = "CAMPAIGN LOADED"; }
        if (Press(Keys.Escape))
        {
            if (_screen == Screen.Title) Exit();
            else if (_screen == Screen.FieldBattle && _fieldBattle is not null) { _fieldBattle.IssueAll(UnitOrder.Withdraw); _notice = "WITHDRAWAL ORDERED"; }
            else { if (_screen == Screen.Siege && _siege is not null) _campaign.FinishSiege(_siege); _screen = Screen.Map; }
        }

        switch (_screen)
        {
            case Screen.Title:
                if (Press(Keys.N)) _screen = Screen.Character;
                if (Press(Keys.L) && File.Exists(_savePath)) { _campaign = Campaign.Load(_savePath); _selectedLocation = _campaign.State.CurrentLocation; _screen = Screen.Map; }
                break;
            case Screen.Character:
                for (var i = 0; i < 6; i++) if (Press(Keys.D1 + i)) { _campaign = new Campaign(Campaign.NewFromTemplate(i)); _screen = Screen.Map; }
                if (Press(Keys.D0)) { _campaign = new Campaign(Campaign.NewCustom("Sir Custom", Environment.TickCount)); _screen = Screen.Dilemma; }
                break;
            case Screen.Dilemma: UpdateDilemma(Press); break;
            case Screen.Map: UpdateMap(Press); break;
            case Screen.Home: UpdateHome(Press); break;
            case Screen.Village: UpdateVillage(Press); break;
            case Screen.Shop: UpdateShop(Press); break;
            case Screen.Tournament: UpdateTournament(Press); break;
            case Screen.FieldBattle: UpdateFieldBattle(Press, gameTime); break;
            case Screen.Siege: UpdateSiege(Press); break;
            case Screen.Overview: if (Press(Keys.Enter) || Press(Keys.O)) _screen = Screen.Map; break;
            case Screen.Ending: if (Press(Keys.Enter)) _screen = Screen.Title; break;
        }
        if (_campaign.State.Victory != VictoryKind.None && _screen != Screen.Title) _screen = Screen.Ending;
        _last = keys;
        base.Update(gameTime);
    }

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

    private void UpdateDilemma(Func<Keys, bool> press)
    {
        for (var i = 0; i < 3; i++)
        {
            if (!press(Keys.D1 + i)) continue;
            _campaign.AnswerDilemma(i);
            if (_campaign.State.YouthDilemmasAnswered >= Youth.Dilemmas.Length)
            {
                _selectedLocation = 0;
                _screen = Screen.Map;
                _notice = "YOUR YEARS OF TRAINING ARE COMPLETE";
            }
        }
    }

    private void UpdateHome(Func<Keys, bool> press)
    {
        if (press(Keys.D1)) _campaign.Build("Steward"); if (press(Keys.D2)) _campaign.Build("Beadle");
        if (press(Keys.D3)) _campaign.Build("Priest"); if (press(Keys.D4)) _campaign.Build("Servant Room");
        if (press(Keys.D5)) _campaign.Build("House"); if (press(Keys.D6)) _campaign.Build("Monastery");
        if (press(Keys.D7)) _campaign.Plant(CropType.Beans); if (press(Keys.D8)) _campaign.Plant(CropType.Vegetables);
        if (press(Keys.Z)) _campaign.Plant(CropType.Grain); if (press(Keys.X)) _campaign.Plant(CropType.Fruit);
        if (press(Keys.D9)) _campaign.DevelopForest(ForestIndustry.Timber); if (press(Keys.G)) _campaign.DevelopForest(ForestIndustry.GoldMine);
        if (press(Keys.I)) _campaign.DevelopForest(ForestIndustry.IronMine); if (press(Keys.C)) _campaign.DevelopForest(ForestIndustry.CoalMine); if (press(Keys.S)) _campaign.DevelopForest(ForestIndustry.SilverMine);
        if (press(Keys.Q)) _campaign.Recruit(UnitType.Swordsmen); if (press(Keys.W)) _campaign.Recruit(UnitType.Halberdiers); if (press(Keys.R)) _campaign.Recruit(UnitType.Knights);
        if (press(Keys.Enter) || press(Keys.H)) _screen = Screen.Map;
    }

    private void UpdateVillage(Func<Keys, bool> press)
    {
        if (press(Keys.B)) _notice = _campaign.Borrow(200) ? "BORROWED 200S AT 50% INTEREST" : "LOAN REFUSED";
        if (press(Keys.D)) _campaign.Donate();
        if (press(Keys.C)) _campaign.Build("Church");
        if (press(Keys.A)) _notice = _campaign.BuyEquipment("Spiked Mace") ? "BOUGHT AND EQUIPPED SPIKED MACE" : "PURCHASE REFUSED";
        if (press(Keys.N)) _notice = _campaign.BuyEquipment("Norman Shield") ? "BOUGHT AND EQUIPPED NORMAN SHIELD" : "PURCHASE REFUSED";
        if (press(Keys.W)) _notice = _campaign.BuyEquipment("War Helm") ? "BOUGHT AND EQUIPPED WAR HELM" : "PURCHASE REFUSED";
        if (press(Keys.K)) { _shopIndex = 0; _screen = Screen.Shop; }
        if (press(Keys.OemPlus) || press(Keys.Add)) _campaign.State.Player.Home.TaxRate = Math.Min(100, _campaign.State.Player.Home.TaxRate + 5);
        if (press(Keys.OemMinus) || press(Keys.Subtract)) _campaign.State.Player.Home.TaxRate = Math.Max(0, _campaign.State.Player.Home.TaxRate - 5);
        if (press(Keys.Enter) || press(Keys.V)) _screen = Screen.Map;
    }

    private void UpdateShop(Func<Keys, bool> press)
    {
        var stock = Balance.Equipment.Where(x => x.Shop).ToArray();
        if (press(Keys.Up)) _shopIndex = (_shopIndex + stock.Length - 1) % stock.Length;
        if (press(Keys.Down)) _shopIndex = (_shopIndex + 1) % stock.Length;
        var item = stock[_shopIndex];
        if (press(Keys.B)) _notice = _campaign.BuyEquipment(item.Name) ? $"BOUGHT {item.Name}" : "PURCHASE REFUSED";
        if (press(Keys.S)) _notice = _campaign.SellEquipment(item.Name) ? $"SOLD {item.Name}" : "YOU DO NOT OWN THAT ITEM";
        if (press(Keys.Enter)) _screen = Screen.Village;
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
            case Screen.Title: DrawTitle(); break; case Screen.Character: DrawCharacter(); break; case Screen.Dilemma: DrawDilemma(); break; case Screen.Map: DrawMap(); break;
            case Screen.Home: DrawHome(); break; case Screen.Village: DrawVillage(); break; case Screen.Shop: DrawShop(); break; case Screen.Tournament: DrawTournament(); break; case Screen.FieldBattle: DrawFieldBattle(); break;
            case Screen.Siege: DrawSiege(); break; case Screen.Overview: DrawOverview(); break; case Screen.Ending: DrawEnding(); break;
        }
        if (_screen is not Screen.Title and not Screen.Character and not Screen.Dilemma) DrawText(_notice, 24, 730, Color.Gold, 2);
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
        Fill(new Rectangle(0, 0, 1024, 768), new Color(39, 50, 35));
        Fill(new Rectangle(90, 90, 844, 590), new Color(91, 63, 38));
        Fill(new Rectangle(110, 110, 804, 550), new Color(25, 30, 24));
        DrawText("CONQUEROR", 250, 190, Color.Gold, 7); DrawText("A.D. 1086", 350, 265, Color.Wheat, 4);
        DrawText("N  NEW CAMPAIGN", 360, 430, Color.White); DrawText("L  LOAD CAMPAIGN", 360, 475, Color.White);
        DrawText(_importedContent is null ? "ORIGINAL RESOURCES: NOT INSTALLED" : $"ORIGINAL RESOURCES: {_importedContent.Count} INSTALLED", 285, 535, _importedContent is null ? Color.Gray : Color.LightGreen, 2);
        DrawText("A MONOGAME REIMPLEMENTATION", 275, 600, new Color(160, 160, 140), 2);
    }

    private void DrawCharacter()
    {
        DrawPanel("CHOOSE YOUR KNIGHT", "THE ROAD FROM BOYHOOD TO LORDSHIP BEGINS");
        for (var i = 0; i < Balance.Templates.Length; i++)
        {
            var t = Balance.Templates[i];
            DrawText($"{i + 1}  {t.Name}   STR {t.Stats.Strength} DEX {t.Stats.Dexterity} PIE {t.Stats.Piety} STA {t.Stats.Stamina} HON {t.Stats.Honor}  {t.Wealth}S", 80, 180 + i * 62, i == 1 ? Color.Gold : Color.White, 2);
        }
        DrawText("0  CUSTOM ROLLED CHARACTER", 80, 575, Color.LightGreen, 2);
    }

    private void DrawDilemma()
    {
        var index = _campaign.State.YouthDilemmasAnswered;
        var dilemma = Youth.Dilemmas[Math.Min(index, Youth.Dilemmas.Length - 1)];
        var stats = _campaign.State.Player.Stats;
        DrawPanel($"YOUTH - YEAR {index + 1} OF 6", dilemma.Title);
        DrawText(dilemma.Prompt, 90, 190, Color.Wheat, 2, 820);
        for (var i = 0; i < dilemma.Choices.Length; i++) DrawText($"{i + 1}  {dilemma.Choices[i].Text}", 110, 300 + i * 75, Color.White);
        DrawText($"STR {stats.Strength}  DEX {stats.Dexterity}  PIETY {stats.Piety}  STAMINA {stats.Stamina}  HONOR {stats.Honor}", 100, 590, Color.Gold, 2);
    }

    private void DrawMap()
    {
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
        var f = _campaign.State.Player.Home; var p = _campaign.State.Player;
        DrawPanel("TACTICAL ROOM", $"WEALTH {p.Wealth}S  POPULATION {f.Population}  SERFS FREE {f.AvailableSerfs}  PRODUCTIVITY {f.Productivity()}%");
        DrawText("CASTLE", 70, 170, Color.Gold); DrawText("1 STEWARD 20S (+10%)   2 BEADLE 10S (+5%)", 70, 215, Color.White, 2);
        DrawText("3 PRIEST 10S (+5%)     4 SERVANT ROOM 50S", 70, 250, Color.White, 2);
        DrawText("VILLAGE AND FARM", 70, 310, Color.Gold); DrawText("5 HOUSE 5S   6 MONASTERY 150S (+15%)", 70, 355, Color.White, 2);
        DrawText("7 BEANS 1S   8 VEGETABLES 1S   Z GRAIN 1S   X FRUIT 5S", 70, 390, Color.White, 2);
        DrawText("FOREST", 70, 450, Color.Gold); DrawText("9 CUT TIMBER 5S", 70, 495, Color.White, 2);
        DrawText("G GOLD   I IRON   C COAL   S SILVER  (MINES 400S)", 70, 525, Color.White, 2);
        DrawText("WAR PLANNING", 70, 565, Color.Gold); DrawText("Q SWORDSMAN   W HALBERDIER   R KNIGHT", 70, 605, Color.White, 2);
        DrawText($"CURRENT: S {p.Army.Units[UnitType.Swordsmen]}  H {p.Army.Units[UnitType.Halberdiers]}  K {p.Army.Units[UnitType.Knights]}", 70, 640, Color.Wheat, 2);
        DrawText("ENTER RETURN TO MAP", 700, 680, Color.LightGreen, 2);
    }

    private void DrawVillage()
    {
        DrawPanel("THE VILLAGE", "VISIT THE MONEYLENDER, CHURCH, BLACKSMITH, AND INN");
        DrawText("B  BORROW 200S (300S DUE AT HARVEST)", 100, 220, Color.White); DrawText("D  DONATE 15S (+1 PIETY)", 100, 280, Color.White);
        DrawText("C  BUILD CHURCH 100S (+15% PRODUCTIVITY)", 100, 340, Color.White); DrawText("K  ENTER BLACKSMITH (ALL WEAPONS AND ARMOR)", 100, 400, Color.LightGreen, 2);
        DrawText($"PLUS MINUS TAX RATE  {_campaign.State.Player.Home.TaxRate}%", 100, 455, Color.Wheat, 2);
        DrawText("ENTER RETURN TO MAP", 100, 520, Color.LightGreen);
    }

    private void DrawShop()
    {
        var stock = Balance.Equipment.Where(x => x.Shop).ToArray();
        var p = _campaign.State.Player;
        DrawPanel("THE BLACKSMITH", $"WEALTH {p.Wealth}S   B BUY   S SELL FOR 75%   ENTER LEAVE");
        var first = Math.Clamp(_shopIndex - 3, 0, Math.Max(0, stock.Length - 8));
        for (var row = 0; row < 8 && first + row < stock.Length; row++)
        {
            var index = first + row; var item = stock[index]; var owned = p.Inventory.Items.Contains(item.Name);
            var details = item.Power > 0 ? $"POWER {item.Power}" : $"ARMOR {item.Armor}";
            DrawText($"{(index == _shopIndex ? ">" : " ")} {item.Name}  {item.BuyPrice}S  {details} {(owned ? "OWNED" : "")}", 85, 180 + row * 58, index == _shopIndex ? Color.Gold : owned ? Color.LightGreen : Color.White, 2);
        }
        DrawText($"EQUIPPED: {p.Inventory.Weapon} / {p.Inventory.Armor} / {p.Inventory.Shield} / {p.Inventory.Helm}", 75, 660, Color.Wheat, 2, 870);
    }

    private void DrawTournament()
    {
        DrawPanel("THE TOURNAMENT", "JOUST UP TO THREE TIMES, SKIRMISH, WAGER, AND COURT THE LADIES");
        Fill(new Rectangle(110, 310, 800, 12), Color.DarkGoldenrod); Fill(new Rectangle(500, 275, 12, 80), Color.Gold);
        Fill(new Rectangle(110 + _joustCursor * 4, 290, 8, 52), Color.White);
        DrawText("PRESS SPACE WHEN THE LANCE MEETS THE GOLD MARK", 170, 390, Color.White, 2);
        DrawText($"JOUSTS {_campaign.State.JoustsThisTournament}/3", 400, 445, Color.Gold, 2);
        var opponent = Balance.TournamentOpponents[_tournamentOpponent];
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
    private void DrawText(string text, int x, int y, Color color, int scale = 3, int wrap = 0) => PixelFont.Draw(_batch, _pixel, text, new Vector2(x, y), color, scale, wrap);
}
