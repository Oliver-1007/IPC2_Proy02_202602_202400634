
// Uso de polimorfismo para la reutilizacion del nodo

namespace Biblioteca.TDA
{
    public class NodoSimple
    {
        public object? Dato { get; set; }
        public NodoSimple? Siguiente { get; set; }

        public NodoSimple(object? dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }
}