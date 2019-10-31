using System.Collections.Concurrent;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;
using Xenko.Rendering;
using Xenko.Rendering.Materials;
using Xenko.Rendering.Materials.ComputeColors;

// ReSharper disable PossibleLossOfFraction

// ReSharper disable MemberCanBePrivate.Global
namespace FogOfWarDirectional
{
    public class FogTile : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public Vector2 Coord { get; }

        private ModelComponent modelComponent;
        private ParameterCollection shaderParams;

        private bool seen;
        private int fadeTimer;
        private float lerpRate;
        private float lerp = 1;
        private int bumpTimer;
        private Vector2 characterPos;
        private Vector2 prevCharacterPos;

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

        private static readonly Vector2 NorthCoord = new Vector2(0, -1);
        private static readonly Vector2 NorthEastCoord = new Vector2(1, -1);
        private static readonly Vector2 EastCoord  = new Vector2(1, 0);
        private static readonly Vector2 SouthEastCoord = new Vector2(1, 1);
        private static readonly Vector2 SouthCoord  = new Vector2(0, 1);
        private static readonly Vector2 SouthWestCoord = new Vector2(-1, 1);
        private static readonly Vector2 WestCoord =  new Vector2(-1, 0);
        private static readonly Vector2 NorthWestCoord = new Vector2(-1, -1);

        private readonly FogOfWarSystem fog;
        private readonly ConcurrentDictionary<Vector2, bool> fogState;
        private readonly float renderDistance;
        private FogTileState tileState = FogTileState.NotVisible;
        private FogTileState prevTileState = FogTileState.NotVisible;

        private enum FogTileState
        {
            NotVisible,
            Visible,
            NorthWestVisible,
            NorthEastVisible,
            SouthEastVisible,
            SouthWestVisible
        }
        
        public FogTile(FogOfWarSystem fog, Vector2 fogTileCoord)
        {
            this.fog = fog;
            fadeTimer = fog.FogFadeTimer;
            lerpRate = 1 / (float)fadeTimer;
            characterPos = fog.CharacterPos;
            fogState = fog.State;
            renderDistance = fog.FogRenderDistance;
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
            // Identify tile state
            seen = fogState.ContainsKey(Coord) && fogState[Coord];
            UpdateFogTileState();

            // Shortcut out if outside of camera view
            characterPos = fog.CharacterPos;
            if (Vector2.Distance(Coord, characterPos) > renderDistance) {
                modelComponent.Enabled = false;
                return;
            }

            // Enable any disabled model components
            if (!modelComponent.Enabled) {
                modelComponent.Enabled = true;
            }

            // Check the bump timer - TODO this is where you are
            if (bumpTimer >= 0) {
                lerp = (1 - (float)bumpTimer * lerpRate);
                shaderParams?.Set(FogTileShaderKeys.Lerp, lerp);
                bumpTimer--;
                return;
            }

            // Shortcut out if no movement
            if (prevCharacterPos == characterPos) {
                return;
            }

            // Reset the bump timer on state change
            if (prevTileState != tileState && bumpTimer < 0) {
                lerp = 0;
                bumpTimer = fadeTimer;
                shaderParams?.Set(FogTileShaderKeys.Lerp, lerp);
                shaderParams?.Set(FogTileShaderKeys.PrevTile, (int)prevTileState);
                shaderParams?.Set(FogTileShaderKeys.CurrentTile, (int)tileState);
                prevTileState = tileState;
            }

            prevCharacterPos = characterPos;
        }

        private void UpdateFogTileState()
        {
            tileState = seen ? FogTileState.Visible : FogTileState.NotVisible;

            if (tileState == FogTileState.NotVisible) {
                return;
            }

            // Update recyclers
            northRecycler = Coord + NorthCoord;
            eastRecycler = Coord + EastCoord;
            southRecycler = Coord + SouthCoord;
            westRecycler = Coord + WestCoord;

            northEastRecycler = Coord + NorthEastCoord;
            southEastRecycler = Coord + SouthEastCoord;
            southWestRecycler = Coord + SouthWestCoord;
            northWestRecycler = Coord + NorthWestCoord;

            northVisible = fogState.ContainsKey(northRecycler) && !fogState[northRecycler];
            eastVisible = fogState.ContainsKey(eastRecycler) && !fogState[eastRecycler];
            southVisible = fogState.ContainsKey(southRecycler) && !fogState[southRecycler];
            westVisible = fogState.ContainsKey(westRecycler) && !fogState[westRecycler];

            northEastVisible = fogState.ContainsKey(northEastRecycler) && fogState[northEastRecycler];
            southEastVisible = fogState.ContainsKey(southEastRecycler) && fogState[southEastRecycler];
            southWestVisible = fogState.ContainsKey(southWestRecycler) && fogState[southWestRecycler];
            northWestVisible = fogState.ContainsKey(northWestRecycler) && fogState[northWestRecycler];

            if (northVisible && !eastVisible && westVisible && !southVisible) {
                tileState = FogTileState.NorthWestVisible;
                return;
            }

            if (northVisible && eastVisible && !westVisible && !southVisible) {
                tileState = FogTileState.NorthEastVisible;
                return;
            }

            if (!northVisible && eastVisible && !westVisible && southVisible) {
                tileState = FogTileState.SouthEastVisible;
                return;
            }

            if (!northVisible && !eastVisible && westVisible && southVisible) {
                tileState = FogTileState.SouthWestVisible;
                return;
            }
        }

        private void InitializeFogTile()
        {
            modelComponent = Entity.Get<ModelComponent>();
            shaderParams = modelComponent?.GetMaterial(0)?.Passes[0]?.Parameters;
        }
    }
}
