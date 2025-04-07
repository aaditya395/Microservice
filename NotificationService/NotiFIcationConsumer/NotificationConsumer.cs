using RabbitMQ.Client;
using System.Net.Mail;
using System.Net;
using RabbitMQ.Client.Events;
using NotificationService.Models;
using System.Text;
using System.Text.Json;
using UserService.DatabaseContext;

namespace NotificationService.NotiFIcationConsumer
{
    public class NotificationConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConnectionFactory _factory;

        public NotificationConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _factory = new ConnectionFactory() { HostName = "rabbitmq" };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "notificationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<Notification>(message);

                Console.WriteLine($"Received Notification: {notification.Message}");

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<NotificationContext>();

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                SendEmail(notification.Email, "Leave Notification", notification.Message);
            };

            channel.BasicConsume(queue: "notificationQueue", autoAck: true, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient("smtp.example.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("test@example.com", "test1"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage("test@example.com", toEmail, subject, body);
            smtp.Send(mailMessage);
        }
    }
}
