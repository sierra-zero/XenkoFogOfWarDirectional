using System;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;

// ReSharper disable PossibleLossOfFraction
// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogTile : EntityComponent
    {
        // Declared public member fields and properties will show in the game studio
        public Vector2 Coord { get; }
        public float Visibility { get; private set; } = 1; // equates to alpha, 1 = visible (black on material)

        private GameTime gameTime;
        private TimeSpan lastStateChange = TimeSpan.Zero;
        private TimeSpan transitionTimer = TimeSpan.Zero;
        private TimeSpan timeDeltaReycler = TimeSpan.Zero;

        private bool seen = false;
        private readonly FogOfWarSystem fog;

        public FogTile(FogOfWarSystem fog, GameTime gameTime, Vector2 fogTileCoord)
        {
            this.fog = fog;
            this.gameTime = gameTime;
            Coord = fogTileCoord;
            transitionTimer = TimeSpan.FromMilliseconds(fog.FogFadeTimerMs);
        }

        public void UpdateSeen(bool seenState)
        {
            //if (seenState != seen) {
            //    lastStateChange = gameTime.Elapsed;
            //    seen = seenState;
            //}

            //timeDeltaReycler = gameTime.Elapsed - lastStateChange;
            //Visibility = (timeDeltaReycler.Milliseconds / transitionTimer.Milliseconds);

            //if (Visibility > 1) {
            //    Visibility = 1;
            //}

            //if (Visibility < 0) {
            //    Visibility = 0;
            //}

            if (seenState)
            {
                Visibility = 1;
            }
            else
            {
                Visibility = 0;
            }
        }
    }
}
