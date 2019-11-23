using System;
using System.Collections.Concurrent;
using System.Linq;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;
using Xenko.Rendering;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace FogOfWarDirectional
{
    public class FogOfWarSystem : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public bool Enable;
        public Prefab Tile;
        public float Scale;
        public int Rows;
        public int Columns;
        public float Vision;
        public float Sweep;
        public float Fade;
        public float Opacity;
        public CharacterComponent Character;
        public ModelComponent FogOfWarPre;
        public ModelComponent FogOfWarPost;

        internal Vector2 CharacterPos { get; private set; }
        internal ConcurrentDictionary<Vector2, bool> State { get; private set; }

        private float StartOffset = .5f;
        private FogTile[] fogMap;
        private ParameterCollection shaderParamsPre;
        private ParameterCollection shaderParamsPost;
        private ConcurrentDictionary<Vector2, FastList<Vector2>> fogVisibilityMap;
        private FastList<Entity> subscribers;
        private Vector3 subscriberPosRecycler;
        private int subscriberPosXRecycler;
        private int subscriberPosZRecycler;
        private Vector2 subscriberPos;
        private Vector3 positionRecycler;
        private Vector3 targetRecycler;
        private Vector2 coordRecycler;
        private Vector3 characterPosRecycler;
        private bool nextTileRecycler;
        private int characterPosXRecycler;
        private int characterPosZRecycler;
        private Simulation simulation;
        private Vector2 prevCharacterPos;

        public override void Start()
        {
            InitializeFogTileSpace();
            RegisterFogOfWarSystem();
        }

        public override void Update()
        {
            UpdateFogState();
        }

        public void Subscribe(Entity entity)
        {
            if (!Enable) {
                return;
            }

            if (!subscribers.Contains(entity)) {
                subscribers.Add(entity);
            }
        }

        public void Unsubscribe(Entity entity)
        {
            if (!Enable) {
                return;
            }

            if (subscribers.Contains(entity)) {
                subscribers.Remove(entity);
            }
        }

        private void UpdateFogState()
        {
            if (!Enable) {
                return;
            }

            // Identify player position
            characterPosRecycler = Character.Entity.Transform.Position;
            characterPosXRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.X / Scale));
            characterPosZRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.Z / Scale));
            CharacterPos = new Vector2(characterPosXRecycler, characterPosZRecycler);

            // Update fog visibility
            foreach (var fogTile in fogMap) {
                fogTile.UpdateVisibility();
            }

            // Shortcut out if the player has not moved
            if (CharacterPos == prevCharacterPos) {
                shaderParamsPre.Set(FogOfWarTileShaderKeys.FogMap, fogMap.Select(a => a.Visibility).ToArray());
                return;
            }

            // Reset State to not visible
            foreach (var fogTile in State) {
                State[fogTile.Key] = false;
            }

            // Set Visibility for this tick per tile in State
            foreach (var subscriber in subscribers) {
                subscriberPosRecycler = subscriber.Transform.Position;
                subscriberPosXRecycler = Convert.ToInt32(Math.Round(subscriberPosRecycler.X / Scale));
                subscriberPosZRecycler = Convert.ToInt32(Math.Round(subscriberPosRecycler.Z / Scale));
                subscriberPos = new Vector2(subscriberPosXRecycler, subscriberPosZRecycler);

                if (fogVisibilityMap.ContainsKey(subscriberPos)) {
                    foreach (var point in fogVisibilityMap[subscriberPos]) {
                        State[point] = true;
                    }
                }
            }

            // Update the ordered array for shader
            foreach (var fogTile in fogMap) {
                fogTile.UpdateSeen(State[fogTile.Coord]);
            }

            // Update shader
            shaderParamsPre.Set(FogOfWarTileShaderKeys.FogMap, fogMap.Select(a => a.Visibility).ToArray());
            prevCharacterPos = CharacterPos;
        }

        private void RegisterFogOfWarSystem()
        {
            Game.Services.AddService(this);
        }

        private void InitializeFogTileSpace()
        {
            if (!Enable) {
                return;
            }

            fogMap = new FogTile[Convert.ToInt32((Rows/Scale) * (Columns/Scale))];
            State = new ConcurrentDictionary<Vector2, bool>();
            fogVisibilityMap = new ConcurrentDictionary<Vector2, FastList<Vector2>>();
            simulation = this.GetSimulation();
            subscribers = new FastList<Entity>();

            StartOffset *= Scale;

            // Generate master fog map
            var fogMapIndex = 0;
            for (var x = 0; x < Columns / Scale; x++) {
                for (var z = 0; z < Rows / Scale; z++) {
                    var coord = new Vector2(x, z);

                    var fogTileEntity = Tile.Instantiate().First();
                    fogTileEntity.Transform.Position = new Vector3(x * Scale + StartOffset,
                        Entity.Transform.Position.Y, z * Scale + StartOffset);
                    fogTileEntity.Transform.Scale = Vector3.One * Scale;

                    var fogTile = new FogTile(this, Game.UpdateTime, coord);
                    fogTileEntity.Add(fogTile);

                    fogMap[fogMapIndex] = fogTile;
                    fogMapIndex++;
                    State.TryAdd(coord, false);
                    fogVisibilityMap.TryAdd(coord, new FastList<Vector2>());
                    Entity.AddChild(fogTileEntity);
                }
            }

            // Update visibility map for every point on the grid
            foreach (var fogTile in fogMap) {
                fogTile.Entity.Transform.GetWorldTransformation(out positionRecycler, out _, out _);
                fogVisibilityMap[fogTile.Coord].Add(fogTile.Coord);

                for (float i = 0; i <= 360; i += Sweep) {
                    targetRecycler = new Vector3(positionRecycler.X + (Vision * 2) * (float)Math.Cos(i), positionRecycler.Y,
                        positionRecycler.Z + (Vision * 2) * (float)Math.Sin(i));

                    nextTileRecycler = false;

                    foreach (var hitResult in simulation.RaycastPenetrating(positionRecycler, targetRecycler)
                        .OrderBy(result => Vector3.Distance(positionRecycler, result.Point))) {

                        if (nextTileRecycler && hitResult.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter10) {
                            coordRecycler = hitResult.Collider.Entity.Get<FogTile>().Coord;
                            if (!fogVisibilityMap[fogTile.Coord].Contains(coordRecycler)) {
                                fogVisibilityMap[fogTile.Coord].Add(coordRecycler);
                            }
                            break;
                        }

                        if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter) {
                            nextTileRecycler = true;
                            continue;
                        }

                        if (Vector3.Distance(positionRecycler, hitResult.Point) > Vision) {
                            continue;
                        }

                        if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter10) {
                            coordRecycler = hitResult.Collider.Entity.Get<FogTile>().Coord;
                            if (!fogVisibilityMap[fogTile.Coord].Contains(coordRecycler)) {
                                fogVisibilityMap[fogTile.Coord].Add(coordRecycler);
                            }
                        }
                    }
                }
            }

            shaderParamsPre = FogOfWarPre.GetMaterial(0)?.Passes[0]?.Parameters;
            shaderParamsPre?.Set(FogOfWarTileShaderKeys.Rows, Rows / Scale);
            shaderParamsPre?.Set(FogOfWarTileShaderKeys.Scale, Scale);
            shaderParamsPre?.Set(FogOfWarTileShaderKeys.FogMap, fogMap.Select(a => a.Visibility).ToArray());

            shaderParamsPost = FogOfWarPost.GetMaterial(0)?.Passes[0]?.Parameters;
            shaderParamsPost?.Set(FogOfWarDirectionalShaderKeys.FogOpacity, Opacity);

            // Disable all fog tile colliders
            foreach (var fogTile in fogMap)
            {
                SceneSystem.SceneInstance.Remove(fogTile.Entity);
            }
        }
    }
}
