using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using System.Text;
using System.Threading.Tasks;

namespace TestWebsocketServer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://localhost:4649/neobot_odom"))
            {
                do
                {
                    ws.OnMessage += (sender, e) =>
                        Console.WriteLine("Odom received: " + e.Data);
                    ws.Connect();
                    ws.Send("Odom");
                } while (Console.ReadKey(true).Key != ConsoleKey.Spacebar);                
            }
        }
    }
}
