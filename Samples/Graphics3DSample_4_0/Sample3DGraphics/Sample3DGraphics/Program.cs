using System;

namespace Graphics3DSample
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Graphics3DSampleGame game = new Graphics3DSampleGame())
            {
                game.Run();
            }
        }
    }
#endif
}

