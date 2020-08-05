using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using proyectoFinalProgramacion3.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace proyectoFinalProgramacion3.Controllers
{
    public class HomeController : Controller
    {
        
        private readonly ILogger<HomeController> _logger;
        List<string[]> datos = new List<string[]>();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CarroVacio()
        {
            return View();
        }

        public IActionResult NoEncontrado()
        {
            return View();
        }
        public IActionResult NoConectado()
        {
            return View();
        }

        public IActionResult DetallesCompra()
        {
            return View();
        }
        public IActionResult DatosEnvio()
        {
            if (User.Identity.IsAuthenticated == true)
            {

                int userID;
                double total = 0;
                string userEmail = User.FindFirst(ClaimTypes.Name).Value;
                var connectionBuilder = new SqliteConnectionStringBuilder();
                connectionBuilder.DataSource = "Proyecto.db";
                using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM carrito WHERE idusuario=(SELECT id FROM usuario Where email=\'" + userEmail + "\')";
                    var cmd = new SqliteCommand(query, con);
                    var reader = cmd.ExecuteReader();
                    List<string[]> Carrodatos = new List<string[]>();
                    List<string[]> CarrodatosPaypal = new List<string[]>();
                    while (reader.Read())
                    {

                        string idUsuario = String.Format("{0}", reader["idUsuario"]);
                        string ProductoID = String.Format("{0}", reader["idProducto"]);
                        string Cantidad = String.Format("{0}", reader["cantidad"]);
                        string Nombre = String.Format("{0}", reader["nombre"]);
                        string Imagen = String.Format("{0}", reader["imagen"]);
                        string Precio = String.Format("{0}", reader["precio"]);
                        string Descripcion = String.Format("{0}", reader["descripcion"]);

                        ViewBag.ProductoID = ProductoID;
                        ViewBag.Cantidad = Cantidad;
                        total += Int32.Parse(Cantidad) * Int32.Parse(Precio);
                        string[] Carrodato = { idUsuario, ProductoID, Cantidad, Nombre, Imagen, Descripcion, Precio };
                        Carrodatos.Add(Carrodato);
                        ViewBag.Total = total;
                    }
                    ViewBag.Carrodata = Carrodatos;
                    ViewBag.CarroLength = Carrodatos.Count();

                    for (int i = 0; i <= ViewBag.CarroLength; i++)
                    {
                        string[] CarroPaypal = { "item_name_" + (i + 1), "item_number_" + (i + 1), "amount_" + (i + 1), "quantity_" + (i + 1) };
                        CarrodatosPaypal.Add(CarroPaypal);
                    }
                    ViewBag.CarroPaypalData = CarrodatosPaypal;
                }
            }
            else
            {
                return RedirectToAction("NoConectado");
            }

            if (ViewBag.CarroLength <= 0)
            {

                return RedirectToAction("CarroVacio");
            }

            return View();
        }
        public IActionResult DatosEnvioEnviar(Envio envio)
        {
            string userEmail = User.FindFirst(ClaimTypes.Name).Value;


            var connectionBuilder = new SqliteConnectionStringBuilder();
            connectionBuilder.DataSource = "Proyecto.db";
            using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
            {
                con.Open();
                string query = "INSERT INTO DetallesPedido (direccion, contacto,latitud,longitud,comentario, userEmail) values('" + envio.Direccion + "', '" + envio.DatosContacto + "','" + envio.Latitud + "', '" + envio.Longitud+ "','" + envio.Comentario+ "','" + userEmail +"')";
                var cmd = new SqliteCommand(query, con);
                cmd.ExecuteNonQuery();

                string query2 = "select * from carrito where idUsuario =(select id from usuario where email =\'" + userEmail + "\')";
                var cmd2 = new SqliteCommand(query2, con);
                var reader = cmd2.ExecuteReader();

                while (reader.Read())
                {
                    string nombre = String.Format("{0}", reader["nombre"]);
                    string idProducto = String.Format("{0}", reader["idProducto"]);
                    int cantidad = Int32.Parse(String.Format("{0}", reader["cantidad"]));
                    string email = userEmail;
                    double precio = Double.Parse(String.Format("{0}", reader["precio"]));
                    double total = cantidad * precio;

                    string query3 = "insert into CarroPedido (nombre, idProducto,cantidad, userEmail,total) values('" + nombre + "', '" + idProducto + "', + '" + cantidad + "', '"+ email +"', '"+ total +"')";
                    var cmd3 = new SqliteCommand(query3, con);
                    cmd3.ExecuteNonQuery();
                }

                con.Close();
            }


            return RedirectToAction("DatosEnvio");
        }
        public IActionResult DetallesPedido()
        {
            string email = Request.Form["email"];
            string Latitud = Request.Form["latitud"];
            string Longitud = Request.Form["longitud"];

            var connectionBuilder = new SqliteConnectionStringBuilder();
            connectionBuilder.DataSource = "Proyecto.db";
            using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
            {
                con.Open();
                string query = "select * from CarroPedido where userEmail = \'"+email+ "\'";
                var cmd = new SqliteCommand(query, con);
                var reader = cmd.ExecuteReader();
                List<string[]> DetallesPedidoList = new List<string[]>();
                while (reader.Read())
                {
                    string Producto = String.Format("{0}", reader["nombre"]);
                    string Cantidad = String.Format("{0}", reader["cantidad"]);
                    string Total = String.Format("{0}", reader["total"]);
                    string[] Pedido = { Producto, Cantidad, Total, Latitud, Longitud };
                    DetallesPedidoList.Add(Pedido);
                }
                ViewBag.PedidoDetalles = DetallesPedidoList;
                ViewBag.PedidoDetallesLength = DetallesPedidoList.Count();
            }
                return View();
        }

        public IActionResult ListadoDePedido()
        {
            var connectionBuilder = new SqliteConnectionStringBuilder();
            connectionBuilder.DataSource = "Proyecto.db";
            using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
            {
                con.Open();
                string query = "select * from DetallesPedido";
                var cmd = new SqliteCommand(query, con);
                var reader = cmd.ExecuteReader();
                List<string[]> ListadoPedido = new List<string[]>();
                while (reader.Read())
                {

                    string Direccion = String.Format("{0}", reader["direccion"]);
                    string DatosContacto = String.Format("{0}", reader["contacto"]);
                    string Latitud = String.Format("{0}", reader["latitud"]);
                    string Longitud = String.Format("{0}", reader["longitud"]);
                    string Comentario = String.Format("{0}", reader["comentario"]);
                    string Email = String.Format("{0}", reader["userEmail"]);

                    string[] Listado = { Direccion, DatosContacto, Latitud, Longitud, Comentario, Email};
                    ListadoPedido.Add(Listado);
                }

                ViewBag.ListadoData = ListadoPedido;
                ViewBag.ListadoDataLength = ListadoPedido.Count();
                con.Close();
            }
            return View();
        }

        public IActionResult Carro()
        {
            
            if (User.Identity.IsAuthenticated == true) {

                int userID;
                double total = 0;
                string userEmail = User.FindFirst(ClaimTypes.Name).Value;
                var connectionBuilder = new SqliteConnectionStringBuilder();
                connectionBuilder.DataSource = "Proyecto.db";
                using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM carrito WHERE idusuario=(SELECT id FROM usuario Where email=\'" + userEmail + "\')";
                    var cmd = new SqliteCommand(query, con);
                    var reader = cmd.ExecuteReader();
                    List<string[]> Carrodatos = new List<string[]>();
                    List<string[]> CarrodatosPaypal = new List<string[]>();
                    while (reader.Read())
                    {
                        
                        string idUsuario = String.Format("{0}",reader["idUsuario"]);
                        string ProductoID = String.Format("{0}", reader["idProducto"]);
                        string Cantidad = String.Format("{0}", reader["cantidad"]);
                        string Nombre = String.Format("{0}", reader["nombre"]);
                        string Imagen = String.Format("{0}", reader["imagen"]);
                        string Precio = String.Format("{0}", reader["precio"]);
                        string Descripcion = String.Format("{0}", reader["descripcion"]);

                        ViewBag.ProductoID = ProductoID;
                        ViewBag.Cantidad = Cantidad;
                        total += Int32.Parse(Cantidad) * Int32.Parse(Precio);
                        string[] Carrodato = {idUsuario, ProductoID, Cantidad, Nombre, Imagen, Descripcion , Precio};
                        Carrodatos.Add(Carrodato);
                        ViewBag.Total = total;
                    }
                        ViewBag.Carrodata = Carrodatos;
                        ViewBag.CarroLength = Carrodatos.Count();

                    for (int i = 0; i <= ViewBag.CarroLength; i++)
                    {
                        string[] CarroPaypal = { "item_name_"+(i+1), "item_number_"+ (i + 1), "amount_"+ (i + 1), "quantity_"+ (i + 1) };
                        CarrodatosPaypal.Add(CarroPaypal);
                    }
                    ViewBag.CarroPaypalData = CarrodatosPaypal;
                }
            }
            else
            {
                return RedirectToAction("NoConectado");
            }

            if (ViewBag.CarroLength  <= 0) {

                return RedirectToAction("CarroVacio");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            var connectionBuilder = new SqliteConnectionStringBuilder();
            connectionBuilder.DataSource = "Proyecto.db";
            using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
            {
                con.Open();
                string query = "SELECT * FROM productos";
                var cmd = new SqliteCommand(query, con);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {

                    string Id = String.Format("{0}", reader["id"]);
                    string Nombre = String.Format("{0}", reader["nombre"]);
                    string Precio = String.Format("{0}", reader["precio"]);
                    string Descripcion = String.Format("{0}", reader["descripcion"]);
                    string Imagen = String.Format("{0}", reader["imagen"]);
                    string Categoria = String.Format("{0}", reader["categoria"]);

                    string[] dato = {Id, Nombre, Precio, Descripcion, Imagen, Categoria };
                    datos.Add(dato);
                }
                ViewBag.length = datos.Count();
                ViewBag.data = datos;
                con.Close();
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult Agregar()
        {
            int Idusuario;
            ViewBag.Found = false;
            
            if (User.Identity.IsAuthenticated == true) {

                string userEmail = User.FindFirst(ClaimTypes.Name).Value;
                var connectionBuilder = new SqliteConnectionStringBuilder();
                connectionBuilder.DataSource = "Proyecto.db";
                using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM usuario WHERE email =\'" + userEmail + "\'";
                    var cmd = new SqliteCommand(query, con);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Idusuario = Int32.Parse(String.Format("{0}", reader["id"]));
                        ViewBag.ID = Idusuario;
                    }

                    int idUsuario = ViewBag.ID;
                    int idProducto = Int32.Parse(Request.Form["id"]);
                    int cantidad = Int32.Parse(Request.Form["cantidad"]);
                    string nombre = Request.Form["nombre"];
                    string imagen = Request.Form["imagen"];
                    string descripcion = Request.Form["descripcion"];
                    int precio = Int32.Parse(Request.Form["precio"]);
                    ViewBag.ProductoCantidadActual = null;

                    string BuscarProducto = "SELECT * FROM carrito WHERE idProducto=" + idProducto + " and idUsuario =" + idUsuario;
                    var BuscarProductoCmd = new SqliteCommand(BuscarProducto, con);
                    var BuscarProductoReader = BuscarProductoCmd.ExecuteReader();

                    while (BuscarProductoReader.Read())
                    {
                        int ProductoCantidadActual = Int32.Parse(String.Format("{0}", BuscarProductoReader["cantidad"]));
                        ViewBag.ProductoCantidadActual = ProductoCantidadActual;

                    }

                    string BuscarDisponibilidad = "SELECT disponible FROM productos WHERE id=" + idProducto;
                    var BuscarDisponibilidadCmd = new SqliteCommand(BuscarDisponibilidad, con);
                    var BuscarDisponibilidadReader = BuscarDisponibilidadCmd.ExecuteReader();

                    while (BuscarDisponibilidadReader.Read())
                    {
                        int ProductoDisponible = Int32.Parse(String.Format("{0}", BuscarDisponibilidadReader["disponible"]));
                        ViewBag.ProductoDisponibilidadActual = ProductoDisponible;
                    }


                    if (ViewBag.ProductoCantidadActual == null)
                    {

                        string EntrarAlCarro = "INSERT INTO carrito(idUsuario, idProducto, cantidad, nombre, imagen, descripcion, precio) VALUES('" + idUsuario + "', '" + idProducto + "','" + cantidad + "','" + nombre + "','" + imagen + "','" + descripcion + "','" + precio + "');";
                        int disponible = ViewBag.ProductoDisponibilidadActual - cantidad;
                        string ActualizaDisponibilidad = "Update productos set disponible =" + disponible + " where id =" + idProducto;
                        var DisponibilidadCMD = new SqliteCommand(ActualizaDisponibilidad, con);
                        var EntrarAlCarroCmd = new SqliteCommand(EntrarAlCarro, con);
                        EntrarAlCarroCmd.ExecuteNonQuery();
                        DisponibilidadCMD.ExecuteNonQuery();

                        con.Close();
                    }
                    else { 

                        int SumarCantidad = cantidad + ViewBag.ProductoCantidadActual;
                        int disponible = ViewBag.ProductoDisponibilidadActual-cantidad;
                        string ActualizarCarro = "UPDATE carrito set cantidad =" +  SumarCantidad + " where idProducto =" + idProducto + " and idUsuario =" + idUsuario;
                        string ActualizaDisponibilidad = "Update productos set disponible =" + disponible + " where id =" + idProducto;
                        var cmd4 = new SqliteCommand(ActualizarCarro, con);
                        var cmd5 = new SqliteCommand(ActualizaDisponibilidad, con);
                        cmd4.ExecuteNonQuery();
                        cmd5.ExecuteNonQuery();
                        con.Close();
                    }

                }
            }
            else
            {

                return RedirectToAction("NoConectado");
            }
            return RedirectToAction("Privacy");
        }
   
        public IActionResult Borrar()
        {
            int idProducto = Int32.Parse(Request.Form["idProducto"]);
            int cantidad = Int32.Parse(Request.Form["cantidad"]);

            string userEmail = User.FindFirst(ClaimTypes.Name).Value;
            var connectionBuilder = new SqliteConnectionStringBuilder();
            connectionBuilder.DataSource = "Proyecto.db";
            using (var con = new SqliteConnection(connectionBuilder.ConnectionString))
            {
                con.Open();

                string getDisponibilidad = "select disponible from productos where id=" + idProducto;
                var disponibleCMD = new SqliteCommand(getDisponibilidad, con);
                var reader = disponibleCMD.ExecuteReader();

                while (reader.Read())
                {
                    int CantidadDisponible = Int32.Parse(String.Format("{0}", reader["disponible"]));
                    ViewBag.CantidadDisponibleBorrar = CantidadDisponible;
                }

                int CantidadRegresar = ViewBag.CantidadDisponibleBorrar + cantidad;
                string query = "Delete from carrito where idProducto =" + idProducto+ " and idUsuario =(Select idUsuario from usuario where email = \'" + userEmail + "\')";
                string query2 = "update productos set disponible = "+ CantidadRegresar +" where id= " + idProducto;
                var CantidadCmd = new SqliteCommand(query2, con);
                var BorrarCmd = new SqliteCommand(query, con);
                BorrarCmd.ExecuteNonQuery();
                CantidadCmd.ExecuteNonQuery();
                con.Close();
            }
                return RedirectToAction("Carro");
        }
    }

}
