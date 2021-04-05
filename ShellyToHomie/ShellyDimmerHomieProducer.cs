﻿using DevBot9.Protocols.Homie;

namespace ShellieToHomie {
    public class ShellyDimmerHomieProducer {
        private HostDevice _hostDevice;
        private Device.PublishToTopicDelegate _publishMqttTopic;
        private Device.SubscribeToTopicDelegate _subscribeMqttTopic;

        public ShellyDimmerClient ShellyClient { get; private set; }

        private HostBooleanProperty _stateProperty;
        private HostIntegerProperty _brightnessProperty;
        private HostBooleanProperty _onOffCommand;

        private HostFloatProperty _actualPowerConsumptionProperty;
        private HostFloatProperty _energyUsedProperty;
        private HostFloatProperty _internalTemperatureProperty;

        public bool IsInitialized { get; private set; }

        public void Initialize(ShellyDimmerClient shellyClient, string homieDeviceId, string homieDeviceFriendlyName, Device.PublishToTopicDelegate publishToTopicDelegate, Device.SubscribeToTopicDelegate subscribeToTopicDelegate) {
            ShellyClient = shellyClient;

            _publishMqttTopic = publishToTopicDelegate;
            _subscribeMqttTopic = subscribeToTopicDelegate;

            _hostDevice = DeviceFactory.CreateHostDevice(homieDeviceId, homieDeviceFriendlyName);

            _hostDevice.UpdateNodeInfo("basic", "Basic", "no-type");
            _stateProperty = _hostDevice.CreateHostBooleanProperty(PropertyType.State, "basic", "actual-state", "Actual state", ShellyClient.ActualState);
            _brightnessProperty = _hostDevice.CreateHostIntegerProperty(PropertyType.Parameter, "basic", "brightness", "Brightness", ShellyClient.Brightness);
            _onOffCommand = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "basic", "on-off-command", "Turn on off");

            _hostDevice.UpdateNodeInfo("advanced", "Advanced", "no-type");
            _actualPowerConsumptionProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "actual-power-consumption", "Actual power consumption", (float)ShellyClient.PowerInW, "W");
            _internalTemperatureProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "internal-temperature", "Internal temperature", (float)shellyClient.TemperatureInC, "°C");
            _energyUsedProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "energy-used", "Energy used", (float)shellyClient.EnergyInKwh, "kWh");

            _onOffCommand.PropertyChanged += (sender, args) => {
                if (_onOffCommand.Value) ShellyClient.TurnOn();
                else ShellyClient.TurnOff();
            };

            _brightnessProperty.PropertyChanged += (sender, args) => {
                var value = _brightnessProperty.Value;

                if (value < 10) {
                    value = 10;
                    _brightnessProperty.Value = value;
                }

                if (value > 100) {
                    value = 100;
                    _brightnessProperty.Value = value;
                }

                ShellyClient.SetBrightness(value);
            };

            _hostDevice.Initialize(_publishMqttTopic, _subscribeMqttTopic);

            ShellyClient.PropertyChanged += (sender, args) => {
                RefreshAllProperties();
            };

            RefreshAllProperties();

            IsInitialized = true;
        }

        private void RefreshAllProperties() {
            _stateProperty.Value = ShellyClient.ActualState;
            _brightnessProperty.Value = ShellyClient.Brightness;
            _actualPowerConsumptionProperty.Value = (float)ShellyClient.PowerInW;
            _internalTemperatureProperty.Value = (float)ShellyClient.TemperatureInC;
            _energyUsedProperty.Value = (float)ShellyClient.EnergyInKwh;
        }

        public void ProcessMqttMessage(string fullTopic, string payload) {
            if (IsInitialized == false) return;

            _hostDevice.HandlePublishReceived(fullTopic, payload);
        }
    }
}