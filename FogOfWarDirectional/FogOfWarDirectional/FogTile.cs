using System.Collections.Concurrent;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;
using Xenko.Rendering.Sprites;

// ReSharper disable MemberCanBePrivate.Global
namespace FogOfWarDirectional
{
    public class FogTile : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public Vector2 Coord { get; }

        private SpriteComponent spriteComponent;
        private SpriteFromSheet spriteProvider;
        private bool northVisible;
        private bool northEastVisible;
        private bool eastVisible;
        private bool southEastVisible;
        private bool southVisible;
        private bool southWestVisible;
        private bool westVisible;
        private bool northWestVisible;

        private Vector2 northRecycler;
        private Vector2 northEastRecycler;
        private Vector2 eastRecycler;
        private Vector2 southEastRecycler;
        private Vector2 southRecycler;
        private Vector2 southWestRecycler;
        private Vector2 westRecycler;
        private Vector2 northWestRecycler;

        private readonly ConcurrentDictionary<Vector2, bool> fogVisibilityState;

        private static readonly Vector2 NorthCoord = new Vector2(0, -1);
        private static readonly Vector2 NorthEastCoord = new Vector2(1, -1);
        private static readonly Vector2 EastCoord  = new Vector2(1, 0);
        private static readonly Vector2 SouthEastCoord = new Vector2(1, 1);
        private static readonly Vector2 SouthCoord  = new Vector2(0, 1);
        private static readonly Vector2 SouthWestCoord = new Vector2(-1, 1);
        private static readonly Vector2 WestCoord =  new Vector2(-1, 0);
        private static readonly Vector2 NorthWestCoord = new Vector2(-1, -1);

        public FogTile(ConcurrentDictionary<Vector2, bool> fogVisibilityState, Vector2 fogTileCoord)
        {
            this.fogVisibilityState = fogVisibilityState;
            Coord = fogTileCoord;
        }

        public override void Start()
        {
            InitializeFogTile();
        }

        public override void Update()
        {
            UpdateVisibility();
        }

        public void DisableRigidBody()
        {
            Entity.Remove(Entity.Get<RigidbodyComponent>());
        }

        private void UpdateVisibility()
        {
            northRecycler = Coord + NorthCoord;
            northEastRecycler = Coord + NorthEastCoord;
            eastRecycler = Coord + EastCoord;
            southEastRecycler = Coord + SouthEastCoord;
            southRecycler = Coord + SouthCoord;
            southWestRecycler = Coord + SouthWestCoord;
            westRecycler = Coord + WestCoord;
            northWestRecycler = Coord + NorthWestCoord;

            northVisible = fogVisibilityState.ContainsKey(northRecycler) && fogVisibilityState[northRecycler];
            northEastVisible = fogVisibilityState.ContainsKey(northEastRecycler) && fogVisibilityState[northEastRecycler];
            eastVisible = fogVisibilityState.ContainsKey(eastRecycler) && fogVisibilityState[eastRecycler];
            southEastVisible = fogVisibilityState.ContainsKey(southEastRecycler) && fogVisibilityState[southEastRecycler];
            southVisible = fogVisibilityState.ContainsKey(southRecycler) && fogVisibilityState[southRecycler];
            southWestVisible = fogVisibilityState.ContainsKey(southWestRecycler) && fogVisibilityState[southWestRecycler];
            westVisible = fogVisibilityState.ContainsKey(westRecycler) && fogVisibilityState[westRecycler];
            northWestVisible = fogVisibilityState.ContainsKey(northWestRecycler) && fogVisibilityState[northWestRecycler];

            if (fogVisibilityState.ContainsKey(Coord)) {
                if (fogVisibilityState[Coord]) {
                    spriteProvider.CurrentFrame = 1;
                }
                else {
                    if (northVisible && eastVisible && southVisible && westVisible) {
                        spriteProvider.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && westVisible && eastVisible) {
                        spriteProvider.CurrentFrame = 0;
                        return;
                    }

                    if (southVisible && westVisible && eastVisible) {
                        spriteProvider.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && westVisible && southVisible) {
                        spriteProvider.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && eastVisible && southVisible) {
                        spriteProvider.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && westVisible) {
                        spriteProvider.CurrentFrame = 4;
                        return;
                    }

                    if (northVisible && eastVisible) {
                        spriteProvider.CurrentFrame = 5;
                        return;
                    }

                    if (southVisible && eastVisible) {
                        spriteProvider.CurrentFrame = 2;
                        return;
                    }

                    if (southVisible && westVisible) {
                        spriteProvider.CurrentFrame = 3;
                        return;
                    }

                    spriteProvider.CurrentFrame = 0;
                }
            }
        }

        private void InitializeFogTile()
        {
            spriteComponent = Entity.FindChild("Sprite").Get<SpriteComponent>();
            spriteProvider = spriteComponent.SpriteProvider as SpriteFromSheet;
        }
    }
}
