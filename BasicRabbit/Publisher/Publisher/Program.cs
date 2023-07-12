// See https://aka.ms/new-console-template for more information

using RabbitMQ.Client;
using System.Text;

string queueName = "rabbit_poc";

try
{
    var factory = new ConnectionFactory() { 
        HostName = "localhost"
    };



    using var connection = factory.CreateConnection();

    using var chanel = connection.CreateModel();

    chanel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    chanel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: Encoding.UTF8.GetBytes("primeira mensagem"));

}
catch (Exception ex)
{

    Console.WriteLine("error: " + ex.Message);
}