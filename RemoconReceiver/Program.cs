using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using WebSocketSharp;
using AudioSwitcher.AudioApi.CoreAudio;

namespace RemoconReceiver
{
    class Program
    {
        static string Pin = "";

        const int KEYEVENTF_EXTENDEDKEY = 1;
        const int KEYEVENTF_KEYUP = 2;
        const int VK_MEDIA_NEXT_TRACK = 0xB0;
        const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        const int VK_MEDIA_PREV_TRACK = 0xB1;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);

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
                    ws.Send("UNVALID");
                    return;
                }

                string senderPin = data[0];
                string command = data[1];

                if (senderPin != Pin)
                {
                    ws.Send("UNAUTHORIZED");
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
                    case "PREVIOUS":
                        keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero); break;
                    case "PAUSE":
                        keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero); break;
                    case "NEXT":
                        keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero); break;
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
                else if (cliCommand == "reconfig")
                {
                    System.IO.File.Delete("remocon.conf");
                    config = Init.GetConfig();
                }
                else if (cliCommand != "exit")
                {
                    Console.WriteLine("Unvalid command");
                }
            }
            while (cliCommand != "exit");
        }
    }
}
