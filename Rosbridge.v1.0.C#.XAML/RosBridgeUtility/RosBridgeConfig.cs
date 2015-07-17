using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Text;

namespace RosBridgeUtility
{
    public class TopicObject
    {
        public TopicObject() { }
        public String name { get; set; }
        public int throttle { get; set; }
    }

    public class RosBridgeConfig
    {
        public String URI { get; set; }
        private List<TopicObject> monitoredTopics;
        private List<Tuple<String, String>> projectedAttributes;
        private List<String> publications;
        private XmlDocument doc;

        public RosBridgeConfig()
        {
            monitoredTopics = new List<TopicObject>();
            projectedAttributes = new List<Tuple<String, String>>();
            publications = new List<String>();
            doc = new XmlDocument();
        }

        public void readConfig(String path)
        {
            doc.Load(path);
            XmlNode node = doc.DocumentElement.SelectSingleNode("/rosbridge_config/network");
            String ipaddress = node.Attributes["ipaddress"].Value;
            int port = Int32.Parse(node.Attributes["port"].Value);
            String protocol = node.Attributes["protocol"].Value;
            URI = protocol + "://" + ipaddress + ":" + port.ToString();
            Console.Out.WriteLine(URI);
            foreach (var item in 
                doc.DocumentElement.SelectSingleNode
                ("/rosbridge_config/subscriptions").ChildNodes)
            {
                int throttle_ = 0;
                try
                {
                    throttle_ = Int32.Parse(((XmlNode)item).Attributes["throttle"].Value);
                }
                catch (NullReferenceException e)
                {
                    Console.Out.WriteLine("{0}: Throttle is not given, assuming 0", e.Data);
                    throttle_ = 0;
                }
                TopicObject tmp = new TopicObject
                    {
                        name = ((XmlNode)item).Attributes["name"].Value,
                        throttle = throttle_
                    };                
                monitoredTopics.Add(tmp);
            }
            try
            {
                foreach (var item in doc.DocumentElement.SelectSingleNode
                    ("/rosbridge_config/publications").ChildNodes)
                {
                    publications.Add(((XmlNode)item).Attributes["name"].Value);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("{0}: No publication secified, ignoring", e.Data);
            }
            foreach (var item in
                doc.DocumentElement.SelectSingleNode
                ("/rosbridge_config/projections").ChildNodes)
            {
                projectedAttributes.Add(new Tuple<String, String>(
                    ((XmlNode)item).Attributes["topic"].Value,
                    ((XmlNode)item).Attributes["attribute"].Value));
            }
        }

        public IList<TopicObject> getTopicList()
        {
            return monitoredTopics;
        }

        public List<Tuple<String, String>> ProjectedAttributes()
        {
            return projectedAttributes;
        }
        public List<String> getPublicationList()
        {
            return this.publications;
        }
    }
}
