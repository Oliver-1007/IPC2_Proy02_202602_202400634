using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using Biblioteca.Servicios;
using Biblioteca.TDA;

namespace IPC2_Proy02.Controllers
{
    public class LibrosController : Controller
    {
        private readonly CatalogoService catalogo;

        public LibrosController(CatalogoService catalogo)
        {
            this.catalogo = catalogo;
        }

        // Registrar un nuevo libro
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(int isbn, string titulo, string autor, string categoria)
        {
            ResultadoOperacion resultado = catalogo.AgregarLibro(isbn, titulo, autor, categoria);
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["Exito"] = resultado.Exito;
            return RedirectToAction("Registrar");
        }

        // Eliminar un libro que ya no está disponible
        [HttpGet]
        public IActionResult Eliminar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Eliminar(int isbn)
        {
            ResultadoOperacion resultado = catalogo.EliminarLibro(isbn);
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["Exito"] = resultado.Exito;
            return RedirectToAction("Eliminar");
        }

        // Mostrar datos de un libro en base a su ISBN
        [HttpGet]
        public IActionResult BuscarPorIsbn(int? isbn)
        {
            if (isbn.HasValue)
            {
                ViewBag.Libro = catalogo.BuscarLibroPorIsbn(isbn.Value);
                ViewBag.IsbnBuscado = isbn.Value;
            }
            return View();
        }

        // Mostrar libro con el menor ISBN
        public IActionResult Menor()
        {
            ViewBag.Libro = catalogo.ObtenerLibroMenor();
            return View();
        }

        // Mostrar libro con el mayor ISBN
        public IActionResult Mayor()
        {
            ViewBag.Libro = catalogo.ObtenerLibroMayor();
            return View();
        }

        // Mostrar los libros en orden ascendente de ISBN
        public IActionResult Listado()
        {
            ListaLibros libros = catalogo.ObtenerLibrosOrdenAscendente();
            return View(libros);
        }
    }
}
