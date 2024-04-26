using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace OreDetector
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// 

        public static ModEntry? instance;

        private OreDetector? detector;

        private Texture2D? ladderTexture;

        public static ModConfig? Config;

        private Dictionary<string, Color> lineColors = new Dictionary<string, Color>()
        {
            ["Red"] = Color.Red,
            ["Blue"] = Color.Blue,
            ["Green"] = Color.Green,
            ["Yellow"] = Color.Yellow,
        };


        public override void Entry(IModHelper helper)
        {
            instance = this;
            detector = OreDetector.GetOreDetector();
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.World.NpcListChanged += OnNPCListChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            ladderTexture = Helper.ModContent.Load<Texture2D>("assets\\ladder.png");
            Config = new ModConfig();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        ///
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Position",
                getValue: () => Config.PositionOption,
                setValue: value => Config.PositionOption = value,
                allowedValues: new string[] { "Above player", "Top left", "Next to cursor", "Custom" }
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Draw line to ladder",
                getValue: () => Config.arrowPointingToLadder,
                setValue: value => Config.arrowPointingToLadder = value
            );
            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => "Line color for ladder",
                getValue: () => Config.arrowToLadderColor,
                setValue: value => Config.arrowToLadderColor = value,
                allowedValues: new string[] { "Red", "Green", "Blue", "Yellow" }
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Draw line to hole",
                getValue: () => Config.arrowPointingToHole,
                setValue: value => Config.arrowPointingToHole = value
            );
            configMenu.AddTextOption(
            mod: this.ModManifest,
            name: () => "Line color for ladder",
            getValue: () => Config.arrowToHoleColor,
            setValue: value => Config.arrowToHoleColor = value,
            allowedValues: new string[] { "Red", "Green", "Blue", "Yellow" }
            );
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) 
                return;

            if (e.Button == SButton.P)
            {
                Game1.activeClickableMenu = new InvisibleMenu();
            }
        }
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft)
            {
                detector.GetOreInCurrentShaft();
                detector.LookForSpawnedLadders();
            }
        }

        private void OnNPCListChanged(object? sender, NpcListChangedEventArgs e)
        {
            if (detector.currentShaft == null)
                return;

            detector.LookForSpawnedLadders();
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e) 
        {
            if (e.Location != detector.currentShaft)
                return;

            foreach (var item in e.Removed)
            {
                if (!detector.Ores.Keys.Contains(item.Value.DisplayName))
                    continue;

                detector.MinedOres[item.Value.DisplayName].Add(item.Value);
            }
            detector.LookForSpawnedLadders();
            detector.LookForSpawnedHoles();
        }

        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if(!Context.IsWorldReady) 
                return;

            if (Game1.player.currentLocation != detector.currentShaft)
                return;

            SpriteBatch batch = Game1.spriteBatch;
            switch(Config.PositionOption)
            {
                case "Above player":
                    DrawOverlayAbovePlayer(batch);
                    break;
                case "Top left":
                    DrawOverlayTopLeftCorner(batch);
                    break;
                case "Next to cursor":
                    DrawOverlayCursor(batch);
                    break;
                case "Custom":
                    DrawOverlayCustomPosition(batch);
                    break;
            }
            if (Config.arrowPointingToLadder)
            {
                DrawLineToTiles(batch, lineColors[Config.arrowToLadderColor], detector.ladderPositions);
            }
            if (Config.arrowPointingToHole)
            {
                DrawLineToTiles(batch, lineColors[Config.arrowToHoleColor], detector.HolePositions);
            }
        }

        private void DrawLineToTiles(SpriteBatch batch, Color color, List<Vector2> tiles)
        {
            if (tiles.Count <= 0) return;

            foreach (Vector2 position in tiles)
            {
                int width = 5;
                Vector2 ladderPosition = new Vector2((position.X * Game1.tileSize) - Game1.viewport.X + Game1.tileSize / 2, (position.Y * Game1.tileSize) - Game1.viewport.Y + Game1.tileSize / 2);
                Vector2 playerPosition = new Vector2(Game1.player.Position.X - Game1.viewport.X + Game1.tileSize / 2, Game1.player.Position.Y - Game1.viewport.Y);
                Vector2 startPos = ladderPosition;
                Vector2 endPos = playerPosition;
                // Create a texture as wide as the distance between two points and as high as
                // the desired thickness of the line.
                var distance = (int)Vector2.Distance(startPos, endPos);
                var texture = new Texture2D(batch.GraphicsDevice, distance, width);

                // Fill texture with given color.

                var data = new Color[distance * width];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = color;
                }
                texture.SetData(data);

                // Rotate about the beginning middle of the line.
                var rotation = (float)Math.Atan2(endPos.Y - startPos.Y, endPos.X - startPos.X);
                var origin = new Vector2(0, width / 2);

                batch.Draw(
                    texture,
                    startPos,
                    null,
                    Color.White,
                    rotation,
                    origin,
                    1.0f,
                    SpriteEffects.None,
                    1.0f);
            }

        }
        private void DrawOverlay(SpriteBatch batch, Vector2 position, float transparency)
        {
            int padding = 0;
            string result = "";
            foreach (var item in detector.Ores)
            {
                string text = $"{item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}\n";
                if (text.Length > padding)
                    padding = text.Length;
                result += text;
            }
            result += "Ladder: ";
            result += detector.LadderRevealed ? "Yes" : "No";

            int counter = 0;
            foreach (var item in detector.Ores)
            {
                string itemId = detector.itemIds[item.Key];

                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
                string itemTypeId = data.GetItemTypeId();
                Texture2D texture = data.GetTexture();
                Rectangle sourceRect = data.GetSourceRect();
                int bigCraftableOffset = itemTypeId == "(BC)" ? 12 : 0;
                bool isBigCraftable = itemTypeId == "(BC)";
                batch.Draw(texture, position + new Vector2(-4 * padding + bigCraftableOffset, Game1.dialogueFont.LineSpacing * counter), sourceRect, Color.White * transparency, 0f, Vector2.Zero, isBigCraftable ? 1.5f : 3f, SpriteEffects.None, 0f);
                counter++;
            }
            batch.Draw(ladderTexture, position + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

            batch.DrawString(Game1.dialogueFont, result, position, Color.White * transparency);
        }

        private void DrawOverlayCustomPosition(SpriteBatch batch)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            Vector2 position = Config.customPosition;
            DrawOverlay(batch, position , 1f);
        }
        private void DrawOverlayTopLeftCorner(SpriteBatch batch)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            Vector2 position = new Vector2(64, 80);
            DrawOverlay(batch, position, 1f);
        }

        private void DrawOverlayCursor(SpriteBatch batch)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;
            Farmer player = Game1.player;
            float transparency = player.isMoving() && !player.UsingTool ? 0.1f : 1f;
            Vector2 position = Game1.getMousePosition().ToVector2() + new Vector2(125, 0);
            DrawOverlay(batch, position, transparency);
        }
        private void DrawOverlayAbovePlayer(SpriteBatch batch)
        {
            if (Game1.activeClickableMenu != null)
                return;

            Farmer player = Game1.player;
            float transparency = player.isMoving() && !player.UsingTool ? 0.1f : 1.0f;
            Vector2 position = new Vector2(player.Position.X - Game1.viewport.X, player.Position.Y - Game1.viewport.Y);

            string result = "";
            int text_offsetY = detector.Ores.Count - 1;

            int padding = 0;
            foreach (var item in detector.Ores)
            {
                //batch.Draw(SpringObjects, position + new Vector2(0, Game1.dialogueFont.LineSpacing * counter), new Rectangle(column * 16, row * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                string text = $"{item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}\n";
                if (text.Length > padding)
                    padding = text.Length;
                result += text;
            }
            padding = padding > 0 ? padding : 16;
            result += "Ladder: ";
            result += detector.LadderRevealed ? "Yes" : "No";
            Vector2 finalPosition = position + new Vector2(-4 * padding, -200 - Game1.dialogueFont.LineSpacing * text_offsetY);

            int counter = 0;
            foreach (var item in detector.Ores)
            {
                string itemId = detector.itemIds[item.Key];

                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
                string itemTypeId = data.GetItemTypeId();
                Texture2D texture = data.GetTexture();
                Rectangle sourceRect = data.GetSourceRect();
                int bigCraftableOffset = itemTypeId == "(BC)" ? 12 : 0;
                bool isBigCraftable = itemTypeId == "(BC)";
                batch.Draw(texture, finalPosition + new Vector2(-4 * padding + bigCraftableOffset, Game1.dialogueFont.LineSpacing * counter), sourceRect, Color.White * transparency, 0f, Vector2.Zero, isBigCraftable ? 1.5f : 3f, SpriteEffects.None, 0f);
                counter++;
            }
            batch.Draw(ladderTexture, finalPosition + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter + 10), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

            batch.DrawString(Game1.dialogueFont, result, finalPosition, Color.White * transparency);
        }
    }
}