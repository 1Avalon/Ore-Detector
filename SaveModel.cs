using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OreDetector
{
    public class SaveModel
    {
        public List<string> blacklistedNames = new List<string>();

        public List<string> discoveredMaterials = new List<string>();

        public List<string> discoveredmaterialsQualifiedIds = new List<string>();
    }
}
