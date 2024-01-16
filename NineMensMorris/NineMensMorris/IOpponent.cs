using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NineMensMorris
{
    public delegate void MessageArrivedHandler(object sender, MessageArrivedEventArgs e);
    public interface IOpponent
    {
        bool IsRunning { get; }
        event MessageArrivedHandler MessageArrived;
        void Stop();
        void SendMessage(string Message);
        bool IsMessage { get; }
    }
}
