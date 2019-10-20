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
        private float fadeRate = .05f;
        private float alpha = 1;
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

            // TODO write a method for identifying complex states.
            tileState = seen ? FogTileState.Visible : FogTileState.NotVisible;

            // TODO need a lerp bump timer of sorts if the state is different

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

            if (!modelComponent.Enabled) {
                modelComponent.Enabled = true;
            }

            // Update shader
            shaderParams?.Set(FogTileShaderKeys.Tile, (int)tileState);
        }

        private void InitializeFogTile()
        {
            modelComponent = Entity.Get<ModelComponent>();
            shaderParams = modelComponent?.GetMaterial(0)?.Passes[0]?.Parameters;
        }
    }
}
