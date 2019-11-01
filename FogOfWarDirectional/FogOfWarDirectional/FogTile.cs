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
            tileState = seen ? FogTileState.Visible : FogTileState.NotVisible;

            // Shortcut out 
            if (!seen && !modelComponent.Enabled) {
                return;
            }

            // Check the bump timer
            if (bumpTimer >= 0) {
                lerp = (1 - (float)bumpTimer * lerpRate);
                shaderParams?.Set(FogTileShaderKeys.Lerp, lerp);
                bumpTimer--;
                return;
            }

            // Shortcut out if no movement
            characterPos = fog.CharacterPos;
            if (prevCharacterPos == characterPos) {
                return;
            }
            prevCharacterPos = characterPos;

            // Reset the bump timer on state change
            if (prevTileState != tileState) {
                lerp = 0;
                bumpTimer = fadeTimer;
                shaderParams?.Set(FogTileShaderKeys.Lerp, lerp);
                shaderParams?.Set(FogTileShaderKeys.PrevTile, (int)prevTileState);
                shaderParams?.Set(FogTileShaderKeys.CurrentTile, (int)tileState);
                prevTileState = tileState;
            }

            if (!seen && bumpTimer < 0) {
                modelComponent.Enabled = false;
                return;
            }

            if (seen) {
                modelComponent.Enabled = true;
            }
        }
        
        private void InitializeFogTile()
        {
            modelComponent = Entity.FindChild("Model").Get<ModelComponent>();
            shaderParams = modelComponent?.GetMaterial(0)?.Passes[0]?.Parameters;
        }
    }
}
