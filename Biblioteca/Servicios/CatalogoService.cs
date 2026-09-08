
using Biblioteca.TDA;
using Biblioteca.Models;
using System.Text;
using System.Xml;

namespace Biblioteca.Servicios
{
    public class CatalogoService
    {
        private ListaSimple<Categoria> raices;
        private ABBIndiceCategorias indiceCategorias;
        private ABBIndiceLibros indiceLibros;

        public CatalogoService()
        {
            raices = new ListaSimple<Categoria>();
            indiceCategorias = new ABBIndiceCategorias();
            indiceLibros = new ABBIndiceLibros();
        }

        public ListaSimple<Categoria> Raices => raices;
        public int TotalLibros => indiceLibros.Cantidad;

        /// <summary>Inicializa el sistema sin ninguna información previa.</summary>
        public void Reiniciar()
        {
            raices = new ListaSimple<Categoria>();
            indiceCategorias = new ABBIndiceCategorias();
            indiceLibros = new ABBIndiceLibros();
        }

        private static int ComparadorCategoriaPorNombre(Categoria a, Categoria b)
        {
            return string.Compare(a.Nombre, b.Nombre, StringComparison.OrdinalIgnoreCase);
        }

        public bool ExisteCategoria(string nombre) => indiceCategorias.Existe(nombre);

        public Categoria? ObtenerCategoria(string nombre) => indiceCategorias.Buscar(nombre);

        // ---------- Gestión de categorías ----------

        public ResultadoOperacion AgregarCategoria(string nombre, string? nombrePadre)
        {
            nombre = (nombre ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                return ResultadoOperacion.Error("El nombre de la categoría no puede estar vacío.");
            }

            if (ExisteCategoria(nombre))
            {
                return ResultadoOperacion.Error($"Ya existe una categoría llamada '{nombre}'.");
            }

            Categoria? padre = null;
            if (!string.IsNullOrWhiteSpace(nombrePadre))
            {
                padre = ObtenerCategoria(nombrePadre.Trim());
                if (padre == null)
                {
                    return ResultadoOperacion.Error($"La categoría padre '{nombrePadre}' no existe.");
                }
            }

            Categoria nueva = new Categoria(nombre, padre);

            if (padre == null)
            {
                raices.InsertarOrdenado(nueva, ComparadorCategoriaPorNombre);
            }
            else
            {
                padre.Subcategorias.InsertarOrdenado(nueva, ComparadorCategoriaPorNombre);
            }

            indiceCategorias.Insertar(nombre, nueva);
            return ResultadoOperacion.Ok($"Categoría '{nombre}' agregada correctamente.");
        }

        // ---------- Gestión de libros ----------

        public ResultadoOperacion AgregarLibro(int isbn, string titulo, string autor, string nombreCategoria)
        {
            if (indiceLibros.Buscar(isbn) != null)
            {
                return ResultadoOperacion.Error($"Ya existe un libro con ISBN {isbn}.");
            }

            Categoria? categoria = ObtenerCategoria(nombreCategoria);
            if (categoria == null)
            {
                return ResultadoOperacion.Error($"La categoría '{nombreCategoria}' no existe.");
            }

            Libro libro = new Libro(isbn, titulo, autor, categoria);
            categoria.Libros.InsertarFinal(libro);
            indiceLibros.Insertar(libro);

            return ResultadoOperacion.Ok($"Libro '{titulo}' (ISBN {isbn}) registrado correctamente.");
        }

        public ResultadoOperacion EliminarLibro(int isbn)
        {
            Libro? libro = indiceLibros.Buscar(isbn);
            if (libro == null)
            {
                return ResultadoOperacion.Error($"No se encontró ningún libro con ISBN {isbn}.");
            }

            libro.CategoriaAsignada.Libros.Eliminar(l => l.Isbn == isbn);
            indiceLibros.Eliminar(isbn);

            return ResultadoOperacion.Ok($"Libro '{libro.Titulo}' (ISBN {isbn}) eliminado correctamente.");
        }

        public Libro? BuscarLibroPorIsbn(int isbn) => indiceLibros.Buscar(isbn);

        public Libro? ObtenerLibroMenor() => indiceLibros.ObtenerMinimo();

        public Libro? ObtenerLibroMayor() => indiceLibros.ObtenerMaximo();

        /// <summary>Todos los libros del catálogo, en orden ascendente de ISBN.</summary>
        public ListaSimple<Libro> ObtenerLibrosOrdenAscendente()
        {
            ListaSimple<Libro> resultado = new ListaSimple<Libro>();
            indiceLibros.RecorrerAscendente(libro => resultado.InsertarFinal(libro));
            return resultado;
        }

        public ListaSimple<Libro> ObtenerLibrosDeCategoria(string nombreCategoria, bool incluirSubcategorias)
        {
            ListaSimple<Libro> resultado = new ListaSimple<Libro>();

            Categoria? categoria = ObtenerCategoria(nombreCategoria);
            if (categoria == null)
            {
                return resultado;
            }

            AcumularLibros(categoria, incluirSubcategorias, resultado);
            return resultado;
        }

        private void AcumularLibros(Categoria categoria, bool incluirSubcategorias, ListaSimple<Libro> acumulador)
        {
            categoria.Libros.Recorrer(libro =>
                acumulador.InsertarOrdenado(libro, (a, b) => a.Isbn.CompareTo(b.Isbn)));

            if (incluirSubcategorias)
            {
                categoria.Subcategorias.Recorrer(sub => AcumularLibros(sub, true, acumulador));
            }
        }

        
    }
}