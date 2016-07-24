using System.Collections.Generic;
using System.Linq;
using MqttLib;
using UnityEngine;
using System;

public class EnvironmentManager : MonoBehaviour
{
    private MqttClient _mqttClient;
    public List<GameObject> _models;

    /// <summary>
    /// Used  for initialization.
    /// </summary>
    public void Start()
    {
        // Initialize models in scene.
        foreach (var model in _models)
        {
            model.SetActive(false);
        }
        _models.FirstOrDefault().SetActive(true);

        // Create MQTT client.
        _mqttClient = new MqttClient();
        _mqttClient.MessageReceived += OnMqttMessageReceived;
        _mqttClient.ConnectToFeed();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    public void Update()
    {

    }

    /// <summary>
    /// Called when the MonoBehaviour will be destroyed.
    /// </summary>
    public void OnDestroy()
    {
        _mqttClient.MessageReceived -= OnMqttMessageReceived;
        _mqttClient.Dispose();
    }

    private void OnMqttMessageReceived(object sender, PublishArrivedArgs e)
    {
        var jsonPayload = System.Text.Encoding.Default.GetString(e.Payload.TrimmedBuffer);

        try
        {
            var message = JsonUtility.FromJson<SwapModelMessage>(jsonPayload);
            Debug.Log(string.Format("Received swap model message: {0}", message.Model));
            SwapModel(message.Model);
        }
        catch { }

        try
        {
            var message = JsonUtility.FromJson<RotateCameraMessage>(jsonPayload);
            Debug.Log(string.Format("Received rotate camera message: {0}", message.Direction));
            RotateCamera(message.Direction);
        }
        catch
        {
            Debug.Log(string.Format("Could not deserialize JSON into known message: {0}", jsonPayload));
        }
    }

    private void SwapModel(string model)
    {

    }

    private void RotateCamera(string direction)
    {
    
    }
}