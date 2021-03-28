using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;

namespace ShellieToHomie {
    class Program {
        private static IMqttClient _mqttClient;
        private static Shelly1PmClient _shelly1PmClient = new Shelly1PmClient();
        private static Shelly1PmHomieProducer _shelly1PmHomieProducer = new Shelly1PmHomieProducer();

        static async Task Main(string[] args) {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);
            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = "ws://192.168.2.2:9001/" } };

            await _mqttClient.ConnectAsync(clientOptions, CancellationToken.None);

            await _shelly1PmClient.InitializeAsync("shellies/shelly1pm-68C63AFADFF9", PublishMqttTopic, SubscribeMqttTopic);
            await _mqttClient.SubscribeAsync("shellies/shelly1pm-68C63AFADFF9/#");

            _shelly1PmHomieProducer.Initialize(_shelly1PmClient, "shelly1pm-68C63AFADFF9", "Office lights", ((topic, payload, level, retained) => {
                var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(level).WithRetainFlag(retained).Build();
                _mqttClient.PublishAsync(message).Wait();
            }), (topic => {
                Console.WriteLine("Subscribing to " + topic);
                _mqttClient.SubscribeAsync(topic).Wait();
            }));

            while (true) {
                Console.WriteLine("Hello World!");

                await Task.Delay(3000);
            }
        }

        private static async Task SubscribeMqttTopic(string topic) {
            await _mqttClient.SubscribeAsync(topic);
        }

        private static async Task PublishMqttTopic(string topic, string value) {
            await _mqttClient.PublishAsync(topic, Encoding.UTF8.GetBytes(value));
        }

        private static void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            var payload = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);

            _shelly1PmClient.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
            _shelly1PmHomieProducer.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
        }
    }
}