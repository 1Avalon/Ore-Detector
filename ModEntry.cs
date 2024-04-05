using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
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

        private Texture2D springObjects;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            detector = new();
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            helper.Events.Display.Rendered += this.OnRendered;
            springObjects = Game1.content.Load<Texture2D>("maps\\springobjects");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        ///

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

            DrawOverlayBottomLeftCorner(Game1.spriteBatch);


        }

        private void DrawOverlayAbovePlayer(SpriteBatch batch)
        {
            if (Game1.activeClickableMenu != null)
                return;

            Farmer player = Game1.player;

            float transparency = player.isMoving() ? 0.1f : 1.0f;
            Vector2 position = new Vector2(player.Position.X - Game1.viewport.X, player.Position.Y - Game1.viewport.Y);

            string result = "";
            int text_offsetY = detector.Ores.Count - 1;

            int padding = 0;
            foreach (var item in detector.Ores)
            {
                string text = $"{item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}\n";
                if (text.Length > padding)
                    padding = text.Length;
                result += text;
            }
            Vector2 finalPosition = position + new Vector2(-4 * padding, -128 - Game1.dialogueFont.LineSpacing * text_offsetY);

            batch.DrawString(Game1.dialogueFont, result, finalPosition, Color.White * transparency);

        }
        private void DrawOverlayTopLeftCorner(SpriteBatch batch)
        {
            if (Game1.activeClickableMenu != null)
                return;

            Vector2 position = new Vector2(0, 80);

            string result = "";
            int text_offsetY = detector.Ores.Count - 1;

            int padding = 0;
            foreach (var item in detector.Ores)
            {
                string text = $"{item.Key}: {detector.MinedOres[item.Key].Count} / {item.Value.Count}\n";
                if (text.Length > padding)
                    padding = text.Length;
                result += text;
            }
            //Vector2 finalPosition = position + new Vector2(0, 128 + Game1.dialogueFont.LineSpacing * text_offsetY);

            batch.DrawString(Game1.dialogueFont, result, position, Color.White);

        }
    }
}