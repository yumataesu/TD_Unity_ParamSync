// based OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack
using System.Runtime.Serialization;
using System;

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using OscJack;

namespace Nqmq.Osc
{

    public class InterfaceData
    {
        public string address { get; set; }
        public string valtype { get; set; }
        public float valmin { get; set; }
        public float valmax { get; set; }
        public int factornum { get; set; }
    };

    [AddComponentMenu("OSC/TD OscDat Receiver")]
    public class TDOscDatReceiver : MonoBehaviour
    {
        public string Address => _oscAddress;
        [HideInInspector] public InterfaceData interfaceData = new InterfaceData() { valtype = "null", valmin = -100, valmax = 100, factornum = 0 };

        #region Receiver data types

        public enum DataType
        {
            None, Int, Float, String,
            Vector2, Vector3, Vector4,
            Vector2Int, Vector3Int
        }

        #endregion

        #region Receiver event classes

        [System.Serializable] class IntEvent : UnityEvent<int> { }
        [System.Serializable] class FloatEvent : UnityEvent<float> { }
        [System.Serializable] class StringEvent : UnityEvent<string> { }
        [System.Serializable] class Vector2Event : UnityEvent<Vector2> { }
        [System.Serializable] class Vector3Event : UnityEvent<Vector3> { }
        [System.Serializable] class Vector4Event : UnityEvent<Vector4> { }
        [System.Serializable] class Vector2IntEvent : UnityEvent<Vector2Int> { }
        [System.Serializable] class Vector3IntEvent : UnityEvent<Vector3Int> { }

        #endregion

        #region Editable fields

        [SerializeField] OscConnection _connection = null;
        [SerializeField] string _oscAddress = "/unity";
        [SerializeField] DataType _dataType = DataType.None;

        [SerializeField] UnityEvent _event = null;
        [SerializeField] IntEvent _intEvent = null;
        [SerializeField] FloatEvent _floatEvent = null;
        [SerializeField] Vector2Event _vector2Event = null;
        [SerializeField] Vector3Event _vector3Event = null;
        [SerializeField] Vector4Event _vector4Event = null;
        [SerializeField] Vector2IntEvent _vector2IntEvent = null;
        [SerializeField] Vector3IntEvent _vector3IntEvent = null;
        [SerializeField] StringEvent _stringEvent = null;

        [SerializeField] float _minvalue = -10.0f;
        [SerializeField] float _maxvalue = 10.0f;
        [SerializeField] int _minvalueInt = -10;
        [SerializeField] int _maxvalueInt = 10;

        #endregion

        #region Internal members

        // Used to store values on initialization
        int _currentPort;
        string _currentAddress;

        // Incoming data queues
        int _bangCount;
        Queue<int> _intQueue;
        Queue<float> _floatQueue;
        Queue<string> _stringQueue;

        int DequeueInt()
        {
            lock (_intQueue) return _intQueue.Dequeue();
        }

        float DequeueFloat()
        {
            lock (_floatQueue) return _floatQueue.Dequeue();
        }

        string DequeueString()
        {
            lock (_stringQueue) return _stringQueue.Dequeue();
        }

        Vector2 DequeueVector2()
        {
            lock (_floatQueue) return new Vector2(
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue()
            );
        }

        Vector3 DequeueVector3()
        {
            lock (_floatQueue) return new Vector3(
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue()
            );
        }

        Vector4 DequeueVector4()
        {
            lock (_floatQueue) return new Vector4(
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue(),
                _floatQueue.Dequeue()
            );
        }

        Vector2Int DequeueVector2Int()
        {
            lock (_intQueue) return new Vector2Int(
                _intQueue.Dequeue(),
                _intQueue.Dequeue()
            );
        }

        Vector3Int DequeueVector3Int()
        {
            lock (_intQueue) return new Vector3Int(
                _intQueue.Dequeue(),
                _intQueue.Dequeue(),
                _intQueue.Dequeue()
            );
        }

        void RegisterCallback()
        {
            var port = _connection?.port ?? 0;

            if (port == 0 || string.IsNullOrEmpty(_oscAddress))
            {
                _currentPort = 0;
                _currentAddress = null;
                return;
            }

            var server = OscMaster.GetSharedServer(port);
            server.MessageDispatcher.AddCallback(_oscAddress, OnDataReceive);

            _currentPort = port;
            _currentAddress = _oscAddress;
        }

        void UnregisterCallback()
        {
            if (_currentPort == 0 || string.IsNullOrEmpty(_currentAddress)) return;

            var server = OscMaster.GetSharedServer(_currentPort);
            server.MessageDispatcher.RemoveCallback(_currentAddress, OnDataReceive);

            _currentAddress = null;
        }

        #endregion

        #region MonoBehaviour implementation

