using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RecepcionPesosServicioWindows
{
    public class RecepcionController
    {
        static List<Thread> threads;
        static List<TcpClient> listaDeClientes;
        private static volatile bool detenerHilos = false;
        private SerialPort _serialPort;
        PesajeDataAccess dataAccess;
        private List<string> _receivedLines;
        private const int LineCount = 5;

        private static StringBuilder dataBuffer;
        public RecepcionController()
        {
            dataAccess= new PesajeDataAccess();
            _receivedLines = new List<string>();
            dataBuffer = new StringBuilder();
        }

        
        
        public void SerialPortReceiver(string portName)
        {
            // Inicializar el puerto serial
            _serialPort = new SerialPort
            {
                PortName = portName,  // Nombre del puerto (COM1, COM2, etc.)
                BaudRate = 9600,  // Velocidad de transmisión
                Parity = Parity.None, // Paridad
                DataBits = 8,         // Bits de datos
                StopBits = StopBits.One, // Bits de parada
                Handshake = Handshake.None, // Sin control de flujo
                Encoding = Encoding.ASCII, // Codificación de los datos
                DtrEnable = true,
                RtsEnable = true
            };

            // Asignar el evento para recibir datos
            _serialPort.DataReceived += OnDataReceived;
            _serialPort.Open();
        }
        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();
            dataBuffer.Append(data);

            // Detectar fin del bloque (varios saltos de línea consecutivos)
            if (dataBuffer.ToString().Contains("\r\n\r\n\r\n\r\n"))
            {
                string completeData = dataBuffer.ToString();
                dataBuffer.Clear(); // Limpiar el buffer después de procesar

                // Procesar el bloque completo
                ProcessCompleteData(completeData);
            }
        }
 
        private void ProcessCompleteData(string data)
        {
            try
            {
                // Divide el bloque en líneas
                string[] lines = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Variables para almacenar los valores extraídos
                string fecha = "";
                string hora = "";
                string bruto = "";
                string tara = "";
                string neto = "";
                DateTime date = DateTime.MinValue;
                DateTime time = DateTime.MinValue;
                decimal gross = 0;
                decimal tare = 0;
                decimal net = 0;
                string unit = string.Empty;


                // Itera sobre las líneas para extraer valores
                foreach (string line in lines)
                {
                    if (line.StartsWith("Date"))
                        fecha = line.Substring(5).Trim();
                    else if (line.StartsWith("Time"))
                        hora = line.Substring(5).Trim();
                    else if (line.StartsWith("Gross"))
                        bruto = line.Substring(6).Trim();
                    else if (line.StartsWith("Tare"))
                        tara = line.Substring(5).Trim();
                    else if (line.StartsWith("Net"))
                        neto = line.Substring(4).Trim();
                }

                foreach (string line in lines)
                {
                    Console.WriteLine(line);
                    date = DateTime.ParseExact(fecha, "yyyy.MM.dd", null);
                    time = DateTime.Parse(hora);
                    gross = decimal.Parse(bruto.Replace("kg", "").Replace('.', ',').Trim());
                    tare = decimal.Parse(tara.Replace("kg", "").Replace('.', ',').Trim());
                    net = decimal.Parse(neto.Replace("kg", "").Replace('.', ',').Trim());

                    unit = lines[4].Substring(lines[4].Length - 2);
                }
                dataAccess.InsertPesaje(date, time, gross, tare, net, unit);
            }catch(Exception ex)
            {
                throw new Exception("Se produjo un error: "+ ex.Message);
            }
            
        }
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
    }
}
