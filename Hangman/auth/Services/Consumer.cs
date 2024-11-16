using System.Text;
using System.Text.Json;
using Auth.Data;
using Auth.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Auth.Services
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly string _hostName;
        private readonly string _username;
        private readonly string _password;
        private readonly string _queueName;
        private readonly IServiceScopeFactory _scopeFactory;

        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _hostName = configuration["RabbitMQ:Host"];
            _username = configuration["RabbitMQ:Username"];
            _password = configuration["RabbitMQ:Password"];
            _queueName = configuration["RabbitMQ:QueueName"];
            _scopeFactory = scopeFactory;

            InitializeRabbitMQListener();
        }

        private void InitializeRabbitMQListener()
        {
            var factory = new ConnectionFactory() { HostName = _hostName, UserName = _username, Password = _password };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<RatingUpdateMessage>(content);

                await HandleMessageAsync(message);

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(RatingUpdateMessage message)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await context.Users.FindAsync(message.UserId);
            if (user != null)
            {
                int difficultyMultiplier = message.Difficulty;
                if (message.IsWin)
                {
                    user.Rating += 4 * difficultyMultiplier;
                }
                else
                {
                    user.Rating = Math.Max(0, user.Rating - 4 * difficultyMultiplier);
                }

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
        }
    }
}