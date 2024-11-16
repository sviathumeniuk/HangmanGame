using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Hangman.Services
{
    public class RabbitMQPublisher
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private readonly string _queueName;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _hostName = configuration["RabbitMQ:Host"];
            _username = configuration["RabbitMQ:Username"];
            _password = configuration["RabbitMQ:Password"];
            _queueName = configuration["RabbitMQ:QueueName"];
        }

        public void PublishRatingUpdate(int userId, bool isWin, int difficulty)
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _username, Password = _password };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var message = new
            {
                UserId = userId,
                IsWin = isWin,
                Difficulty = difficulty
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: _queueName,
                                 basicProperties: properties,
                                 body: body);
        }
    }
}