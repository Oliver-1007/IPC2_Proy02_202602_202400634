
using System.Diagnostics;

namespace Biblioteca.Servicios
{
    public class GraphvizService
    {
        private readonly string carpetaSalida;
        private readonly string carpetaReporte;

        public GraphvizService(IWebHostEnvironment entorno)
        {
            carpetaSalida = Path.Combine(entorno.WebRootPath, "graficas");
            carpetaReporte = Path.Combine(entorno.ContentRootPath, "Reporte Dot");

            if (!Directory.Exists(carpetaSalida))
            {
                Directory.CreateDirectory(carpetaSalida);
            }

            if (!Directory.Exists(carpetaReporte))
            {
                Directory.CreateDirectory(carpetaReporte);
            }
        }

        public string GenerarImagen(string contenidoDot, string nombreArchivo)
        {
            string rutaDot = Path.Combine(carpetaSalida, nombreArchivo + ".dot");
            string rutaPng = Path.Combine(carpetaSalida, nombreArchivo + ".png");

            File.WriteAllText(rutaDot, contenidoDot);

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{rutaDot}\" -o \"{rutaPng}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proceso = new Process { StartInfo = info })
            {
                proceso.Start();
                proceso.WaitForExit();

                if (proceso.ExitCode != 0)
                {
                    string error = proceso.StandardError.ReadToEnd();
                    throw new Exception("Graphviz finalizó con error: " + error);
                }
            }

            return "/graficas/" + nombreArchivo + ".png";
        }

        public void GuardarImagenEnReporte(string nombreArchivo)
        {
            string rutaOrigen = Path.Combine(carpetaSalida, nombreArchivo + ".png");
            string rutaDestino = Path.Combine(carpetaReporte, nombreArchivo + ".png");
            File.Copy(rutaOrigen, rutaDestino, true);
        }

        public byte[] ObtenerImagen(string nombreArchivo)
        {
            string rutaImagen = Path.Combine(carpetaSalida, nombreArchivo + ".png");
            return File.ReadAllBytes(rutaImagen);
        }
    }
}