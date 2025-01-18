using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//Para leer el archivo de texto del LOG
using System;
using System.IO;
using System.Linq;


namespace ModbusPlug
{
    public partial class MainWindow : Window
    {
        //Variables Globales del MainWindow
        //Variable del log
        private static readonly string RutaMisDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string RutaCarpetaLogs = System.IO.Path.Combine(RutaMisDocumentos, "ModbusPlug", "Logs");
        private static readonly string RutaArchivoLogs = System.IO.Path.Combine(RutaCarpetaLogs, "log.txt");


        public MainWindow()
        {
            InitializeComponent();
            AsegurarDirectorioLogs();
            CargarLog();
            
            // Desplaza hacia el final después de un pequeño retraso para asegurar que el RichTextBox esté actualizado
            //Mensaje de que se ha iniciado la aplicación y cargado el log
            Dispatcher.BeginInvoke(new Action(() => TextBoxLogs.ScrollToEnd()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            
            EscribirLog("Aplicación iniciada y log cargado.");

        }

        // Método para asegurar que las carpetas de logs existan
        private void AsegurarDirectorioLogs()
        {
            if (!Directory.Exists(RutaCarpetaLogs))
            {
                Directory.CreateDirectory(RutaCarpetaLogs);
            }
        }

        // Método para imprimir mensajes en el archivo de log y la interfaz
        public void EscribirLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
                File.AppendAllText(RutaArchivoLogs, logEntry + Environment.NewLine);
                CargarLog(); // Actualiza la interfaz de usuario
                
            });
        }

        // Método para cargar las últimas 100 líneas del archivo de log en el RichTextBox
        private void CargarLog()
        {
            if (File.Exists(RutaArchivoLogs))
            {
                var lastLines = File.ReadLines(RutaArchivoLogs).Reverse().Take(100).Reverse();
                TextBoxLogs.Document.Blocks.Clear();
                foreach (var line in lastLines)
                {
                    TextBoxLogs.Document.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0) });
                }
                TextBoxLogs.ScrollToEnd();
            }
            else
            {
                File.Create(RutaArchivoLogs);
                var lastLines = File.ReadLines(RutaArchivoLogs).Reverse().Take(100).Reverse();
                TextBoxLogs.Document.Blocks.Clear();
                foreach (var line in lastLines)
                {
                    TextBoxLogs.Document.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0) });
                }
                TextBoxLogs.ScrollToEnd();

            }
        }
        // Definición del evento para el botón "Run Server"
        private void RunServer_Click(object sender, RoutedEventArgs e)
        {
            // Aquí irá la lógica para iniciar el servidor Modbus
            EscribirLog("Run Server button clicked!");
        }


        private ClienteModbus? _clienteModbus;


        // Definición del evento para el botón "Start Client Reading"
        private void LecturaClientes_Boton(object sender, RoutedEventArgs e)
        {
            if (_clienteModbus == null)
            {
                _clienteModbus = new ClienteModbus("192.168.1.106", 502, 1, EscribirLog);

                // Configurar las direcciones a leer con sus intervalos
                _clienteModbus.DireccionDeLectura.Add(new DireccionModbus(0, 10, 1000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                _clienteModbus.DireccionDeLectura.Add(new DireccionModbus(1, 1, 1000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                // Iniciar la lectura
                EscribirLog("Iniciando lectura del cliente...");
                _clienteModbus.LecturaCliente();
                EscribirLog("Client started reading.");
                
            }
            else
            {
                EscribirLog("La lectura de clientes ya esta activada");
            }

        }

        private void DesconectarClientes_Boton(object sender, RoutedEventArgs e)
        {
            if (_clienteModbus == null)
            {
                EscribirLog("La lectura de clientes no esta activada");
            }
            else
            {
                _clienteModbus.DesconectarClientes();   
                _clienteModbus = null;
                EscribirLog("Lectura de clientes desactivada.");
            }
            
        }
        
    }
}