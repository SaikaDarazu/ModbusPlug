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
        private static readonly string DocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static readonly string LogsFolderPath = System.IO.Path.Combine(DocumentsPath, "ModbusPlug", "Logs");
        private static readonly string LogFilePath = System.IO.Path.Combine(LogsFolderPath, "log.txt");


        public MainWindow()
        {
            InitializeComponent();
            EnsureLogsDirectoryExists();
            LoadLastLinesFromLog();
            // Desplaza hacia el final después de un pequeño retraso para asegurar que el RichTextBox esté actualizado
            //Mensaje de que se ha iniciado la aplicación y cargado el log
            Dispatcher.BeginInvoke(new Action(() => StatusRichTextBox.ScrollToEnd()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            LogMessage("Aplicación iniciada y log cargado.");

        }

        // Método para asegurar que las carpetas de logs existan
        private void EnsureLogsDirectoryExists()
        {
            if (!Directory.Exists(LogsFolderPath))
            {
                Directory.CreateDirectory(LogsFolderPath);
            }
        }

        // Método para imprimir mensajes en el archivo de log y la interfaz
        public void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                LoadLastLinesFromLog(); // Actualiza la interfaz de usuario
                
            });
        }

        // Método para cargar las últimas 100 líneas del archivo de log en el RichTextBox
        private void LoadLastLinesFromLog()
        {
            if (File.Exists(LogFilePath))
            {
                var lastLines = File.ReadLines(LogFilePath).Reverse().Take(100).Reverse();
                StatusRichTextBox.Document.Blocks.Clear();
                foreach (var line in lastLines)
                {
                    StatusRichTextBox.Document.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0) });
                }
                StatusRichTextBox.ScrollToEnd();
            }
            else
            {
                File.Create(LogFilePath);
                var lastLines = File.ReadLines(LogFilePath).Reverse().Take(100).Reverse();
                StatusRichTextBox.Document.Blocks.Clear();
                foreach (var line in lastLines)
                {
                    StatusRichTextBox.Document.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0) });
                }
                StatusRichTextBox.ScrollToEnd();

            }
        }
        // Definición del evento para el botón "Run Server"
        private void RunServer_Click(object sender, RoutedEventArgs e)
        {
            // Aquí irá la lógica para iniciar el servidor Modbus
            LogMessage("Run Server button clicked!");
        }


        private ModbusClient _modbusClient;


        // Definición del evento para el botón "Start Client Reading"
        private void StartClientReading_Click(object sender, RoutedEventArgs e)
        {
            if (_modbusClient == null)
            {
                _modbusClient = new ModbusClient("192.168.1.112", 502, 1, LogMessage);

                // Configurar las direcciones a leer con sus intervalos
                _modbusClient.AddressesToRead.Add(new ModbusAddress(0, 1, 1000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                _modbusClient.AddressesToRead.Add(new ModbusAddress(1, 1, 1000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                _modbusClient.AddressesToRead.Add(new ModbusAddress(2, 1, 5000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                // Iniciar la lectura
                LogMessage("Iniciando lectura del cliente...");
                _modbusClient.StartReading();
                LogMessage("Client started reading.");

                /*
                //hago una task en la que espero 5 segundos y luego desconecto el cliente y le hago null

                Task.Delay(5000).ContinueWith(t =>
                {
                    _modbusClient.Disconnect();
                    _modbusClient = null;
                });
                */
            }
            else
            {
                LogMessage("Client is already reading.");
            }

        }

    }
}