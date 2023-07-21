namespace Publisher.Application;

public static class PublishChanel
{

    public static void Publisher()
    {

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

            Console.WriteLine("Por favor digite a mensagem");
            string mensagem = Console.ReadLine();

            chanel.BasicPublish(exchange: "",
                                                     routingKey: queueName,
                                                     basicProperties: null,
                                                     body: Encoding.UTF8.GetBytes(mensagem));

        }
        catch (Exception ex)
        {

            Console.WriteLine("error: " + ex.Message);
        }
    }


}
