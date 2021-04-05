using System;
using System.ComponentModel;
using System.Globalization;
using DevBot9.Protocols.Homie;

namespace ShellieToHomie {
    public class Shelly1PmClient : INotifyPropertyChanged {
        private Device.PublishToTopicDelegate _publishMqttTopic;

        public bool IsInitialized { get; private set; }
        public string RootMqttTopic { get; private set; }


        private bool _outputState;
        public bool OutputState {
            get => _outputState;
            private set {
                if (_outputState == value) return;

                _outputState = value;
                NotifyPropertyChanged(nameof(OutputState));
            }
        }

        private double _powerInW;
        public double PowerInW {
            get => _powerInW;
            private set {
                if (Math.Abs(_powerInW - value) < float.Epsilon) return;

                _powerInW = value;
                NotifyPropertyChanged(nameof(PowerInW));
            }
        }

        private double _energyInKwh;
        public double EnergyInKwh {
            get => _energyInKwh;
            private set {
                if (Math.Abs(_energyInKwh - value) < float.Epsilon) return;

                _energyInKwh = value;
                NotifyPropertyChanged(nameof(EnergyInKwh));
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
                if (Math.Abs(_temperatureInC - value) < float.Epsilon) return;

                _temperatureInC = value;
                NotifyPropertyChanged(nameof(TemperatureInC));
            }
        }


        public void EnableRelay() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");
            _publishMqttTopic($"{RootMqttTopic}/relay/0/command", "on", 1, false);
        }

        public void DisableRelay() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");
            _publishMqttTopic($"{RootMqttTopic}/relay/0/command", "off", 1, false);
        }

        public void SetBrightness(int brightness) {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");

            var payload = $"{{ \"brightness\": {brightness} }}";

            _publishMqttTopic($"{RootMqttTopic}/light/0/set", payload, 1, false);
        }

        public void ProcessMqttMessage(string topic, string value) {
            //if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before messages can be processed.");

            if (topic == $"{RootMqttTopic}/relay/0") {
                OutputState = value == "on";
            } else if (topic == $"{RootMqttTopic}/relay/0/power") {
                PowerInW = double.Parse(value, CultureInfo.InvariantCulture);
            } else if (topic == $"{RootMqttTopic}/relay/0/energy") {
                var energyInWattMinutes = int.Parse(value, CultureInfo.InvariantCulture);
                EnergyInKwh = energyInWattMinutes / 60000.0;
            } else if (topic == $"{RootMqttTopic}/input/0") {
                InputState = value == "1";
            } else if (topic == $"{RootMqttTopic}/temperature") {
                TemperatureInC = double.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        public void Initialize(string rootMqttTopic, Device.PublishToTopicDelegate publishMqttTopic, Device.SubscribeToTopicDelegate subscribeMqttTopic) {
            if (IsInitialized) throw new InvalidOperationException("Object can only be initialized once.");

            _publishMqttTopic = publishMqttTopic;
            RootMqttTopic = rootMqttTopic;

            subscribeMqttTopic($"{rootMqttTopic}/#");
            IsInitialized = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}