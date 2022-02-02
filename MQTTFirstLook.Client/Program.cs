using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using Serilog;
using System.Threading;


namespace MQTTFirstLook.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                                        .WithClientId("www.google.com")
                                        .WithTcpServer("localhost", 707);

            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(60))
                                    .WithClientOptions(builder.Build())
                                    .Build();

            IManagedMqttClient _mqttClient = new MqttFactory().CreateManagedMqttClient();

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(a => {
                Log.Logger.Information("Message recieved: {payload}", a.ApplicationMessage);
            });

            _mqttClient.StartAsync(options).GetAwaiter().GetResult();

            while (true)
            {
                int milliseconds = 2000;
            Random rd = new Random();

            int Temp_value = rd.Next(30, 35);      

            

            Thread.Sleep(milliseconds); 
                string json = JsonConvert.SerializeObject(new { Temperature_value = Temp_value.ToString(), Timestamp= DateTimeOffset.UtcNow });
                _mqttClient.PublishAsync("dev.to/topic/json", json);

                Task.Delay(1000).GetAwaiter().GetResult();
            }
        }

        public static void OnConnected(MqttClientConnectedEventArgs obj)
        {
            Log.Logger.Information("Successfully connected.");
        }

        public static void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
        {
            Log.Logger.Warning("Couldn't connect to broker.");
        }

        public static void OnDisconnected(MqttClientDisconnectedEventArgs obj)
        {
            Log.Logger.Information("Successfully disconnected.");
        }
    }
}
