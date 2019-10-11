using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Xenko.Core.Collections;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Physics;

// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogTileSpace : StartupScript
    {
        // Declared public member fields and properties will show in the game studio
        public bool Enable;
        public Prefab FogTile;
        public float FogTileSpacing;
        public float FogTileStartX;
        public float FogTileStartZ;
        public ushort FogRows;
        public ushort FogColumns;
        public float VisionRadius;
        public float ElevationY;

        private ConcurrentDictionary<Point, FogTile> fogMap;
        private ConcurrentDictionary<Point, bool> fogState;
        private ConcurrentDictionary<Point, FastList<Point>> fogVisibilityMap;

        private Vector3 positionRecycler;
        private Vector3 targetRecycler;
        private Point coordRecycler;
        private Simulation simulation;

        public override void Start()
        {
            InitializeFogTileSpace();
        }

        private void InitializeFogTileSpace()
        {
            if (!Enable) {
                return;
            }

            fogMap = new ConcurrentDictionary<Point, FogTile>();
            fogState = new ConcurrentDictionary<Point, bool>();
            fogVisibilityMap = new ConcurrentDictionary<Point, FastList<Point>>();
            simulation = this.GetSimulation();

            // Generate master fog map
            for (var x = 0; x < FogRows; x++) {
                for (var z = 0; z < FogColumns; z++) {
                    var coord = new Point(x, z);

                    var fogTileEntity = FogTile.Instantiate().First();
                    fogTileEntity.Transform.Position = new Vector3(x * FogTileSpacing + FogTileStartX,
                        0, z * FogTileSpacing + FogTileStartZ);

                    var fogTile = new FogTile(fogState, coord);
                    fogTileEntity.Add(fogTile);

                    fogMap.TryAdd(coord, fogTile);
                    fogState.TryAdd(coord, false);
                    fogVisibilityMap.TryAdd(coord, new FastList<Point>());
                    Entity.AddChild(fogTileEntity);
                }
            }

            // Update visibility map for every point on the grid
            foreach (var fogTile in fogMap) {
                fogTile.Value.Entity.Transform.GetWorldTransformation(out positionRecycler, out _, out _);

                for (var i = 0; i <= 360; i++) {
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
