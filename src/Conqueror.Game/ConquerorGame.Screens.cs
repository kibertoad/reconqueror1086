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
        if (!DrawOriginal("Village.Inn", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Inn screen requires its verified original background art.");

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
        if (!DrawOriginal("Dialogue.Frame", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Conversation screen requires its verified original frame art.");
        if (node?.PortraitFile is not null && _conversationPortraits.TryGetValue(node.PortraitFile, out var portrait))
            _batch.Draw(portrait, ScaleBounds(ConversationPresentationDefinitions.Portrait), Color.White);
        else if (!DrawOriginal(patron.PortraitRole, ScaleBounds(ConversationPresentationDefinitions.Portrait)))
            throw new InvalidOperationException("Conversation screen requires its verified original patron portrait.");
        DrawText(node?.Speaker ?? patron.Name, 155, 385, Color.White, 2, 260);
        var promptBounds = ScaleBounds(ConversationPresentationDefinitions.Prompt);
        DrawText(prompt ?? "Conversation data is not installed.", promptBounds.X,
            promptBounds.Y + 16, Color.White, 2, promptBounds.Width);
        if (node is null)
            DrawText($"ENTER  RETURN TO THE {_conversationReturnScreen.ToString().ToUpperInvariant()}", 75, 500, Color.Cyan, 2, 850);
        else
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
    }

    private void DrawBlacksmith()
    {
        if (!DrawOriginal("Blacksmith.Workshop", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Blacksmith screen requires its verified original background art.");
        DrawSceneHoverLabel(_blacksmithHotspots, BlacksmithPresentationDefinitions.HoverLabelBounds);
    }

    private void DrawBlacksmithDialogue()
    {
        if (!DrawOriginal("Dialogue.Frame", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Blacksmith conversation requires its verified original frame art.");
        if (!DrawOriginal("Blacksmith.Portrait", ScaleBounds(BlacksmithDialoguePresentationDefinitions.Portrait)))
            throw new InvalidOperationException("Blacksmith conversation requires its verified original portrait art.");
        DrawText(BlacksmithDialoguePresentationDefinitions.Speaker, 155, 385, Color.White, 2, 260);
        DrawText(BlacksmithDialoguePresentationDefinitions.FallbackPrompt, 435, 65, Color.White, 2, 520);
        for (var index = 0; index < BlacksmithDialoguePresentationDefinitions.Commands.Count; index++)
            DrawText(BlacksmithDialoguePresentationDefinitions.Commands[index].Label, 75, 500 + index * 55, Color.Cyan, 2, 850);
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
        if (!DrawOriginal("Shop.Inventory", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Shop screen requires its verified original background art.");
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
        if (!DrawOriginal("Tournament.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Tournament screen requires its verified original background art.");
        Fill(new Rectangle(110, 310, 800, 12), Color.DarkGoldenrod); Fill(new Rectangle(500, 275, 12, 80), Color.Gold);
        var joustCursor = (int)Math.Round(_joustCursor, MidpointRounding.AwayFromZero);
        Fill(new Rectangle(110 + joustCursor * 4, 290, 8, 52), Color.White);
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
        var originalDialogueTracksCourtship = _campaign.State.ConversationVariables.Count
            > OriginalCampaignVariables.LadyColors;
        var courtshipLine = originalDialogueTracksCourtship
            ? $"UP DOWN LADY: {lady.Name}   C REQUEST COLORS"
            : $"UP DOWN LADY: {lady.Name}   WINS {_campaign.State.Player.CourtshipWins.GetValueOrDefault(lady.Name)}   C REQUEST COLORS";
        DrawText(courtshipLine, 150, 515, lady.CourtAble ? Color.Wheat : Color.Gray, 2);
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

        var required = SiegeTextureDependencies.RenderTextures(
            imported.Scene, imported.Layout, imported.SourceOriginX, imported.SourceOriginY,
            _campaign.State.Player.HeraldicColor);
        var acquisitionRequired = SiegeTextureDependencies.AcquisitionTextures(imported.Scene);
        var sources = ImportedSiegeLayouts.LoadCombatTextureSources(
            _importedContent, imported.ArchiveId, required.Concat(acquisitionRequired));

        var textures = new Dictionary<int, Texture2D>();
        foreach (var index in required)
        {
            var decoded = sources[index];
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
            textures, sources, palette.Rgb, colorMaps, imported.Backdrop, backdrop);
        _siegeVisuals = visuals;
        return visuals;
    }

    private Texture2D SceneWallTexture(SiegeRayHit hit)
    {
        var visuals = _siegeVisuals ?? throw new InvalidOperationException(
            "Imported siege visuals must be active before rendering.");
        var block = SceneBlockForHit(hit) ?? throw new InvalidDataException(
            "The imported siege ray contacted a block outside the active scene.");
        var face = hit.Face switch
        {
            SiegeWallFace.North => DynamixSceneFace.North,
            SiegeWallFace.East => DynamixSceneFace.East,
            SiegeWallFace.South => DynamixSceneFace.South,
            _ => DynamixSceneFace.West
        };
        var colorMapIndex = SiegeColorMapping.WallDistanceMap(
            hit.Distance, _siegeVisuals.Scene.ColorMapping, block.ColorMapOffset);
        var textureIndex = block.TextureForFace(face);
        return visuals.TextureFor(textureIndex, colorMapIndex) ?? throw new InvalidDataException(
            $"The imported siege wall references unavailable texture {textureIndex}.");
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

    private (Texture2D Texture, bool Flip) SceneEnemyTexture(
        SiegeEnemy enemy, bool friendly, double distance)
    {
        var visuals = _siegeVisuals ?? throw new InvalidOperationException(
            "Imported siege visuals must be active before rendering.");
        if (enemy.VisualId < 0 || enemy.VisualId >= visuals.Scene.Blocks.Count)
            throw new InvalidDataException($"Siege actor references invalid visual block {enemy.VisualId}.");
        var block = visuals.Scene.Blocks[enemy.VisualId];
        var colors = SiegeActorColorMapping.Normalize(
            _campaign.State.Player.HeraldicColor, friendly, block.Flags, block.Surface0);
        var actorHeading = enemy.OriginalHeading8 ?? (int)enemy.Facing << 6;
        var viewerHeading = _siege is null ? 0 : (int)_siege.Facing << 6;
        var relativeHeading = (actorHeading - viewerHeading) & 0xff;
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
            colors, distance, visuals.Scene.ColorMapping, selectedBlock.ColorMapOffset);
        return (visuals.TextureFor(textureIndex, colorMapIndex) ?? throw new InvalidDataException(
                $"Siege actor block {enemy.VisualId} references unavailable texture {textureIndex}."),
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

    private DynamixSceneBlock SceneActorBlock(SiegeEnemy enemy)
    {
        var visuals = _siegeVisuals ?? throw new InvalidOperationException(
            "Imported siege visuals must be active before rendering.");
        if (enemy.VisualId < 0 || enemy.VisualId >= visuals.Scene.Blocks.Count)
            throw new InvalidDataException($"Siege actor references invalid visual block {enemy.VisualId}.");
        var initial = visuals.Scene.Blocks[enemy.VisualId];
        return enemy.VisualState switch
        {
            SiegeEnemyVisualState.Attack => ActorStateBlock(initial, 1),
            SiegeEnemyVisualState.Hit => ActorStateBlock(initial, 2),
            SiegeEnemyVisualState.Dying => ActorStateBlock(initial, 3),
            _ => initial
        };
    }

    private Texture2D FirstSceneTexture(DynamixSceneBlock block)
    {
        var visuals = _siegeVisuals ?? throw new InvalidOperationException(
            "Imported siege visuals must be active before rendering.");
        foreach (var index in block.TextureReferences())
            if (visuals.TextureFor(index) is { } texture) return texture;
        throw new InvalidDataException($"Siege block {block.Index} has no available render texture.");
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
        var backdrop = _siegeVisuals?.Backdrop ?? throw new InvalidOperationException(
            "Imported siege backdrop must be active before rendering.");

        var decoded = _siegeVisuals?.DecodedBackdrop ?? throw new InvalidOperationException(
            "Imported siege backdrop metadata must be active before rendering.");
        var slice = SiegeCombatPresentation.BackdropSlice(
            facing, backdrop.Width, backdrop.Height, decoded.Horizon, decoded.Mode);
        var destination = new Rectangle(
            viewport.X + slice.Destination.X * viewport.Width / SiegeCombatPresentation.Viewport.Width,
            viewport.Y + slice.Destination.Y * viewport.Height / SiegeCombatPresentation.Viewport.Height,
            slice.Destination.Width * viewport.Width / SiegeCombatPresentation.Viewport.Width,
            slice.Destination.Height * viewport.Height / SiegeCombatPresentation.Viewport.Height);
        _batch.Draw(backdrop, destination,
            new Rectangle(slice.Source.X, slice.Source.Y, slice.Source.Width, slice.Source.Height),
            Color.White);
    }

    private void DrawSiege()
    {
        if (_siege is null) return;
        if (_siegeVisuals is null)
            throw new InvalidOperationException("A siege session cannot render without imported scene visuals.");
        if (!DrawOriginal("Combat.Shell", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidDataException("The required original combat shell is unavailable.");
        var viewport = ScaleSiegeBounds(SiegeCombatPresentation.Viewport);
        DrawSiegeBackdrop(viewport, _siege.Facing);
        var depths = new double[viewport.Width];
        for (var column = 0; column < viewport.Width; column++)
        {
            var hit = SiegeViewProjection.CastColumn(_siege, column, viewport.Width);
            depths[column] = hit.Distance;
            var wall = SiegeWallBounds(hit, viewport);
            var wallTexture = SceneWallTexture(hit);
            var sourceX = Math.Clamp((int)(hit.TextureOffset * wallTexture.Width), 0, wallTexture.Width - 1);
            _batch.Draw(wallTexture, new Rectangle(viewport.X + column, wall.Y, 1, wall.Height),
                new Rectangle(sourceX, 0, 1, wallTexture.Height), Color.White);
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
        DrawSiegeForeground(viewport, originalScale: true);
        DrawOriginalSiegeStatus();
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
        if (!DrawOriginal(FieldBattlePresentation.BackgroundArtRole, new Rectangle(0, 0, 1024, 728)))
            throw new InvalidOperationException("Field battle requires its verified original battlefield art.");

        var draws = FieldBattlePresentation.SpriteDrawsFor(
            _fieldBattle.Squads, _selectedUnit, _fieldBattle.TickNumber);
        if (!_originalAnimations.TryGetValue(FieldBattlePresentation.UnitAnimationRole, out var animation))
            throw new InvalidOperationException("Field battle requires its verified original unit animation.");
        foreach (var draw in draws)
        {
            if (draw.Frame >= animation.Frames.Count) continue;
            var frame = animation.Frames[draw.Frame];
            _batch.Draw(frame, ScaleBounds(new UiBounds(draw.X, draw.Y, frame.Width, frame.Height)), Color.White);
            if (draw.Selected && OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayFrame < animation.Frames.Count)
            {
                var overlay = animation.Frames[OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayFrame];
                _batch.Draw(overlay, ScaleBounds(new UiBounds(
                    draw.X + OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfWidth
                        - OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayOffsetX,
                    draw.Y + OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfHeight
                        - OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayOffsetY,
                    overlay.Width, overlay.Height)), Color.White);
            }
        }
        foreach (var draw in draws)
        {
            var squad = _fieldBattle.Squads[draw.SquadIndex];
            var count = ScaleBounds(new UiBounds(draw.X + 34, draw.Y + 34, 30, 14));
            DrawText($"{squad.Count}", count.X, count.Y, Color.White, 1, count.Width);
        }
        DrawText($"FIELD BATTLE - TICK {_fieldBattle.TickNumber}", 34, 18, Color.Gold, 2);
        DrawText($"SELECTED {_selectedUnit}: CLICK FRIENDLY, THEN CLICK GROUND TO MOVE", 20, 700, Color.White, 1);
        var selectedSquad = _fieldBattle.Friendly.FirstOrDefault(squad => squad.Type == _selectedUnit);
        var selectedOrder = selectedSquad?.Order;
        if (selectedSquad is { DestinationX: { } targetX, DestinationY: { } targetY })
        {
            var cell = FieldBattlePresentation.CellBounds(targetX, targetY);
            var target = ScaleBounds(new UiBounds(cell.X, cell.Y, cell.Width, cell.Height));
            DrawOutline(new Rectangle(target.Center.X - 10, target.Center.Y - 10, 20, 20), Color.Gold, 2);
        }
        foreach (var button in FieldBattlePointerControls.Buttons)
        {
            var bounds = ScaleBounds(button.Bounds);
            Fill(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height),
                !button.AllUnits && button.Order == selectedOrder ? Color.DarkOliveGreen : Color.DarkSlateGray);
            DrawText(button.Label, bounds.X + 4, bounds.Y + 6, Color.White, 1, bounds.Width - 8);
        }
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
        if (!DrawOriginal("Home.Overview", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Personal overview requires its verified original background art.");
        DrawText(p.Name, 325, 78, Color.Wheat, 2, 360);
        DrawText($"{p.Age}", 130, 61, Color.Wheat, 2);
        DrawText($"{p.Home.Population}", 175, 237, Color.Wheat, 2);
        DrawText($"{p.Wealth}", 710, 157, Color.Wheat, 2);
        DrawText($"{s.Strength}", 710, 200, Color.Wheat, 2);
        DrawText($"{s.Piety}", 710, 239, Color.Wheat, 2);
        DrawText($"{s.Honor}", 710, 278, Color.Wheat, 2);
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
        var visuals = _siegeVisuals ?? throw new InvalidOperationException(
            "Imported siege visuals must be active before rendering.");
        if (projection.Object.VisualId < 0 || projection.Object.VisualId >= visuals.Scene.Blocks.Count)
            throw new InvalidDataException(
                $"Siege object references invalid visual block {projection.Object.VisualId}.");
        var texture = FirstSceneTexture(visuals.Scene.Blocks[projection.Object.VisualId]);
        var layout = SiegeViewProjection.ObjectLayout(projection, viewport.Width, viewport.Height,
            texture.Width, texture.Height);
        var left = viewport.X + layout.Left;
        var top = viewport.Y + layout.Top;
        var width = layout.Width;
        var height = layout.Height;
        for (var x = Math.Max(viewport.Left, left); x < Math.Min(viewport.Right, left + width); x++)
        {
            if (projection.ForwardDistance >= depths[x - viewport.X]) continue;
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
        var layout = SiegeViewProjection.ActorLayout(
            projection, SceneActorBlock(projection.Enemy), viewport.Width, viewport.Height);
        var left = viewport.X + layout.Left;
        var top = viewport.Y + layout.Top;
        var width = layout.Width;
        var height = layout.Height;
        var visibleTop = Math.Max(viewport.Top, top);
        var visibleBottom = Math.Min(viewport.Bottom, top + height);
        if (visibleBottom <= visibleTop) return;
        var sourceY = Math.Clamp((visibleTop - top) * texture.Height / height, 0, texture.Height - 1);
        var sourceHeight = Math.Max(1,
            Math.Min(texture.Height - sourceY, (visibleBottom - visibleTop) * texture.Height / height));
        for (var x = Math.Max(viewport.Left, left); x < Math.Min(viewport.Right, left + width); x++)
        {
            if (projection.ForwardDistance >= depths[x - viewport.X]) continue;
            var sourceX = Math.Clamp((x - left) * texture.Width / width, 0, texture.Width - 1);
            if (flip) sourceX = texture.Width - 1 - sourceX;
            _batch.Draw(texture, new Rectangle(x, visibleTop, 1, visibleBottom - visibleTop),
                new Rectangle(sourceX, sourceY, 1, sourceHeight), Color.White);
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
    private void DrawText(string text, int x, int y, Color color, int scale = 3, int wrap = 0) =>
        PixelFont.Draw(_batch, _pixel, text, new Vector2(x, y), color, scale, wrap);
}
