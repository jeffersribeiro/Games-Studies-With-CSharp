﻿using System;
using FirstWorkingGame.Source;

namespace FirstWorkingGame
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