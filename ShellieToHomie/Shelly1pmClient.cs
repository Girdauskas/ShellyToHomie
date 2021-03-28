using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ShellieToHomie {
    public class Shelly1pmClient {
        private Func<string, string, Task> _publishMqttTopic;

        public bool IsInitialized { get; private set; }
        public string RootMqttTopic { get; private set; }
        public bool RelayState { get; private set; }
        public double RelayPower { get; private set; }
        public int RelayEnergy { get; private set; }
        public bool ButtonState { get; private set; }
        public double Temperature { get; private set; }

        public async Task EnableRelayAsync() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");
            await _publishMqttTopic($"{RootMqttTopic}/relay/0/command", "on");
        }

        public async Task DisableRelayAsync() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");
            await _publishMqttTopic($"{RootMqttTopic}/relay/0/command", "off");
        }

        public void ProcessMqttMessage(string topic, string value) {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before messages can be processed.");

            if (topic == $"{RootMqttTopic}/relay/0") RelayState = value == "on";
            else if (topic == $"{RootMqttTopic}/relay/0/power") RelayPower = double.Parse(value, CultureInfo.InvariantCulture);
            else if (topic == $"{RootMqttTopic}/relay/0/energy") RelayEnergy = int.Parse(value, CultureInfo.InvariantCulture);
            else if (topic == $"{RootMqttTopic}/input/0") ButtonState = value == "1";
            else if (topic == $"{RootMqttTopic}/temperature") Temperature = double.Parse(value, CultureInfo.InvariantCulture);
        }

        public void Initialize(string rootMqttTopic, Func<string, string, Task> publishMqttTopic) {
            if (IsInitialized) throw new InvalidOperationException("Object can only be initialized once.");

            _publishMqttTopic = publishMqttTopic;
            RootMqttTopic = rootMqttTopic;
            IsInitialized = true;
        }
    }
}