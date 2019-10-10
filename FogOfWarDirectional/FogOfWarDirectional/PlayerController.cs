using Xenko.Core.Mathematics;
using Xenko.Input;
using Xenko.Engine;
using Xenko.Physics;

namespace FogOfWarDirectional
{
    public class PlayerController : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        private CharacterComponent characterComponent;
        private Vector3 velocity;

        private const float Speed = 2.5f;

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

            if (Input.Keyboard.IsKeyDown(Keys.W)) {
                velocity -= Vector3.UnitZ * Speed;
            }

            if (Input.Keyboard.IsKeyDown(Keys.S)) {
                velocity += Vector3.UnitZ * Speed;
            }

            if (Input.Keyboard.IsKeyDown(Keys.A)) {
                velocity -= Vector3.UnitX * Speed;
            }

            if (Input.Keyboard.IsKeyDown(Keys.D)) {
                velocity += Vector3.UnitX * Speed;
            }

            characterComponent.SetVelocity(velocity);
        }

        private void InitializePlayer()
        {
            characterComponent = Entity.Get<CharacterComponent>();
        }
    }
}
