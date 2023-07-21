namespace WorkerConsumer.Application.Service;

public class Consumer : IConsumer
{
    public Consumer()
    {
           
    }
    string queueName = "rabbit_poc";
    public Task ConsumerChannel()
    {
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

        return Task.CompletedTask;
    }
}
