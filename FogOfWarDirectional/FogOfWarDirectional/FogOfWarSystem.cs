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
        public ushort Rows;
        public ushort Columns;
        public float Elevation;
        public float Vision;
        public float Sweep;

        public CharacterComponent Character;
        public ModelComponent FogOfWar;
        public int FogFadeTimerMs;

        internal Vector2 CharacterPos { get; private set; }
        internal ConcurrentDictionary<Vector2, bool> State { get; private set; }

        private ParameterCollection shaderParams;
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

            // Vector out camera position
            characterPosRecycler = Character.Entity.Transform.Position;
            characterPosXRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.X / Scale));
            characterPosZRecycler = Convert.ToInt32(Math.Round(characterPosRecycler.Z / Scale));
            CharacterPos = new Vector2(characterPosXRecycler, characterPosZRecycler);

            // Shortcut out
            if (CharacterPos == prevCharacterPos) {
                return;
            }

            foreach (var fogTile in State) {
                State[fogTile.Key] = false;
            }

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

            foreach (var fogTile in fogMap)
            {
                fogTile.Value.UpdateSeen(State[fogTile.Key]);
            }

            // TODO update shader with state information

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

            shaderParams = FogOfWar.GetMaterial(0)?.Passes[0]?.Parameters;
            fogMap = new ConcurrentDictionary<Vector2, FogTile>();
            State = new ConcurrentDictionary<Vector2, bool>();
            fogVisibilityMap = new ConcurrentDictionary<Vector2, FastList<Vector2>>();
            simulation = this.GetSimulation();
            subscribers = new FastList<Entity>();

            // Generate master fog map
            var rowDistance = Rows / Scale / 2;
            var colDistance = Columns / Scale / 2;
            for (var x = -rowDistance; x < rowDistance; x++) {
                for (var z = -colDistance / 2; z < colDistance; z++) {
                    var coord = new Vector2(x, z);

                    var fogTileEntity = Tile.Instantiate().First();
                    fogTileEntity.Transform.Position = new Vector3(x * Scale, 0, z * Scale);

                    var fogTile = new FogTile(this, Game.UpdateTime, coord);
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

                for (float i = 0; i <= 360; i += Sweep) {
                    targetRecycler = new Vector3(positionRecycler.X + (Vision * 2) * (float)Math.Cos(i), Elevation,
                        positionRecycler.Z + (Vision * 2) * (float)Math.Sin(i));

                    nextTileRecycler = false;

                    foreach (var hitResult in simulation.RaycastPenetrating(positionRecycler, targetRecycler)
                        .OrderBy(result => Vector3.Distance(positionRecycler, result.Point))) {

                        if (nextTileRecycler && hitResult.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter10) {
                            coordRecycler = hitResult.Collider.Entity.Get<FogTile>().Coord;
                            if (!fogVisibilityMap[fogTile.Key].Contains(coordRecycler)) {
                                fogVisibilityMap[fogTile.Key].Add(coordRecycler);
                            }
                            break;
                        }

                        if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter) {
                            nextTileRecycler = true;
                            continue;
                        }

                        if (Vector3.Distance(positionRecycler, hitResult.Point) > Vision) {
                            //nextTileRecycler = true;
                            continue;
                            //break;
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
            foreach (var fogTile in fogMap.Values)
            {
                SceneSystem.SceneInstance.Remove(fogTile.Entity);
            }
        }
    }
}
