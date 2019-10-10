using System;
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
        public float DegreeStep;

        private long frame;
        private Vector3 positionRecycler;
        private Vector3 targetRecycler;
        private FastList<HitResult> resultList;
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

            for (float i = 0; i <= 360; i += DegreeStep) {
                targetRecycler = new Vector3(positionRecycler.X + VisionRadius * (float)Math.Cos(i), positionRecycler.Y, 
                    positionRecycler.Z + VisionRadius * (float)Math.Sin(i));

                resultList.Clear();
                resultList = simulation.RaycastPenetrating(positionRecycler, targetRecycler);

                foreach (var hitResult in resultList) {
                    // TODO tag the tiles as seen
                }
            }
        }

        private void InitializeDetector()
        {
            simulation = Entity.GetParent().Get<CharacterComponent>().Simulation;
            resultList = new FastList<HitResult>();

            if (FrequencyModulo == 0) {
                FrequencyModulo = 1;
            }
        }
    }
}
