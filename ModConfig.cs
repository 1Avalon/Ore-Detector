using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;

namespace OreDetector
{
    public sealed class ModConfig
    {
        public string PositionOption { get; set; } = "Above player";

        public bool arrowPointingToLadder = false;

        public string arrowToLadderColor = "Red";

        public bool arrowPointingToHole = false;

        public string arrowToHoleColor = "Blue";

        public bool showOreName = true;

        public Vector2 customPosition = new Vector2(0, 0);
    }
}
