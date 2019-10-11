using Xenko.Engine;

namespace FogOfWarDirectional
{
    public class FogDetector : StartupScript
    {
        // Declared public member fields and properties will show in the game studio

        public override void Start()
        {
            InitializeDetector();
        }

        private void InitializeDetector()
        {
            Game.Services.GetService<FogOfWarSystem>().Subscribe(Entity);
        }

        public override void Cancel()
        {
            base.Cancel();
            Game.Services.GetService<FogOfWarSystem>().Unsubscribe(Entity);
        }
    }
}
