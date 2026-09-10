using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void DrawInn()
    {
        var original = DrawOriginal("Village.Inn", new Rectangle(0, 0, 1024, 768));
        if (!original)
        {
            DrawPanel("THE INN", "TRAVELLERS AND LOCAL PATRONS GATHER HERE");
            DrawText("ORIGINAL INN ART IS NOT INSTALLED", 190, 340, Color.Wheat, 2);
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

        var node = _conversationSession?.CurrentNode;
        var prompt = _conversationSession?.Prompt;
        if (DrawOriginal("Dialogue.Frame", new Rectangle(0, 0, 1024, 768)))
        {
            if (node?.PortraitFile is not null && _conversationPortraits.TryGetValue(node.PortraitFile, out var portrait))
                _batch.Draw(portrait, ScaleBounds(ConversationPresentationDefinitions.Portrait), Color.White);
            else
                DrawOriginal(patron.PortraitRole, ScaleBounds(ConversationPresentationDefinitions.Portrait));
            DrawText(node?.Speaker ?? patron.Name, 155, 385, Color.White, 2, 260);
            var promptBounds = ScaleBounds(ConversationPresentationDefinitions.Prompt);
            DrawText(prompt ?? "Conversation data is not installed.", promptBounds.X,
                promptBounds.Y + 16, Color.White, 2, promptBounds.Width);
            if (node is not null)
            {
                var (mouseX, mouseY) = OriginalPoint(_lastMouse);
                for (var index = 0; index < node.Responses.Count; index++)
                {
                    var originalBounds = ConversationPresentationDefinitions.ResponseBounds(index);
                    var bounds = ScaleBounds(originalBounds);
                    var color = originalBounds.Contains(mouseX, mouseY) ? Color.Yellow : Color.Cyan;
                    DrawText(node.Responses[index].Text, bounds.X + 4, bounds.Y + 8, color, 2, bounds.Width - 8);
                }
            }
            else
                DrawText("ENTER  RETURN TO THE INN", 75, 500, Color.Cyan, 2, 850);
            return;
        }

        DrawPanel((node?.Speaker ?? patron.Name).ToUpperInvariant(), (prompt ?? "CONVERSATION DATA IS NOT INSTALLED").ToUpperInvariant());
        if (node is null)
            DrawText("ENTER  RETURN TO THE INN", 100, 300, Color.LightGreen, 2);
        else
            for (var index = 0; index < node.Responses.Count; index++)
                DrawText($"{index + 1}  {node.Responses[index].Text}", 100, 300 + index * 55, Color.LightGreen, 2, 820);
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

    private void ActivateSiegeVisuals(ImportedSiegeScene? imported)
    {
        ClearSiegeVisuals();
        if (imported is null || _importedContent is null) return;
        var paletteId = _importedContent.FindId("resource", ":skirmish.pal");
        var palette = paletteId is null ? null : _importedContent.DecodePalette(paletteId);
        if (palette is null) return;

        var required = new HashSet<int>();
        void Require(DynamixSceneBlock block)
        {
            foreach (var index in block.TextureReferences())
                if (index >= 0) required.Add(index);
        }

        var layoutTiles = imported.Layout.CopyTiles();
        for (var x = 0; x < layoutTiles.GetLength(0); x++)
        for (var y = 0; y < layoutTiles.GetLength(1); y++)
        {
            var block = imported.Scene.BlockAt(x + imported.SourceOriginX, y + imported.SourceOriginY);
            Require(block);
            if (layoutTiles[x, y] is not (SiegeTile.Door or SiegeTile.SecretDoor)) continue;
            for (var stateOffset = 1; stateOffset <= 2 && block.Index + stateOffset < imported.Scene.Blocks.Count;
                 stateOffset++)
            {
                var state = imported.Scene.Blocks[block.Index + stateOffset];
                if (state.Kind == block.Kind && state.Name.Equals(block.Name, StringComparison.OrdinalIgnoreCase))
                    Require(state);
            }
        }
        foreach (var spawn in imported.Layout.Enemies.Where(spawn => spawn.VisualId >= 0 &&
                     spawn.VisualId < imported.Scene.Blocks.Count))
        {
            var block = imported.Scene.Blocks[spawn.VisualId];
            if (block.Kind != 4 || block.Surface0 < 0) continue;
            for (var frame = 0; frame < 15 && block.Surface0 + frame < imported.Scene.TextureCount; frame++)
                required.Add(block.Surface0 + frame);
            foreach (var (stateOffset, frameCount) in new[] { (1, 9), (2, 3), (3, 8) })
            {
                var stateIndex = block.Index + stateOffset;
                if (stateIndex >= imported.Scene.Blocks.Count) continue;
                var state = imported.Scene.Blocks[stateIndex];
                if (state.Kind != 4 || !state.Name.Equals(block.Name, StringComparison.OrdinalIgnoreCase) ||
                    state.Surface0 < 0) continue;
                for (var frame = 0; frame < frameCount && state.Surface0 + frame < imported.Scene.TextureCount; frame++)
                    required.Add(state.Surface0 + frame);
            }
        }
        var textures = new Dictionary<int, Texture2D>();
        var sources = new Dictionary<int, DynamixSceneTexture>();
        foreach (var id in _importedContent.Ids("resource").Where(id =>
                     id.StartsWith(imported.ArchiveId + "#", StringComparison.OrdinalIgnoreCase) &&
                     id.Contains(":TEX", StringComparison.OrdinalIgnoreCase)))
        {
            var decoded = _importedContent.DecodeSceneTexture(id);
            if (decoded is null || !required.Contains(decoded.Index) || textures.ContainsKey(decoded.Index)) continue;
            var texture = new Texture2D(GraphicsDevice, decoded.Width, decoded.Height, false, SurfaceFormat.Color);
            texture.SetData(IndexedScenePixels.ToRgba(decoded.Indices, palette.Rgb));
            textures.Add(decoded.Index, texture);
            sources.Add(decoded.Index, decoded);
        }
        Texture2D? backdrop = null;
        if (imported.Backdrop is { } decodedBackdrop)
        {
            backdrop = new Texture2D(GraphicsDevice, decodedBackdrop.Width, decodedBackdrop.Height,
                false, SurfaceFormat.Color);
            backdrop.SetData(IndexedScenePixels.ToRgba(
                decodedBackdrop.Indices, palette.Rgb, transparentZero: false));
        }
        _siegeVisuals = new SiegeVisuals(
            imported.Scene, imported.SourceOriginX, imported.SourceOriginY,
            textures, sources, palette.Rgb, imported.ColorMaps, backdrop);
    }

    private Texture2D? SceneWallTexture(SiegeRayHit hit, int colorMapIndex)
    {
        if (_siegeVisuals is null) return null;
        var sourceX = hit.MapX + _siegeVisuals.SourceOriginX;
        var sourceY = hit.MapY + _siegeVisuals.SourceOriginY;
        if (sourceX is < 0 or >= DynamixScene.MapWidth || sourceY is < 0 or >= DynamixScene.MapHeight)
            return null;
        var block = _siegeVisuals.Scene.BlockAt(sourceX, sourceY);
        if (hit.Tile == SiegeTile.OpeningDoor && _siege?.DoorOpeningProgress(hit.MapX, hit.MapY) is { } progress)
        {
            var stateOffset = progress < 0.5 ? 1 : 2;
            var candidateIndex = block.Index + stateOffset;
            if (candidateIndex < _siegeVisuals.Scene.Blocks.Count)
            {
                var candidate = _siegeVisuals.Scene.Blocks[candidateIndex];
                if (candidate.Kind == block.Kind && candidate.Name.Equals(block.Name, StringComparison.OrdinalIgnoreCase))
                    block = candidate;
            }
        }
        var face = hit.Face switch
        {
            SiegeWallFace.North => DynamixSceneFace.North,
            SiegeWallFace.East => DynamixSceneFace.East,
            SiegeWallFace.South => DynamixSceneFace.South,
            _ => DynamixSceneFace.West
        };
        return _siegeVisuals.TextureFor(block.TextureForFace(face), colorMapIndex);
    }

    private (Texture2D? Texture, bool Flip) SceneEnemyTexture(SiegeEnemy enemy)
    {
        if (_siegeVisuals is null || enemy.VisualId < 0 || enemy.VisualId >= _siegeVisuals.Scene.Blocks.Count)
            return (null, false);
        var block = _siegeVisuals.Scene.Blocks[enemy.VisualId];
        var frame = _siege is null
            ? new SiegeEnemyFrame(4, false)
            : SiegeViewProjection.FrameFor(enemy, _siege.PlayerX, _siege.PlayerY);
        var textureIndex = block.Surface0 + enemy.WalkFrame * 5 + frame.DirectionOffset;
        if (enemy.VisualState == SiegeEnemyVisualState.Attack)
            textureIndex = ActorStateTexture(block, 1, enemy.VisualFrame, textureIndex);
        else if (enemy.VisualState == SiegeEnemyVisualState.Hit)
            textureIndex = ActorStateTexture(block, 2, (frame.DirectionOffset + 1) / 2, textureIndex);
        else if (enemy.VisualState == SiegeEnemyVisualState.Dying)
            textureIndex = ActorStateTexture(block, 3, enemy.VisualFrame, textureIndex);
        return (_siegeVisuals.TextureFor(textureIndex) ?? FirstSceneTexture(block),
            enemy.VisualState is SiegeEnemyVisualState.Attack or SiegeEnemyVisualState.Dying
                ? false : frame.FlipHorizontally);
    }

    private int ActorStateTexture(DynamixSceneBlock initial, int stateOffset, int frame, int fallback)
    {
        if (_siegeVisuals is null || initial.Index + stateOffset >= _siegeVisuals.Scene.Blocks.Count) return fallback;
        var state = _siegeVisuals.Scene.Blocks[initial.Index + stateOffset];
        return state.Kind == 4 && state.Name.Equals(initial.Name, StringComparison.OrdinalIgnoreCase) && state.Surface0 >= 0
            ? state.Surface0 + frame
            : fallback;
    }

    private Texture2D? FirstSceneTexture(DynamixSceneBlock block)
    {
        if (_siegeVisuals is null) return null;
        foreach (var index in block.TextureReferences())
            if (_siegeVisuals.TextureFor(index) is { } texture) return texture;
        return null;
    }

    private void ClearSiegeVisuals()
    {
        _siegeVisuals?.Dispose();
        _siegeVisuals = null;
        _siegeWeaponFrame = -1;
        _siegeWeaponEnd = 0;
        _siegeWeaponElapsed = 0;
        _siegeBloodFrame = -1;
        _siegeBloodElapsed = 0;
        _siegeImpactFrame = -1;
        _siegeImpactElapsed = 0;
    }

    private void DrawSiegeBackdrop(Rectangle viewport, Facing facing)
    {
        if (_siegeVisuals?.Backdrop is not { } backdrop)
        {
            Fill(new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height / 2), new Color(32, 31, 34));
            Fill(new Rectangle(viewport.X, viewport.Center.Y, viewport.Width, viewport.Height / 2), new Color(42, 35, 29));
            return;
        }

        var sourceWidth = Math.Min(640, backdrop.Width);
        var sourceX = (int)facing * backdrop.Width / 4 % backdrop.Width;
        var firstWidth = Math.Min(sourceWidth, backdrop.Width - sourceX);
        var firstDestinationWidth = firstWidth * viewport.Width / sourceWidth;
        _batch.Draw(backdrop, new Rectangle(viewport.X, viewport.Y, firstDestinationWidth, viewport.Height),
            new Rectangle(sourceX, 0, firstWidth, backdrop.Height), Color.White);
        if (firstWidth < sourceWidth)
        {
            var remaining = sourceWidth - firstWidth;
            _batch.Draw(backdrop,
                new Rectangle(viewport.X + firstDestinationWidth, viewport.Y,
                    viewport.Width - firstDestinationWidth, viewport.Height),
                new Rectangle(0, 0, remaining, backdrop.Height), Color.White);
        }
    }

    private void DrawSiege()
    {
        if (_siege is null) return;
        var originalShell = DrawOriginal("Combat.Shell", new Rectangle(0, 0, 1024, 768));
        var viewport = originalShell
            ? ScaleSiegeBounds(SiegeCombatPresentation.Viewport)
            : new Rectangle(0, 85, 1024, 520);
        if (!originalShell) Fill(new Rectangle(0, 0, 1024, 768), new Color(22, 19, 18));
        DrawSiegeBackdrop(viewport, _siege.Facing);
        var depths = new double[viewport.Width];
        for (var column = 0; column < viewport.Width; column++)
        {
            var camera = 2.0 * column / (viewport.Width - 1) - 1.0;
            var hit = SiegeViewProjection.Cast(_siege, camera);
            depths[column] = hit.Distance;
            var wallHeight = Math.Min(viewport.Height, (int)(viewport.Height / hit.Distance));
            var top = viewport.Center.Y - wallHeight / 2;
            var baseColor = hit.Tile switch
            {
                SiegeTile.Door => new Color(126, 83, 48),
                SiegeTile.SecretDoor => new Color(76, 73, 68),
                SiegeTile.OpeningDoor => new Color(102, 78, 54),
                _ => new Color(128, 126, 120)
            };
            var distanceShade = Math.Clamp(1.05f - (float)hit.Distance / 32f, 0.22f, 1f);
            var colorMapIndex = SiegeColorMapping.WallDistanceMap(hit.Distance);
            if (SceneWallTexture(hit, colorMapIndex) is { } wallTexture)
            {
                var sourceX = Math.Clamp((int)(hit.TextureOffset * wallTexture.Width), 0, wallTexture.Width - 1);
                _batch.Draw(wallTexture, new Rectangle(viewport.X + column, top, 1, wallHeight),
                    new Rectangle(sourceX, 0, 1, wallTexture.Height), Color.White);
            }
            else
            {
                Fill(new Rectangle(viewport.X + column, top, 1, wallHeight), baseColor * distanceShade);
            }
        }
        foreach (var projection in SiegeViewProjection.ProjectEnemies(_siege))
        {
            var (enemyTexture, flipEnemy) = SceneEnemyTexture(projection.Enemy);
            var wallHeight = viewport.Height / projection.ForwardDistance;
            var height = enemyTexture is null
                ? Math.Clamp((int)(wallHeight * 0.75), 20, viewport.Height)
                : Math.Clamp((int)(wallHeight * enemyTexture.Height / 256.0), 20, viewport.Height);
            var width = enemyTexture is null
                ? Math.Max(10, height / 2)
                : Math.Max(10, height * enemyTexture.Width / enemyTexture.Height);
            var center = viewport.X + (int)(projection.ScreenPosition * viewport.Width);
            var left = center - width / 2;
            var floor = viewport.Center.Y + (int)(wallHeight / 2);
            var top = floor - height;
            var body = projection.Enemy.Champion ? Color.DarkRed : new Color(120, 75, 50);
            for (var x = Math.Max(viewport.Left, left); x < Math.Min(viewport.Right, left + width); x++)
            {
                if (projection.ForwardDistance >= depths[x - viewport.X]) continue;
                if (enemyTexture is not null)
                {
                    var sourceX = Math.Clamp((x - left) * enemyTexture.Width / width, 0, enemyTexture.Width - 1);
                    if (flipEnemy) sourceX = enemyTexture.Width - 1 - sourceX;
                    _batch.Draw(enemyTexture, new Rectangle(x, top, 1, height),
                        new Rectangle(sourceX, 0, 1, enemyTexture.Height), Color.White);
                }
                else
                {
                    Fill(new Rectangle(x, top + height / 4, 1, height * 3 / 4), body);
                    Fill(new Rectangle(x, top, 1, height / 4), Color.Gray);
                }
            }
        }
        DrawSiegeForeground(viewport, originalShell);
        if (originalShell)
            DrawOriginalSiegeStatus();
        else
        {
            DrawText($"HEALTH {_siege.Health}/{_siege.MaxHealth}  ENEMIES {_siege.Enemies.Count(enemy => enemy.Health > 0)}  ALLIES {_siege.AlliesAlive}", 25, 25, Color.White, 2);
            DrawText($"FACING {_siege.Facing}  ARMOR {_siege.ArmorRating()}  GOLD FOUND {_siege.GoldFound}", 25, 55, Color.Wheat, 2);
            if (_showRadar) DrawRadar(_siege);
            DrawText("W/S MOVE  A/D TURN  E OPEN  SPACE SWING  X CROSSBOW", 130, 655, Color.Gold, 2);
            DrawText("M RADAR  R RETREAT", 380, 685, Color.Gold, 2);
        }
    }

    private void DrawOriginalSiegeStatus()
    {
        if (_siege is null) return;
        var health = ScaleSiegeBounds(SiegeCombatPresentation.HealthBar);
        Fill(health, Color.Black);
        var healthWidth = _siege.MaxHealth == 0 ? 0 : health.Width * _siege.Health / _siege.MaxHealth;
        if (healthWidth > 0) Fill(new Rectangle(health.X, health.Y, healthWidth, health.Height), Color.LimeGreen);

        var primary = ScaleSiegeBounds(SiegeCombatPresentation.PrimaryStatus);
        DrawText($"HEALTH {_siege.Health}/{_siege.MaxHealth}", primary.X, primary.Y, Color.White, 2);
        DrawText($"ENEMIES {_siege.Enemies.Count(enemy => enemy.Health > 0)}", primary.X, primary.Y + 18,
            Color.White, 2);
        var secondary = ScaleSiegeBounds(SiegeCombatPresentation.SecondaryStatus);
        DrawText($"ARMOR {_siege.ArmorRating()}", secondary.X, secondary.Y, Color.Wheat, 2);
        DrawText($"ALLIES {_siege.AlliesAlive}", secondary.X, secondary.Y + 18, Color.Wheat, 2);
        DrawText($"GOLD {_siege.GoldFound}", secondary.X, secondary.Y + 36, Color.Wheat, 2);

        if (_showRadar) DrawRadar(_siege, ScaleSiegeBounds(SiegeCombatPresentation.Radar));
        var message = ScaleSiegeBounds(SiegeCombatPresentation.Message);
        DrawText(_siege.LastMessage.ToUpperInvariant(), message.X, message.Y, Color.Wheat, 2, message.Width);
    }

    private void DrawSiegeForeground(Rectangle viewport, bool originalScale)
    {
        if (!_originalAnimations.TryGetValue("Combat.FirstPerson", out var animation)) return;
        if (_siegeWeaponFrame >= 0 && _siegeWeaponFrame < animation.Frames.Count)
        {
            var texture = animation.Frames[_siegeWeaponFrame];
            var width = originalScale
                ? texture.Width * 1024 / SiegeCombatPresentation.OriginalWidth
                : Math.Max(1, (int)Math.Round(texture.Width * Math.Min(2.5f, viewport.Height / 200f)));
            var height = originalScale
                ? texture.Height * 768 / SiegeCombatPresentation.OriginalHeight
                : Math.Max(1, (int)Math.Round(texture.Height * Math.Min(2.5f, viewport.Height / 200f)));
            DrawClipped(texture,
                new Rectangle(viewport.Center.X - width / 2, viewport.Bottom - height, width, height), viewport);
        }
        if (_siegeBloodFrame >= 0 && _siegeBloodFrame < animation.Frames.Count)
        {
            var texture = animation.Frames[_siegeBloodFrame];
            DrawSiegeEffect(texture, viewport, originalScale);
        }
        if (_siegeImpactFrame >= 0 && _siegeImpactFrame < animation.Frames.Count)
        {
            var texture = animation.Frames[_siegeImpactFrame];
            DrawSiegeEffect(texture, viewport, originalScale);
        }
    }

    private void DrawSiegeEffect(Texture2D texture, Rectangle viewport, bool originalScale)
    {
        var scale = Math.Min(3f, viewport.Height / 160f);
        var width = originalScale
            ? texture.Width * 1024 / SiegeCombatPresentation.OriginalWidth
            : Math.Max(1, (int)Math.Round(texture.Width * scale));
        var height = originalScale
            ? texture.Height * 768 / SiegeCombatPresentation.OriginalHeight
            : Math.Max(1, (int)Math.Round(texture.Height * scale));
        DrawClipped(texture,
            new Rectangle(viewport.Center.X - width / 2, viewport.Center.Y - height / 2, width, height), viewport);
    }

    private void DrawClipped(Texture2D texture, Rectangle destination, Rectangle clip)
    {
        var visible = Rectangle.Intersect(destination, clip);
        if (visible.Width <= 0 || visible.Height <= 0) return;
        var source = new Rectangle(
            (visible.X - destination.X) * texture.Width / destination.Width,
            (visible.Y - destination.Y) * texture.Height / destination.Height,
            Math.Max(1, visible.Width * texture.Width / destination.Width),
            Math.Max(1, visible.Height * texture.Height / destination.Height));
        source.Width = Math.Min(source.Width, texture.Width - source.X);
        source.Height = Math.Min(source.Height, texture.Height - source.Y);
        _batch.Draw(texture, visible, source, Color.White);
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

    private void DrawRadar(SiegeSession siege, Rectangle? requestedBounds = null)
    {
        var bounds = requestedBounds ?? new Rectangle(0, 0, 144, 144);
        var availableWidth = requestedBounds?.Width ?? 128;
        var availableHeight = requestedBounds?.Height ?? 128;
        var scale = Math.Clamp(Math.Min(availableWidth / siege.Width, availableHeight / siege.Height), 1, 9);
        var ox = requestedBounds is null
            ? 1004 - siege.Width * scale
            : bounds.X + (bounds.Width - siege.Width * scale) / 2;
        var oy = requestedBounds is null
            ? 85
            : bounds.Y + (bounds.Height - siege.Height * scale) / 2;
        Fill(requestedBounds ?? new Rectangle(ox - 8, oy - 8, siege.Width * scale + 16, siege.Height * scale + 16),
            new Color(10, 10, 10, 220));
        for (var x = 0; x < siege.Width; x++) for (var y = 0; y < siege.Height; y++)
        {
            var tile = siege.TileAt(x, y);
            var color = tile switch { SiegeTile.Wall => Color.Gray, SiegeTile.Door => Color.SaddleBrown, SiegeTile.SecretDoor => Color.DarkSlateGray, SiegeTile.OpeningDoor => Color.Peru, SiegeTile.Barrel => Color.Green, SiegeTile.Treasure => Color.Gold, _ => new Color(35, 35, 35) };
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
    private static Rectangle ScaleSiegeBounds(UiBounds bounds) => new(
        bounds.X * 1024 / SiegeCombatPresentation.OriginalWidth,
        bounds.Y * 768 / SiegeCombatPresentation.OriginalHeight,
        bounds.Width * 1024 / SiegeCombatPresentation.OriginalWidth,
        bounds.Height * 768 / SiegeCombatPresentation.OriginalHeight);
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
        var mouse = Mouse.GetState();
        var point = _controllerPointerActive
            ? new Point((int)(_controllerPointer.X * 1024 / 640), (int)(_controllerPointer.Y * 768 / 480))
            : new Point(PresentationScaling.ToVirtual(mouse.X, mouse.Y, CanvasBounds()).X,
                PresentationScaling.ToVirtual(mouse.X, mouse.Y, CanvasBounds()).Y);
        if (!_originalAnimations.TryGetValue(OriginalCursorAnimationRole, out var cursor))
        {
            if (_controllerPointerActive)
            {
                Fill(new Rectangle(point.X - 8, point.Y - 1, 17, 3), Color.Gold);
                Fill(new Rectangle(point.X - 1, point.Y - 8, 3, 17), Color.Gold);
            }
            return;
        }
        var frameIndex = OriginalCursorDefinitions.Frame(CurrentCursorKind(mouse));
        if (cursor.Frames.Count <= frameIndex) return;
        var frame = cursor.Frames[frameIndex];
        _batch.Draw(frame, new Rectangle(point.X, point.Y,
            frame.Width * 1024 / 640, frame.Height * 768 / 480), Color.White);
    }

    private Rectangle CanvasDestination()
    {
        var bounds = CanvasBounds();
        return new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
    }

    private UiBounds CanvasBounds() => PresentationScaling.Destination(
        GraphicsDevice.PresentationParameters.BackBufferWidth,
        GraphicsDevice.PresentationParameters.BackBufferHeight, _settings.IntegerScaling);

    private OriginalCursorKind CurrentCursorKind(MouseState mouse)
    {
        if (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed
            || _controllerPointerPressed)
            return OriginalCursorKind.Hand;
        var (x, y) = OriginalPoint(mouse);
        return _screen switch
        {
            Screen.Map => OriginalCursorKind.Travel,
            Screen.FieldBattle or Screen.Siege => OriginalCursorKind.Target,
            Screen.Inn when _innLayout.Patrons.Any(patron => patron.Bounds.Contains(x, y)) => OriginalCursorKind.Talk,
            Screen.InnDialogue or Screen.BlacksmithDialogue => OriginalCursorKind.Talk,
            Screen.Blacksmith when HitSceneHotspot(_blacksmithHotspots, mouse)?.Action
                == SceneNavigationAction.BlacksmithDialogue => OriginalCursorKind.Talk,
            _ => OriginalCursorKind.Sword
        };
    }
    private void DrawText(string text, int x, int y, Color color, int scale = 3, int wrap = 0) => PixelFont.Draw(_batch, _pixel, text, new Vector2(x, y), color, scale, wrap);
}
