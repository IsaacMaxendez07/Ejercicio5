﻿/*En este primer bloque, observamos todas las librerías que tiene el programa.
 Entre ellas, se encuentra la librería de la base de datos, así como otras librerías
  genéricas. */

//Primer bloque
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Npgsql;
///////////////////////////////////////////////////////////////////////////////////////////////////////

//Segundo bloque 

/* En este segundo bloque abordamos todo el tema de configuración de accesos a los 
datos y conexión a estos mismos.*/
class Program
{
    static IConfigurationRoot Configuration { get; set; }// Propiedad para acceder 
    // a la configuración cargada desde un archivo JSON
    static string connectionString; // Cadena de conexión construida desde el 
    // archivo de configuración
    static void Main()
    {
        LoadConfiguration();// Cargar configuración desde archivo JSON

        if (!TryConnectToDatabase())  // Intentar conectar a la base de datos
        {
            Console.WriteLine("La conexión a la base de datos ha fallado. El programa se cerrará.");
            return;
        }

        Console.WriteLine("\nBienvenido al sistema de gestión de pagos."); // Mensaje de bienvenida

        bool usingMachine = true;

        //Tercer bloque 

        /* En este tercer bloque se crea un menú principal que, dependiendo de la opción, el switch 
        realizará una acción u otra. Estas opciones son:
        Ver facturas, Añadir una nueva factura, Ver balances de cuentas, Añadir 
        una nueva cuenta bancaria, Pagar facturas pendientes*/

        while (usingMachine)// Bucle principal del menú
        {
            Console.WriteLine("\n¿Qué deseas hacer?");
            Console.WriteLine("1. Ver facturas");
            Console.WriteLine("2. Añadir factura");
            Console.WriteLine("3. Ver balance de cuentas");
            Console.WriteLine("4. Añadir cuenta");
            Console.WriteLine("5. Pagar facturas");
            Console.WriteLine("6.Eliminar facturas");
            Console.WriteLine("7.Eliminar cuenta" );
            Console.WriteLine("8. Salir");
            Console.Write("Elige una opción: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    ViewInvoices();
                    break;

                case "2":
                    AddInvoice();
                    break;

                case "3":
                    ViewAccountBalances();
                    break;

                case "4":
                    AddAccount();
                    break;

                case "5":
                    PayInvoices();
                    break;

                case "6":
                    DeleteInvoices(); // Este es la nueva opcion que hemos hecho
                    break; 

                case "7":
                    DeleteAccount(); //Estamos incluyendo la opción de eliminar la cuenta.
                    break; 

                case "8":
                    Console.WriteLine("Saliendo del programa...");
                    usingMachine = false;
                    break;

                default:
                    Console.WriteLine("Opción no válida. Intenta de nuevo.");
                    break;
            }
        }
    }

    //Cuarto bloque

    /*En este bloque lo que hacemos es cargar configuración desde el archivo json/settings.json*/

    static void LoadConfiguration()
    {
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())// Establece el directorio base
                .AddJsonFile("json/settings.json", optional: false, reloadOnChange: true);// Agrega archivo JSON obligatorio

