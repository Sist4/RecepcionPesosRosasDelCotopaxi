using RecepcionPesosServicioWindows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace RecepcionPesosSerialServicioWindows
{
    public partial class Service1 : ServiceBase
    {
        RecepcionController recepcion;
        public Service1()
        {
            InitializeComponent();
            recepcion = new RecepcionController();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                string parametro = GetValueOrDefault(args);
                string puerto = GetAvailableSerialPort(parametro);
                recepcion.SerialPortReceiver(puerto);
                Log($"Service started with parameter: {parametro} Serial Port {puerto}");

            }
            catch (Exception ex)
            {
                // Log o manejar la excepción
                EventLog.WriteEntry($"Error en OnStart: {ex.Message}", EventLogEntryType.Error);
                Log($"Error: {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            recepcion.Close();
        }
        private void Log(string message)
        {
            // Obtener el directorio del ejecutable del servicio
            string logDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logPath = System.IO.Path.Combine(logDirectory, "Log.txt");

            System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        
        string GetValueOrDefault(string[] valores)
        {
            // Verificar si el array tiene al menos un elemento
            if (valores != null && valores.Length > 0)
            {
                return valores[0]; // Retornar el primer elemento si existe
            }
            else
            {
                // Retornar un string vacío si no hay elementos
                return string.Empty;
            }


        }
        static string GetAvailableSerialPort(string preferredPort)
        {
            // Obtener el puerto desde App.config
            string configPort = ConfigurationManager.AppSettings["SerialPort"];

            // Si se pasa un puerto preferencial como parámetro
            if (!string.IsNullOrEmpty(preferredPort))
            {
                if (IsPortAvailable(preferredPort))
                {
                    UpdateAppConfig("SerialPort", preferredPort); // Guardar el puerto en App.config
                    return preferredPort;
                }
                else
                {
                    Console.WriteLine($"El puerto preferido '{preferredPort}' no está disponible.");
                }
            }

            // Verificar el puerto en App.config
            if (!string.IsNullOrEmpty(configPort) && IsPortAvailable(configPort))
            {
                return configPort;
            }

            // Buscar el primer puerto disponible en el sistema
            string[] availablePorts = SerialPort.GetPortNames();
            if (availablePorts.Length > 0)
            {
                string firstAvailablePort = availablePorts[0];
                UpdateAppConfig("SerialPort", firstAvailablePort); // Guardar en App.config
                return firstAvailablePort;
            }

            throw new Exception("No hay puertos COM disponibles en el sistema.");
        }

        static bool IsPortAvailable(string portName)
        {
            try
            {
                using (SerialPort serialPort = new SerialPort(portName))
                {
                    serialPort.Open();
                    serialPort.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        static void UpdateAppConfig(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Console.WriteLine($"Actualizado el archivo App.config: {key} = {value}");
        }
    }
}
