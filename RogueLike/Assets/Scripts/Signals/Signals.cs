using System.Collections.Generic;

namespace Signals
{
    public class Sender<T>
    {
        public Reciever<T> Reciever { get; private set; }
        public T Data { get; private set; }
        public bool Connected { get; private set; }

        public Sender(Reciever<T> reciever, T data)
        {
            /*
                Returns a new Sender Object, Basically functions as a Cable that connects the Sender to the Reciever 

                The Sender may send over Data of any type

                The Reciever may receive the Data

                Arguments: 
                    Reciever 
                    Data

                Returns: 
                    Connection 
            */

            Reciever = reciever;
            Data = data;
            Connected = true;

            // Add this sender to the receiver's list of senders
            Reciever?.AddSender(this);
        }

        public void Disconnect()
        {
            if (!Connected)
            {
                return;
            }
            Reciever?.RemoveSender(this);
            Reciever = null;
            Connected = false;
        }

        public void TransferData()
        {
            if (Reciever != null && Connected)
            {
                Reciever.ReceiveData(Data);
            }
        }
    }

    public class Reciever<T>
    {
        public List<Sender<T>> Senders { get; private set; } = new List<Sender<T>>();
        public T ReceivedData { get; private set; }

        public Reciever()
        {
            // Empty constructor
        }

        public void AddSender(Sender<T> sender)
        {
            if (!Senders.Contains(sender))
            {
                Senders.Add(sender);
            }
        }

        public void RemoveSender(Sender<T> sender)
        {
            if (Senders.Contains(sender))
            {
                Senders.Remove(sender);
            }
        }

        public void ReceiveData(T data)
        {
            ReceivedData = data;
        }
    }
}