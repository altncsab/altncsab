using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NineMensMorris
{

    public class TCPClient : IOpponent, IDisposable
    {
        private TcpClient client;
        public event MessageArrivedHandler MessageArrived;
        Thread ClientThread;
        Thread MainThread;
        string messageToSend;
        string messageReceived;
        object messageToSendLock = new object();
        object messageReceivedLock = new object();
        bool isRunning;
        bool isDisposing;
        public bool IsRunning { get {return isRunning; } }
        public TCPClient(string hostName)
        {
            ClientThread = new Thread(ListenerThread);
            MainThread = Thread.CurrentThread;
            isRunning = true;
            ClientThread.Start(hostName);

        }
        private string MessageToSend
        {
            get
            {
                lock (messageToSendLock)
                {
                    return messageToSend;
                }
            }
        }
        public bool IsMessage { get { return messageToSend != null; } }
        public void Stop()
        {
            if (IsMessage)
            {
                Thread.Sleep(100);
                messageToSend = null;
            }
            isRunning = false;
            ClientThread.Join(100);
        }

        public void SendMessage(string Message)
        {
            if (!isRunning) throw new Exception("Connection does not active");
            lock (messageToSendLock)
            {
                messageToSend = Message;
            }
        }
        private void SetMessageArrived(string Message)
        {
            lock (messageReceivedLock)
            {
                messageReceived = Message;
            }
            MessageArrived?.Invoke(this, new MessageArrivedEventArgs(messageReceived));
        }
        private void ListenerThread(object hostName)
        {
            try
            {
                while (isRunning)
                {
                    client = new TcpClient((string)hostName, 9999);
                    NetworkStream ns = client.GetStream();
                    if (client.Connected )
                    {
                        SetMessageArrived(Utils.MessageXML("Connected to Server"));
                    }
                    while (client.Connected )
                    {
                        if (!string.IsNullOrEmpty(MessageToSend))
                        {
                            string msg = MessageToSend;
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(msg);
                            ns.Write(buffer, 0, buffer.Length);
                            SendMessage(null);
                        }
                        if (ns.CanRead && ns.DataAvailable)
                        {
                            byte[] msg = new byte[1024];
                            StringBuilder completeMessage = new StringBuilder();
                            do
                            {
                                int numberofByte = ns.Read(msg, 0, msg.Length);
                                completeMessage.Append(Encoding.UTF8.GetString(msg, 0, numberofByte));
                            } while (ns.DataAvailable);                            
                            SetMessageArrived(completeMessage.ToString());
                        }
                        if (((int)MainThread.ThreadState & (int)ThreadState.Stopped) == (int)ThreadState.Stopped || !isRunning)
                        {
                            client.Close();
                            return;
                        }
                        Thread.Sleep(10);
                    }
                    client.Close();
                    if (((int)MainThread.ThreadState & (int)ThreadState.Stopped) == (int)ThreadState.Stopped)
                    {
                        client.Close();
                        return;
                    }
                }
            }
            catch(ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                MessageArrived?.Invoke(this, new MessageArrivedEventArgs(ex));
            }
            finally
            {
                client?.Close();
                isRunning = false;
            }

        }

        public void Dispose()
        {
            if (!isDisposing)
            {
                isDisposing = true;
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            isRunning = false;
            if (client != null)
            {
                client.Close();
            }
        }
    }
}
