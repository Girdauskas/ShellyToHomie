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
        private static readonly Shelly1PmClient Shelly1PmClient = new Shelly1PmClient();
        private static readonly ShellyDimmerClient ShellyDimmerClient = new ShellyDimmerClient();
        private static readonly Shelly1PmHomieProducer Shelly1PmHomieProducer = new Shelly1PmHomieProducer();
        private static readonly ShellyDimmerHomieProducer ShellyDimmerHomieProducer = new ShellyDimmerHomieProducer();

        static async Task Main(string[] args) {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);
            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = "ws://192.168.2.2:9001/" } };
            await _mqttClient.ConnectAsync(clientOptions, CancellationToken.None);

            while (_mqttClient.IsConnected == false) {
                await Task.Delay(100);
            }

            Shelly1PmClient.Initialize("shellies/shelly1pm-68C63AFADFF9", PublishToTopicDelegate, SubscribeToTopicDelegate);
            Shelly1PmHomieProducer.Initialize(Shelly1PmClient, "shelly1pm-68C63AFADFF9", "Office lights", PublishToTopicDelegate, SubscribeToTopicDelegate);

            ShellyDimmerClient.Initialize("shellies/shellydimmer-D0E18A", PublishToTopicDelegate, SubscribeToTopicDelegate);
            ShellyDimmerHomieProducer.Initialize(ShellyDimmerClient, "shellydimmer-D0E18A","Living room dimmer", PublishToTopicDelegate, SubscribeToTopicDelegate);

            while (true) {
                //Shelly1PmClient.EnableRelay();
                //ShellyDimmerClient.TurnOn();
                //ShellyDimmerClient.SetBrightness(33);

                await Task.Delay(1000);
            }
        }

        private static void PublishToTopicDelegate(string topic, string payload, byte qoslevel, bool isretained) {
            var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(qoslevel).WithRetainFlag(isretained).Build();
            _mqttClient.PublishAsync(message).Wait();
        }

        private static void SubscribeToTopicDelegate(string topic) {
            Console.WriteLine("Subscribing to " + topic);
            _mqttClient.SubscribeAsync(topic).Wait();
        }

        private static void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            var payload = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);

            Shelly1PmClient.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
            Shelly1PmHomieProducer.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
            ShellyDimmerClient.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
            ShellyDimmerHomieProducer.ProcessMqttMessage(obj.ApplicationMessage.Topic, payload);
        }
    }
}