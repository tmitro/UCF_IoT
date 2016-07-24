using System;
using MqttLib;
using UnityEngine;

/// <summary>
/// Handler for receiving an MQTT message.
/// </summary>
/// <param name="sender">Sender.</param>
/// <param name="e">EventArgs.</param>
public delegate void MqttMessageHandler(object sender, MqttLib.PublishArrivedArgs e);

/// <summary>
/// Client to listen on an Adafruit IO feed and dispatch when an message has been triggered.
/// </summary>
public class MqttClient : IMqttClient
{
    public event MqttMessageHandler MessageReceived;
    private readonly string _connectionString = "https://io.adafruit.com";
    private readonly string _feed = "UCF_IoT/feeds/intelligent-home";
    private IMqtt _client;

    /// <summary>
    /// Initializes a new <see cref="MqttClient" /> instance.
    /// </summary>
    public MqttClient()
    {
    }

    /// <summary>
    /// Creates a new MQTT client and connects to the Adafruit IO feed.
    /// </summary>
    public void ConnectToFeed()
    {
        _client = MqttClientFactory.CreateClient(_connectionString, Guid.NewGuid().ToString());
        _client.Connected += OnConnected;
        _client.Connect(true);
    }

    /// <summary>
    /// Disconnects the client and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        _client.PublishArrived -= OnPublishReceived;
        _client.Disconnect();
    }

    private void OnConnected(object sender, EventArgs e)
    {
        Debug.Log(string.Format("MQTT client connected at: {0}", _connectionString));
        _client.PublishArrived += OnPublishReceived;
        SubscribeToFeed();
    }

    private void SubscribeToFeed()
    {
        _client.Subscribed += OnSubscribed;
        _client.Subscribe(_feed, QoS.OnceAndOnceOnly);
    }

    private void OnSubscribed(object sender, CompleteArgs e)
    {
        Debug.Log(string.Format("MQTT client subscribed to feed {0}; {1}", _feed, e.MessageID));
    }

    private bool OnPublishReceived(object sender, PublishArrivedArgs e)
    {
        if (MessageReceived != null)
        {
            MessageReceived(sender, e);
            return true;
        }

        return false;
    }
}

public interface IMqttClient
{
    event MqttMessageHandler MessageReceived;
} 