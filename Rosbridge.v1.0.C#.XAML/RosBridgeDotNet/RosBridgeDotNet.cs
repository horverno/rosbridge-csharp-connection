using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Net.Sockets;


namespace RosBridgeDotNet
{
    public class RosBridgeDotNet
    {
        private IPEndPoint _IPEndPoint;
        private TcpClient _TcpClient = new TcpClient();
        private Socket _socket;
        public bool Connected { get; private set; }
        public string info { get; private set; }
        // Incoming data from the client.
        public static string data = null;
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];

        /// <summary>
        /// Builds up a connection to the address RosBridge
        /// </summary>
        /// <param name="IpAddress">Address of the target computer(ROS).</param>
        /// <param name="port">The port address to connect to. By default it is set to 9090.</param>
        public RosBridgeDotNet(string IpAddress, int port = 9090)
        {
            try
            {
                byte[] handShake = Encoding.UTF8.GetBytes("raw\r\n\r\n");
                _IPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), port);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _socket.Connect(_IPEndPoint);
                _socket.Send(handShake);
                Connected = true;
            }
            catch (Exception)
            {
                Connected = false;
                //throw new ConnectionException();
            }
        }

        /// <summary>
        /// Sends the message to the connected pc.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ConnectionException"></exception>
        public void Publish(string receiver, string type, object msg)
        {
            if (receiver == null || type == null || msg == null)
                throw new ArgumentNullException();
            //if (!Connected)
            //    throw new ConnectionException();
            PublishMessage m = new PublishMessage(receiver, type, msg);
            string needToSend = JsonConvert.SerializeObject(m);
            _socket.Send(new byte[] { 0 });      // \x00
            _socket.Send(Encoding.UTF8.GetBytes(needToSend));
            _socket.Send(new byte[] { 255 });    // \xff
            System.Diagnostics.Debug.WriteLine("JSON published: " + needToSend); //debug

        }

        public void Subscribe()
        {
            //Socket socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            object[] poseSubscribe = { "/turtle1/pose", -1 };
            SubscribeMessage m = new SubscribeMessage("/rosbridge/subscribe", poseSubscribe);
            _socket.Send(new byte[] { 0 });
            _socket.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m)));
            _socket.Send(new byte[] { 255 });
            System.Diagnostics.Debug.WriteLine("JSON published: " + JsonConvert.SerializeObject(m)); //debug


        }
        public void CloseConnection()
        {
            if (Connected)
            {
                Connected = false;
                _socket.Close();
            }
        }

        //{"receiver":"/topic","msg":{"fieldname":"value"},"type":"ros/type"}
        //{"receiver":"/turtle1/command_velocity","type":"turtlesim/Velocity","msg":{"linear":20.0,"angular":20.0},"strArgs":null,"linear":0.0,"angular":0.0}
        public class PublishMessage
        {
            public string receiver { get; set; }
            public string type { get; set; }
            public object msg { get; set; }
            public string strArgs { get; set; }
            public double linear { get; set; }
            public double angular { get; set; }

            public PublishMessage() { }

            public PublishMessage(string _receiver, string _type, object _msg)
            {
                receiver = _receiver;
                type = _type;
                msg = _msg;
            }

            public PublishMessage(string _receiver, string _type, double _linear, double _angular)
            {
                receiver = _receiver;
                type = _type;
                linear = _linear;
                angular = _angular;
            }

            public PublishMessage(string _receiver, string _type, string _sarg)
            {
                receiver = _receiver;
                type = _type;
                strArgs = _sarg;
            }
            public PublishMessage(string _receiver, string _type)
            {
                receiver = _receiver;
                type = _type;
            }
            public PublishMessage(string _msg)
            {
                msg = _msg;
            }
        }

        //{"receiver":"/rosbridge/subscribe","msg":["/topic",-1]}
        //{"reciever":"/rosjs/subscribe", "msg":["/turtle1/pose",-1]}
        public class SubscribeMessage
        {
            public string receiver { get; set; }
            public object[] msg { get; set; }

            public SubscribeMessage() { }

            public SubscribeMessage(string _receiver, object[] _msg)
            {
                receiver = _receiver;
                msg = _msg;
            }
        }
        //'{"linear":20, "angular":20}'
        public class TurtleSim
        {
            public double linear { get; set; }
            public double angular { get; set; }

            public TurtleSim(double _linear, double _angular)
            {
                linear = _linear;
                angular = _angular;
            }
        }
        //'{"angularVelocity":[0.1,0.1],"driveActive":[1,1],"quickStop":[0,0],"disableBrake":[1,1]}'
        // {"angularVelocity":[0.0,0.0],"driveActive":[true,false],"quickStop":[true,false],"disableBrake":[true,false]}
        public class Neobotix
        {
            public double[] angularVelocity { get; set; }
            public bool[] driveActive { get; set; }
            public bool[] quickStop { get; set; }
            public bool[] disableBrake { get; set; }

            public Neobotix(double[] _angularVelocity, bool[] _driveActive, bool[] _quickStop, bool[] _disableBrake)
            {
                angularVelocity = _angularVelocity;
                driveActive = _driveActive;
                quickStop = _quickStop;
                disableBrake = _disableBrake;

            }
            public void Debug(object o)
            {
                System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(o));
            }
        }
        //{"msg": {"y": 6.43528413772583, "x": 6.383671283721924, "linear_velocity": 0.0, "angular_velocity": 0.0, "theta": 1.3104441165924072}, "receiver": "/turtle1/pose"}
        public class TurtlePoseResponse
        {
            public TurtlePoseResponseSubMsg msg { get; set; }
            public string receiver { get; set; }
        }
        //"y": 6.43528413772583, "x": 6.383671283721924, "linear_velocity": 0.0, "angular_velocity": 0.0, "theta": 1.3104441165924072}
        public class TurtlePoseResponseSubMsg
        {
            public double y { get; set; }
            public double x { get; set; }
            public double linear_velocity { get; set; }
            public double angular_velocity { get; set; }
            public double theta { get; set; }
        }
        //topic = "/sick_s300/scan";
        public class NeoResponse
        {
            //todo
        }
        public class FreeString
        {
            public string command { get; set; }

            public FreeString(string _command)
            {
                command = _command;

            }
        }
    }
}
