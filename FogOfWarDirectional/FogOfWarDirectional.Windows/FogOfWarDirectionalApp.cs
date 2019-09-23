using Xenko.Engine;

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
