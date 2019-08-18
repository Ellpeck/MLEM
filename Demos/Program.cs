using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Demos {
    public static class Program {

        private static readonly Dictionary<string, Func<Game>> Demos = new Dictionary<string, Func<Game>>();

        static Program() {
            Demos.Add("Ui", () => new UiDemo());
            Demos.Add("AutoTiling", () => new AutoTilingDemo());
            Demos.Add("Animation", () => new AnimationDemo());
            Demos.Add("Pathfinding", () => new PathfindingDemo());
        }

        public static void Main(string[] args) {
            Func<Game> demoUsed;
            if (args.Length <= 0 || !Demos.ContainsKey(args[0])) {
                beforeDemo:
                Console.WriteLine("Please type the name of the demo you want to see and press Enter.");
                Console.WriteLine("The following demos are available: " + string.Join(", ", Demos.Keys));
                Console.WriteLine("(Alternatively, you can supply the name of the demo you want to see as the first argument)");
                var demo = Console.ReadLine();
                if (!Demos.ContainsKey(demo)) {
                    Console.WriteLine("Not a valid demo.");
                    goto beforeDemo;
                } else {
                    demoUsed = Demos[demo];
                }
            } else {
                demoUsed = Demos[args[0]];
            }

            using (var game = demoUsed.Invoke())
                game.Run();
        }

    }
}