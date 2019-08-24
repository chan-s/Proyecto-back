using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Tools;
//using Negocio;
using Microsoft.AnalysisServices.AdomdClient;
using System.Configuration;
using System.Data;
using System.Collections.Generic;

namespace API.Northwind.Controllers
{
    [EnableCors(origins:"*", headers: "*", methods: "*")]
    [RoutePrefix("Northwind")]
    public class NorthwindController : ApiController
    {
        [HttpGet]
        [Route ("Testing")]
        public HttpResponseMessage Testing()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "prueba de API");
        }

        [HttpPost]
        [Route ("Top")]
        public HttpResponseMessage Top(Reporte info)
        {
            string nombreDimension = @"";
            switch (info.dimension)
            {
                case 1: //Clientes
                default:
                    nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].CHILDREN } ";
                    break;
                case 2: //Productos
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].CHILDREN } ";
                    break;
                case 3: //Categorias
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].CHILDREN } ";
                    break;
                case 4: //Empleadoss
                    nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].CHILDREN } ";
                    break;
            }

            string WITH = @"WITH SET [TopVentas] AS NONEMPTY( ORDER (STRTOSET(@Dimension), [Measures].[Fact Ventas Netas], BDESC ) ) ";
            string COLUMNS = @"NON EMPTY { [Measures].[Fact Ventas Netas] } ON COLUMNS, ";
            string ROWS = @"NON EMPTY { HEAD([TopVentas], " + info.top + ") } ON ROWS ";
            string CUBO_NAME = @"[DWH Northwind] ";
            string MDXQuery = WITH + "SELECT " + COLUMNS + ROWS + "FROM " + CUBO_NAME;

            List<Venta> ventasUsuario = new List<Venta>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            Venta v = new Venta();
                            v.mes = dr.GetString(0);
                            v.ventas = dr.GetDecimal(1);
                            ventasUsuario.Add(v);
                        }
                        dr.Close();
                    }
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK, ventasUsuario);
        }

        [HttpPost]
        [Route("ventas")]
        public HttpResponseMessage ventas(Filtro info)
        {
            //string WITH = @"WITH SET [TopVentas] AS NONEMPTY( ORDER (STRTOSET(@Dimension), [Measures].[Fact Ventas Netas], BDESC ) ) ";
            string COLUMNS = @" NON EMPTY { [Measures].[Fact Ventas Netas] } ON COLUMNS,";
            string ROWS = @"NON EMPTY { [Dim Tiempo].[Dim Tiempo Año].CHILDREN } * { [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN } ON ROWS ";
            string CUBO_NAME = @"[DWH Northwind] ";

            string WHERE = "";

            if (info.Item != 0 && info.Nombre != "")
            {
                string dimension = @"";
                switch (info.Item)
                {
                    case 1: dimension = @"[Dim Cliente].[Dim Cliente Nombre].&[" + info.Nombre + "]"; break;
                    case 2: dimension = @"[Dim Producto].[Dim Producto Nombre].&[" + info.Nombre + "]"; break;
                    case 3: dimension = @"[Dim Producto].[Dim Producto Categoria].&[" + info.Nombre + "]"; break;
                    case 4: dimension = @"[Dim Empleado].[Dim Empleado Nombre].&[" + info.Nombre + "]"; break;
                }

                WHERE = " WHERE " + dimension;
            }

            string MDXQuery = "SELECT " + COLUMNS + ROWS + "FROM " + CUBO_NAME + WHERE;

            List<Venta> ventasUsuario = new List<Venta>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            Venta v = new Venta();
                            //v.ano = ;
                            v.mes = dr.GetString(1) + " " + dr.GetInt16(0);
                            v.ventas = dr.GetDecimal(2);
                            ventasUsuario.Add(v);
                        }
                        dr.Close();
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, ventasUsuario);
        }

        [HttpPost]
        [Route("usuarios")]
        public HttpResponseMessage comparativausu(Reporte num)
        {
            string COLUMNS = @" NON EMPTY [Measures].[Fact Ventas Netas] ON COLUMNS,";
            string CUBO_NAME = @"[DWH Northwind] ";

            string WHERE = "";

            string dimension = @"";
            switch (num.dimension)
            {
                case 1: dimension = @"[Dim Cliente].[Dim Cliente Nombre].CHILDREN"; break;
                case 2: dimension = @"[Dim Producto].[Dim Producto Nombre].CHILDREN"; break;
                case 3: dimension = @"[Dim Producto].[Dim Producto Categoria].CHILDREN"; break;
                case 4: dimension = @"[Dim Empleado].[Dim Empleado Nombre].CHILDREN"; break;
            }

            string ROWS = @" NON EMPTY " + dimension + " ON ROWS";
            //WHERE = " WHERE " + dimension;

            string MDXQuery = "SELECT " + COLUMNS + ROWS + " FROM " + CUBO_NAME;

            List<usuario> ventasUsuario = new List<usuario>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            usuario u = new usuario();
                            //v.ano = ;
                            u.nombre = dr.GetString(0);
                            ventasUsuario.Add(u);
                        }
                        dr.Close();
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, ventasUsuario);
        }

        [HttpPost]
        [Route("comparativa")]
        public HttpResponseMessage comparativa(DatosComparar comp)
        {
            string ROWS = @"NON EMPTY { [Dim Tiempo].[Dim Tiempo Año].CHILDREN } * { [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN } ON ROWS ";
            string CUBO_NAME = @"[DWH Northwind] ";
            string WHERE = "";
            string dimension = @"";
            string coma = "";

            foreach (var dato in comp.Nombres)
            {
                switch (comp.dimension)
                {
                    case 1: dimension += coma + @"[Dim Cliente].[Dim Cliente Nombre].&[" + dato + "]"; break;
                    case 2: dimension += coma + @"[Dim Producto].[Dim Producto Nombre].&[" + dato + "]"; break;
                    case 3: dimension += coma + @"[Dim Producto].[Dim Producto Categoria].&[" + dato + "]"; break;
                    case 4: dimension += coma + @"[Dim Empleado].[Dim Empleado Nombre].&[" + dato + "]"; break;
                }
                coma = ", ";
            }
            
            string COLUMNS = @" NON EMPTY { [Measures].[Fact Ventas Netas] } * { " + dimension + " } ON COLUMNS,";
            string MDXQuery = "SELECT " + COLUMNS + ROWS + "FROM " + CUBO_NAME + WHERE;

            List<Comparativa> ventasUsuario = new List<Comparativa>();
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        int fi = dr.FieldCount;

                        while (dr.Read())
                        {
                            Comparativa v = new Comparativa();
                            List<decimal> listaUsu = new List<decimal>();

                            v.fecha = dr.GetString(1) + " " + dr.GetString(0);
                            for (int i = 2; i < fi; i++)
                            {
                                decimal valor = dr.IsDBNull(i) ? 0 : dr.GetDecimal(i);
                                listaUsu.Add(valor);
                            }
                            v.Datos = listaUsu;
                            ventasUsuario.Add(v);
                        }
                        dr.Close();
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, ventasUsuario);
        }
    }
    
    public class Filtro
    {
        public int Item { get; set; }
        public string Nombre { get; set; }
    }

    public class Venta
    {
        public string mes { get; set; }
        public decimal ventas { get; set; }
    }

    public class Reporte
    {
        public int top { get; set; }
        public int dimension { get; set; }
    }

    public class DatosComparar
    {
        public int dimension { get; set; }
        public List<string> Nombres { get; set; }
    }

    public class usuario
    {
        public string nombre { get; set; }
    }

    public class Comparativa
    {
        public string fecha { get; set; }
        public List<decimal> Datos { get; set; }
    }

    public class nombreUSuario
    {
        public string nombre { get; set; }
    }

    public class datosUsuario
    {
        //public string nombre { get; set; }
        public decimal valor { get; set; }
    }
}
