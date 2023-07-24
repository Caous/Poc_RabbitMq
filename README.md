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
   
   ### <h2> Criando nosso appsetings.json
   
   ```Json
   {
      "Logging": {
         "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
         }
      },
  "AllowedHosts": "*",
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Chanel": "customer-redis"
      }
   }

   ```
   
   
### <h2> Criação de Classes

Vamos criar a classe que será responsável por conectar ao canal e configurar o cache.
```C#
public interface ICachingService
{
    Task SetAsync(string Key, string value);
    Task<string> GetAsync(string Key);
}
```

Próxima etapa será implementar está interface
```C#

public class CachingService : ICachingService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _options;

    public CachingService(IDistributedCache cache)
    {
        _cache = cache;
        _options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(120),
            SlidingExpiration = TimeSpan.FromSeconds(120)
        };
    }
    public async Task<string> GetAsync(string key)
    {
        return await _cache.GetStringAsync(key);
    }

    public async Task SetAsync(string key, string value)
    {
        await _cache.SetStringAsync(key, value, _options);
    }
}

```
</br>

Vamos também aproveitar e criar nossa classe que irá fazer o Publish no canal
```C#
using StackExchange.Redis;

namespace PocWebCacheRedis.Infrastructure.Publish;
public class PublishCustomer
{
    private readonly IConfiguration _configuration;
    private static ConnectionMultiplexer _connection;
    public PublishCustomer(IConfiguration configuration)
    {
        _configuration = configuration;
        _connection = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"] ?? string.Empty);
    }

    public async void PublishChannel(Customer customer) =>  await _connection.GetSubscriber().PublishAsync(_configuration["Redis:Chanel"] ?? string.Empty, $"Cliente cadastro com sucesso {customer.Name}", CommandFlags.HighPriority);

}
```
   Também não podemos nos esquecer de configurar no arquivo service, que será chamado pela controller todo nosso serviço do redis
   
```C#

 public class CustomerService
 {
    private readonly IRepository<Customer> _customerRepository;
    private readonly IMapper _mapper;
    private readonly ICachingService _cache;
    private readonly IConfiguration _configuration;
    private string _cacheKey = "customer_";

    public CustomerService(IRepository<Customer> customerRepository, IMapper mapper, ICachingService cache, IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<CustomerDto> RegisterCustomer(CustomerDto customer)
    {
        var customerSave = await _customerRepository.Save(_mapper.Map<Customer>(customer));
        await _cache.SetAsync(_cacheKey + customerSave, JsonSerializer.Serialize(customer));
        new PublishCustomer(_configuration).PublishChannel(_mapper.Map<Customer>(customer));
        return customer;
    }

    public async Task<CustomerDto?> GetCustomerDto(CustomerDto customer)
    {
        var customerCache = await _cache.GetAsync(_cacheKey + customer.Id);

        if (customerCache != null)
            return JsonSerializer.Deserialize<CustomerDto>(customerCache);
        else
        {
            var result = await _customerRepository.Get(customer.Id);
            if (result != null)
            {
                await _cache.SetAsync(_cacheKey + result.Id, JsonSerializer.Serialize(customer));
                return _mapper.Map<CustomerDto>(result);
            }
        }
        return null;
    }

    public async Task<List<CustomerDto>?> GetAllCustomers()
    {
        var customerCache = await _cache.GetAsync(_cacheKey + "all");

        if (customerCache != null)
            return JsonSerializer.Deserialize<List<CustomerDto>>(customerCache);

        var result = await _customerRepository.GetAll();

        if (result != null)
        {
            await _cache.SetAsync(_cacheKey + "all", JsonSerializer.Serialize(result));
            return _mapper.Map<List<CustomerDto>>(result.ToList());
        }

        return null;
    }

    public async Task EditCustomer(CustomerDto customer) => await _customerRepository.Edit(_mapper.Map<Customer>(customer));

    public async Task DeleteCustomer(CustomerDto customer) => await _customerRepository.Delete(customer.Id);
 }
```
   
Agora vamos configurar nossa controller
   
````C#
[Route("api/[controller]")]
[ApiController]
public class CustomerController : ControllerBase
{
    private CustomerService _customerService;

    public CustomerController(IRepository<Customer> repositoryCustomer, IMapper mapper, ICachingService cache, IConfiguration configuration)
    {
        _customerService = new CustomerService(repositoryCustomer, mapper, cache, configuration);        
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _customerService.GetAllCustomers());
    }

    // GET api/<ValuesController>/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await _customerService.GetCustomerDto(new CustomerDto() { Id = id }));
    }

    // POST api/<ValuesController>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerDto customer)
    {
        return Ok(await _customerService.RegisterCustomer(customer));
    }

    // PUT api/<ValuesController>/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromBody] CustomerDto value)
    {
        await _customerService.EditCustomer(value);
        return Ok("Edited");
    }

    // DELETE api/<ValuesController>/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _customerService.DeleteCustomer(new CustomerDto() { Id = id });
        return Ok("Deleted");
    }
}

````

Por ultimo vamos criar um projeto console em .net 7 e implementar a parte de ouvir o canal e sempre que alguém colocar alguma coisa nele, nosso projeto reproduzir a informação e manipular como bem entender
   
Program.cs   
````C#

using StackExchange.Redis;

string RedisConnectionString = "localhost:6379";

ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(RedisConnectionString);

string Channel = "customer-redis";

Console.WriteLine("Ouvindo o canal");

connection.GetSubscriber().Subscribe(Channel, (channel, message) => Console.WriteLine("Usuario recebido no canal mensagem: " + message));

Console.ReadLine();

````
   
   
### <h5> [IDE Utilizada]</h5>
![VisualStudio](https://img.shields.io/badge/Visual_Studio_2019-000000?style=for-the-badge&logo=visual%20studio&logoColor=purple)

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
