using Xenko.Core.Mathematics;
using Xenko.Input;
using Xenko.Engine;
using Xenko.Physics;
using System;

namespace FogOfWarDirectional
{
    public class PlayerController : SyncScript
    {
        public float Speed;

        private CharacterComponent characterComponent;
        private float speedDelta;
        private Vector3 velocity;

        public override void Start()
        {
            InitializePlayer();
        }

        public override void Update()
        {
            PlayerInput();
        }

        private void PlayerInput()
        {
            velocity = Vector3.Zero;
            speedDelta = Speed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

            if (Input.Keyboard.IsKeyDown(Keys.W)) {
                velocity -= Vector3.UnitZ * speedDelta;
            }

            if (Input.Keyboard.IsKeyDown(Keys.S)) {
                velocity += Vector3.UnitZ * speedDelta;
            }

            if (Input.Keyboard.IsKeyDown(Keys.A)) {
                velocity -= Vector3.UnitX * speedDelta;
            }

            if (Input.Keyboard.IsKeyDown(Keys.D)) {
                velocity += Vector3.UnitX * speedDelta;
            }

            characterComponent.SetVelocity(velocity);
        }

        private void InitializePlayer()
        {
            characterComponent = Entity.Get<CharacterComponent>();
        }
    }
}
