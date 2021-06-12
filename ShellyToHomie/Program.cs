using System.Threading.Tasks;
using ShellieToHomie;

namespace ShellyToHomie {
    class Program {
        private static readonly Shelly1PmClient Shelly1PmClient = new();
        private static readonly ShellyDimmerClient ShellyDimmerClient = new();

        private static readonly Shelly1PmHomieProducer Shelly1PmHomieProducer = new();
        private static readonly ShellyDimmerHomieProducer ShellyDimmerHomieProducer = new();

        static async Task Main(string[] args) {
            var mqttBrokerIp = "192.168.2.2";

            Shelly1PmClient.Initialize("shellies/shelly1pm-68C63AFADFF9", mqttBrokerIp);
            Shelly1PmHomieProducer.Initialize(Shelly1PmClient, "shelly1pm-68c63afadff9", "Office lights", mqttBrokerIp);

            ShellyDimmerClient.Initialize("shellies/shellydimmer-D0E18A", mqttBrokerIp);
            ShellyDimmerHomieProducer.Initialize(ShellyDimmerClient, $"shellydimmer-d0e18a", $"Living room dimmer", mqttBrokerIp);

            while (true) {
                //Shelly1PmClient.EnableRelay();
                //ShellyDimmerClient.TurnOn();
                //ShellyDimmerClient.SetBrightness(33);

                await Task.Delay(1000);
            }
        }
    }
}