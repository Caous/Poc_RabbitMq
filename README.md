# RabbitMq

Projeto com objetivo em mostrar a implementação do Redis, uma ferramenta muito utilizada para cache mas também com outros recursos que você não sabia.

![MediatR](https://linuxiac.b-cdn.net/wp-content/uploads/2021/06/redis-how-it-works.png)



### <h2>Fala Dev, seja muito bem-vindo
   Está POC é para mostrar como podemos implementar o <b>Redis</b> em diversos projetos, com adaptação para o cenário que você precisa, juntamente mostrando outros serviços dentro do próprio Redis, também te explico <b>o que é o Redis</b> e como usar em diversas situações. Espero que encontre o que procura. <img src="https://media.giphy.com/media/WUlplcMpOCEmTGBtBW/giphy.gif" width="30"> 
</em></p></h5>
  
  </br>
  


<img align="right" src="https://redis.com/wp-content/uploads/2023/04/mkt-13879-caching-framework-diagrams-RI-CDC-01.png" width="500" height="400"/>


</br></br>

### <h2>Redis <a href="https://redis.io/docs/" target="_blank"><img alt="Redis" src="https://img.shields.io/badge/Redis-blue?style=flat&logo=google-chrome"></a>

 <a href="https://redis.io/" target="_blank">Redis </a> armazenamento de dados em memória, de código aberto usado por milhões de desenvolvedores, cache, mecanismo de streaming, Menssage Broker e muito mais. Redis é uma ótima alternativa para diversos projetos, trabalhar com cache é uma necessidade de diversos sistemas e hoje uma das ferramentas mais queridas do mercado, utilizada para fazer cache distribuido principalmente para aplicações com grande escalonamento por exemplo Micro Serviço, o <b>redis normalmente é usado para manipular o Cache nesse tipo de sistema</b>.
 
<b>Objetivo:</b> uma ferramenta Open Source, ou seja, uma ferramenta de código-fonte aberto, popularmente conhecido como uma ferramenta de <b>cache em memória</b>, com diversos recursos para salvar dados temporariamente, ele se destaca por manter seus dados de cache armazenados em momória, diferente de muitos outros que é no disco rígido, o Redis também contem alguns serviços inclusos que algumas pessoas desconhecem, sendo muito útil para outras funções como:
   <br>
   <b>Menssage Broke</b><br>
   <b>Submit/Publish</b><br>
   <b>Transactions</b>.
   
   Vamos <b>pontuar apenas dois tópicos dos serviços que o Redis oferece, Pub/Sub e Cache</b>. Vamos focar no objetivo de cada um, com sua vantagem e desvantagem.
  
  
   <b>[Cache]</b> Redis consegue armanezar os dados em memória possibilitando que você consiga ter acesso as informações de forma mais rápida, por sua estrutura ser <b>cache distribuídos</b> outras aplicações podem acessar a mesma informação, muito utilizado hoje em dia em <b>aplicações que estão em containers com docker e kubernets</b>, onde temos diversas instâncias virtuais daquele servidor, também dependendo da sua utilização é possível tirar <b>snapshots para armazenar os dados em disco</b>. Este tipo de cache pode melhorar a performance da aplicação, além de facilitar a sua escalabilidade. Quando se trabalha com cache distribuído, o dado:

 - É coerente (consistente) entre as requisições pelos vários servidores da aplicação;
 - Não se perde se a aplicação reiniciar;
 - Memorial local é configurável.
 - Velocidade de resposta
 - Configurações que permitem você deixar o cachê dinâmico para armezar (Tempo de armazenamento, snapshots, objetos duplicados...)
  
   
<b>[Pub/Sub]</b> SUBSCRIBE, UNSUBSCRIBE e PUBLISH este é um padrão de projeto arquitetural para mensagens Publicar/Assinar onde remetentes não são programados para enviar suas mensagens para receptores específicos (assinantes). Ou seja quem publica a mensagem não quer saber quem vai consumir, ele apenas deixa em um local disponível, caracterizados em canais (Chanel), sem conhecimento de quais assinantes podem existir. Os assinantes manifestam interesse em um ou mais canais e só recebem mensagens de seu interesse, sem saber quais (se houver) editores existem. Este padrão é muito usado e simples para comunicações, mas deve-se atentar alguns pontos, se a biblioteca que estiver usando não for configuravel, você pode perder suas mensagens depois de um certo tempo, além disso não existe uma fila esses pontos podem ser classificados como negativos se sua aplicação está preparada para trabalhar com fila.
   
Legal né? Mas agora a pergunta é como posso usar o Redis? Abaixo dou um exemplo de caso de uso.

</br></br>

### <h2>[Cenário de Uso]
Vamos imaginar o seguinte cenário, você tem uma API Rest <b>que gerencia seu cliente</b>, desta forma, você precisa fazer várias manipulações com seu cliente, como <b>alterar, cadastrar, excluir, listar, filtrar etc...</b> Alem deste fator de manipular o cliente, vamos precisar precisa que nossa API notifique outro sistema sempre que cadastrar um usuário novo, sendo assim oque você vai precisar fazer é sua API notificar o Canal que ela está inscrita e o aplicação do outro lado receber a notificação.

   
## <h2> Primeiro passo vamos colocar o Redis em um container
   
   Execute o comando abaixo no seu promt de comando
   
   ````
   docker run --name local-redis -p 6379:6379 -d redis
   ````
   
   Agora execute o redis e abra sua interface para criarmos um canal
   ````
   docker exec -it local-redis redis-cli
   ````
   
   Para criar uma chave com um valor:
   
   ````
   127.0.0.1:6379> SET “customer-redis" “Welcome to redis”
   ````
   
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
