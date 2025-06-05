using MQTTnet;
using MQTTnet.Client;

namespace StarMqttClient;

public partial class BrokerClient
{
    private readonly IMqttClient _mqttClient;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    public string ClientId { get; }

    public BrokerClient(string host, int port, string username, string password, string clientId)
    {
        _host = host;
        _port = port;
        _username = username;
        _password = password;
        ClientId = clientId;
        
        // Initialize the MQTTNet client object
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
    }

    /// <summary>
    /// Connects the created MQTT client to a broker.
    /// </summary>
    public async Task Connect()
    {
        var clientOptions = new MqttClientOptionsBuilder()
            .WithClientId(ClientId)
            .WithTcpServer(_host, _port)
            .WithCredentials(_username, _password)
            .WithCleanSession()
            .Build();

        await _mqttClient.ConnectAsync(clientOptions);
    }

    /// <summary>
    /// Disconnects the MQTT client socket.
    /// </summary>
    public async Task Disconnect()
    {
        await _mqttClient.DisconnectAsync();
    }
    
}