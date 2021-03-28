using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using DevBot9.Protocols.Homie;

namespace ShellieToHomie {
    public class ShellyDimmerClient : INotifyPropertyChanged {
        private Device.PublishToTopicDelegate _publishMqttTopic;

        public string RootMqttTopic { get; private set; }

        private bool _actualState;
        public bool ActualState {
            get => _actualState;
            private set {
                if (_actualState == value) return;

                _actualState = value;
                NotifyPropertyChanged(nameof(ActualState));
            }
        }

        private int _brightness;
        public int Brightness {
            get => _brightness;
            private set {
                if (_brightness == value) return;

                _brightness = value;
                NotifyPropertyChanged(nameof(Brightness));
            }
        }

        private double _temperatureInC;
        public double TemperatureInC {
            get => _temperatureInC;
            set {
                if (Math.Abs(_temperatureInC - value) < float.Epsilon) return;

                _temperatureInC = value;
                NotifyPropertyChanged(nameof(TemperatureInC));
            }
        }

        private double _powerInW;
        public double PowerInW {
            get => _powerInW;
            set {
                if (Math.Abs(_powerInW - value) < float.Epsilon) return;

                _powerInW = value;
                NotifyPropertyChanged(nameof(PowerInW));
            }
        }

        private double _energyInKwh;
        public double EnergyInKwh {
            get => _energyInKwh;
            set {
                if (Math.Abs(_energyInKwh - value) < float.Epsilon) return;

                _energyInKwh = value;
                NotifyPropertyChanged(nameof(EnergyInKwh));
            }
        }

        public bool IsInitialized { get; private set; }

        public void TurnOn() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");

            _publishMqttTopic($"{RootMqttTopic}/light/0/command", "on", 1, false);
        }

        public void TurnOff() {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");

            _publishMqttTopic($"{RootMqttTopic}/light/0/command", "off", 1, false);
        }

        public void SetBrightness(int level) {
            if (IsInitialized == false) throw new InvalidOperationException("Object must be initialized before commands can be executed.");

            var json = $"{{ \"brightness\": {level} }}";
            _publishMqttTopic($"{RootMqttTopic}/light/0/set", json, 1, false);
        }


        public void Initialize(string rootMqttTopic, Device.PublishToTopicDelegate publishMqttTopic, Device.SubscribeToTopicDelegate subscribeMqttTopic) {
            // if (IsInitialized) throw new InvalidOperationException("Object can only be initialized once.");

            _publishMqttTopic = publishMqttTopic;
            RootMqttTopic = rootMqttTopic;

            subscribeMqttTopic($"{rootMqttTopic}/#");
            IsInitialized = true;
        }

        public void ProcessMqttMessage(string topic, string value) {
            if (topic == $"{RootMqttTopic}/light/0") {
                ActualState = value == "on";
            } else if (topic == $"{RootMqttTopic}/light/0/status") {
                var status = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(value);
                Brightness = status["brightness"].GetInt32();
            } else if (topic == $"{RootMqttTopic}/light/0/power") {
                PowerInW = double.Parse(value, CultureInfo.InvariantCulture);
            } else if (topic == $"{RootMqttTopic}/light/0/energy") {
                var energyInWattMinutes = int.Parse(value, CultureInfo.InvariantCulture);
                EnergyInKwh = energyInWattMinutes / 60000.0;
            } else if (topic == $"{RootMqttTopic}/temperature") {
                TemperatureInC = double.Parse(value, CultureInfo.InvariantCulture);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}