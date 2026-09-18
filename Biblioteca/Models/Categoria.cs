
using Biblioteca.TDA;

namespace Biblioteca.Models
{
    public class Categoria
    {
        public string Nombre { get; set; }
        public Categoria? Padre { get; set; }
        public ListaCategorias Subcategorias { get; set; }
        public ListaLibros Libros { get; set; }

        public Categoria(string nombre, Categoria? padre = null)
        {
            Nombre = nombre;
            Padre = padre;
            Subcategorias = new ListaCategorias();
            Libros = new ListaLibros();
        }
    }
}