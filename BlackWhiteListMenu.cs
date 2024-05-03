using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace OreDetector
{
    public class BlackWhiteListMenu : IClickableMenu
    {
        private ClickableTextureComponent buttonBlacklist;

        private ClickableTextureComponent buttonWhitelist;
        public BlackWhiteListMenu() 
        {
            int width = Game1.viewport.Width / 4;
            int height = Game1.viewport.Height / 4;
            base.initialize(Game1.viewport.Width / 2 - width / 2, Game1.viewport.Height / 2 - height / 2, width, height);
            buttonBlacklist = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + this.width / 8, yPositionOnScreen + this.height / 2, 35 * 5, 13 * 5), ModEntry.blackListButtonTexture, new Rectangle(0,0,0,0), 5f);
            buttonWhitelist = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + this.width - this.width / 2, yPositionOnScreen + this.height / 2, 35 * 5, 13 * 5), ModEntry.whiteListButtonTexture, new Rectangle(0, 0, 0, 0), 5f);
        }
        public override void update(GameTime time)
        {
            base.update(time);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
        }
        public override void performHoverAction(int x, int y)
        {
            if (buttonBlacklist.containsPoint(x, y))
            {
                buttonBlacklist.scale = 6f;
            }
            else
            {
                buttonBlacklist.scale = 5f;
            }
            if (buttonWhitelist.containsPoint(x, y))
            {
                buttonWhitelist.scale = 6f;
            }
            else
            {
                buttonWhitelist.scale = 5f;
            }
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            buttonBlacklist.draw(b);

            buttonWhitelist.draw(b);

            drawMouse(b);
        }
    }
}
