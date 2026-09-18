using System;
using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using Biblioteca.Servicios;

namespace IPC2_Proy02.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly CatalogoService catalogo;
        private readonly GraphvizService graphviz;

        public CategoriasController(CatalogoService catalogo, GraphvizService graphviz)
        {
            this.catalogo = catalogo;
            this.graphviz = graphviz;
        }

        // Mostrar estructura de categorías y subcategorías
        public IActionResult Estructura(string? nombreInicio)
        {
            ViewBag.NombreInicio = nombreInicio;

            if (!string.IsNullOrWhiteSpace(nombreInicio))
            {
                ViewBag.CategoriaInicio = catalogo.ObtenerCategoria(nombreInicio);
            }

            return View(catalogo.Raices);
        }

        // Estructura de categorías representada con Graphviz
        public IActionResult Grafica(string? nombreCategoria)
        {
            try
            {
                string dot = catalogo.GenerarDotEstructura(nombreCategoria);
                string nombreArchivo = "estructura_" +
                    (string.IsNullOrWhiteSpace(nombreCategoria) ? "general" : nombreCategoria.Replace(" ", "_"));

                ViewBag.RutaImagen = graphviz.GenerarImagen(dot, nombreArchivo);
                ViewBag.NombreCategoria = nombreCategoria;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // Mostrar gráficamente los libros de una categoría 
        public IActionResult GraficaLibros(string nombreCategoria)
        {
            try
            {
                string dot = catalogo.GenerarDotLibrosCategoria(nombreCategoria);
                string nombreArchivo = "libros_" + nombreCategoria.Replace(" ", "_");

                ViewBag.RutaImagen = graphviz.GenerarImagen(dot, nombreArchivo);
                ViewBag.NombreCategoria = nombreCategoria;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }

        // Agregar una categoría
        [HttpGet]
        public IActionResult AgregarCategoria()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AgregarCategoria(string nombre, string? nombrePadre)
        {
            ResultadoOperacion resultado = catalogo.AgregarCategoria(nombre, nombrePadre);
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["Exito"] = resultado.Exito;
            return RedirectToAction("AgregarCategoria");
        }
    }
}
