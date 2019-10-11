using System;
using System.Collections.Concurrent;
using System.Linq;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogOfWarSystem : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public bool Enable;
        public Prefab FogTile;
        public float FogTileScaling;
        public float FogTileStartX;
        public float FogTileStartZ;
        public ushort FogRows;
        public ushort FogColumns;
        public float VisionRadius;
        public int VisionStep;
        public float ElevationY;
        public CharacterComponent Character;
        public float FogRenderDistance;

        internal Vector2 CharacterPos { get; private set; }
        internal ConcurrentDictionary<Vector2, bool> State { get; private set; }

        private ConcurrentDictionary<Vector2, FogTile> fogMap;
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
        private int characterPosXRecycler;
        private int characterPosZRecycler;
        private Simulation simulation;

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
            if (!subscribers.Contains(entity)) {
                subscribers.Add(entity);
            }
        }

        public void Unsubscribe(Entity entity)
        {
            if (subscribers.Contains(entity)) {
                subscribers.Remove(entity);
            }
        }

        private void UpdateFogState()
        {
            // Vector out camera position
            characterPosRecycler = Character.Entity.Transform.Position;
            characterPosXRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.X * 1/FogTileScaling));
            characterPosZRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.Z * 1/FogTileScaling));
            CharacterPos = new Vector2(characterPosXRecycler, characterPosZRecycler);

            foreach (var fogTile in State) {
                State[fogTile.Key] = false;
            }

            foreach (var subscriber in subscribers) {
                subscriberPosRecycler = subscriber.Transform.Position;
                subscriberPosXRecycler = Convert.ToInt32(Math.Round(subscriberPosRecycler.X * 1/FogTileScaling));
                subscriberPosZRecycler = Convert.ToInt32(Math.Round(subscriberPosRecycler.Z * 1/FogTileScaling));
                subscriberPos = new Vector2(subscriberPosXRecycler, subscriberPosZRecycler);

                if (fogVisibilityMap.ContainsKey(subscriberPos)) {
                    foreach (var point in fogVisibilityMap[subscriberPos]) {
                        State[point] = true;
                    }
                }
            }
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

            fogMap = new ConcurrentDictionary<Vector2, FogTile>();
            State = new ConcurrentDictionary<Vector2, bool>();
            fogVisibilityMap = new ConcurrentDictionary<Vector2, FastList<Vector2>>();
            simulation = this.GetSimulation();
            subscribers = new FastList<Entity>();

            // Generate master fog map
            for (var x = 0; x < FogRows; x++) {
                for (var z = 0; z < FogColumns; z++) {
                    var coord = new Vector2(x, z);

                    var fogTileEntity = FogTile.Instantiate().First();
                    fogTileEntity.Transform.Position = new Vector3(x * FogTileScaling + FogTileStartX,
                        0, z * FogTileScaling + FogTileStartZ);

                    var fogTile = new FogTile(this, coord);
                    fogTileEntity.Add(fogTile);

                    fogMap.TryAdd(coord, fogTile);
                    State.TryAdd(coord, false);
                    fogVisibilityMap.TryAdd(coord, new FastList<Vector2>());
                    Entity.AddChild(fogTileEntity);
                }
            }

            // Update visibility map for every point on the grid
            foreach (var fogTile in fogMap) {
                fogTile.Value.Entity.Transform.GetWorldTransformation(out positionRecycler, out _, out _);
                fogVisibilityMap[fogTile.Key].Add(fogTile.Key);

                for (var i = 0; i <= 360; i += VisionStep) {
                    targetRecycler = new Vector3(positionRecycler.X + VisionRadius * (float)Math.Cos(i), ElevationY,
                        positionRecycler.Z + VisionRadius * (float)Math.Sin(i));

                    foreach (var hitResult in simulation.RaycastPenetrating(positionRecycler, targetRecycler)
                        .OrderBy(result => Vector3.Distance(positionRecycler, result.Point))) {

                        if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter) {
                            break;
                        }

                        if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter10) {
                            coordRecycler = hitResult.Collider.Entity.Get<FogTile>().Coord;
                            if (!fogVisibilityMap[fogTile.Key].Contains(coordRecycler)) {
                                fogVisibilityMap[fogTile.Key].Add(coordRecycler);
                            }
                        }
                    }
                }
            }

            // Disable all fog tile colliders
            foreach (var fogTile in fogMap.Values) {
                fogTile.DisableRigidBody();
            }
        }
    }
}
