using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xenko.Core.Mathematics;
using Xenko.Input;
using Xenko.Engine;

namespace FogOfWarDirectional
{
    public class FogTile : SyncScript
    {
        // Declared public member fields and properties will show in the game studio

        private Point fogTileCoord;
        private ConcurrentDictionary<Point, bool> fogVisibilityMap;

        public FogTile(ConcurrentDictionary<Point, bool> fogVisibilityMap, Point fogTileCoord)
        {
            this.fogVisibilityMap = fogVisibilityMap;
            this.fogTileCoord = fogTileCoord;
        }

        public override void Start()
        {
            // Initialization of the script.
        }

        public override void Update()
        {
            UpdateVisibility();
        }

        internal void Seen()
        {
            if (fogVisibilityMap.ContainsKey(fogTileCoord)) {
                fogVisibilityMap[fogTileCoord] = true;
            }
        }

        private void UpdateVisibility()
        {
            throw new NotImplementedException();
        }
    }
}
