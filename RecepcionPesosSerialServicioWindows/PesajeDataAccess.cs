using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecepcionPesosServicioWindows
{
    public class PesajeDataAccess
    {
        private readonly string _connectionString;

        // Constructor que toma la conexión desde App.config
        public PesajeDataAccess()
        {
            // Leer la cadena de conexión del archivo de configuración
            _connectionString = ConfigurationManager.ConnectionStrings["RecepcionDB"].ConnectionString;
        }

        // Método para insertar datos en la tabla Pesaje
        public void InsertPesaje(DateTime fecha, DateTime hora, decimal pesoBruto, decimal pesoTara, decimal pesoNeto, string unidad)
        {
            string query = @"
            INSERT INTO Pesaje (Fecha, Hora, PesoBruto, PesoTara, PesoNeto, Unidad)
            VALUES (@Fecha, @Hora, @PesoBruto, @PesoTara, @PesoNeto, @Unidad);";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Agregar los parámetros al comando
                    command.Parameters.AddWithValue("@Fecha", fecha);
                    command.Parameters.AddWithValue("@Hora", hora);
                    command.Parameters.AddWithValue("@PesoBruto", pesoBruto);
                    command.Parameters.AddWithValue("@PesoTara", pesoTara);
                    command.Parameters.AddWithValue("@PesoNeto", pesoNeto);
                    command.Parameters.AddWithValue("@Unidad", unidad);

                    // Abrir la conexión y ejecutar el comando
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
