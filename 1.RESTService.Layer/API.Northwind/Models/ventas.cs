using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.Northwind.Models
{
    public class ventas
    {
        public string nombre { get; set; }
        public decimal venta { get; set; }
    }

    public class venta
    {
        //public int ano { get; set; }
        public string mes { get; set; }
        public decimal ventas { get; set; }
    }

    //public class ReporteVentas
    //{
    //    public int dimension { get; set; }
    //    public string nombre { get; set; }
    //    public bool busqueda { get; set; }
    //}
}