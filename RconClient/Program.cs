using RCONServerLib;
using System;
using System.Text;

namespace RconClient
{
    class Program
    {
        private static bool _authProcessed;
        static void Main(string[] args)
        {
            var ip = "127.0.0.1";
            var port = 27015;
            var password = "changeme";
            if (args.Length == 3)
            {
                ip = args[0];
                int.TryParse(args[1], out port);
                password = args[2];
            }

            var client = new RemoteConClient();
            client.OnLog += message => { Console.WriteLine(string.Format("Client Log: {0}", message)); };
            client.OnAuthResult += result => { _authProcessed = true; };
            client.OnConnectionStateChange += state =>
            {
                Console.WriteLine("Connection changed: " + state);
                if (state == 0)
                {
                    client.Authenticate(password);
                }
            };

            client.Connect(ip, port);
            while (true)
            {
                if (!client.Connected)
                {
                    Console.ReadKey();
                    client.Connect(ip, port);
                    continue;
                }

                if (_authProcessed && !client.Authenticated)
                {
                    _authProcessed = false;
                    Console.WriteLine("Password: ");
                    var enteredPwd = Console.ReadLine();
                    client.Authenticate(enteredPwd);
                    continue;
                }

                if (!client.Authenticated)
                    continue;

                var cmd = Console.ReadLine();
                if (cmd == "exit" || cmd == "quit")
                {
                    client.Disconnect();
                    return;
                }

                client.SendCommand(StringToUnicode(cmd), result => { Console.WriteLine(">>" + UnicodeToString(result)
                    .Replace("<br/>", "\n")); });
            }
        }

        private static string StringToUnicode(string s)
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(string.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }

        private static string UnicodeToString(string srcText)
        {
            string dst = "";
            string src = srcText;
            int len = srcText.Length / 6;
            for (int i = 0; i <= len - 1; i++)
            {
                string str = src.Substring(0, 6).Substring(2);
                src = src.Substring(6);
                byte[] bytes = new byte[2];
                bytes[1] = byte.Parse(int.Parse(str.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                bytes[0] = byte.Parse(int.Parse(str.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                dst += Encoding.Unicode.GetString(bytes);
            }
            return dst;
        }
    }
}