        void OnEnable()
        {
            //UnregisterCallback();
            RegisterCallback();

            interfaceData.address = _oscAddress;

            switch (_dataType)
            {
                case DataType.Int:
                    interfaceData.valtype = "i1";
                    interfaceData.valmin = _minvalueInt;
                    interfaceData.valmax = _maxvalueInt;
                    interfaceData.factornum = 1;
                    if (_intQueue == null) _intQueue = new Queue<int>(4);
                    break;
                case DataType.Vector2Int:
                    interfaceData.valtype = "i2";
                    interfaceData.valmin = _minvalueInt;
                    interfaceData.valmax = _maxvalueInt;
                    interfaceData.factornum = 2;
                    if (_intQueue == null) _intQueue = new Queue<int>(4);
                    break;
                case DataType.Vector3Int:
                    interfaceData.valtype = "i3";
                    interfaceData.valmin = _minvalueInt;
                    interfaceData.valmax = _maxvalueInt;
                    interfaceData.factornum = 3;

                    if (_intQueue == null) _intQueue = new Queue<int>(4);
                    break;

                case DataType.Float:
                    interfaceData.valtype = "f1";
                    interfaceData.valmin = _minvalue;
                    interfaceData.valmax = _maxvalue;
                    interfaceData.factornum = 1;

                    if (_floatQueue == null) _floatQueue = new Queue<float>(4);
                    break;
                case DataType.Vector2:
                    interfaceData.valtype = "f2";
                    interfaceData.valmin = _minvalue;
                    interfaceData.valmax = _maxvalue;
                    interfaceData.factornum = 2;

                    if (_floatQueue == null) _floatQueue = new Queue<float>(4);
                    break;
                case DataType.Vector3:
                    interfaceData.valtype = "f3";
                    interfaceData.valmin = _minvalue;
                    interfaceData.valmax = _maxvalue;
                    interfaceData.factornum = 3;

                    if (_floatQueue == null) _floatQueue = new Queue<float>(4);
                    break;
                case DataType.Vector4:
                    interfaceData.valtype = "f4";
                    interfaceData.factornum = 4;

                    if (_floatQueue == null) _floatQueue = new Queue<float>(4);
                    break;

                case DataType.String:
                    if (_stringQueue == null) _stringQueue = new Queue<string>();
                    break;
            }
        }

        void OnDisable()
          => UnregisterCallback();

        void OnValidate()
        {
            if (Application.isPlaying && enabled)
                OnEnable(); // Update the settings.
        }

        void Update()
        {
            switch (_dataType)
            {
                case DataType.None:
                    while (_bangCount > 0)
                    {
                        _event.Invoke();
                        _bangCount--;
                    }
                    break;

                case DataType.Int:
                    while (_intQueue.Count > 0)
                        _intEvent.Invoke(DequeueInt());
                    break;

                case DataType.Float:
                    while (_floatQueue.Count > 0)
                        _floatEvent.Invoke(DequeueFloat());
                    break;

                case DataType.String:
                    while (_stringQueue.Count > 0)
                        _stringEvent.Invoke(DequeueString());
                    break;

                case DataType.Vector2:
                    while (_floatQueue.Count > 0)
                        _vector2Event.Invoke(DequeueVector2());
                    break;

                case DataType.Vector3:
                    while (_floatQueue.Count > 0)
                        _vector3Event.Invoke(DequeueVector3());
                    break;

                case DataType.Vector4:
                    while (_floatQueue.Count > 0)
                        _vector4Event.Invoke(DequeueVector4());
                    break;

                case DataType.Vector2Int:
                    while (_intQueue.Count > 0)
                        _vector2IntEvent.Invoke(DequeueVector2Int());
                    break;

                case DataType.Vector3Int:
                    while (_intQueue.Count > 0)
                        _vector3IntEvent.Invoke(DequeueVector3Int());
                    break;
            }
        }

        #endregion

        #region OSC event callback

        void OnDataReceive(string address, OscDataHandle data)
        {
            switch (_dataType)
            {
                case DataType.None:
                    _bangCount++;
                    break;

                case DataType.Int:
                    lock (_intQueue)
                        _intQueue.Enqueue(data.GetElementAsInt(0));
                    break;

                case DataType.Float:
                    lock (_floatQueue)
                        _floatQueue.Enqueue(data.GetElementAsFloat(0));
                    break;

                case DataType.String:
                    lock (_stringQueue)
                        _stringQueue.Enqueue(data.GetElementAsString(0));
                    break;

                case DataType.Vector2:
                    lock (_floatQueue)
                    {
                        _floatQueue.Enqueue(data.GetElementAsFloat(0));
                        _floatQueue.Enqueue(data.GetElementAsFloat(1));
                    }
                    break;

                case DataType.Vector3:
                    lock (_floatQueue)
                    {
                        _floatQueue.Enqueue(data.GetElementAsFloat(0));
                        _floatQueue.Enqueue(data.GetElementAsFloat(1));
                        _floatQueue.Enqueue(data.GetElementAsFloat(2));
                    }
                    break;

                case DataType.Vector4:
                    lock (_floatQueue)
                    {
                        _floatQueue.Enqueue(data.GetElementAsFloat(0));
                        _floatQueue.Enqueue(data.GetElementAsFloat(1));
                        _floatQueue.Enqueue(data.GetElementAsFloat(2));
                        _floatQueue.Enqueue(data.GetElementAsFloat(3));
                    }
                    break;

                case DataType.Vector2Int:
                    lock (_intQueue)
                    {
                        _intQueue.Enqueue(data.GetElementAsInt(0));
                        _intQueue.Enqueue(data.GetElementAsInt(1));
                    }
                    break;

                case DataType.Vector3Int:
                    lock (_intQueue)
                    {
                        _intQueue.Enqueue(data.GetElementAsInt(0));
                        _intQueue.Enqueue(data.GetElementAsInt(1));
                        _intQueue.Enqueue(data.GetElementAsInt(2));
                    }
                    break;
            }
        }

        #endregion
    }
}
