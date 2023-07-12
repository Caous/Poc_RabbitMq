
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string queueName = "rabbit_poc";

try
{
    var factory = new ConnectionFactory()
    {
        HostName = "localhost"
    };

    using var connection = factory.CreateConnection();

    using var chanel = connection.CreateModel();

    chanel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

    var consumer = new EventingBasicConsumer(chanel);

    consumer.Received += (sender, args) =>
    {
        var body = args.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($"Mensagem recebida: {message}");

    };

    chanel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

    Console.ReadLine();
}
catch (Exception ex)
{

    Console.WriteLine("error: " + ex.Message);
}