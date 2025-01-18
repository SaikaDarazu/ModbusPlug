using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NModbus;
using static System.Text.StringBuilder;

public class DireccionModbus
{
    public int DireccionDeArranque { get; set; }
    public int NumeroDirecciones { get; set; }
    public int IntervaloMs { get; set; } // Frecuencia en milisegundos

    public DireccionModbus(int direccionDeArranque, int numeroDirecciones, int intervaloMs)
    {
        DireccionDeArranque = direccionDeArranque;
        NumeroDirecciones = numeroDirecciones;
        IntervaloMs = intervaloMs;
    }
}

public class ClienteModbus
{
    private TcpClient? _clienteTcp;
    private IModbusMaster _master;
    private readonly Action<string> _logMethod; // Delegado para el método de log

    public string DireccionIp { get; set; }
    public int Puerto { get; set; }
    public int ExclavoID { get; set; }
    public List<DireccionModbus> DireccionDeLectura { get; set; } = new List<DireccionModbus>();

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private bool _estaConectado = false;

    // Modificamos el constructor para recibir el método de log
    public ClienteModbus(string direccionIp, int puerto, int exclavoId, Action<string> logMethod)
    {
        DireccionIp = direccionIp;
        Puerto = puerto;
        ExclavoID = exclavoId;
        _logMethod = logMethod; // Guardamos el método de log
        ConectarCliente();
    }
    // Método para conectar al cliente Modbus
    private void ConectarCliente()
    {
        if (_clienteTcp != null)
            return; // Ya está conectado
        try
        {
            _clienteTcp = new TcpClient(DireccionIp, Puerto);
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_clienteTcp);
            _estaConectado = true;
            _logMethod($"Conectado al cliente {DireccionIp}:{Puerto}");
        }
        catch (Exception ex)
        {
            _logMethod($"No se puede conectar al cliente {DireccionIp}:{Puerto} - {ex.Message}");
            _estaConectado = false;
        }
    }

    public void DesconectarClientes()
    {
        _cts.Cancel(); // Cancelar todas las tareas en curso
        _clienteTcp?.Close();
        _clienteTcp = null;
        _estaConectado = false;
        //Espera 1 segundo
        Thread.Sleep(1000); // Esperar 1 segundo (1000 ms)
        _cts = new CancellationTokenSource(); // Restablecer para permitir nuevas lecturas
        _logMethod($"Desconectado del cliente: {DireccionIp}:{Puerto}");
    }

    // Método para iniciar lecturas en paralelo según la frecuencia de cada dirección
    public void LecturaCliente()
    {
        _logMethod("Iniciando lectura de datos...");
        foreach (var address in DireccionDeLectura)
        {
            _logMethod($"Empezando lectura de la direccion: {address.DireccionDeArranque}");
            Task.Run(() => TareaLecturaCliente(address), _cts.Token);
        }
    }

    // Tarea de lectura para una dirección específica con un intervalo configurado
    private async Task TareaLecturaCliente(DireccionModbus address)
    {
        _logMethod($"Leyendo direcciones...");
        while (!_cts.Token.IsCancellationRequested)
        {
            if (!_estaConectado)
            {
                _logMethod($"Intentando reconectarse al cliente {DireccionIp}:{Puerto}");
                ConectarCliente();

                if (!_estaConectado)
                {
                    await Task.Delay(5000, _cts.Token); // Espera antes de intentar reconectar
                    continue;
                }
            }

            try
            {
                ushort[] registros = _master.ReadHoldingRegisters((byte)ExclavoID, (ushort)address.DireccionDeArranque, (ushort)address.NumeroDirecciones);
                
                // Usar StringBuilder para construir el mensaje completo de manera eficiente
                var mensaje = new StringBuilder();
                mensaje.AppendLine($"Lectura de {DireccionIp}:{Puerto}, Direcciones desde {address.DireccionDeArranque}:");
                // Agregar los valores de los registros en una sola línea
                for (int i = 0; i < registros.Length; i++)
                {
                    mensaje.AppendLine($"  Dirección {address.DireccionDeArranque + i}: {registros[i]}");
                }
                _logMethod(mensaje.ToString());
                
            }
            catch (Exception ex)
            {
                _logMethod($"Error reading from client {DireccionIp}:{Puerto} - {ex.Message}");
                _estaConectado = false; // Marcar como desconectado para intentar reconectar
            }

            await Task.Delay(address.IntervaloMs, _cts.Token);
        }
    }

    // Método de escritura utilizando _logMethod para registrar mensajes
    public bool EscribirRegistros(int address, ushort value)
    {
        try
        {
            if (!_estaConectado)
            {
                _logMethod("Write failed - not connected");
                return false;
            }

            _master.WriteSingleRegister((byte)ExclavoID, (ushort)address, value);
            _logMethod($"Write successful to {DireccionIp}:{Puerto}, Address {address} - Value: {value}");
            return true;
        }
        catch (Exception ex)
        {
            _logMethod($"Error writing to client {DireccionIp}:{Puerto} - {ex.Message}");
            _estaConectado = false; // Marcar como desconectado si ocurre un error
            return false;
        }
    }
}
