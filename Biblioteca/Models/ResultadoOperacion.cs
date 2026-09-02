


namespace Biblioteca.Models
{
    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }

        public ResultadoOperacion(bool exito, string mensaje)
        {
            Exito = exito;
            Mensaje = mensaje;
        }

        public static ResultadoOperacion Ok(string mensaje) => new ResultadoOperacion(true, mensaje);
        public static ResultadoOperacion Error(string mensaje) => new ResultadoOperacion(false, mensaje);
    }
}