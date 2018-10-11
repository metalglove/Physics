using Physics.Helpers;
using System;
using EV3WifiLib;
using System.Net;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;

namespace Physics.Services
{
    public class EV3TcpService : BindableBase, IService, IDisposable
    {
        private EV3Wifi _EV3Wifi;
        private ObservableCollection<string> _messages;
        private string _errorMessage;
        private volatile bool _isConnected = false;

        public EventHandler<string> MessagesChanged;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsConnected");
            }
        }

        public bool IsConnected => _isConnected;
        public ObservableCollection<string> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        public EV3TcpService()
        {
            _EV3Wifi = new EV3Wifi();
            _messages = new ObservableCollection<string>();
        }
        ~EV3TcpService()
        {
            Dispose();
        }

        public bool Connect(string ipAddress)
        {
            Disconnect();
            if (!IPAddress.TryParse(ipAddress, out IPAddress address))
            {
                ErrorMessage = "Fill in valid IP address of EV3";
            }
            else if (_EV3Wifi.Connect("1234", ipAddress))
            {
                // start listener  receivedMessagesListBox.Items.Clear();
                // start listener timer messageReceiveTimer.Start();
                _isConnected = true;
                Debug.WriteLine("Connected successfully!");
            }
            else
            {
                Disconnect();
                ErrorMessage = "Failed to connect to EV3 with IP address " + ipAddress;
            }
            RaisePropertyChanged("IsConnected");
            return IsConnected;
        }
        public void Disconnect()
        {
            if(_isConnected)
            {
                Thread.Sleep(100);
                _EV3Wifi.Disconnect();
                _isConnected = false;
                Debug.WriteLine("Disconnected successfully!");
            }
        }
        public void StartReceivingMessages()
        {
            while (_isConnected)
            {
                ReceiveMessage();
                Thread.Sleep(100);
            }
        }
        public void ReceiveMessage()
        {
            if (_isConnected)
            {
                // EV3: ReceiveMessage is asynchronous so it actually gets the message read at the previous call to ReceiveMessage
                //      and it triggers reading the next message from the specified mailbox.
                //      Due to the simple implementation the message read does not contain information of the mailbox it came from.
                //      Therefore the advise is to only use one mailbox to read from: EV3_OUTBOX0.
                string strMessage = _EV3Wifi.ReceiveMessage("EV3_OUTBOX0");
                if (strMessage != "" && strMessage != "??" && strMessage != "?" && strMessage != "m")
                {
                    Messages.Add(strMessage);
                    MessagesChanged.Invoke(this, strMessage);
                    Debug.WriteLine($"[{_messages.Count}] {strMessage}");
                }
            }
        }
        public bool SendMessage(string message)
        {
            if (_isConnected)
            {
                _EV3Wifi.SendMessage(message, "0");
            }
            else
            {
                ErrorMessage = "The EV3 is not connected.";
            }
            return IsConnected;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
