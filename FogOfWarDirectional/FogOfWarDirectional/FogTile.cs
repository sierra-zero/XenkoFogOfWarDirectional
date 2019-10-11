using System.Collections.Concurrent;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;

// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogTile : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public Point Coord { get; }

        private readonly ConcurrentDictionary<Point, bool> fogState;

        public FogTile(ConcurrentDictionary<Point, bool> fogState, Point fogTileCoord)
        {
            this.fogState = fogState;
            Coord = fogTileCoord;
        }

        public override void Start()
        {
        }

        public override void Update()
        {
            UpdateVisibility();
        }

        public void DisableRigidBody()
        {
            Entity.Get<RigidbodyComponent>().Enabled = false;
            //Entity.Get<SpriteComponent>().Enabled = false;
        }

        private void UpdateVisibility()
        {
        }
    }
}
