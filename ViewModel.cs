using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Data.SqlClient;
using Microsoft.VisualBasic;

namespace Test
{
    internal class ViewModel
    {
        public ViewModel() { }

        //El view model se encarga de Instanciar otra parte de la clase de base de datos para pedirle los datos de la tabla view
        //De esta forma, el view y el modelo quedan aislados.
        public DataTable ViewContent()
        {
            DB temp = new DB();
            DataTable dt;
            dt = temp.GetDataFromView();
            return dt;
        }

    }
}
