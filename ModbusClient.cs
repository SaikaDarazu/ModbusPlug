using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus;

public class ModbusAddress
{
    public int StartAddress { get; set; }
    public int NumberOfPoints { get; set; }
    public int IntervalMs { get; set; } // Frecuencia en milisegundos

    public ModbusAddress(int startAddress, int numberOfPoints, int intervalMs)
    {
        StartAddress = startAddress;
        NumberOfPoints = numberOfPoints;
        IntervalMs = intervalMs;
    }
}

public class ModbusClient
{
    private TcpClient _client;
    private IModbusMaster _master;
    private readonly Action<string> _logMethod; // Delegado para el método de log

    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int SlaveId { get; set; }
    public List<ModbusAddress> AddressesToRead { get; set; } = new List<ModbusAddress>();

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _isConnected = false;

    // Modificamos el constructor para recibir el método de log
    public ModbusClient(string ipAddress, int port, int slaveId, Action<string> logMethod)
    {
        IpAddress = ipAddress;
        Port = port;
        SlaveId = slaveId;
        _logMethod = logMethod; // Guardamos el método de log
        Connect();
    }
    // Método para conectar al cliente Modbus
    private void Connect()
    {
        if (_client != null)
            return; // Ya está conectado

        try
        {
            _client = new TcpClient(IpAddress, Port);
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_client);
            _isConnected = true;
            _logMethod($"Connected to client {IpAddress}:{Port}");
        }
        catch (Exception ex)
        {
            _logMethod($"Failed to connect to client {IpAddress}:{Port} - {ex.Message}");
            _isConnected = false;
        }
    }

    public void Disconnect()
    {
        _cts.Cancel(); // Cancelar todas las tareas en curso
        _cts = new CancellationTokenSource(); // Restablecer para permitir nuevas lecturas
        _client?.Close();
        _client = null;
        _isConnected = false;
        _logMethod($"Disconnected from client {IpAddress}:{Port}");
    }

    // Método para iniciar lecturas en paralelo según la frecuencia de cada dirección
    public void StartReading()
    {
        _logMethod("Iniciando StartReading...");
        foreach (var address in AddressesToRead)
        {
            _logMethod($"Starting reading task for Address: {address.StartAddress}");
            Task.Run(() => StartReadingAddress(address), _cts.Token);
        }
    }

    // Tarea de lectura para una dirección específica con un intervalo configurado
    private async Task StartReadingAddress(ModbusAddress address)
    {
        _logMethod($"Leyendo direcciones...");
        while (!_cts.Token.IsCancellationRequested)
        {
            if (!_isConnected)
            {
                _logMethod($"Attempting to reconnect to {IpAddress}:{Port}");
                Connect();

                if (!_isConnected)
                {
                    await Task.Delay(5000, _cts.Token); // Espera antes de intentar reconectar
                    continue;
                }
            }

            try
            {
                ushort[] registers = _master.ReadHoldingRegisters((byte)SlaveId, (ushort)address.StartAddress, (ushort)address.NumberOfPoints);
                _logMethod($"Read from {IpAddress}:{Port}, Address {address.StartAddress}: {string.Join(", ", registers)}");
            }
            catch (Exception ex)
            {
                _logMethod($"Error reading from client {IpAddress}:{Port} - {ex.Message}");
                _isConnected = false; // Marcar como desconectado para intentar reconectar
            }

            await Task.Delay(address.IntervalMs, _cts.Token);
        }
    }

    // Método de escritura utilizando _logMethod para registrar mensajes
    public bool WriteRegister(int address, ushort value)
    {
        try
        {
            if (!_isConnected)
            {
                _logMethod("Write failed - not connected");
                return false;
            }

            _master.WriteSingleRegister((byte)SlaveId, (ushort)address, value);
            _logMethod($"Write successful to {IpAddress}:{Port}, Address {address} - Value: {value}");
            return true;
        }
        catch (Exception ex)
        {
            _logMethod($"Error writing to client {IpAddress}:{Port} - {ex.Message}");
            _isConnected = false; // Marcar como desconectado si ocurre un error
            return false;
        }
    }
}
