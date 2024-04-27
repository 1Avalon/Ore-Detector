using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace OreDetector
{
    public sealed class ModConfig
    {
        public string PositionOption { get; set; } = I18n.OreDetector_Config_AbovePlayer();

        public bool arrowPointingToLadder = false;

        public string arrowToLadderColor = I18n.OreDetector_Config_Color_Red();

        public bool arrowPointingToHole = false;

        public string arrowToHoleColor = I18n.OreDetector_Config_Color_Blue();

        public bool showOreName = true;

        public Vector2 customPosition = new Vector2(0, 0);

        public SButton customPositionKeybind = SButton.P;

        public SButton hideInformationKeybind = SButton.J;
    }
}
