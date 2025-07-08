using System;
using GameStudiesWithCSharp.FirstWorkingGame.Source;

namespace GameStudiesWithCSharp.FirstWorkingGame
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game(800, 600, "GltfMesh + OpenGL"))
            {
                game.Run();
            }
        }
    }

}