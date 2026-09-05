

namespace Biblioteca.TDA
{
    public class NodoIndiceLibro
    {
        public Models.Libro Libro { get; set; }
        public NodoIndiceLibro? Izquierdo { get; set; }
        public NodoIndiceLibro? Derecho { get; set; }

        public NodoIndiceLibro(Models.Libro libro)
        {
            Libro = libro;
        }
    }
}