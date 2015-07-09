using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace RosBridgeUtility
{
    class MsgOperation
    {
        public String op { get; set; }
    }

    class UnsubscribeMessage : MsgOperation
    {
        public UnsubscribeMessage()
        {
            op = "unsubscribe";
        }
        public UnsubscribeMessage(String _topic)
        {
            op = "unsubscribe";
            topic = _topic;
        }
        public String topic { get; set; }
    }

    class SubscribeMessage : UnsubscribeMessage
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
    }

    class PublishMessage : SubscribeMessage
    {
        public PublishMessage(String _topic)
            : base(_topic)
        {
            op = "publish";
        }
        public JObject msg { get; set; }
    }



    public class RosBridgeLogic
    {
        WebSocket ws;
        JObject lastMessage;
        IROSBridgeController subject;
        List<String> CollectedData;


        public void Initialize(String ipaddress)
        {
            ws = new WebSocket(ipaddress);            
        }

        public void Initialize(String ipaddress, IROSBridgeController parentwindow)
        {
            ws = new WebSocket(ipaddress);
            subject = parentwindow;
            ws.OnMessage += UpdateOnReceive;
            Console.Out.WriteLine("Initialized RosBridgeLogic component");
            
        }

        public void Connect()
        {
            ws.Connect();
            Console.Out.WriteLine("Successfully connected to: {0}", ws.Url);
        }

        public void Disconnect()
        {
            if (ws.ReadyState == WebSocketState.Open)
            {
                ws.Close();
            }
        }

        public void CollectData(Object Sender, MessageEventArgs a)
        {
            CollectedData.Add(a.Data);
        }

        public void StartCollect(String topic)
        {
            sendSubscription(topic);
            CollectedData = new List<string>();
            ws.OnMessage += CollectData;
        }

        public List<String> StopCollection(String topic)
        {
            sendSubscription(topic);
            ws.OnMessage -= CollectData;
            return CollectedData;
        }

        public void sendSubscription(String topic)
        {
            SubscribeMessage sub1 = new SubscribeMessage(topic);
            ws.Send(JsonConvert.SerializeObject(sub1).ToString());
        }

        public void sendUnsubscribe(String topic)
        {
            UnsubscribeMessage mes1 = new UnsubscribeMessage(topic);
            ws.Send(JsonConvert.SerializeObject(mes1).ToString());
        }

        public void ReceiveData(Object Sender, MessageEventArgs e)
        {
            lastMessage = JObject.Parse(e.Data);
        }

        public void sendPublish(String topic, JObject msg)
        {
            PublishMessage pub1 = new PublishMessage(topic);
            JObject jsondata = JObject.Parse(JsonConvert.SerializeObject(pub1));
            jsondata["msg"] = msg;
            ws.Send(jsondata.ToString());
        }

        public void setSubject(IROSBridgeController _subject)
        {
            this.subject = _subject;
        }

        public void UpdateOnReceive(Object Sender, MessageEventArgs e)
        {
            subject.ReceiveUpdate(e.Data);
        }

        public String getUrl()
        {
            return ws.Url.ToString();
        }

        public void AddMessageEventListener(EventHandler<WebSocketSharp.MessageEventArgs> a)
        {
            ws.OnMessage += a;
        }
    }
}
