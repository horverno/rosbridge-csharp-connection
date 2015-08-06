using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Globalization;

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
        public String ipaddress { get; set; }
        public int port { get; set; }
        public String protocol { get; set; }
        public String target { get; set; }
        public double vis_scaleFactor { get; set; }
        public String laserFieldTopic { get; set; }
        public String odometryTopic { get; set; }
        public String showState { get; set; }

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
            ipaddress = node.Attributes["ipaddress"].Value;
            port = Int32.Parse(node.Attributes["port"].Value);
            protocol = node.Attributes["protocol"].Value;
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
                    try
                    {
                        target = ((XmlNode)item).Attributes["target"].Value;
                        Console.WriteLine("New target: {0}", target);
                    }
                    catch (NullReferenceException e1)
                    {
                        Console.WriteLine("{0}: no target specified, ignoring", e1.Data);
                    }
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
            try
            {
                double tmp;
                Double.TryParse(doc.DocumentElement.SelectSingleNode("/rosbridge_config/visualization/scale").Attributes["r"].Value.ToString(), 
                    NumberStyles.Number,
                    CultureInfo.CreateSpecificCulture("en-US"),
                    out tmp);
                vis_scaleFactor = tmp;
                Console.WriteLine("Using scale factor: {0}", vis_scaleFactor);
                laserFieldTopic = doc.DocumentElement.SelectSingleNode("/rosbridge_config/visualization/laser_field").Attributes["topic"].Value;
                odometryTopic = doc.DocumentElement.SelectSingleNode("/rosbridge_config/visualization/odometry").Attributes["topic"].Value;
                showState = doc.DocumentElement.SelectSingleNode("/rosbridge_config/visualization/showState").Attributes["topic"].Value;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine("One visualization does not exist, ignoring: {0}", e.Data);
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
