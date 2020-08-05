using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace proyectoFinalProgramacion3.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    
    public class Envio
    {
       
        public string Direccion { get; set; }

        
        public string DatosContacto { get; set; }

        
        public double Latitud { get; set; }

        
        public double Longitud { get; set; }

        
        public string Comentario { get; set; }

        public string Email { get; set; }
    }
    public class Envios
    {
        public List<Envio> envios{ get; set;}
    }
    
}
