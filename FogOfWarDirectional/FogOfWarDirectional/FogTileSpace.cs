using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xenko.Core.Mathematics;
using Xenko.Engine;
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogTileSpace : StartupScript
    {
        // Declared public member fields and properties will show in the game studio
        public Prefab FogTile;
        public float FogTileSpacing;
        public ushort FogRows;
        public ushort FogColumns;

        private ConcurrentDictionary<Point, bool> fogVisibilityMap;

        public override void Start()
        {
            InitializeFogTileSpace();
        }

        private void InitializeFogTileSpace()
        {
            fogVisibilityMap = new ConcurrentDictionary<Point, bool>();

            for (var i = 0; i < FogRows; i++) {
                for (var j = 0; j < FogColumns; j++) {
                    var coord = new Point(j, i);
                    fogVisibilityMap.TryAdd(coord, false);

                    var fogTile = FogTile.Instantiate().First();
                    fogTile.Transform.Position = new Vector3(j * FogTileSpacing, 0, i * FogTileSpacing);
                    fogTile.Add(new FogTile(fogVisibilityMap, coord));
                    Entity.AddChild(fogTile);
                }
            }
        }
    }
}
