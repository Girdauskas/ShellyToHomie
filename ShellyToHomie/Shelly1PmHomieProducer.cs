using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace ShellieToHomie {
    public class Shelly1PmHomieProducer {
        private HostDevice _hostDevice;
        public ResilientHomieBroker Broker { get; } = new();

        public Shelly1PmClient ShellyClient { get; private set; }

        private HostBooleanProperty _relayStateProperty;
        private HostBooleanProperty _inputStateProperty;
        private HostBooleanProperty _relayControlProperty;

        private HostFloatProperty _actualPowerConsumptionProperty;
        private HostFloatProperty _energyUsedProperty;
        private HostFloatProperty _internalTemperatureProperty;

        public bool IsInitialized { get; private set; }

        public void Initialize(Shelly1PmClient shellyClient, string homieDeviceId, string homieDeviceFriendlyName, string mqttBrokerIpAddress) {
            ShellyClient = shellyClient;

            _hostDevice = DeviceFactory.CreateHostDevice(homieDeviceId, homieDeviceFriendlyName);

            _hostDevice.UpdateNodeInfo("basic", "Basic", "no-type");
            _relayStateProperty = _hostDevice.CreateHostBooleanProperty(PropertyType.State, "basic", "actual-relay-state", "Actual relay state", ShellyClient.OutputState);
            _inputStateProperty = _hostDevice.CreateHostBooleanProperty(PropertyType.State, "basic", "actual-input-state", "Actual input state", ShellyClient.InputState);
            _relayControlProperty = _hostDevice.CreateHostBooleanProperty(PropertyType.Command, "basic", "relay-control", "Manual relay control");

            _hostDevice.UpdateNodeInfo("advanced", "Advanced", "no-type");
            _actualPowerConsumptionProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "actual-power-consumption", "Actual power consumption", (float)ShellyClient.PowerInW, "W");
            _internalTemperatureProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "internal-temperature", "Internal temperature", (float)shellyClient.TemperatureInC, "°C");
            _energyUsedProperty = _hostDevice.CreateHostFloatProperty(PropertyType.State, "advanced", "energy-used", "Energy used", (float)shellyClient.EnergyInKwh, "kWh");

            _relayControlProperty.PropertyChanged += (sender, args) => {
                if (_relayControlProperty.Value) ShellyClient.EnableRelay();
                else ShellyClient.DisableRelay();
            };

            Broker.Initialize(mqttBrokerIpAddress, _hostDevice.WillTopic, _hostDevice.WillPayload);
            Broker.PublishReceived += _hostDevice.HandlePublishReceived;
            _hostDevice.Initialize(Broker.PublishToTopic, Broker.SubscribeToTopic);

            ShellyClient.PropertyChanged += (sender, args) => {
                RefreshAllProperties();
            };

            RefreshAllProperties();

            IsInitialized = true;
        }

        private void RefreshAllProperties() {
            _relayStateProperty.Value = ShellyClient.OutputState;
            _inputStateProperty.Value = ShellyClient.InputState;
            _actualPowerConsumptionProperty.Value = (float)ShellyClient.PowerInW;
            _internalTemperatureProperty.Value = (float)ShellyClient.TemperatureInC;
            _energyUsedProperty.Value = (float)ShellyClient.EnergyInKwh;
        }
    }
}