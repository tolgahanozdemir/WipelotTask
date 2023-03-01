using RabbitMQ.Client;
using System;

namespace WipelotTask.RabbitMQProducer
{
    public class Initial : IInitial
    {
        public void Publish(byte[] data)
        {
            //RabbitMQ bağlantısı tanımlanır.
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new("amqps://iulngoel:cvW8HhZ67gz3hs1XV3N_dBJBD0EUjNU0@goose.rmq2.cloudamqp.com/iulngoel");

            //Bağlantıyı ve kanalı oluşturur.
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            //Queue tanımlanır
            channel.QueueDeclare(queue: "listenerapp",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            //Mesajlar gönderilir.
            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: "listenerapp",
                                 basicProperties: null,
                                 body: data);
        }
    }
}
