using System;
namespace Coop_Vr
{
    public static class Log
    {
        public static bool Enabled = true;
        public static void Do(string str)
        {
            if(Enabled)
                Console.WriteLine(str);
        }

        public static void Do(object obj)
        {
            Do(obj.ToString());
        }
    }
}
