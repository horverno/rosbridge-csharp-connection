using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    public partial class MainWindow : Window, RosBridgeUtility.IROSBridgeController
    {
        RosBridgeUtility.RosBridgeLogic bridgeLogic;
        RosBridgeUtility.RosBridgeConfig bridgeConfig;


        private RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse _responseObj = new RosBridgeDotNet.RosBridgeDotNet.TurtlePoseResponse();
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        enum ConnectionState
        {
            Disconnected = 0, Connected
        }
        int connectionState;
        enum SubscriptionState
        {
            Unsubscribed = 0, Subscribed
        }
        int subscriptionState;
       
        public MainWindow()
        {
            InitializeComponent();
            stackControls.Visibility = System.Windows.Visibility.Hidden;
            this.DataContext = _responseObj;
            this.bridgeLogic = new RosBridgeUtility.RosBridgeLogic();
            this.connectionState = (int)ConnectionState.Disconnected;
            this.subscriptionState = (int)SubscriptionState.Unsubscribed;
            bridgeLogic.setSubject(this);
            this.bridgeConfig = new RosBridgeUtility.RosBridgeConfig();
            bridgeConfig.readConfig("XMLFile1.xml");
        }
        

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
     
            if (connectionState==(int)(ConnectionState.Disconnected))
            {
                if (ConnectToRos())
                {
                    connectionState = (int)ConnectionState.Connected;
                    btnConnect.Background = Brushes.LightGreen;
                    btnConnect.Content = "Disconnect";
                    stackControls.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                DisconnectFromRos();
                connectionState = (int)ConnectionState.Disconnected;
                btnConnect.Background = Brushes.OrangeRed;
                btnConnect.Content = "Connect";
                stackControls.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void btnSubscribe_Click(object sender, RoutedEventArgs e)
        {
            if (subscriptionState == (int)SubscriptionState.Unsubscribed)
            {
                try
                {
                    foreach (var item in bridgeConfig.getTopicList())
                    {
                        bridgeLogic.sendSubscription(item.name,item.throttle);
                    }
                    subscriptionState = (int)SubscriptionState.Subscribed;
                    btnSubscribe.Content = "Unsubscribe";
                    bridgeLogic.SetUpdateListener();
                }
                catch (Exception se)
                {
                    MessageBox.Show(se.Message);
                }
            }
            else
            {
                try
                {
                    foreach (var item in bridgeConfig.getTopicList())
                    {
                        bridgeLogic.sendUnsubscribe(item.name);
                    }
                    //bridgeLogic.sendUnsubscribe("/turtle1/pose");
                    subscriptionState = (int)SubscriptionState.Unsubscribed;
                    btnSubscribe.Content = "Subscribe";
                }
                catch (Exception se)
                {
                    MessageBox.Show(se.Message);
                }
            }
        }

        private double convertTextBlocktoRadians()
        {
            double deg = Double.Parse(txtTheta.Text);
            return deg * Math.PI / 180.0;
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(Double.Parse(txtLgth.Text), 0);
        }

        private void btnBackward_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(-Double.Parse(txtLgth.Text), 0);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {

            PublishturtleMessage(Double.Parse(txtLgth.Text), convertTextBlocktoRadians());
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            PublishturtleMessage(Double.Parse(txtLgth.Text), -convertTextBlocktoRadians());
            //Update();
        }
        /// <summary>
        /// Return true if connection was succesful
        /// </summary>
        /// <returns></returns>
        /// 

        

        bool ConnectToRos()
        {
            if (txtIP.Text == "" || txtPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return false;
            }
            
            try
            {
                //bridgeLogic.Initialize("ws://" + txtIP.Text + ":" + txtPort.Text, this);
                bridgeLogic.Initialize(bridgeConfig.URI);
                bridgeLogic.Connect();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return false;
            }
        }
        void DisconnectFromRos()
        {
            bridgeLogic.Disconnect();
        }
                
        private void PublishturtleMessage(double linear, double angular)
        {
            try
            {
                var v = new { linear = linear, angular = angular };
                Object[] lin = { linear, 0.0, 0.0 };
                Object[] ang = { 0.0, 0.0, angular };
                foreach (var item in bridgeConfig.getPublicationList())
                {
                    bridgeLogic.PublishTwistMsg(item, lin, ang);
                }
                
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public delegate void UpdateTextElements(String data);

        public void ReceiveUpdate(String data)
        {
            JObject jsonData = JObject.Parse(data);
            Dispatcher.Invoke(new Action(() => labelX.Content = "x: " + jsonData["msg"][bridgeConfig.ProjectedAttributes()[0].Item2]));
            Dispatcher.Invoke(new Action(() => labelY.Content = "y: " + jsonData["msg"][bridgeConfig.ProjectedAttributes()[1].Item2]));
            Dispatcher.Invoke(new Action(() => labelTheta.Content = "t: " + jsonData["msg"][bridgeConfig.ProjectedAttributes()[2].Item2] +
                " (Deg: " + Math.Round((float)jsonData["msg"][bridgeConfig.ProjectedAttributes()[0].Item2] 
                * 180 / Math.PI, 4).ToString() + ")"));
        }
        

    }
}
