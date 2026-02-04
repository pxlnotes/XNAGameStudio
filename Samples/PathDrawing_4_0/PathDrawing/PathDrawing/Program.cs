namespace PathDrawing
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PathDrawingGame game = new PathDrawingGame())
            {
                game.Run();
            }
        }
    }
#endif
}
