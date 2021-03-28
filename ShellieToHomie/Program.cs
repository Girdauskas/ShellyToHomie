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
        private static Shelly1pmClient _shelly1PmClient = new Shelly1pmClient();

        static async Task Main(string[] args) {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);
            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = "ws://192.168.2.2:9001/" } };

            await _mqttClient.ConnectAsync(clientOptions, CancellationToken.None);

            _shelly1PmClient.Initialize("shellies/shelly1pm-68C63AFADFF9", PublishMqttTopic);
            await _mqttClient.SubscribeAsync("shellies/shelly1pm-68C63AFADFF9/#");

            while (true) {
                Console.WriteLine("Hello World!");

                if (_shelly1PmClient.RelayState) await _shelly1PmClient.DisableRelayAsync();
                else await _shelly1PmClient.EnableRelayAsync();

                await Task.Delay(3000);
            }
        }

        private static async Task PublishMqttTopic(string topic, string value) {
            await _mqttClient.PublishAsync(topic, Encoding.UTF8.GetBytes(value));
        }

        private static void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            _shelly1PmClient.ProcessMqttMessage(obj.ApplicationMessage.Topic, Encoding.UTF8.GetString(obj.ApplicationMessage.Payload));
        }
    }
}