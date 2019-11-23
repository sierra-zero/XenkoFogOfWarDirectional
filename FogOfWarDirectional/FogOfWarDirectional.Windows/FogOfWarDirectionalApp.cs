using Xenko.Engine;
// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ArrangeTypeModifiers

namespace FogOfWarDirectional.Windows
{
    class FogOfWarDirectionalApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
