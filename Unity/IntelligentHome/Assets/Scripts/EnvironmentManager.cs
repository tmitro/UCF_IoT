using System.Collections.Generic;
using System.Linq;
using MqttLib;
using UnityEngine;
using System;

public class EnvironmentManager : MonoBehaviour
{
    /// <summary>
    /// A collection of swapable models.
    /// </summary>
    public List<GameObject> _models;

    private MqttClient _mqttClient;
    private GameObject _activeModel;
    private IDictionary<string, GameObject> _modelsByName;
    private bool _rotatingCamera;
    private RotationAxis _rotationAxis;
    private float _rotationAngle;
    private float _transitionTime = 2.0f;
    private float _elapsedTransitionTime;

    /// <summary>
    /// Used for initialization.
    /// </summary>
    public void Start()
    {
        _modelsByName = new Dictionary<string, GameObject>();

        // Initialize models in scene.
        foreach (var model in _models)
        {
            _modelsByName[model.name] = model;
            model.SetActive(false);
        }
        _activeModel = _models.FirstOrDefault();
        _activeModel.SetActive(true);

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
        // TODO: remove local test stubs.
        if (Input.GetKeyUp(KeyCode.C))
        {
            SwapModel("Cube");
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            SwapModel("Sphere");
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            RotateCamera(CameraDirection.Right);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            RotateCamera(CameraDirection.Right);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            RotateCamera(CameraDirection.Left);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            RotateCamera(CameraDirection.Right);
        }
        if (Input.GetKeyUp(KeyCode.Keypad0))
        {
            RotateCamera(CameraDirection.OneEighty);
        }

        if (_rotatingCamera)
        {
            float xRotation = 0f;
            float zRotation = 0f;

            //Camera.main.transform.LookAt(_activeModel.transform);

            if (_rotationAxis == RotationAxis.Vertical)
            {
                xRotation = Input.GetAxis("Vertical") * _rotationAngle;
            }
            else
            {
                zRotation = Input.GetAxis("Horizontal") * _rotationAngle;
            }

            float step = _transitionTime * Time.deltaTime;
            float orbitCircumfrance = 2F * rDistance * Mathf.PI;
            float distanceDegrees = (_transitionTime / orbitCircumfrance) * 360;
            float distanceRadians = (_transitionTime / orbitCircumfrance) * 2 * Mathf.PI;

            if (_rotationAngle > 0)
            {
                transform.RotateAround(_activeModel.transform.position, Vector3.up, -rotationAmount);
                _rotationAngle -= rotationAmount;
            }
            else if (_rotationAngle < 0)
            {
                transform.RotateAround(_activeModel.transform.position, Vector3.up, rotationAmount);
                _rotationAngle += rotationAmount;
            }


            Camera.main.transform.eulerAngles = Vector3.Lerp(Camera.main.transform.eulerAngles, 
                new Vector3(xRotation, 0, zRotation), Time.deltaTime * _transitionTime);

            //Quaternion rotation = Quaternion.Euler(tiltAroundX, 0, tiltAroundZ);
            //transform.rotation = Quaternion.Slerp(
            //    Camera.main.transform.rotation, rotation, Time.time * _transitionTime);
            _elapsedTransitionTime += Time.deltaTime;

            if (_elapsedTransitionTime >= _transitionTime)
            {
                _elapsedTransitionTime = 0f;
                _rotatingCamera = false;
            }
        }
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
        _activeModel.SetActive(false);
        _activeModel = _modelsByName[model];
        _activeModel.SetActive(true);
    }

    private void RotateCamera(CameraDirection direction)
    {
        switch (direction)
        {
            case CameraDirection.Up:
                _rotationAxis = RotationAxis.Vertical;
                _rotationAngle = 90f;
                break;
            case CameraDirection.Down:
                _rotationAxis = RotationAxis.Vertical;
                _rotationAngle = -90f;
                break;
            case CameraDirection.Left:
                _rotationAxis = RotationAxis.Horizontal;
                _rotationAngle = -90f;
                break;
            case CameraDirection.Right:
                _rotationAxis = RotationAxis.Horizontal;
                _rotationAngle = 90f;
                break;
            case CameraDirection.OneEighty:
                _rotationAxis = RotationAxis.Horizontal;
                _rotationAngle = 180f;
                break;
        }

        _rotatingCamera = true;
    }

    private enum RotationAxis
    {
        Horizontal,
        Vertical
    }
}