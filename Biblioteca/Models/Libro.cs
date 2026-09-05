


namespace Biblioteca.Models
{
    public class Libro
    {
        public int Isbn { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public Categoria CategoriaAsignada { get; set; }

        public Libro(int isbn, string titulo, string autor, Categoria categoriaAsignada)
        {
            Isbn = isbn;
            Titulo = titulo;
            Autor = autor;
            CategoriaAsignada = categoriaAsignada;
        }
    }
}