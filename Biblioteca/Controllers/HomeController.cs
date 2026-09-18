using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Biblioteca.Models;
using Biblioteca.Servicios;

namespace IPC2_Proy02.Controllers
{
    public class HomeController : Controller
    {
        private readonly CatalogoService catalogo;

        public HomeController(CatalogoService catalogo)
        {
            this.catalogo = catalogo;
        }

        // a. Inicialización / pantalla principal
        public IActionResult Index()
        {
            ViewBag.TotalLibros = catalogo.TotalLibros;
            ViewBag.TotalCategorias = catalogo.Raices.Cantidad;
            return View();
        }

        // b. Cargar un archivo XML de entrada
        [HttpGet]
        public IActionResult CargarXml()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CargarXml(IFormFile archivoXml)
        {
            if (archivoXml == null || archivoXml.Length == 0)
            {
                TempData["Mensaje"] = "Debe seleccionar un archivo XML.";
                TempData["Exito"] = false;
                return RedirectToAction("CargarXml");
            }

            string contenido;
            using (StreamReader lector = new StreamReader(archivoXml.OpenReadStream()))
            {
                contenido = await lector.ReadToEndAsync();
            }

            ResultadoOperacion resultado = catalogo.CargarDesdeXml(contenido);
            TempData["Mensaje"] = resultado.Mensaje;
            TempData["Exito"] = resultado.Exito;

            return RedirectToAction("CargarXml");
        }

        [HttpPost]
        public IActionResult Reiniciar()
        {
            catalogo.Reiniciar();
            TempData["Mensaje"] = "El sistema fue inicializado sin información previa.";
            TempData["Exito"] = true;
            return RedirectToAction("Index");
        }

        // e. Ayuda
        public IActionResult Ayuda()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
