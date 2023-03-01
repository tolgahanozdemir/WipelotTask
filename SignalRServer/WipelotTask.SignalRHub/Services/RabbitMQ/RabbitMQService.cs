using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using WipelotTask.SignalRHub.Entities;
using System.Text.RegularExpressions;
using WipelotTask.SignalRHub.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace WipelotTask.SignalRHub.Services.RabbitMQ
{
    public class RabbitMQService : IRabbitMQService
    {
        protected readonly ConnectionFactory _factory;
        protected readonly IConnection _connection;
        protected readonly IModel _channel;

        protected readonly IServiceProvider _serviceProvider;

        public RabbitMQService(IServiceProvider serviceProvider)
        {
            // RabbitMQ bağlantısını açar.
            _factory = new ConnectionFactory();
            _factory.Uri = new("amqps://iulngoel:cvW8HhZ67gz3hs1XV3N_dBJBD0EUjNU0@goose.rmq2.cloudamqp.com/iulngoel");
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();

            _serviceProvider = serviceProvider;
        }

        public virtual void Connect()
        {
            // RabbitMQ Queue bilgileri girilir.
            _channel.QueueDeclare(queue: "listenerapp", durable: false, exclusive: false, autoDelete: false);

            var consumer = new EventingBasicConsumer(_channel);

            // SignalR'dan mesaj alınınca;
            consumer.Received += delegate (object model, BasicDeliverEventArgs ea)
            {
                //WipelotHub isteğini alır.
                var wipelotHub = (IHubContext<WipelotHub>)_serviceProvider.GetService(typeof(IHubContext<WipelotHub>));
                var data = Encoding.ASCII.GetString(ea.Body.Span);
                var splittedData = data.Split("/");
                if (splittedData.Length == 3)
                {
                    //Gelen mesajı ClientSocket nesnesine dönüştürüp tüm client'lara gönderir.
                    var uniqueNumber = Convert.ToInt32(Regex.Match(splittedData[0], @"\d+").Value);
                    var timeStamp = Convert.ToInt32(Regex.Match(splittedData[1], @"\d+").Value);
                    var randomValue = Convert.ToInt32(Regex.Match(splittedData[2], @"\d+").Value);
                    ClientSocket clientSocket = new ClientSocket(uniqueNumber, timeStamp, randomValue);

                    wipelotHub.Clients.All.SendAsync("messageReceived", clientSocket);
                }

            };

            _channel.BasicConsume(queue: "listenerapp", autoAck: true, consumer: consumer);
        }
    }
}
