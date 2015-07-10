using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        List<Tuple<String, String>> CollectedData;

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
            JObject jData = JObject.Parse(a.Data);
            CollectedData.Add(new Tuple<String, String>(jData["topic"].ToString(),
                jData["msg"].ToString()));
        }

        public void initializeCollection()
        {
            CollectedData = new List<Tuple<string,string>>();
            ws.OnMessage += CollectData;
        }

        public void StartCollection(String topic)
        {
            sendSubscription(topic);
        }

        public void RemoveCollection(String topic)
        {
            sendUnsubscribe(topic);            
        }

        public List<Tuple<String, String>> StopCollection()
        {
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

        public void sendPublish(String topic, String msg)
        {
            PublishMessage pub1 = new PublishMessage(topic);
            JObject jsondata = JObject.Parse(JsonConvert.SerializeObject(pub1));
            jsondata["msg"] = msg;
            ws.Send(jsondata.ToString());
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

        public List<T> projectionResponse<T>(String topic, String field)
        {
            List<T> resp = new List<T>();
            var converter = TypeDescriptor.GetConverter(typeof(T));
            foreach (var item in CollectedData.Where(
                s => s.Item1.Equals("\""+topic+"\"")))
            {
                JObject firstElement = JObject.Parse(item.Item2);
                resp.Add((T)converter.ConvertFromString(null, 
                    CultureInfo.CreateSpecificCulture("en-US"), 
                    (firstElement[field]).ToString()));                
            }
            return resp;
        }

        public System.Collections.IList getResponseAttribute(String topic, String field)
        {
            JObject firstElement = JObject.Parse(
                CollectedData.First(s => s.Item1.Equals("\""+topic+"\"") ).Item2);
            bool test;
            int testInt;
            double testDouble;
            if (Boolean.TryParse((firstElement[field]).ToString(),out test))
            {
                return projectionResponse<Boolean>(topic,field);
            }
            else if (Int32.TryParse((firstElement[field]).ToString(), out testInt))
            {
                return projectionResponse<Int32>(topic,field);
            }
            else if (Double.TryParse((firstElement[field]).ToString(),
                NumberStyles.Number, CultureInfo.CreateSpecificCulture("en-US"),
                out testDouble))
            {
                return projectionResponse<Double>(topic,field);
            }
            return null;
        }
    }
}
