using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NineMensMorris
{
    public class MessageArrivedEventArgs : EventArgs
    {
        public MessageArrivedEventArgs(string message)
        {
            Message = message;
        }
        public MessageArrivedEventArgs(Exception ex)
        {
            Ex = ex;
        }
        public string Message { get; private set; }
        public Exception Ex {get; private set;}
    }
    public class TCPServer : IOpponent
    {
        TcpListener listener;
        Thread serverThread;
        Thread MainThread;
        string messageToSend;
        string messageReceived;
        object messageToSendLock = new object();
        object messageReceivedLock = new object();

        bool isRunning;
        public event MessageArrivedHandler MessageArrived;

        public TCPServer()
        {
            serverThread = new Thread(ListenerThread);
            MainThread = Thread.CurrentThread;
            isRunning = true;
            serverThread.Start();
        }
        public bool IsRunning { get { return isRunning; } }
        public void Stop()
        {
            if(IsMessage)
            {
                Thread.Sleep(100);
                messageToSend = null;
            }
            isRunning = false;
            serverThread.Join(100);
        }
        private string MessageToSend {
            get
            {
                lock (messageToSendLock)
                {
                    return messageToSend ;
                }
            }
        }
        public bool IsMessage { get {return messageToSend != null; } }
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
        private void ListenerThread()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, 9999);
                listener.Start();
                while (isRunning)
                {
                    if (listener.Pending())
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        NetworkStream ns = client.GetStream();
                        while (client.Connected)
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
                                listener.Stop();
                                return;
                            }
                            Thread.Sleep(10);
                        }
                        client.Close();
                    }
                    if (((int)MainThread.ThreadState & (int)ThreadState.Stopped) == (int) ThreadState.Stopped)
                    {
                        listener.Stop();
                        return;
                    }
                    Thread.Sleep(10);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                MessageArrived?.Invoke(this, new MessageArrivedEventArgs(ex));
            }
            finally
            {
                listener.Stop();
                isRunning = false;
            }
        }
    }
}
