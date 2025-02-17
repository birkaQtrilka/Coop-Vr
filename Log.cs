using System;
namespace Coop_Vr
{
    public static class Log
    {
        public static bool Enabled = true;
        public static int Layer = 0;

        public static void Do(string str, int layer = 0)
        {
            if(Enabled && Layer >= layer)
                Console.WriteLine(str);
        }

        public static void Do(object obj, int layer = 0)
        {
            Do(obj.ToString(), layer);
        }
    }
}
