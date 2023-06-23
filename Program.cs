// See https://aka.ms/new-console-template for more information
using System;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Publishing;

class Program
{
    static async Task Main(string[] args)
    {
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("mqtt_server_address", 1883) // MQTT sunucusunun adresini ve bağlantı noktasını belirtin
            .Build();

        mqttClient.UseConnectedHandler(async e =>
        {
            Console.WriteLine("MQTT sunucusuna bağlandı.");
            await mqttClient.SubscribeAsync("topic_name"); // Abone olunacak konu adını belirtin
        });

        mqttClient.UseDisconnectedHandler(async e =>
        {
            Console.WriteLine("MQTT sunucusundan bağlantı kesildi.");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await mqttClient.ConnectAsync(options); // Bağlantıyı yeniden kurmaya çalışın
            }
            catch
            {
                Console.WriteLine("MQTT sunucusuna yeniden bağlanma hatası.");
            }
        });

        mqttClient.UseApplicationMessageReceivedHandler(e =>
        {
            Console.WriteLine("Yeni mesaj alındı:");
            Console.WriteLine($"\tKonu: {e.ApplicationMessage.Topic}");
            Console.WriteLine($"\tMesaj: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        });

        try
        {
            await mqttClient.ConnectAsync(options); // MQTT sunucusuna bağlanma işlemi
            Console.WriteLine("MQTT sunucusuna bağlanılıyor...");
        }
        catch
        {
            Console.WriteLine("MQTT sunucusuna bağlanma hatası.");
        }

        // Mesaj yayınlama
        var message = new MqttApplicationMessageBuilder()
            .WithTopic("topic_name") // Yayınlamak istediğiniz konu adını belirtin
            .WithPayload("Hello, MQTT!") // Yayınlamak istediğiniz mesajı belirtin
            .WithExactlyOnceQoS()
            .WithRetainFlag()
            .Build();

        await mqttClient.PublishAsync(message);

        Console.ReadLine(); // Programın sonlanmaması için bekleyin
    }
