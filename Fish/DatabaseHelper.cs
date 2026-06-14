using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace Fish
{
    internal class DatabaseHelper
    {
        public static string connectionString = "Server=localhost\\SQLEXPRESS;Database=База_Мероприятий;Integrated Security=True;";
    }
}
