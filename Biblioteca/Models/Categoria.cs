
using Biblioteca.TDA;

namespace Biblioteca.Models
{
    public class Categoria
    {
        public string Nombre { get; set; }
        public Categoria? Padre { get; set; }
        public ListaSimple<Categoria> Subcategorias { get; set; }
        public ListaSimple<Libro> Libros { get; set; }

        public Categoria(string nombre, Categoria? padre = null)
        {
            Nombre = nombre;
            Padre = padre;
            Subcategorias = new ListaSimple<Categoria>();
            Libros = new ListaSimple<Libro>();
        }
    }
}