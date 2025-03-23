using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using Consumer.Delete.Contact.Application.DTO;
using Consumer.Delete.Contact.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Consumer.Delete.Contact.Infrastructure.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQSettings _rabbitMqSettings;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger, IServiceProvider serviceProvider, RabbitMQSettings rabbitMqSettings)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMqSettings = rabbitMqSettings;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _rabbitMqSettings.Host,
                    UserName = _rabbitMqSettings.Username,
                    Password = _rabbitMqSettings.Password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue: _rabbitMqSettings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Conectado ao RabbitMQ em {0} e aguardando mensagens na fila '{1}'...",
                    _rabbitMqSettings.Host, _rabbitMqSettings.QueueName);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao conectar ao RabbitMQ: {0}", ex.Message);
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Mensagem recebida: {0}", messageJson);

                    var jsonObject = JsonNode.Parse(messageJson);
                    var messageNode = jsonObject?["message"];

                    if (messageNode != null)
                    {
                        var contatoJson = messageNode.ToString();
                        var contato = JsonSerializer.Deserialize<ContatoDto>(contatoJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (contato != null)
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var contatoService = scope.ServiceProvider.GetRequiredService<IContatoService>();

                            // Verificação de ID antes de chamar o método de exclusão
                            if (contato.Id > 0)
                            {
                                await contatoService.DeletarContatoAsync(contato.Id);
                                _channel.BasicAck(ea.DeliveryTag, false);
                                _logger.LogInformation("Contato com ID {0} excluído com sucesso!", contato.Id);
                            }
                            else
                            {
                                _logger.LogWarning("ID inválido para exclusão: {0}", contato.Id);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Falha ao desserializar o contato.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("JSON recebido não contém a propriedade 'message'.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem: {0}", ex.Message);
                }
            };

            _channel.BasicConsume(queue: _rabbitMqSettings.QueueName, autoAck: false, consumer: consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogInformation("Finalizando RabbitMQConsumer...");
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }

    public class RabbitMQSettings
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
    }
}
