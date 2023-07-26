# RabbitMq

Projeto com objetivo em mostrar a implementação do RabbitMq, uma ferramenta muito utilizada para cache mas também com outros recursos que você não sabia.

![RabbitMq](https://www.luisdev.com.br/wp-content/uploads/2021/01/mesageria-service-bus-1.png) </br><p>Imagem de direito autoral do <a href="https://www.luisdev.com.br" target="_blank"> Luis Dev </a> </p>



### <h2>Fala Dev, seja muito bem-vindo
   Está POC é para mostrar como podemos implementar o <b>RabbitMq</b> em diversos projetos, com adaptação para o cenário que você precisa, também te explico <b>o que é o RabbitMq</b> e como usar. Espero que encontre o que procura. <img src="https://media.giphy.com/media/WUlplcMpOCEmTGBtBW/giphy.gif" width="30"> 
</em></p></h5>
  
  </br>
  


<img align="right" src="https://blog.zenika.com/wp-content/uploads/2012/03/RabbitMQ-1.jpg" width="450" height="375"/>


</br></br>

### <h2>RabbitMq <a href="https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html" target="_blank"><img alt="RabbitMq" src="https://img.shields.io/badge/rabbitmq-blue?style=flat&logo=google-chrome"></a>

 <a href="https://www.rabbitmq.com/" target="_blank">RabbitMq </a> um Message Broker você deve ter ouvido esta palavra em algum lugar, caso nunca tenha ouvido, também não tem problema porque vou te explicar agora. Um Message Broker é responsável por controlar e reter todas as mensagens que trafegam entre aplicações, normalmente usados para comunicação entre microsserviços e filas. Desta forma ele recebe uma mensagem, retem e qualquer aplicação que tiver interesse consome a mensagem. Normlamente existe uma estrutura de fila que se baseia em alguns pilares, vamos abordar como são esses pilares para o RabbitMq.</b>.
 
<b>Objetivo:</b> O RabbitMq foi criado para suporta o trafego de mensagem entre linguagens, sendo assim ele utiliza o protocolo de comunicação AMQP, MQTT, STOMP,HTTP. Para conseguir transportar e enviar mensagens de um sistema o outro. Mas não se engane o RabbitMq apenas retém as mensagens ele não envia ou vai buscar mensagem.
  
  
**Linguagem:** Erlang.

**Mensageria:** Enfileiramento de mensagens de forma assync o conceito do Rabbit é muito semelhante a fila utilizado em estrutura de dados, o padrão que o RabbitMq pode utilizar é publish/subcribe (pub/sub) mas por ser um menssage broker ele já detém um maneira de fila bem **resiliente**.

**Pilares:** 

- **Publish**
Publish sempre será aquele que vai enviar a mensagem para a Exchange, onde ele notifica e depois disso não precisa se preocupar com o resto para frente, mas o fluxo é Publish publica a mensagem vai para a exchange, da exchange para o Queue e depois o Consumer lê a mensagem.
- **Queue**
São as filas que vão ter as mensagens, eles são responsáveis por receber as mensagens e deixar elas disponíveis.
- **Consumer**
O Consumer é responsável por ficar ouvindo o Queue e sempre que aparecer uma mensagem no qual o Consumer está inscrito ele vai consumir a mensagem.
- **Exchange**
A exchange sempre será responsável por enviar os dados para uma fila quando estamos falando de mensage broker, temos 4 tipos de exchange, utilizando atributos de cabeçalho, routing keys, ou bindings.
**Direct:** Ele é responsável por enviar a mensagem para todas as filas, uma diferenciação é que através de Routing Key o exchange vai saber para onde enviar a mensagem, para qual fila.
**Fanout:** O Fanout é um tipo de fila que ao receber a mensagem vai enviar a mensagem para todas as filas.
**Topic:** Normalmente uma Exchange mais flexível, quando temos uma mensagem com algum tipo de padrão da Router Key, conseguimos fazer regras para enviar de forma flexível as mensagens, podendo enviar para N filas ou específica.
**Headers**
   
Legal né? Mas agora a pergunta é como posso usar o RabbitMq? Abaixo dou um exemplo de caso de uso.

</br></br>

### <h2>[Cenário de Uso]
Vamos imaginar o seguinte cenário, você tem uma API Rest <b>que precisa notificar</b> uma aplicação, mas o volume é enorme que chegará nesta outra aplicação, então para não termos o risco de sobrecarregar a outra aplicação com o volume de solicitação, então vamos publicar em uma fila as notificações e deixar disponível. Para a outra aplicação processar conforme ela pegar no canal.

   
## <h2> Primeiro passo vamos colocar o RabbitMq em um container criando o usuário
   
   Execute o comando abaixo no seu promt de comando
   
   ````
   $ docker run -d --hostname my-rabbit --name some-rabbit -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=password -p 8080:15672 rabbitmq:3-management
   ````
   
   Agora execute o RabbitMq e execute através da interface do Docker hub
   
   ### <h2> Agora vamos criar o Projeto Console.Application para postar a mensagem na fila

   Primeiro vamos criar a classe que se conecta na fila e em seguida posta na fila
   ```C#
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

            Console.WriteLine("Por favor digite a mensagem: ");

            chanel.BasicPublish(exchange: "",
                                                     routingKey: queueName,
                                                     basicProperties: null,
                                                     body: Encoding.UTF8.GetBytes(Console.ReadLine()));

        }
        catch (Exception ex)
        {

            Console.WriteLine("error: " + ex.Message);
        }
    }


}
   ```
   Vamos configurar o Program.Cs para executar essa classe

   ```C#
   PublishChanel.Publisher();
   ```
### <h2> Criação o projeto Consumer que irá consumir as mensagens postadas na fila, esse projeto é um WorkService

Vamos primeiro criar a interface para utilizarmos via injeção de dependência.
```C#
public interface IConsumer
{
    Task ConsumerChannel();
}

```

Próxima etapa será implementar está interface
```C#

public class Consumer : IConsumer
{
    public Consumer()
    {
           
    }
    const string queueName = "rabbit_poc";
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

        }
        catch (Exception ex)
        {

            Console.WriteLine("error: " + ex.Message);
        }

        return Task.CompletedTask;
    }
}

```
</br>

Vamos agora configurar nossa classe Worker.cs para chamar o método de consumir mensagem na fila
```C#
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConsumer _consumer;

    public Worker(ILogger<Worker> logger, IConsumer consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await _consumer.ConsumerChannel();
            await Task.Delay(1000, stoppingToken);
        }
    }
```
   E por ultimo vamos injetar nosso consumer no Program.cs
   
```C#


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>().AddSingleton<IConsumer, Consumer>();
    })
    .Build();

host.Run();
```
   

### <h5> Agora Dev basta apenas executar o Docker e seus projetos, espero que eu tenha ajudado, nos vemos por aí
   
   
### <h5> [IDE Utilizada]</h5>
![VisualStudio](https://img.shields.io/badge/Visual_Studio_2022-000000?style=for-the-badge&logo=visual%20studio&logoColor=purple)

### <h5> [Linguagem Programação Utilizada]</h5>
![C#](https://img.shields.io/badge/C%23-000000?style=for-the-badge&logo=c-sharp&logoColor=purple)

### <h5> [Versionamento de projeto] </h5>
![Github](http://img.shields.io/badge/-Github-000000?style=for-the-badge&logo=Github&logoColor=green)

</br></br></br></br>


<p align="center">
  <i>🤝🏻 Vamos nos conectar!</i>

  <p align="center">
    <a href="https://www.linkedin.com/in/gusta-nascimento/" alt="Linkedin"><img src="https://github.com/nitish-awasthi/nitish-awasthi/blob/master/174857.png" height="30" width="30"></a>
    <a href="https://www.instagram.com/gusta.nascimento/" alt="Instagram"><img src="https://github.com/nitish-awasthi/nitish-awasthi/blob/master/instagram-logo-png-transparent-background-hd-3.png" height="30" width="30"></a>
    <a href="mailto:caous.g@gmail.com" alt="E-mail"><img src="https://github.com/nitish-awasthi/nitish-awasthi/blob/master/gmail-512.webp" height="30" width="30"></a>   
  </p>
