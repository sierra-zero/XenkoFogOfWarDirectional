using System.Collections.Concurrent;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Graphics;
using Xenko.Physics;
using Xenko.Rendering.Sprites;

// ReSharper disable MemberCanBePrivate.Global
namespace FogOfWarDirectional
{
    public class FogTile : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public Vector2 Coord { get; }

        private SpriteFromSheet sprite;
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
            Entity.Get<RigidbodyComponent>().Enabled = false;
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
                    sprite.CurrentFrame = 1;
                }
                else {
                    if (northVisible && westVisible && eastVisible) {
                        sprite.CurrentFrame = 0;
                        return;
                    }

                    if (southVisible && westVisible && eastVisible) {
                        sprite.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && westVisible && southVisible) {
                        sprite.CurrentFrame = 0;
                        return;
                    }

                    if (northVisible && eastVisible && southVisible) {
                        sprite.CurrentFrame = 0;
                        return;
                    }

                    //if (northVisible &&  northWestVisible &&westVisible) {
                    //    sprite.CurrentFrame = 4;
                    //    return;
                    //}

                    //if (northVisible && northEastVisible && eastVisible) {
                    //    sprite.CurrentFrame = 5;
                    //    return;
                    //}

                    //if (southVisible && southEastVisible && eastVisible) {
                    //    sprite.CurrentFrame = 2;
                    //    return;
                    //}

                    //if (southVisible && southWestVisible && westVisible) {
                    //    sprite.CurrentFrame = 3;
                    //    return;
                    //}

                    if (northVisible && westVisible) {
                        sprite.CurrentFrame = 4;
                        return;
                    }

                    if (northVisible && eastVisible) {
                        sprite.CurrentFrame = 5;
                        return;
                    }

                    if (southVisible && eastVisible) {
                        sprite.CurrentFrame = 2;
                        return;
                    }

                    if (southVisible && westVisible) {
                        sprite.CurrentFrame = 3;
                        return;
                    }


                    sprite.CurrentFrame = 0;
                }
            }
        }

        private void InitializeFogTile()
        {
            sprite = Entity.FindChild("Sprite").Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
        }
    }
}
