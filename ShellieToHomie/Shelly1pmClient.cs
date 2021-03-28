using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace ShellieToHomie {
    public class Shelly1PmClient : INotifyPropertyChanged {
        private Func<string, string, Task> _publishMqttTopic;

        public bool IsInitialized { get; private set; }
        public string RootMqttTopic { get; private set; }


        private bool _relayState;
        public bool RelayState {
            get => _relayState;
            private set {
                if (_relayState == value) return;

                _relayState = value;
                NotifyPropertyChanged(nameof(RelayState));
            }
        }

        private double _relayPowerInW;
        public double RelayPowerInW {
            get => _relayPowerInW;
            private set {
                if (_relayPowerInW == value) return;

                _relayPowerInW = value;
                NotifyPropertyChanged(nameof(RelayPowerInW));
            }
        }

        private double _energyUsageInKwh;
        public double RelayEnergyUsageInKwh {
            get => _energyUsageInKwh;
            private set {
                if (_energyUsageInKwh == value) return;

                _energyUsageInKwh = value;
                NotifyPropertyChanged(nameof(RelayEnergyUsageInKwh));
            }
        }

        private bool _inputState;
        public bool InputState {
            get => _inputState;
            private set {
                if (_inputState == value) return;

                _inputState = value;
                NotifyPropertyChanged(nameof(InputState));
            }
        }

        private double _temperatureInC;
        public double TemperatureInC {
            get => _temperatureInC;
            private set {
                if (_temperatureInC == value) return;

                _temperatureInC = value;
                NotifyPropertyChanged(nameof(TemperatureInC));
            }
        }


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
            else if (topic == $"{RootMqttTopic}/relay/0/power") RelayPowerInW = double.Parse(value, CultureInfo.InvariantCulture);
            else if (topic == $"{RootMqttTopic}/relay/0/energy") {
                var energyInWattMinutes = int.Parse(value, CultureInfo.InvariantCulture);
                RelayEnergyUsageInKwh = energyInWattMinutes / 60000.0;
            } else if (topic == $"{RootMqttTopic}/input/0") InputState = value == "1";
            else if (topic == $"{RootMqttTopic}/temperature") TemperatureInC = double.Parse(value, CultureInfo.InvariantCulture);
        }

        public async Task InitializeAsync(string rootMqttTopic, Func<string, string, Task> publishMqttTopic, Func<string, Task> subscribeMqttTopic) {
            if (IsInitialized) throw new InvalidOperationException("Object can only be initialized once.");

            _publishMqttTopic = publishMqttTopic;
            RootMqttTopic = rootMqttTopic;

            await subscribeMqttTopic($"{rootMqttTopic}/#");
            IsInitialized = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}