using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RemoconReceiver
{
    class Init
    {
        public static Config GetConfig()
        {
            if (!File.Exists("remocon.conf"))
            {
                Config defaultConfig = new Config();

                Console.Write("Host: ");
                defaultConfig.HostName = Console.ReadLine();

                Console.Write("Pin length: ");
                defaultConfig.PinLength = Convert.ToInt32(Console.ReadLine());

                defaultConfig.Save();
                return defaultConfig;
            }

            return Config.Load();
        }

        public static string GeneratePIN(int length = 6)
        {
            Random rand = new Random();
            char[] set = "1234567890".ToCharArray();
            string pin = "";
            for (int i = 0; i < length; i++)
            {
                char next = set[rand.Next(0, set.Length - 1)];
                pin += next;
            }

            return pin;
        }
    }
}
