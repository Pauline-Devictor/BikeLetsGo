using System;
using System.Collections.Generic;
using System.Linq;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace ServerBike2
{
    public class ActiveMQService
    {

        private Uri connecturi = new Uri("activemq:tcp://localhost:61616");
        private ConnectionFactory connectionFactory;
        private IConnection connection;

        //Create Constructor so BikeService will only use 1 instance of ActiveMQService
        public ActiveMQService()
        {
            // Create a Connection Factory
            connectionFactory = new ConnectionFactory(connecturi);
            // Create a single Connection from the Connection Factory.
            connection = connectionFactory.CreateConnection();
            connection.Start();
        }

        //Send the message to the correct person using the queueName
        public void SendMessage(string queueName, string msg)
        {
            char[] delims = new[] { '\r', '\n' };
            List<string> messageToSend = msg.Split(delims, StringSplitOptions.RemoveEmptyEntries).ToList();
            ISession session = createSession(queueName, out IMessageProducer producer);

            // Finally, to send messages:
            //For each part of the message, create a ITextMessage so that the producer can send it to the client ! 
            foreach (ITextMessage message in messageToSend.Select(serializedMessage => session.CreateTextMessage(serializedMessage)))
                producer.Send(message);

            // Don't forget to close your session and connection when finished.
            session.Close();
            connection.Close();
        }


        private ISession createSession(string queueName, out IMessageProducer producer)
        {
            // Create a session from the Connection.
            ISession session = connection.CreateSession();
            // Use the session to target a queue.
            IDestination destination = session.GetQueue(queueName);
            // Create a Producer targetting the selected queue.
            producer = session.CreateProducer(destination);
            //To avoid to stock every message on the server because default mode is Persistent
            producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
            return session;
        }
    }
}