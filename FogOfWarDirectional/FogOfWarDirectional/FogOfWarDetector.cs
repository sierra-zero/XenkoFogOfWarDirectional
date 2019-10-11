using System;
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
    public class FogOfWarDetector : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        public bool Enable;
        public byte FrequencyModulo;
        public float VisionRadius;
        public int DegreeStep;

        private long frame;
        private Vector3 positionRecycler;
        private Vector3 lastPositionRecycler;
        private Vector3 targetRecycler;
        private IEnumerable<HitResult> resultList;
        private FogTile lastSeenRecycler;
        private FastList<FogTile> lastSeenTiles;
        private Simulation simulation;

        public override void Start()
        {
            InitializeDetector();
        }

        public override void Update()
        {
            PerformRaycastSweep();
        }

        private void PerformRaycastSweep()
        {
            if (!Enable) {
                return;
            }

            frame++;
            if (frame % FrequencyModulo != 0) {
                return;
            }

            Entity.Transform.GetWorldTransformation(out positionRecycler, out _, out _);
            if (positionRecycler == lastPositionRecycler) {
                foreach (var lastSeenTile in lastSeenTiles) {
                    lastSeenTile.Seen();
                }
                return;
            }

            lastSeenTiles.Clear();
            lastPositionRecycler = positionRecycler;

            for (int i = 0; i <= 360; i += DegreeStep) {
                targetRecycler = new Vector3(positionRecycler.X + VisionRadius * (float)Math.Cos(i), positionRecycler.Y, 
                    positionRecycler.Z + VisionRadius * (float)Math.Sin(i));

                foreach (var hitResult in simulation.RaycastPenetrating(positionRecycler, targetRecycler)
                    .OrderBy(b => Vector3.Distance(positionRecycler, b.Point))) {

                    if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.StaticFilter) {
                        break;
                    }

                    if (hitResult.Collider.CollisionGroup == CollisionFilterGroups.CustomFilter10) {
                        lastSeenRecycler = hitResult.Collider.Entity.Get<FogTile>();
                        lastSeenRecycler.Seen();
                        lastSeenTiles.Add(lastSeenRecycler);
                    }
                }
            }
        }

        private void InitializeDetector()
        {
            simulation = Entity.GetParent().Get<CharacterComponent>().Simulation;
            resultList = new FastList<HitResult>();
            lastSeenTiles = new FastList<FogTile>();

            if (FrequencyModulo == 0) {
                FrequencyModulo = 1;
            }
        }
    }
}
