using System.Collections.Concurrent;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;
using Xenko.Rendering;
using Xenko.Rendering.Sprites;

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
        private int lerpTimer;
        private float lerpRate = .05f;
        private float lerpValue = 1;
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
        }
        
        public FogTile(FogOfWarSystem fog, Vector2 fogTileCoord)
        {
            this.fog = fog;
            lerpTimer = fog.FogFadeTimer;
            lerpRate = 1 / (float)fog.FogFadeTimer;
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
            tileState = seen ? FogTileState.Visible : FogTileState.NotVisible;

            // Check the bump timer - TODO this is where you are
            if (bumpTimer > 0) {
                lerpValue = 1 - bumpTimer * lerpRate;
                shaderParams?.Set(FogTileShaderKeys.Lerp, 0);
                bumpTimer--;
                return;
            }

            // Reset the bump timer on state change
            if (prevTileState != tileState) {
                bumpTimer = lerpTimer;
                lerpValue = 1 - bumpTimer * lerpRate;
                shaderParams?.Set(FogTileShaderKeys.Lerp, lerpValue);
                shaderParams?.Set(FogTileShaderKeys.PrevTile, (int)prevTileState);
                shaderParams?.Set(FogTileShaderKeys.CurrentTile, (int)tileState);
                prevTileState = tileState;
                return;
            }

            // Shortcut out if no movement
            characterPos = fog.CharacterPos;
            if (prevCharacterPos == characterPos) {
                return;
            }

            // Shortcut out if outside of camera view
            if (Vector2.Distance(Coord, characterPos) > renderDistance) {
                modelComponent.Enabled = false;
                return;
            }

            // Enable any disabled model components
            if (!modelComponent.Enabled) {
                modelComponent.Enabled = true;
            }

            // Update state and set shader params
            prevTileState = tileState;
            shaderParams?.Set(FogTileShaderKeys.Lerp, lerpValue);
            shaderParams?.Set(FogTileShaderKeys.PrevTile, (int)prevTileState);
            shaderParams?.Set(FogTileShaderKeys.CurrentTile, (int)tileState);
        }

        private void InitializeFogTile()
        {
            modelComponent = Entity.Get<ModelComponent>();
            shaderParams = modelComponent?.GetMaterial(0)?.Passes[0]?.Parameters;
        }
    }
}
