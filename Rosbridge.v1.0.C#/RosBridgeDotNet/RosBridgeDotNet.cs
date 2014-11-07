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

        private Socket socket;
        public bool Connected { get;private set; }
        public string info { get; private set; }

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
                info = "ok1";
                _IPEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), port);
                info = "ok2";
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                info = "ok3";
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                info = "ok4";
                socket.Connect(_IPEndPoint);
                info = "ok5";
                socket.Send(handShake);
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
            if (!Connected)
                throw new ConnectionException();
            Message m = new Message(receiver, type, msg);
            string needToSend = JsonConvert.SerializeObject(m);
            System.Diagnostics.Debug.WriteLine("JSON published: " + needToSend); //debug
            socket.Send(new byte[] { 0 });      // \x00
            socket.Send(Encoding.UTF8.GetBytes(needToSend));
            socket.Send(new byte[] { 255 });    // \xff
        }
        /// <summary>
        /// Sends the string message to the connected pc.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void PublishString(string receiver, string type, string strArguments)
        {
            if (receiver == null || type == null || strArguments == null)
                throw new ArgumentNullException();
            if (!Connected)
                throw new ConnectionException();
            Message m = new Message(receiver, type, strArguments);
            string needToSend = JsonConvert.SerializeObject(m);

            socket.Send(new byte[] { 0 });
            socket.Send(Encoding.UTF8.GetBytes(needToSend));
            socket.Send(new byte[] { 255 });
        }

        public void Subscribe()
        {
            //todo
            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint senderRemote = (EndPoint)sender;
            //byte[] messag = new Byte[256];
            //socket.ReceiveFrom(messag, 0, messag.Length, SocketFlags.None, ref senderRemote);
            //Console.Write(messag);
            ////object msg;
            ////Message m = new Message("/rosjs/subscribe", "turtle1/pose", msg); 
            ////string needToSend = JsonConvert.SerializeObject(m);

            ////socket.Send(new byte[] { 0 });
            ////socket.Send(Encoding.UTF8.GetBytes(needToSend));
            ////socket.Send(new byte[] { 255 });
        }
        public void CloseConnection()
        {
            if (Connected)
            {
                Connected = false;
                socket.Close();
            }
        }

        private class Message
        {
            public string receiver { get; set; }
            public string type { get; set; }
            public object msg { get; set; }
            public string strArgs { get; set; }
            public double linear { get; set; }
            public double angular { get; set; }

            public Message(){}

            public Message(string _receiver, string _type, object _msg)
            {
                receiver = _receiver;
                type = _type;
                msg = _msg;
            }

            public Message(string _receiver, string _type, double _linear, double _angular)
            {
                receiver = _receiver;
                type = _type;
                linear = _linear;
                angular = _angular;
            }

            public Message(string _receiver, string _type, string _sarg)
            {
                receiver = _receiver;
                type = _type;
                strArgs = _sarg;
            }
            public Message(string _msg)
            {
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
        public class Neobotix
        {
            public double[] angularVelocity { get; set; }
            public bool[] driveActive { get; set; }
            public bool[] quickStop { get; set; }
            public bool[] disableBrake { get; set; }

            public Neobotix(double[] _angularVelocity, bool[] _driveActive, bool[] _quickStop, bool[] _disableBrake)
            {
                //angularVelocity = new double[2];

                angularVelocity = _angularVelocity;
                driveActive = _driveActive;
                quickStop = _quickStop;
                disableBrake = _disableBrake;
            }
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
