using System.Data;
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
using Microsoft.VisualBasic;
using System.Data.SqlClient;

namespace Test
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            datagrid1.IsReadOnly = true;
            datagrid2.IsReadOnly = true;
            datagrid3.IsReadOnly = true;
        }

        //Connection será el string de conexion general y conndb el string de conexion para la base de datos
        private string connection;
        private string conndb;
        DB Basedatos;

        //Instancio la clase DB para crear la base de datos en SQL y obtener el string de conexion.
        //Llamamos a la funcion Mostrar para ver las tres tablas en los datagrid.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connection = "Data Source=.;Integrated Security=True;Encrypt=False";
                if(connection == "")
                {
                    Exception ex = new Exception("String is null");
                    throw ex;
                }
                else
                {
                    Basedatos = new DB(connection);
                    conndb = Basedatos.Start();
                    MostrarTablas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Para añadir, seleccionamos el tipo en el datagrid de tipos y solicitamos al usuario las distintas 
        //propiedades necesarias.
        private void Añadir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (conndb == "" || conndb == null)
                {
                    Exception ex = new Exception("String is null");
                    throw ex;
                }
                else
                {
                    using(SqlConnection conn = new SqlConnection(conndb))
                    {
                        conn.Open();
                        //Seleccionamos el tipo y verificamos que 1 este seleccionado, luego le sumamos +1 ya que el indice es ID -1 en el datagrid
                        int selectedIndex = datagrid1.SelectedIndex + 1;
                        if (selectedIndex != -1)
                        {
                            string N = Interaction.InputBox("Nombre: ");
                            double P;
                            int C;
                            if(!double.TryParse(Interaction.InputBox("Precio: "), out P)) { Exception ex = new Exception("Double only"); }
                            if (!int.TryParse(Interaction.InputBox("Cantidad: "), out C)) { Exception ex = new Exception("int only"); }
                            //Le pasamos los parametros al sp que maneja la insercion.
                            using (SqlCommand cmd = new SqlCommand("sp_InsertarProducto", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new SqlParameter("@IdTipoProducto", selectedIndex));
                                cmd.Parameters.Add(new SqlParameter("@Nombre", N));
                                cmd.Parameters.Add(new SqlParameter("@Precio", P));
                                cmd.Parameters.Add(new SqlParameter("@Cantidad", C));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        conn.Close();
                    }
                    //Mostramos los datos nuevamente.
                    MostrarTablas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Para modificar, tambien seleccionamos el objeto a modificar y a travez de los radio buttons, una propiedad que deseemos modificar
        //Seguro hay una forma mucho mas rapida y eficiente de hacerlo
        private void Modificar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (conndb == "" || conndb == null)
                {
                    Exception ex = new Exception("String is null");
                    throw ex;
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(conndb))
                    {
                        conn.Open();
                        if (datagrid2.SelectedItem != null)
                        {
                            DataRowView row = (DataRowView)datagrid2.SelectedItem;
                            int id = int.Parse(row[0].ToString());
                            if (id != -1)
                            {
                                string N;
                                double P;
                                int C;
                                int I;
                                if(n_radio.IsChecked == true)
                                {
                                    N = Interaction.InputBox("Nombre: ");
                                    using(SqlCommand cmd = new SqlCommand("sp_ModificarProducto", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                                        cmd.Parameters.Add(new SqlParameter("@Nombre", N));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else if(c_radio.IsChecked == true) 
                                {
                                    if (!int.TryParse(Interaction.InputBox("Cantidad: "), out C)) { Exception ex = new Exception("Int only"); }
                                    using (SqlCommand cmd = new SqlCommand("sp_ModificarProducto", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                                        cmd.Parameters.Add(new SqlParameter("@Cantidad", C));
                                        cmd.ExecuteNonQuery();
                                    }

                                }
                                else if(p_radio.IsChecked == true) 
                                { 
                                    if (!double.TryParse(Interaction.InputBox("Precio: "), out P)) { Exception ex = new Exception("Double only"); }
                                    using (SqlCommand cmd = new SqlCommand("sp_ModificarProducto", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                                        cmd.Parameters.Add(new SqlParameter("@Precio", P));
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                                else if(t_radio.IsChecked == true) 
                                { 
                                    if (!int.TryParse(Interaction.InputBox("Id de tipo de producto: "), out I)) { Exception ex = new Exception("Int only"); }
                                    using (SqlCommand cmd = new SqlCommand("sp_ModificarProducto", conn))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add(new SqlParameter("@Id", id));
                                        cmd.Parameters.Add(new SqlParameter("@IdTipoProducto", I));
                                        cmd.ExecuteNonQuery();
                                    }                                
                                }
                            }
                        }
                        else { Exception ex = new Exception("Nada seleccionado"); }
                        conn.Close();
                    }
                    MessageBox.Show("Modificación Exitosa");
                    MostrarTablas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Lo mismo para eliminar, simplemente seleccionamos un objeto de la lista de productos y lo eliminamos, esto hara que se elimine de la tabla de stock tambien
        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (conndb == "" || conndb == null)
                {
                    Exception ex = new Exception("String is null");
                    throw ex;
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(conndb))
                    {
                        conn.Open();
                        if (datagrid2.SelectedItem != null)
                        {
                            DataRowView row = (DataRowView)datagrid2.SelectedItem;
                            int id = int.Parse(row[0].ToString());
                            if (id != -1)
                            {
                                using (SqlCommand cmd = new SqlCommand("sp_EliminarProducto", conn))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.Add(new SqlParameter("@Id", id));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        conn.Close();
                    }
                    MostrarTablas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Codigo reutilizable para mostrar y actualizar las tablas en los datagrid
        private void MostrarTablas()
        {
            datagrid1.ItemsSource = null;
            datagrid2.ItemsSource = null;
            datagrid3.ItemsSource = null;

            string cmdString = "SELECT * FROM TipoProducto";

            using (SqlConnection con = new SqlConnection(conndb))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(cmdString, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                datagrid1.ItemsSource = dt.DefaultView;
                con.Close();
            }

            string cmdString2 = "SELECT * FROM Producto";

            using (SqlConnection con = new SqlConnection(conndb))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(cmdString2, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                datagrid2.ItemsSource = dt.DefaultView;
                con.Close();
            }

            string cmdString3 = "SELECT * FROM Stock";

            using (SqlConnection con = new SqlConnection(conndb))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(cmdString3, con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                datagrid3.ItemsSource = dt.DefaultView;
                con.Close();
            }
        }

        //Mostramos la ventana de front
        private void FrontAccess_Click(object sender, RoutedEventArgs e)
        {
            Front front = new Front();
            front.Show();
        }
    }
}