using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RemoconReceiver
{
    class Config
    {
        public string HostName;
        public int PinLength;

        public Config()
        {
        }

        public void Save()
        {
            StreamWriter writer = new StreamWriter("remocon.conf");
            string text = String.Format("host={0}\npin_length={1}", this.HostName, this.PinLength.ToString());
            writer.Write(text);
            writer.Close();
        }

        public static Config Load()
        {
            Config conf = new Config();
            StreamReader reader = new StreamReader("remocon.conf");
            while (!reader.EndOfStream)
            {
                string[] config = reader.ReadLine().Split('=');
                string key = config[0];
                string value = config[1];

                switch (key)
                {
                    case "host":
                        conf.HostName = value; break;
                    case "pin_length":
                        conf.PinLength = Convert.ToInt32(value); break;
                }
            }

            return conf;
        }
    }
}
