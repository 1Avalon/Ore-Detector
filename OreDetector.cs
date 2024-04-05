using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OreDetector
{
    public class OreDetector
    {
        public Dictionary<string, List<StardewValley.Object>> Ores;

        public Dictionary<string, List<StardewValley.Object>> MinedOres;

        public OreDetector()
        {
        }
        public void GetOreInCurrentShaft()
        {
            Ores = new Dictionary<string, List<StardewValley.Object>>();
            MinedOres = new Dictionary<string, List<StardewValley.Object>>();

            MineShaft currentShaft = (MineShaft)Game1.player.currentLocation;
            OverlaidDictionary current_ores = currentShaft.Objects;
            foreach (var ore in current_ores.Values)
            {
                //Debug.WriteLine($"{ore.Name} {ore.Category} {ore.ParentSheetIndex}");
                if ((ore.Category == -999 && ore.Name != "Weeds") || ore.Category == -2 || (ore.Category == -9 && ore.Name == "Barrel"))
                {
                    if (!Ores.ContainsKey(ore.DisplayName))
                    {
                        Ores.Add(ore.DisplayName, new List<StardewValley.Object>());
                        MinedOres.Add(ore.DisplayName, new List<StardewValley.Object>());
                    }
                    Ores[ore.DisplayName].Add(ore);
                }
            }
        }
    }
}
