using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketSharp;
using Newtonsoft.Json.Linq;

namespace TestApplicationConnect
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
    class MsgOperation
    {
        public String op { get; set; }
    }
    class SubscribeMessage: MsgOperation
    {
        public SubscribeMessage()
        {
            op = "subscribe";
        }
        public SubscribeMessage(String _topic)
        {
            op = "subscribe";
            topic = _topic;
        }
        public String topic { get; set; }
    }
    
    class PublishMessage : SubscribeMessage
    {
        public PublishMessage(String _topic): base(_topic)
        {
            op = "publish";
        }
        public JObject msg { get; set; }
    }
    
    class Program
    {
        private const int port = 9090;

        private static String response = String.Empty;

        public static void processROSMessage(object Sender, MessageEventArgs e)
        {
            //JsonTextReader reader = new JsonTextReader(new System.IO.StringReader(e.Data));
            JObject jsonData = JObject.Parse(e.Data);
            /*
            Console.WriteLine("Pos X: {0}",(float)jsonData["msg"]["x"]);
            Console.WriteLine("Pos Y: {0}",(float)jsonData["msg"]["y"]);
            Console.WriteLine("Theta: {0}",(float)jsonData["msg"]["theta"]);
             * */
        }

        /**
         * Used to make the turtle to run in circles pre-Hydro
         * 
         * */
        public static void sendTurtleCircleGroovy(WebSocket ws)
        {
            var v = new { linear = 0.0, angular= -3.14};
            PublishMessage pub1 = new PublishMessage("/turtle1/command_velocity");
            JObject jsondata = JObject.Parse(JsonConvert.SerializeObject(pub1));            
            jsondata["msg"] = JObject.Parse(JsonConvert.SerializeObject(v));
            Console.WriteLine(jsondata.ToString());
            ws.Send(jsondata.ToString());
        }

        static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://10.2.94.144:9090"))
            {
                ws.OnMessage += processROSMessage;
                    
                ws.Connect();
                SubscribeMessage sub1 = new SubscribeMessage("/turtle1/pose");
                //Console.WriteLine(JsonConvert.SerializeObject(sub1).ToString());
                ws.Send(JsonConvert.SerializeObject(sub1).ToString());
                //ws.Send("{\"op\": \"subscribe\", \"topic\":\"/turtle1/pose\"}");
                sendTurtleCircleGroovy(ws);
                Console.ReadKey(true);                
            }
        }
    }
}
