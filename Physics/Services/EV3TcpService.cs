using Physics.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EV3WifiLib;
using System.Net;
using Physics.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Physics.Services
{
    public class EV3TcpService : BindableBase, IService
    {
        private EV3Wifi _EV3Wifi;
        private ObservableCollection<EV3Message> _messages;
        private string _errorMessage;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                RaisePropertyChanged();
            }
        }

        public bool IsConnected => _EV3Wifi.isConnected;
        public ObservableCollection<EV3Message> Messages
        {
            get => _messages ?? (_messages = new ObservableCollection<EV3Message>());
            set
            {
                _messages = value;
                RaisePropertyChanged();
            }
        }

        public EV3TcpService()
        {
            _EV3Wifi = new EV3Wifi();
        }

        public bool Connect(string ipAddress)
        {
            _EV3Wifi.Disconnect(); //
            if (!IPAddress.TryParse(ipAddress, out IPAddress address))
            {
                ErrorMessage = "Fill in valid IP address of EV3";
            }
            else if (_EV3Wifi.Connect("1234", ipAddress))
            {

                // start listener  receivedMessagesListBox.Items.Clear();
                // start listener timer messageReceiveTimer.Start();

                
            }
            else
            {
                _EV3Wifi.Disconnect();
                ErrorMessage = "Failed to connect to EV3 with IP address " + ipAddress;
            }
            RaisePropertyChanged("IsConnected");
            return IsConnected;
        }
        public void Disconnect()
        {
            _EV3Wifi.Disconnect();
        }
        public void StartReceivingMessages()
        {
            while (IsConnected)
            {
                ReceiveMessage();
            }
        }
        public void ReceiveMessage()
        {
            if (_EV3Wifi.isConnected)
            {
                // EV3: ReceiveMessage is asynchronous so it actually gets the message read at the previous call to ReceiveMessage
                //      and it triggers reading the next message from the specified mailbox.
                //      Due to the simple implementation the message read does not contain information of the mailbox it came from.
                //      Therefore the advise is to only use one mailbox to read from: EV3_OUTBOX0.
                string strMessage = _EV3Wifi.ReceiveMessage("EV3_OUTBOX0");
                if (strMessage != "")
                {
                    string[] data = strMessage.Split(' ');
                    if (data.Length == 2)
                    {
                        Messages.Add(new EV3Message(data[0], data[1]));
                    }
                }
            }
        }
        public bool SendMessage(string message)
        {
            if (_EV3Wifi.isConnected)
            {
                _EV3Wifi.SendMessage(message, "0");
            }
            else
            {
                ErrorMessage = "The EV3 is not connected.";
            }
            return IsConnected;
        }
    }
}
