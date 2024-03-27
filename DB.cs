using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Test
{
    internal class DB : DbContext
    {
        string Cstring { get; set; }

        public DB()
        { }

        public DB(string Connection) 
        { 
            Cstring = Connection;
        }

        //Este metodo hace toda la magia, crea la base de datos si no existe y devuelve el string de conexion para la base de datos
        //Tambien maneja la creacion de los sp y la view
        public string Start()
        {
            try
            {
                string cmdText = "SELECT name FROM master.dbo.sysdatabases WHERE name = 'Test'";
                SqlConnectionStringBuilder builder;
                string bdcstring;
                using (SqlConnection connection = new SqlConnection(Cstring))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand(cmdText, connection))
                    {
                        string dbName = (string)cmd.ExecuteScalar();

                        if (dbName != null)
                        {
                            MessageBox.Show("La base de datos 'Test' ya existe!");
                            builder = new SqlConnectionStringBuilder(Cstring);
                            bdcstring = $"Data Source={builder.DataSource};Initial Catalog=Test;Integrated Security={builder.IntegratedSecurity};Encrypt={builder.Encrypt}";
                            connection.Close();
                            return bdcstring;
                        }
                        else
                        {

                            using (SqlCommand commandcreate = new SqlCommand("CREATE DATABASE Test;", connection))
                            {
                                commandcreate.ExecuteNonQuery();
                            }

                            using (SqlCommand commanduse = new SqlCommand("USE Test;", connection))
                            {
                                commanduse.ExecuteNonQuery();
                            }

                            using (SqlCommand commandtables = new SqlCommand(@"
                                CREATE TABLE TipoProducto (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                Descripcion NVARCHAR(50)
                            );

                                CREATE TABLE Producto (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                IdTipoProducto INT FOREIGN KEY REFERENCES TipoProducto(Id),
                                Nombre NVARCHAR(50),
                                Precio FLOAT(24)
                            );

                                CREATE TABLE Stock (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                IdProducto INT FOREIGN KEY REFERENCES Producto(Id),
                                Cantidad INT
                                );
                                ", connection))
                            {
                                commandtables.ExecuteNonQuery();
                            }
                            string CrearView = @"
                                CREATE VIEW vw_StockProducto AS
                                SELECT Producto.Id, Producto.Nombre, Stock.Cantidad, Producto.Precio
                                FROM Producto
                                JOIN Stock ON Producto.Id = Stock.IdProducto 
                                ";
                            using (SqlCommand commandview = new SqlCommand(CrearView, connection))
                            {
                                commandview.ExecuteNonQuery();
                            }

                            string sp_InsertarProducto = @"
                                CREATE PROCEDURE sp_InsertarProducto
                                    @IdTipoProducto INT,
                                    @Cantidad INT,
                                    @Nombre NVARCHAR(50),
                                    @Precio FLOAT
                                AS
                                BEGIN
                                    INSERT INTO Producto(IdTipoProducto, Nombre, Precio)
                                    VALUES (@IdTipoProducto, @Nombre, @Precio);

                                    DECLARE @IdProducto INT;
                                    SET @IdProducto = SCOPE_IDENTITY();

                                    INSERT INTO Stock(IdProducto, Cantidad)
                                    VALUES (@IdProducto, @Cantidad);
                                END";

                            using (SqlCommand commandinsertar = new SqlCommand(sp_InsertarProducto, connection))
                            {
                                commandinsertar.ExecuteNonQuery();
                            }

                            string sp_ModificarProducto = @"
                                CREATE PROCEDURE sp_ModificarProducto
                                    @Id INT,
                                    @IdTipoProducto INT = NULL,
                                    @Cantidad INT = NULL,
                                    @Nombre NVARCHAR(50) = NULL,
                                    @Precio FLOAT = NULL
                                AS
                                BEGIN
                                    UPDATE Producto
                                    SET IdTipoProducto = ISNULL(@IdTipoProducto, IdTipoProducto), 
                                        Nombre = ISNULL(@Nombre, Nombre),
                                        Precio = ISNULL(@Precio, Precio)
                                    WHERE ID = @Id;

                                    UPDATE Stock
                                    SET Cantidad = ISNULL(@Cantidad, Cantidad)
                                    WHERE IdProducto = @Id;
                                END";

                            using (SqlCommand commandmodificar = new SqlCommand(sp_ModificarProducto, connection))
                            {
                                commandmodificar.ExecuteNonQuery();
                            }

                            string sp_EliminarProducto = @"
                                CREATE PROCEDURE sp_EliminarProducto
                                    @Id INT
                                AS
                                BEGIN
                                    DELETE FROM Stock
                                    WHERE IdProducto = @Id;

                                    DELETE FROM Producto
                                    WHERE ID = @Id;
                                END";

                            using (SqlCommand commandeliminar = new SqlCommand(sp_EliminarProducto, connection))
                            {
                                commandeliminar.ExecuteNonQuery();
                            }

                            InsertProductType(connection, "Comestibles");
                            InsertProductType(connection, "Electronicos");
                            InsertProductType(connection, "Ropa");
                            InsertProductType(connection, "Electrodomesticos");
                            InsertProductType(connection, "Cuidado Personal");

                            static void InsertProductType(SqlConnection connection, string Descripcion)
                            {
                                using (SqlCommand command = new SqlCommand("INSERT INTO TipoProducto (Descripcion) VALUES (@Descripcion)", connection))
                                {
                                    command.Parameters.AddWithValue("@Descripcion", Descripcion);
                                    command.ExecuteNonQuery();
                                }
                            }

                            builder = new SqlConnectionStringBuilder(Cstring);
                            bdcstring = $"Data Source={builder.DataSource};Initial Catalog=Test;Integrated Security={builder.IntegratedSecurity};Encrypt={builder.Encrypt}";
                            connection.Close();
                            return bdcstring;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return "";
            }
            
        }

        //Metodo para el ViewModel para obtener los datos del view creado anteriormente
        public DataTable GetDataFromView()
        {
            Cstring = "Data Source=.;Integrated Security=True;Encrypt=False";
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(Cstring);
            string bdcstring = $"Data Source={builder.DataSource};Initial Catalog=Test;Integrated Security={builder.IntegratedSecurity};Encrypt={builder.Encrypt}";
            string cmdString = "SELECT * FROM vw_StockProducto";
            using (SqlConnection con = new SqlConnection(bdcstring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(cmdString, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                con.Close();
                return dt;
            }
        }

    }
}
