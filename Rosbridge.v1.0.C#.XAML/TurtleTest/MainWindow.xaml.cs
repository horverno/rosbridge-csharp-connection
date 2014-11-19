using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public String _pureResponse = "";
        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[1];
        }
        public MainWindow()
        {
            InitializeComponent();
            stackControls.Visibility = System.Windows.Visibility.Hidden;
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
               }
               else
               {
                   rosSubscMsg = new RosBridgeDotNet.RosBridgeDotNet.SubscribeMessage("/rosbridge/unsubscribe", poseSubscribe);
               }
               string needToSend = JsonConvert.SerializeObject(u);
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


        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(1, 0);
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(-1, 0);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(0, 1);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(0, -1);
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
                System.String szData = new System.String(chars);
                try
                {
                    //RetrieveReceivedEventData(pureResponse);
                    szData = Regex.Replace(szData, @"[^\u0000-\u007F]", string.Empty);
                    System.Diagnostics.Debug.WriteLine(szData.ToString() + " ");
                    //richTextRxMessage.Text = richTextRxMessage.Text + "a";

                }
                catch (Exception e)
                {
                    //MessageBox.Show(e.ToString());
                }

                //If we finished receiving the response (EventData object) from the Broker
                //then we will recover it
                /*if (richTextRxMessage.Text.Contains("ENDOFMESSAGE"))
                {
                    //Remove the flag "ENDOFMESSAGE"
                    String pureResponse = richTextRxMessage.Text
                        .Remove(richTextRxMessage.Text.IndexOf("ENDOFMESSAGE"), 12);
                    RetrieveReceivedEventData(pureResponse);

                    richTextBoxHistoryResponses.Text += "\n \\\\\\\\\\\\ \n Response : " + pureResponse;
                    richTextRxMessage.Text = "";
                }*/

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
    }
}
