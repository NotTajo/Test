using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// Interaction logic for Front.xaml
    /// </summary>
    public partial class Front : Window
    {
        //Instanciamos el view model en el front para obtener los datos y mostrarlos
        ViewModel VM = new ViewModel();

        //Simplemente mostramos los datos y agregamos un boton para actualizar (se puede automatizar con un objectcollection)
        public Front()
        {
            InitializeComponent();
            datagrid1.IsReadOnly = true;
            this.DataContext = VM;
            DataTable dt = VM.ViewContent();
            datagrid1.ItemsSource = dt.DefaultView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = VM.ViewContent();
            datagrid1.ItemsSource = null;
            datagrid1.ItemsSource = dt.DefaultView;
        }
    }
}
