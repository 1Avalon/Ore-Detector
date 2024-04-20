using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using xTile.Layers;

namespace OreDetector
{
    public class OreDetector
    {
        public Dictionary<string, List<StardewValley.Object>> Ores;

        public Dictionary<string, List<StardewValley.Object>> MinedOres;

        public List<Vector2> ladderPositions = new List<Vector2>();

        public Dictionary<string, string> itemIds;

        public MineShaft currentShaft;

        private static OreDetector instance;

        public bool LadderRevealed { get => currentShaft.ladderHasSpawned || ladderPositions.Count > 0; }

        private OreDetector() { }

        public static OreDetector GetOreDetector()
        {
            return instance != null ? instance : new OreDetector();    
        }
        public void LookForSpawnedLadders()
        {
            if (currentShaft == null)
                return;

            Layer layer = currentShaft.Map.GetLayer("Buildings");
            for (int y = 0; y < layer.LayerHeight; y++)
            {
                for (int x = 0; x < layer.LayerWidth; x++)
                {
                    var tile = layer.Tiles[x, y];
                    if (tile?.TileIndex == 173)
                    {
                        Vector2 ladderPostion = new Vector2(x, y);
                        if (!ladderPositions.Contains(ladderPostion))
                        {
                            ladderPositions.Add(ladderPostion);
                        }
                    }
                }
            }
        }
        public void GetOreInCurrentShaft()
        {
            Ores = new Dictionary<string, List<StardewValley.Object>>();
            MinedOres = new Dictionary<string, List<StardewValley.Object>>();
            itemIds = new Dictionary<string, string>();
            ladderPositions = new List<Vector2>();

            currentShaft = (MineShaft)Game1.player.currentLocation;
            OverlaidDictionary current_ores = currentShaft.Objects;
            foreach (var ore in current_ores.Values)
            {
                //Debug.WriteLine($"{ore.Name} {ore.Category} {ore.ParentSheetIndex}");
                if ((ore.Category == -999 && ore.Name != "Weeds") || ore.Category == -2 || (ore.Category == -9 && ore.Name == "Barrel"))
                {
                    if (!Ores.ContainsKey(ore.DisplayName))
                    {
                        Ores.Add(ore.DisplayName, new List<StardewValley.Object>());
                        itemIds.Add(ore.DisplayName, ore.ItemId);
                        MinedOres.Add(ore.DisplayName, new List<StardewValley.Object>());
                    }
                    Ores[ore.DisplayName].Add(ore);
                }
            }
        }
    }
}
