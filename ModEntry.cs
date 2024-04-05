﻿using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using System.Security.Cryptography;

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

        public static ModEntry instance;

        private MineShaft currentShaft;

        private OreDetector detector;

        private Texture2D ladderTexture;

        private static ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            detector = new();
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.Display.Rendered += this.OnRendered;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
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
                allowedValues: new string[] { "Above player", "Top left", "Next to cursor" }
            );
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation is MineShaft shaft)
            {
                currentShaft = shaft;
                detector.GetOreInCurrentShaft();
                foreach(var item in detector.Ores)
                {
                    Monitor.Log($"Max {item.Key}: {item.Value.Count}");
                }
            }
        }
        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e) 
        {
            if (e.Location != currentShaft)
                return;

            foreach (var item in e.Removed)
            {
                if (!detector.Ores.Keys.Contains(item.Value.DisplayName))
                    continue;

                detector.MinedOres[item.Value.DisplayName].Add(item.Value);
                //break; ??
            }
            foreach (var item in detector.Ores)
            {
                Monitor.Log($"Mined {item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}");
            }
        }
        private void OnRendered(object? sender, RenderedEventArgs e)
        {
            if(!Context.IsWorldReady) 
                return;

            if (Game1.player.currentLocation != currentShaft)
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
            }
        }
        private void DrawOverlay(SpriteBatch batch, Vector2 position, string result, float transparency)
        {
            if (Game1.activeClickableMenu != null)
                return;

            int padding = 0;
            foreach (var item in detector.Ores)
            {
                string text = $"{item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}\n";
                if (text.Length > padding)
                    padding = text.Length;
                result += text;
            }
            result += "Ladder: ";
            result += detector.currentShaft.ladderHasSpawned ? "Yes" : "No";

            int counter = 0;
            foreach (var item in detector.Ores)
            {
                string itemId = detector.itemIds[item.Key];
                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
                Texture2D texture = data.GetTexture();
                Rectangle sourceRect = data.GetSourceRect();
                batch.Draw(texture, position + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter), sourceRect, Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                counter++;
            }
            batch.Draw(ladderTexture, position + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

            batch.DrawString(Game1.dialogueFont, result, position, Color.White * transparency);
        }
        private void DrawOverlayTopLeftCorner(SpriteBatch batch)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;

            Vector2 position = new Vector2(64, 80);
            string result = "";
            DrawOverlay(batch, position, result, 1f);
        }

        private void DrawOverlayCursor(SpriteBatch batch)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null)
                return;
            Farmer player = Game1.player;
            float transparency = player.isMoving() && !player.UsingTool ? 0.1f : 1f;
            Vector2 position = Game1.getMousePosition().ToVector2() + new Vector2(125, 0);
            string result = "";
            DrawOverlay(batch, position, result, transparency);
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
            result += "Ladder: ";
            result += detector.currentShaft.ladderHasSpawned ? "Yes" : "No";
            Vector2 finalPosition = position + new Vector2(-4 * padding, -200 - Game1.dialogueFont.LineSpacing * text_offsetY);

            int counter = 0;
            foreach (var item in detector.Ores)
            {
                string itemId = detector.itemIds[item.Key];
                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
                Texture2D texture = data.GetTexture();
                Rectangle sourceRect = data.GetSourceRect();
                batch.Draw(texture, finalPosition + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter), sourceRect, Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
                counter++;
            }
            batch.Draw(ladderTexture, finalPosition + new Vector2(-4 * padding, Game1.dialogueFont.LineSpacing * counter), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);

            batch.DrawString(Game1.dialogueFont, result, finalPosition, Color.White * transparency);
        }
    }
}