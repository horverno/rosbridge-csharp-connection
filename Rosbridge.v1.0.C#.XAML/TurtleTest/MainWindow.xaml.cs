using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TurtleTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _clientSocket;
        private AsyncCallback _pfnCallBack;
        private IAsyncResult _result;
        private String _pureResponse = "";
        public String _lastLine = "";
        private RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse _responseObj = new RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse();
        private String[] _lines;
        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[1];
        }
        public MainWindow()
        {
            InitializeComponent();
            stackControls.Visibility = System.Windows.Visibility.Hidden;
            this.DataContext = _responseObj;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (btnConnect.Content.Equals("Connect"))
            {
                if (ConnectToRos())
                {
                    btnConnect.Background = Brushes.LightGreen;
                    btnConnect.Content = "Disonnect";
                    stackControls.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {

                DisconnectFromRos();
                btnConnect.Background = Brushes.OrangeRed;
                btnConnect.Content = "Connect";
                stackControls.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
           RosBridgeDotNet.RosBridgeDotNet.SubscribeMessage rosSubscMsg;
           try
           {
               object[] poseSubscribe = { "/turtle1/pose", 500 };
               if (btnSubscribe.Content.Equals("Subcribe"))
               {
                   rosSubscMsg = new RosBridgeDotNet.RosBridgeDotNet.SubscribeMessage("/rosbridge/subscribe", poseSubscribe);
                   btnSubscribe.Content = "Unsubcribe";
               }
               else
               {
                   rosSubscMsg = new RosBridgeDotNet.RosBridgeDotNet.SubscribeMessage("/rosbridge/unsubscribe", poseSubscribe);
                   btnSubscribe.Content = "Subcribe";
               }
               string needToSend = JsonConvert.SerializeObject(rosSubscMsg);
               //Object objData = SerializeEventData(new EventData(richTextTxName.Text));
               //byte[] byData = System.Text.Encoding.UTF8.GetBytes(objData.ToString());
               if (_clientSocket != null)
               {
                   _clientSocket.Send(new byte[] { 0 });    // \x00
                   _clientSocket.Send(Encoding.UTF8.GetBytes(needToSend));
                   _clientSocket.Send(new byte[] { 255 });    // \xff
               }
           }
           catch (SocketException se)
           {
               MessageBox.Show(se.Message);
           }
           Update();
       }


        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(1, 0);
            Update();
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(-1, 0);
            Update();
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(0, 1);
            Update();
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(0, -1);
            Update();
        }
        /// <summary>
        /// Return true if connection was succesful
        /// </summary>
        /// <returns></returns>
        bool ConnectToRos()
        {
            // See if we have text on the IP and Port text fields
            if (txtIP.Text == "" || txtPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return false;
            }
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(txtIP.Text);
                int iPortNo = System.Convert.ToInt16(txtPort.Text);
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);
                // Connect to the remote host
                _clientSocket.Connect(ipEnd);
                if (_clientSocket.Connected)
                {
                    //Wait for data asynchronously 
                    WaitForData();
                }
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                byte[] handShake = Encoding.UTF8.GetBytes("raw\r\n\r\n");
                _clientSocket.Send(handShake);
                return true;
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                MessageBox.Show(str);
                return false;
            }
        }
        void DisconnectFromRos()
        {
            if (_clientSocket != null)
            {
                _clientSocket.Close();
                _clientSocket = null;
            }
        }
        public void WaitForData()
        {
            try
            {
                if (_pfnCallBack == null)
                {
                    _pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = _clientSocket;
                // Start listening to the data asynchronously
                _result = _clientSocket.BeginReceive(theSocPkt.dataBuffer,
                                                        0, theSocPkt.dataBuffer.Length,
                                                        SocketFlags.None,
                                                        _pfnCallBack,
                                                        theSocPkt);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
            catch (NullReferenceException ne)
            {
                MessageBox.Show(ne.Message);
            }

        }
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                String szData = new String(chars);
                try
                {
                    //RetrieveReceivedEventData(pureResponse);
                    szData = Regex.Replace(szData, @"[^\u0000-\u007F]", String.Empty);
                    //System.Diagnostics.Debug.WriteLine(szData.ToString() + " ");
                    _pureResponse = Regex.Replace(_pureResponse, @"\0", String.Empty);
                    _pureResponse += szData;
                    _lines = _pureResponse.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);
                    if (_lines[_lines.Length - 1].Contains("pose\"}"))
                    {
                        _lastLine = _lines[_lines.Length - 1];
                        //MessageBox.Show(_lines[_lines.Length - 1]);
                    }
                    _responseObj = JsonConvert.DeserializeObject<RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse>(_lastLine);
                    System.Diagnostics.Debug.WriteLine("x: " + _responseObj.msg.x + " y: " + _responseObj.msg.y);

                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                }
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void PublishturtleMessage(double linear, double angular)
        {
            try
            {
                string topic = "/turtle1/command_velocity";
                string msgtype = "turtlesim/Velocity";
                RosBridgeDotNet.RosBridgeDotNet.TurtleSim turtleGo1 = new RosBridgeDotNet.RosBridgeDotNet.TurtleSim(linear, angular);
                RosBridgeDotNet.RosBridgeDotNet.PublishMessage m = new RosBridgeDotNet.RosBridgeDotNet.PublishMessage(topic, msgtype, turtleGo1);
                string needToSend = JsonConvert.SerializeObject(m);
                //Object objData = SerializeEventData(new EventData(richTextTxName.Text));
                //byte[] byData = System.Text.Encoding.UTF8.GetBytes(objData.ToString());
                if (_clientSocket != null)
                {
                    _clientSocket.Send(new byte[] { 0 });    // \x00
                    _clientSocket.Send(Encoding.UTF8.GetBytes(needToSend));
                    _clientSocket.Send(new byte[] { 255 });    // \xff
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void Update()
        {
            try
            {
                labelX.Content = "x: " + _responseObj.msg.x;
                labelY.Content = "y: " + _responseObj.msg.y;
                labelTheta.Content = "t: " + _responseObj.msg.theta;
            }
            catch
            {
                labelX.Content = "x";
                labelY.Content = "y";
                labelTheta.Content = "t";
            }
        }

    }
}
