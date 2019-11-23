using System;
using Xenko.Core.Mathematics;
using Xenko.Engine;
using Xenko.Games;
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleLossOfFraction
// ReSharper disable MemberCanBePrivate.Global

namespace FogOfWarDirectional
{
    public class FogTile : EntityComponent
    {
        public Vector2 Coord { get; }
        public float Visibility { get; private set; } 

        private float fadeDelta;
        private bool tileSeen;

        private readonly GameTime gameTime;
        private readonly float fadeRate;

        public FogTile(FogOfWarSystem fog, GameTime gameTime, Vector2 fogTileCoord)
        {
            this.gameTime = gameTime;
            Coord = fogTileCoord;
            fadeRate = fog.Fade;
        }

        public void UpdateSeen(bool seen)
        {
            tileSeen = seen;
        }

        public void UpdateVisibility()
        {
            fadeDelta = fadeRate * (float)gameTime.Elapsed.TotalSeconds;

            if (tileSeen && Visibility - fadeDelta < 0)
            {
                Visibility = 0;
                return;
            }

            if (!tileSeen && Visibility + fadeDelta > 1)
            {
                Visibility = 1;
                return;
            }

            if (tileSeen)
            {
                Visibility -= fadeDelta;
                return;
            }

            Visibility += fadeDelta;
        }
    }
}
