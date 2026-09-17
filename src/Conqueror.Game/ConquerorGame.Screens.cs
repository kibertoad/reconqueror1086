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
                DrawText($"ENTER  RETURN TO THE {_conversationReturnScreen.ToString().ToUpperInvariant()}", 75, 500, Color.Cyan, 2, 850);
            return;
        }

        DrawPanel((node?.Speaker ?? patron.Name).ToUpperInvariant(), (prompt ?? "CONVERSATION DATA IS NOT INSTALLED").ToUpperInvariant());
        if (node is null)
            DrawText($"ENTER  RETURN TO THE {_conversationReturnScreen.ToString().ToUpperInvariant()}", 100, 300, Color.LightGreen, 2);
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
        DrawText("SPACE JOUST   I SPEAK   K SKIRMISH   ENTER LEAVE", 190, 610, Color.LightGreen, 2);
    }

    private SiegeVisuals ActivateSiegeVisuals(ImportedSiegeScene imported)
    {
        ClearSiegeVisuals();
        ArgumentNullException.ThrowIfNull(imported);
        var palette = ImportedSiegeLayouts.LoadCombatPalette(_importedContent);
        var colorMaps = imported.ColorMaps is null ? null :
            DynamixSceneColorMapGenerator.RegenerateFirstFamily(
                imported.ColorMaps, palette.Rgb, imported.Scene.ColorMapping);

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
        foreach (var stage in imported.Layout.Objects.SelectMany(item => item.Stages)
                     .Where(stage => stage.VisualId >= 0 && stage.VisualId < imported.Scene.Blocks.Count))
            Require(imported.Scene.Blocks[stage.VisualId]);
        var acquisitionRequired = imported.Scene.Blocks
            .SelectMany(block => block.RaycastTextureReferences())
            .ToHashSet();
        var textures = new Dictionary<int, Texture2D>();
        var sources = new Dictionary<int, DynamixSceneTexture>();
        foreach (var id in _importedContent.Ids("resource").Where(id =>
                     id.StartsWith(imported.ArchiveId + "#", StringComparison.OrdinalIgnoreCase) &&
                     id.Contains(":TEX", StringComparison.OrdinalIgnoreCase)))
        {
            var decoded = _importedContent.DecodeSceneTexture(id);
            if (decoded is null || sources.ContainsKey(decoded.Index)
                || !required.Contains(decoded.Index) && !acquisitionRequired.Contains(decoded.Index)) continue;
            sources.Add(decoded.Index, decoded);
            if (!required.Contains(decoded.Index)) continue;
            var texture = new Texture2D(GraphicsDevice, decoded.Width, decoded.Height, false, SurfaceFormat.Color);
            texture.SetData(IndexedScenePixels.ToRgba(decoded.Indices, palette.Rgb));
            textures.Add(decoded.Index, texture);
        }
        Texture2D? backdrop = null;
        if (imported.Backdrop is { } decodedBackdrop)
        {
            backdrop = new Texture2D(GraphicsDevice, decodedBackdrop.Width, decodedBackdrop.Height,
                false, SurfaceFormat.Color);
            backdrop.SetData(IndexedScenePixels.ToRgba(
                decodedBackdrop.Indices, palette.Rgb, transparentZero: false));
        }
        var visuals = new SiegeVisuals(
            imported.Scene, imported.SourceOriginX, imported.SourceOriginY,
            textures, sources, palette.Rgb, colorMaps, backdrop);
        _siegeVisuals = visuals;
        return visuals;
    }

    private Texture2D? SceneWallTexture(SiegeRayHit hit)
    {
        if (_siegeVisuals is null || SceneBlockForHit(hit) is not { } block) return null;
        var face = hit.Face switch
        {
            SiegeWallFace.North => DynamixSceneFace.North,
            SiegeWallFace.East => DynamixSceneFace.East,
            SiegeWallFace.South => DynamixSceneFace.South,
            _ => DynamixSceneFace.West
        };
        var colorMapIndex = SiegeColorMapping.WallDistanceMap(
            hit.Distance, _siegeVisuals.Scene.ColorMapping, block.ColorMapOffset);
        return _siegeVisuals.TextureFor(block.TextureForFace(face), colorMapIndex);
    }

    private DynamixSceneBlock? SceneBlockForHit(SiegeRayHit hit)
    {
        if (_siegeVisuals is null) return null;
        if (hit.SceneBlockIndex >= 0 && hit.SceneBlockIndex < _siegeVisuals.Scene.Blocks.Count)
            return _siegeVisuals.Scene.Blocks[hit.SceneBlockIndex];
        var sourceX = hit.MapX + _siegeVisuals.SourceOriginX;
        var sourceY = hit.MapY + _siegeVisuals.SourceOriginY;
        if (sourceX is < 0 or >= DynamixScene.MapWidth || sourceY is < 0 or >= DynamixScene.MapHeight)
            return null;
        if (_siege?.ObjectAt(hit.MapX, hit.MapY) is { VisualId: >= 0 } state &&
            state.VisualId < _siegeVisuals.Scene.Blocks.Count)
            return _siegeVisuals.Scene.Blocks[state.VisualId];
        return _siegeVisuals.Scene.BlockAt(sourceX, sourceY);
    }

    private SiegeProjectedBlock? SceneProjectionBlockAt(int localX, int localY, bool includePlayer = false)
    {
        if (_siegeVisuals is null || _siege is null || localX < 0 || localY < 0 ||
            localX >= _siege.Width || localY >= _siege.Height) return null;
        if (includePlayer && _siege.PlayerActor is { VisualId: >= 0 } player &&
            player.X == localX && player.Y == localY && player.VisualId < _siegeVisuals.Scene.Blocks.Count)
            return ProjectionActor(player);
        if (_siege.EnemyAt(localX, localY) is { VisualId: >= 0 } enemy &&
            enemy.VisualId < _siegeVisuals.Scene.Blocks.Count)
            return ProjectionActor(enemy);
        if (_siege.RetainerAt(localX, localY) is { VisualId: >= 0 } retainer &&
            retainer.VisualId < _siegeVisuals.Scene.Blocks.Count)
            return ProjectionActor(retainer);
        if (_siege.ObjectAt(localX, localY) is { VisualId: >= 0 } item &&
            item.VisualId < _siegeVisuals.Scene.Blocks.Count)
        {
            var active = _siegeVisuals.Scene.Blocks[item.VisualId];
            return new SiegeProjectedBlock(active, active.InitialXOffset8,
                active.InitialYOffset8, active.Index)
            {
                Object = item
            };
        }

        var sourceX = localX + _siegeVisuals.SourceOriginX;
        var sourceY = localY + _siegeVisuals.SourceOriginY;
        var index = _siegeVisuals.Scene.BlockIndexAt(sourceX, sourceY);
        var block = _siegeVisuals.Scene.Blocks[index];
        // A moved actor leaves its state-target block behind in the original map.
        if ((block.Behavior & 0x80) != 0 && block.InteractionSelector == 1 &&
            (uint)block.StateTarget < (uint)_siegeVisuals.Scene.Blocks.Count)
            block = _siegeVisuals.Scene.Blocks[block.StateTarget];
        return new SiegeProjectedBlock(block, block.InitialXOffset8,
            block.InitialYOffset8, block.Index);

        SiegeProjectedBlock ProjectionActor(SiegeEnemy actor)
        {
            var initial = _siegeVisuals.Scene.Blocks[actor.VisualId];
            var active = actor.VisualState switch
            {
                SiegeEnemyVisualState.Attack => ActorStateBlock(initial, 1),
                SiegeEnemyVisualState.Hit => ActorStateBlock(initial, 2),
                SiegeEnemyVisualState.Dying => ActorStateBlock(initial, 3),
                _ => initial
            };
            active = active with { Surface3 = actor.OriginalHeading8 ?? (int)actor.Facing << 6 };
            return new SiegeProjectedBlock(active, actor.OffsetX8, actor.OffsetY8, active.Index)
            {
                Actor = actor
            };
        }
    }

    private void ActivateSiege(ImportedSiegeScene imported, SiegeSession siege)
    {
        ArgumentNullException.ThrowIfNull(imported);
        ArgumentNullException.ThrowIfNull(siege);
        var visuals = ActivateSiegeVisuals(imported);
        _siege = siege;
        siege.ConfigureActorRaycast((source, target) => OriginalSiegeActorAcquisition.CastToward(
            siege, visuals.Scene,
            visuals.SourceOriginX, visuals.SourceOriginY,
            source, target, (x, y) => SceneProjectionBlockAt(x, y, includePlayer: true),
            visuals.SourceFor));
    }

    private Rectangle SiegeWallBounds(SiegeRayHit hit, Rectangle viewport)
    {
        var block = SceneBlockForHit(hit);
        var lower = block?.LowerElevation ?? 0;
        var upper = block?.UpperElevation ?? 0x100;
        var depth8 = Math.Max(0x10, hit.Distance8);
        var horizon = viewport.Y + viewport.Height / 2;
        var top = Math.Max(viewport.Top, horizon - (upper - 0x80) * viewport.Width / depth8);
        var bottom = Math.Min(viewport.Bottom - 1, horizon + (0x80 - lower) * viewport.Width / depth8);
        return new Rectangle(viewport.X, top, viewport.Width, Math.Max(1, bottom - top + 1));
    }

    private (Texture2D? Texture, bool Flip) SceneEnemyTexture(
        SiegeEnemy enemy, bool friendly, double distance)
    {
        if (_siegeVisuals is null || enemy.VisualId < 0 || enemy.VisualId >= _siegeVisuals.Scene.Blocks.Count)
            return (null, false);
        var block = _siegeVisuals.Scene.Blocks[enemy.VisualId];
        var colors = SiegeActorColorMapping.Normalize(
            _campaign.State.Player.HeraldicColor, friendly, block.Flags, block.Surface0);
        var frame = _siege is null
            ? new SiegeEnemyFrame(4, false)
            : SiegeViewProjection.FrameFor(enemy, _siege.PlayerX, _siege.PlayerY);
        var relativeHeading = frame.FlipHorizontally
            ? (256 - frame.DirectionOffset * 32) & 0xff
            : frame.DirectionOffset * 32;
        var selectedBlock = block;
        if (enemy.VisualState == SiegeEnemyVisualState.Attack)
            selectedBlock = ActorStateBlock(block, 1);
        else if (enemy.VisualState == SiegeEnemyVisualState.Hit)
            selectedBlock = ActorStateBlock(block, 2);
        else if (enemy.VisualState == SiegeEnemyVisualState.Dying)
            selectedBlock = ActorStateBlock(block, 3);
        if (enemy.VisualState == SiegeEnemyVisualState.Walk)
            selectedBlock = selectedBlock with { Surface0 = colors.WalkTextureBase };
        var selected = selectedBlock.TextureForBillboardHeading(relativeHeading);
        var textureIndex = selected.TextureIndex;
        if (enemy.VisualState == SiegeEnemyVisualState.Walk && block.Surface2 > 0)
        {
            var stride = enemy.OriginalMovement?.SurfaceIndexDeltaPerTick is > 0 and <= 4096
                ? enemy.OriginalMovement.SurfaceIndexDeltaPerTick
                : block.Surface2 / 2 + 1;
            textureIndex += enemy.WalkFrame * stride;
        }
        var colorMapIndex = SiegeActorColorMapping.DistanceMapIndex(
            colors, distance, _siegeVisuals.Scene.ColorMapping, selectedBlock.ColorMapOffset);
        return (_siegeVisuals.TextureFor(textureIndex, colorMapIndex) ?? FirstSceneTexture(selectedBlock),
            selected.FlipHorizontally);
    }

    private DynamixSceneBlock ActorStateBlock(DynamixSceneBlock initial, int stateOffset)
    {
        if (_siegeVisuals is null || initial.Index + stateOffset >= _siegeVisuals.Scene.Blocks.Count) return initial;
        var state = _siegeVisuals.Scene.Blocks[initial.Index + stateOffset];
        return state.Kind == 4 && state.Name.Equals(initial.Name, StringComparison.OrdinalIgnoreCase) && state.Surface0 >= 0
            ? state
            : initial;
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
        ClearSiegeWeapon();
        _siegeHitEffect = null;
    }

    private void ClearSiegeWeapon()
    {
        _siegeWeaponTrajectory = null;
        _siegeWeaponFrame = -1;
        _siegeWeaponRun = default;
        _siegeWeaponElapsed = 0;
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
            var hit = SiegeViewProjection.CastColumn(_siege, column, viewport.Width);
            depths[column] = hit.Distance;
            var wall = SiegeWallBounds(hit, viewport);
            var baseColor = hit.Tile switch
            {
                SiegeTile.Door => new Color(126, 83, 48),
                SiegeTile.SecretDoor => new Color(76, 73, 68),
                SiegeTile.Exit => new Color(86, 74, 58),
                _ => new Color(128, 126, 120)
            };
            var distanceShade = Math.Clamp(1.05f - (float)hit.Distance / 32f, 0.22f, 1f);
            if (SceneWallTexture(hit) is { } wallTexture)
            {
                var sourceX = Math.Clamp((int)(hit.TextureOffset * wallTexture.Width), 0, wallTexture.Width - 1);
                _batch.Draw(wallTexture, new Rectangle(viewport.X + column, wall.Y, 1, wall.Height),
                    new Rectangle(sourceX, 0, 1, wallTexture.Height), Color.White);
            }
            else
            {
                Fill(new Rectangle(viewport.X + column, wall.Y, 1, wall.Height), baseColor * distanceShade);
            }
        }
        var actors = SiegeViewProjection.ProjectEnemies(_siege)
            .Select(projection => (Projection: projection, Friendly: false))
            .Concat(SiegeViewProjection.ProjectRetainers(_siege)
                .Select(projection => (Projection: projection, Friendly: true)))
            .OrderByDescending(item => item.Projection.ForwardDistance)
            .ToArray();
        var objects = SiegeViewProjection.ProjectObjects(_siege);
        var actorIndex = 0;
        var objectIndex = 0;
        while (actorIndex < actors.Length || objectIndex < objects.Count)
        {
            if (objectIndex < objects.Count && (actorIndex >= actors.Length ||
                objects[objectIndex].ForwardDistance > actors[actorIndex].Projection.ForwardDistance))
            {
                DrawSiegeObject(objects[objectIndex++], viewport, depths);
                continue;
            }
            var actor = actors[actorIndex++];
            DrawSiegeActor(actor.Projection, actor.Friendly, viewport, depths);
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
            DrawText("1 ATTACK  2 DEFEND  3 FOLLOW  4/R RETAINERS", 210, 685, Color.Gold, 2);
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
            if (_siegeWeaponTrajectory is { } trajectory)
            {
                var localX = trajectory.Mirror ? trajectory.AnchorX : trajectory.AnchorX - texture.Width;
                var destination = new Rectangle(
                    viewport.X + localX * viewport.Width / SiegeCombatPresentation.Viewport.Width,
                    viewport.Y + trajectory.AnchorY * viewport.Height / SiegeCombatPresentation.Viewport.Height,
                    Math.Max(1, texture.Width * viewport.Width / SiegeCombatPresentation.Viewport.Width),
                    Math.Max(1, texture.Height * viewport.Height / SiegeCombatPresentation.Viewport.Height));
                DrawClipped(texture, destination, viewport,
                    trajectory.Mirror ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            }
            else
            {
                var width = originalScale
                    ? texture.Width * 1024 / SiegeCombatPresentation.OriginalWidth
                    : Math.Max(1, (int)Math.Round(texture.Width * Math.Min(2.5f, viewport.Height / 200f)));
                var height = originalScale
                    ? texture.Height * 768 / SiegeCombatPresentation.OriginalHeight
                    : Math.Max(1, (int)Math.Round(texture.Height * Math.Min(2.5f, viewport.Height / 200f)));
                DrawClipped(texture,
                    new Rectangle(viewport.Center.X - width / 2, viewport.Bottom - height, width, height), viewport);
            }
        }
        var hitFrame = _siegeHitEffect?.Frame ?? -1;
        if (hitFrame >= 0 && hitFrame < animation.Frames.Count)
        {
            var texture = animation.Frames[hitFrame];
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

    private void DrawClipped(Texture2D texture, Rectangle destination, Rectangle clip,
        SpriteEffects effects = SpriteEffects.None)
    {
        var visible = Rectangle.Intersect(destination, clip);
        if (visible.Width <= 0 || visible.Height <= 0) return;
        var source = new Rectangle(
            (visible.X - destination.X) * texture.Width / destination.Width,
            (visible.Y - destination.Y) * texture.Height / destination.Height,
            Math.Max(1, visible.Width * texture.Width / destination.Width),
            Math.Max(1, visible.Height * texture.Height / destination.Height));
        if ((effects & SpriteEffects.FlipHorizontally) != 0) source.X = texture.Width - source.Right;
        source.Width = Math.Min(source.Width, texture.Width - source.X);
        source.Height = Math.Min(source.Height, texture.Height - source.Y);
        _batch.Draw(texture, visible, source, Color.White, 0, Vector2.Zero, effects, 0);
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
            var color = tile switch { SiegeTile.Wall => Color.Gray, SiegeTile.Door => Color.SaddleBrown, SiegeTile.SecretDoor => Color.DarkSlateGray, SiegeTile.Barrel => Color.Green, SiegeTile.Treasure => Color.Gold, SiegeTile.Exit => Color.DarkRed, SiegeTile.Destructible => Color.SaddleBrown, _ => new Color(35, 35, 35) };
            Fill(new Rectangle(ox + x * scale, oy + y * scale, scale - 1, scale - 1), color);
        }
        foreach (var enemy in siege.Enemies.Where(enemy => enemy.Health > 0))
            Fill(new Rectangle(ox + enemy.X * scale, oy + enemy.Y * scale, scale - 1, scale - 1),
                enemy.Champion ? Color.Magenta : Color.Red);
        foreach (var retainer in siege.Retainers.Where(retainer => retainer.Health > 0))
            Fill(new Rectangle(ox + retainer.X * scale, oy + retainer.Y * scale, scale - 1, scale - 1),
                retainer.Selected ? Color.Gold : Color.RoyalBlue);
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
        var state = _campaign.State;
        var victory = state.Victory;
        Fill(new Rectangle(0, 0, 1024, 768), victory == VictoryKind.Defeat ? Color.Black : new Color(50, 30, 15));
        var title = victory switch
        {
            VictoryKind.Crown => "KING OF ENGLAND",
            VictoryKind.Dragon => "SLAYER OF THE DRAGON",
            _ when state.EndReason == CampaignEndReason.Dragon => "THE DRAGON CLAIMS ANOTHER VICTIM",
            _ when state.EndReason == CampaignEndReason.Drogo => "DROGO HAS KILLED YOU",
            _ when state.EndReason == CampaignEndReason.AgeLimit => "YOUR TIME HAS PASSED",
            _ => "YOUR QUEST HAS ENDED"
        };
        DrawText(title, 150, 260, victory == VictoryKind.Defeat ? Color.Gray : Color.Gold, 4, 760);
        DrawText("PRESS ENTER", 410, 520, Color.White, 2);
    }

    private void DrawSiegeObject(SiegeObjectProjection projection, Rectangle viewport, double[] depths)
    {
        Texture2D? texture = null;
        if (_siegeVisuals is not null && projection.Object.VisualId >= 0 &&
            projection.Object.VisualId < _siegeVisuals.Scene.Blocks.Count)
            texture = FirstSceneTexture(_siegeVisuals.Scene.Blocks[projection.Object.VisualId]);
        var layout = SiegeViewProjection.ObjectLayout(projection, viewport.Width, viewport.Height,
            texture?.Width, texture?.Height);
        var left = viewport.X + layout.Left;
        var top = viewport.Y + layout.Top;
        var width = layout.Width;
        var height = layout.Height;
        for (var x = Math.Max(viewport.Left, left); x < Math.Min(viewport.Right, left + width); x++)
        {
            if (projection.ForwardDistance >= depths[x - viewport.X]) continue;
            if (texture is null)
            {
                Fill(new Rectangle(x, top, 1, height), Color.SaddleBrown);
                continue;
            }
            var sourceX = Math.Clamp((x - left) * texture.Width / width, 0, texture.Width - 1);
            _batch.Draw(texture, new Rectangle(x, top, 1, height),
                new Rectangle(sourceX, 0, 1, texture.Height), Color.White);
        }
    }

    private void DrawSiegeActor(
        SiegeEnemyProjection projection, bool friendly, Rectangle viewport, double[] depths)
    {
        var (texture, flip) = SceneEnemyTexture(
            projection.Enemy, friendly, projection.ForwardDistance);
        var layout = SiegeViewProjection.ActorLayout(projection, viewport.Width, viewport.Height,
            texture?.Width, texture?.Height);
        var left = viewport.X + layout.Left;
        var top = viewport.Y + layout.Top;
        var width = layout.Width;
        var height = layout.Height;
        var body = friendly ? Color.RoyalBlue : projection.Enemy.Champion ? Color.DarkRed : new Color(120, 75, 50);
        for (var x = Math.Max(viewport.Left, left); x < Math.Min(viewport.Right, left + width); x++)
        {
            if (projection.ForwardDistance >= depths[x - viewport.X]) continue;
            if (texture is not null)
            {
                var sourceX = Math.Clamp((x - left) * texture.Width / width, 0, texture.Width - 1);
                if (flip) sourceX = texture.Width - 1 - sourceX;
                _batch.Draw(texture, new Rectangle(x, top, 1, height),
                    new Rectangle(sourceX, 0, 1, texture.Height), Color.White);
            }
            else
            {
                Fill(new Rectangle(x, top + height / 4, 1, height * 3 / 4), body);
                Fill(new Rectangle(x, top, 1, height / 4), Color.Gray);
            }
        }
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
