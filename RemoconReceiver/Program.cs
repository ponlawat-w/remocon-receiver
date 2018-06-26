using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using AudioSwitcher.AudioApi.CoreAudio;

namespace RemoconReceiver
{
    class Program
    {
        static string Pin = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Loading config...");
            Config config = Init.GetConfig();

            Pin = Init.GeneratePIN(config.PinLength);

            Console.WriteLine("***********");
            Console.WriteLine("PIN: {0}", Pin);
            Console.WriteLine("***********");

            Console.WriteLine("Connecting audio device...");
            CoreAudioDevice audioDevice = (new CoreAudioController()).DefaultPlaybackDevice;

            WebSocket ws = new WebSocket(String.Format("wss://{0}:{1}/receiver", config.HostName, config.Port));
            
            ws.OnOpen += (sender, e) =>
            {
                Console.WriteLine("Connected");
            };

            ws.OnMessage += (sender, e) =>
            {
                string[] data = e.Data.Split('\n');
                if (data.Length != 2)
                {
                    ws.Send("Incorrect format!");
                    return;
                }

                string senderPin = data[0];
                string command = data[1];

                if (senderPin != Pin)
                {
                    ws.Send("Incorrect pin!");
                    return;
                }

                Console.WriteLine(command);

                switch (command)
                {
                    case "VOLUP":
                        audioDevice.Volume += 2; break;
                    case "VOLDOWN":
                        audioDevice.Volume -= 2; break;
                    case "MUTE":
                        audioDevice.ToggleMute(); break;
                }

                Console.Write("> ");
            };

            ws.OnError += (sender, e) =>
            {
                Console.WriteLine("ERROR {0}", e.Message);
            };

            Console.WriteLine("Connecting to server...");
            ws.Connect();

            string cliCommand = "";
            do
            {
                Console.Write("> ");
                cliCommand = Console.ReadLine();

                if (cliCommand == "reset")
                {
                    Pin = Init.GeneratePIN(config.PinLength);
                    Console.WriteLine("New PIN: {0}", Pin);
                }
                else if (cliCommand == "setpin")
                {
                    Console.Write("Custom PIN: ");
                    Pin = Console.ReadLine();

                    Console.WriteLine("New PIN: {0}", Pin);
                }
                else
                {
                    Console.WriteLine("Unvalid command");
                }
            }
            while (cliCommand != "exit");
        }
    }
}