            Configuration = builder.Build();
            //usa los valores del archivo JSON
            connectionString = $"Host={Configuration["DatabaseConfig:Host"]};" +
                               $"Port={Configuration["DatabaseConfig:Port"]};" +
                               $"Username={Configuration["DatabaseConfig:Username"]};" +
                               $"Password={Configuration["DatabaseConfig:Password"]};" +
                               $"Database={Configuration["DatabaseConfig:Database"]};";
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error al cargar la configuración: {e.Message}. El programa se cerrará.");
            Environment.Exit(1); // El '1' indica que el programa terminó con un error.
}
    }
    // Quinto bloque 
        /*En este bloque intentamos conectar con la base de datos y controlamos posibles errores de conexión mediante excepciones. */
    static bool TryConnectToDatabase()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();// Abre la conexión
                    return true;// Conexión exitosa
                }
            }
            catch (Exception ex)
            {    // Muestra el mensaje de error en caso de fallo
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }
    /*En este apartado comenzaremos de nuevo con el conteo de los bloques, ya que aquí definiremos la lógica del menú.*/
    /*Ademas debes de tomar en cuenta que veras lineas de codigo comentada solo una vez porque esa misma linea de codigo se repite*/

    // Bloque 1 facturas pendientes
    static void ViewInvoices()
    {
        Console.WriteLine("\nFacturas pendientes:"); // pides una factaura al usuario 
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open(); // Abrir conexión a la base de datos
            string query = "SELECT paymentid, paymentdescription, paymentamount, paymentdue FROM payments WHERE paymentcompleted = FALSE";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                int index = 1;
                while (reader.Read()) // Leer cada fila del resultado
                {
                    Console.WriteLine($"{index}. {reader["paymentdescription"]} - Monto: {reader["paymentamount"]} - Fecha de vencimiento: {reader["paymentdue"]}");
                    index++;
                }
            }
        }
    }
    // Bloque 2 Añadir una nueva factura a la base de datos
    static void AddInvoice()

    {
         // Declara las variables fuera del try
        string description;
        decimal amount;
        DateTime dueDate;
        
        try
        {
            Console.WriteLine("\nAñadir nueva factura:"); // Le damos la opción de agregar una factura

            Console.Write("Descripción de la factura: "); // Leer descripción
            description = Console.ReadLine();

            Console.Write("Monto de la factura: ");
            amount = decimal.Parse(Console.ReadLine()); //Leer monto 

            Console.Write("Fecha de vencimiento (yyyy-mm-dd): ");
            dueDate = DateTime.Parse(Console.ReadLine());
        }

        catch (FormatException ex)
        {
            Console.WriteLine("Error: El monto o la fecha ingresada no tienen el formato correcto. Asegúrate de usar números para el monto y el formato yyyy-mm-dd para la fecha.");
            return;
        }  

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();// Abrir conexión
            //// Query de inserción
            string query = "INSERT INTO payments (paymentdescription, paymentamount, paymentdue) VALUES (@description, @amount, @dueDate)";
            using (var command = new NpgsqlCommand(query, connection))
            {
                //inyección SQL
                command.Parameters.AddWithValue("description", description);
                command.Parameters.AddWithValue("amount", amount);
                command.Parameters.AddWithValue("dueDate", dueDate);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Factura añadida correctamente.");
    }
    // Bloque 3 mostrar los balances de todas las cuentas bancarias
    static void ViewAccountBalances()
    {
        Console.WriteLine("\nBalance de cuentas:");

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT accountname, accountbalance FROM bankaccount";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) // Leer cada cuenta
                {
                    Console.WriteLine($"{reader["accountname"]}: {reader["accountbalance"]}");
                }
            }
        }
    }
    // Bloque 4 Añadir una nueva cuenta bancaria
    static void AddAccount()
    {
        Console.WriteLine("\nAñadir nueva cuenta:"); // Se pide que añade una cuenta 

        Console.Write("Nombre de la cuenta: ");
        string accountName = Console.ReadLine();
        // Declara la variable fuera del try
        decimal accountBalance; /* Esta tipo de dato lo tuve que declarar global porque la excepcion no la pillaba 
         esto lo hice en todas las funciones que tuve que hacer una excepcion2*/
        try
        {
            Console.Write("Balance de la cuenta: ");
            accountBalance = decimal.Parse(Console.ReadLine());
        }
        catch (FormatException ex)
        {
            Console.WriteLine("Error: El balance de la cuenta no tiene un formato numérico válido. Por favor, ingresa un número.");
            return;
        }
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "INSERT INTO bankaccount (accountname, accountbalance) VALUES (@accountName, @accountBalance)";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("accountName", accountName);
                command.Parameters.AddWithValue("accountBalance", accountBalance);
                command.ExecuteNonQuery();
            }
        }

        Console.WriteLine("Cuenta añadida correctamente.");
    }
    // Bloque 5 permitir pagar facturas y descontar el total de una cuenta bancaria
    static void PayInvoices()
    {
        Console.WriteLine("\nSelecciona las facturas que deseas pagar:");

        List<int> unpaidInvoiceIds = new List<int>(); // Lista de IDs 
        decimal totalAmountToPay = 0; // contador de factura 

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT paymentid, paymentdescription, paymentamount FROM payments WHERE paymentcompleted = FALSE";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                int index = 1;
                while (reader.Read())
                {
                    Console.WriteLine($"{index}. {reader["paymentdescription"]} - Monto: {reader["paymentamount"]}");
                    unpaidInvoiceIds.Add((int)reader["paymentid"]);
                    totalAmountToPay += (decimal)reader["paymentamount"];
                    index++;
                }
            }
        }

      




   

        Console.Write("Ingresa el número de la factura a pagar o 0 para cancelar: ");
        
            int invoiceToPay = int.Parse(Console.ReadLine());
        while (invoiceToPay != 0)
        {
            if (invoiceToPay > 0 && invoiceToPay <= unpaidInvoiceIds.Count)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE payments SET paymentcompleted = TRUE WHERE paymentid = @paymentId";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("paymentId", unpaidInvoiceIds[invoiceToPay - 1]);
                        command.ExecuteNonQuery(); // Aqui marcamos la factura como marcada 
                    }
                }

                Console.WriteLine($"Factura {invoiceToPay} pagada.");
            }
            else
            {
                Console.WriteLine("Número de factura no válido.");
            }

            Console.Write("Ingresa el número de la factura a pagar o 0 para cancelar: ");
            invoiceToPay = int.Parse(Console.ReadLine());
        }
        // Bloque 6 seleccionar cuenta para pagar
        Console.WriteLine("\nSelecciona la cuenta para pagar:");
        ViewAccountBalances();

        Console.Write("Ingresa el nombre de la cuenta: ");
        string accountToUse = Console.ReadLine();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT accountbalance FROM bankaccount WHERE accountname = @accountName";
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("accountName", accountToUse);
                decimal accountBalance = (decimal)command.ExecuteScalar();

                if (accountBalance >= totalAmountToPay)
                {
                    decimal newBalance = accountBalance - totalAmountToPay;

                    string updateBalanceQuery = "UPDATE bankaccount SET accountbalance = @newBalance WHERE accountname = @accountName";
                    using (var updateCommand = new NpgsqlCommand(updateBalanceQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("newBalance", newBalance);
                        updateCommand.Parameters.AddWithValue("accountName", accountToUse);
                        updateCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Pago realizado con éxito. Nuevo balance de la cuenta '{accountToUse}': {newBalance}");
                }
                else
                {
                    Console.WriteLine("Saldo insuficiente en la cuenta para realizar el pago.");
                }
            }
        }
    }
      //////////////////////////////////////////////////////////
        /*   Aqui estamos colocando la nueva opcion */
    
      static void DeleteInvoices()
    {
        Console.WriteLine("\nEliminar facturas:"); // Le pedimos al usuario la factura que quiere eliminar 
        List<int> invoiceIdsToDelete = new List<int>();// aqui almacenamos los paymentid

        using (var connection = new NpgsqlConnection(connectionString)) // abrimos la base de datos 
        {
            connection.Open();
            string query = "SELECT paymentid, paymentdescription, paymentamount FROM payments";
            using (var command = new NpgsqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                int index = 1;
                if (!reader.HasRows) // Añadido para manejar el caso de no facturas
                {
                    Console.WriteLine("No hay facturas para eliminar.");
                    return;
                }
                while (reader.Read())
                {
                    Console.WriteLine($"{index}. {reader["paymentdescription"]} - Monto: {reader["paymentamount"]}");
                    invoiceIdsToDelete.Add((int)reader["paymentid"]);
                    index++;
                }
            }
        }
        ////////////////////////////Nueva funcion de eliminar cuentas ///////////////////////////
    
    // Bloque 7 Eliminar cuenta bancaria
       static void DeleteAccount()
        {
            Console.WriteLine("\nEliminar cuenta bancaria:");
            List<int> accountIdsToDelete = new List<int>(); // Aquí almacenaremos los IDs de las cuentas

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT accountid, accountname, accountbalance FROM bankaccount";
                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    int index = 1;
                    if (!reader.HasRows) // Añadido para manejar el caso de no facturas
                    {
                        Console.WriteLine("No hay cuentas para eliminar.");
                        return;
                    }
                    while (reader.Read())
                    {
                        Console.WriteLine($"{index}. {reader["accountname"]} - Monto: {reader["accountbalance"]}");
                        accountIdsToDelete.Add((int)reader["accountid"]);
                        index++;
                    }
                }
            }

            Console.WriteLine("ingrese el número de la cuenta que desea eliminar");
            Console.WriteLine("ingresar 0 para cancelar la operación");
            string respuesta = Console.ReadLine();
            int conversionExitosa;
            if (!int.TryParse(respuesta, out conversionExitosa))
            {
                Console.WriteLine("Error: La entrada no es un número válido. Por favor, intenta de nuevo.");
                return;
            }
            if (conversionExitosa == 0)
            {
                Console.WriteLine("Operación ha sido cancelada.");
                return;
            }
            if (conversionExitosa < 1 || conversionExitosa > accountIdsToDelete.Count)
            {
                Console.WriteLine("Número de cuenta no válido. Por favor, intenta de nuevo.");
                return;
            }
            int idCuentaParaEliminar;
            idCuentaParaEliminar = accountIdsToDelete[conversionExitosa - 1];
            Console.WriteLine("Estas seguro que quiere eliminar la cuenta SI o NO");
            string confirmacionDeeliminar = Console.ReadLine();
            if (confirmacionDeeliminar.ToUpper() != "SI")
            {
                Console.WriteLine("Operación de eliminación de cuenta cancelada.");
                return;
            }
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT accountid, accountname, accountbalance FROM bankaccount WHERE accountid != @idCuentaParaExcluir";
                using (var command = new NpgsqlCommand(query, connection))
                command.Parameters.AddWithValue("@idCuentaParaExcluir", idCuentaParaEliminar);
                using (var reader = command.ExecuteReader())
                {
                    List<int> otrosIDdecuenta = new List<int>();
                    while (reader.Read())
                    {

                        otrosIDdecuenta.Add((int)reader["accountid"]);
                    }
                    if (otrosIDdecuenta.Count > 0)
                    {
                        Console.WriteLine("transferir los fondos de la cuenta que va a eliminar a otra de sus cuentas existentes SI o no");
                    }
                    else
                    {
                        Console.WriteLine("como no tiene otras cuentas, el dinero de la cuenta que va a eliminar se perderá");
                    }
                    string transferirFondosRespuesta = Console.ReadLine();
                    if (transferirFondosRespuesta.ToUpper() != "SI")
                    {
                        Console.WriteLine("Transferencia de fondos cancelada.");
                    }
                    else
                    {
                        Console.WriteLine("a a listar las cuentas disponibles para la transferencia");
                    }
                    for (int i = 0; i < otrosIDdecuenta.Count; i++)
                    {
                        Console.WriteLine($"{i + 1}. Cuenta ID: {otrosIDdecuenta[i]}");
                    }
                    Console.WriteLine("ingrese el NÚMERO de la opción de la cuenta a la que desea transferir los fondos");
                    string seleccionDestinoStr = Console.ReadLine();
                    int seleccionDestinoIndex;
                    if (!int.TryParse(seleccionDestinoStr, out seleccionDestinoIndex))
                    {
                        Console.WriteLine("Error: La entrada no es un número válido. Por favor, ingresa un número.");
                        return;
                    }
                     if (seleccionDestinoIndex < 1 || seleccionDestinoIndex > otrosIDdecuenta.Count)
                    {
                         Console.WriteLine("Error: El número de cuenta de destino no es válido. Por favor, selecciona una de las opciones listadas.");
                        return;
                     

                }
                

        }      
        ////////////////////////////////////////////////////////////////////////////////////////////////////
            int newDelete = int.Parse(Console.ReadLine()!);
        if (newDelete == 0)
        {
            Console.WriteLine("Operación Cancelada.");
        }
        else if (newDelete > 0 && newDelete <= invoiceIdsToDelete.Count)
        {
            int IdDeFacturaAEliminar = invoiceIdsToDelete[newDelete - 1];
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM payments WHERE paymentid = @parametroAEliminar";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@parametroAEliminar", IdDeFacturaAEliminar);
                    command.ExecuteNonQuery();
                    Console.WriteLine("Factura eliminada correctamente.");
                }
            }
        }
        else
        {
            Console.WriteLine("Número de factura no válido. Por favor, intenta de nuevo.");
        }
    }

}




        

