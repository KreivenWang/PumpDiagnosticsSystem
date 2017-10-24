using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace PumpDiagnosticsSystem.Business
{
    class MessageProducer
    {
        private static readonly ConnectionFactory _factory = new ConnectionFactory() {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 6012
        };

        public MessageProducer()
        {
            
        }

        public static void Send(string msg)
        {
            //return; //暂时不用
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare(queue: "Graph", durable: false, exclusive: false, autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(msg);

                channel.BasicPublish(exchange: "", routingKey: "Graph", basicProperties: null, body: body);
                //Console.WriteLine(" [x] Sent {0}", msg);
            }
        }
    }
}
