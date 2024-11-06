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
            string logEntry = $"{DateTime.Now:HH:mm:ss} - {message}";
            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            LoadLastLinesFromLog();
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
                    StatusRichTextBox.Document.Blocks.Add(new Paragraph(new Run(line)) {Margin = new Thickness(0) });
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
                _modbusClient = new ModbusClient("192.168.1.10", 502, 1, LogMessage);

                // Configurar las direcciones a leer con sus intervalos
                _modbusClient.AddressesToRead.Add(new ModbusAddress(0, 10, 1000));  // Lee 10 registros desde la dirección 0 cada 1 segundo
                _modbusClient.AddressesToRead.Add(new ModbusAddress(20, 5, 5000));  // Lee 5 registros desde la dirección 20 cada 5 segundos

                // Iniciar la lectura
                _modbusClient.StartReading();
                LogMessage("Client started reading.");
            }
            else
            {
                LogMessage("Client is already reading.");
            }

        }

    }
}