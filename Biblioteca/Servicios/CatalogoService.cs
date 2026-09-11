
using System.Text;
using System.Xml;
using Biblioteca.Models;
using Biblioteca.TDA;

namespace Biblioteca.Servicios
{
    public class CatalogoService
    {
        private ListaCategorias raices;
        private ArbolIndiceCategorias indiceCategorias;
        private ArbolIndiceLibros indiceLibros;

        public CatalogoService()
        {
            raices = new ListaCategorias();
            indiceCategorias = new ArbolIndiceCategorias();
            indiceLibros = new ArbolIndiceLibros();
        }

        public ListaCategorias Raices => raices;
        public int TotalLibros => indiceLibros.Cantidad;

        public void Reiniciar()
        {
            raices = new ListaCategorias();
            indiceCategorias = new ArbolIndiceCategorias();
            indiceLibros = new ArbolIndiceLibros();
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

        public ListaLibros ObtenerLibrosOrdenAscendente()
        {
            ListaLibros resultado = new ListaLibros();
            indiceLibros.RecorrerAscendente(libro => resultado.InsertarFinal(libro));
            return resultado;
        }

        public ListaLibros ObtenerLibrosDeCategoria(string nombreCategoria, bool incluirSubcategorias)
        {
            ListaLibros resultado = new ListaLibros();

            Categoria? categoria = ObtenerCategoria(nombreCategoria);
            if (categoria == null)
            {
                return resultado;
            }

            AcumularLibros(categoria, incluirSubcategorias, resultado);
            return resultado;
        }

        private void AcumularLibros(Categoria categoria, bool incluirSubcategorias, ListaLibros acumulador)
        {
            categoria.Libros.Recorrer(libro =>
                acumulador.InsertarOrdenado(libro, (a, b) => a.Isbn.CompareTo(b.Isbn)));

            if (incluirSubcategorias)
            {
                categoria.Subcategorias.Recorrer(sub => AcumularLibros(sub, true, acumulador));
            }
        }

        // ---------- Carga incremental desde XML ----------

        public ResultadoOperacion CargarDesdeXml(string contenidoXml)
        {
            try
            {
                XmlDocument documento = new XmlDocument();
                documento.LoadXml(contenidoXml);

                int categoriasAgregadas = 0;
                int librosAgregados = 0;
                string advertencias = string.Empty;

                XmlNode? nodoListaCategorias = documento.SelectSingleNode("//listaCategorias");
                if (nodoListaCategorias != null)
                {
                    foreach (XmlNode nodoCategoria in nodoListaCategorias.ChildNodes)
                    {
                        if (nodoCategoria.NodeType != XmlNodeType.Element || nodoCategoria.Name != "categoria")
                        {
                            continue;
                        }

                        string nombre = nodoCategoria.InnerText.Trim();
                        string? padre = nodoCategoria.Attributes?["padre"]?.Value;

                        ResultadoOperacion resultado = AgregarCategoria(nombre, padre);
                        if (resultado.Exito)
                        {
                            categoriasAgregadas++;
                        }
                        else
                        {
                            advertencias += resultado.Mensaje + " ";
                        }
                    }
                }

                XmlNode? nodoListaLibros = documento.SelectSingleNode("//listaLibros");
                if (nodoListaLibros != null)
                {
                    foreach (XmlNode nodoLibro in nodoListaLibros.ChildNodes)
                    {
                        if (nodoLibro.NodeType != XmlNodeType.Element || nodoLibro.Name != "libro")
                        {
                            continue;
                        }

                        string isbnTexto = nodoLibro.SelectSingleNode("ISBN")?.InnerText.Trim() ?? string.Empty;
                        string titulo = nodoLibro.SelectSingleNode("titulo")?.InnerText.Trim() ?? string.Empty;
                        string autor = nodoLibro.SelectSingleNode("autor")?.InnerText.Trim() ?? string.Empty;
                        string categoria = nodoLibro.SelectSingleNode("categoria")?.InnerText.Trim() ?? string.Empty;

                        if (!int.TryParse(isbnTexto, out int isbn))
                        {
                            advertencias += $"El libro '{titulo}' tiene un ISBN inválido. ";
                            continue;
                        }

                        ResultadoOperacion resultado = AgregarLibro(isbn, titulo, autor, categoria);
                        if (resultado.Exito)
                        {
                            librosAgregados++;
                        }
                        else
                        {
                            advertencias += resultado.Mensaje + " ";
                        }
                    }
                }

                string mensaje = $"Se agregaron {categoriasAgregadas} categoría(s) y {librosAgregados} libro(s).";
                if (!string.IsNullOrEmpty(advertencias))
                {
                    mensaje += $" Advertencias: {advertencias.Trim()}";
                }

                return ResultadoOperacion.Ok(mensaje);
            }
            catch (Exception ex)
            {
                return ResultadoOperacion.Error($"Error al procesar el archivo XML: {ex.Message}");
            }
        }

        // ---------------GESTION DE DESCRIPCION DOT---------------

        public string GenerarDotEstructura(string? nombreCategoriaInicio)
        {
            StringBuilder dot = new StringBuilder();
            dot.AppendLine("digraph EstructuraCatalogo {");
            dot.AppendLine("    rankdir=TB;");
            dot.AppendLine("    node [shape=box, style=\"rounded,filled\", fillcolor=\"#EAF2FB\", fontname=\"Helvetica\"];");

            if (string.IsNullOrWhiteSpace(nombreCategoriaInicio))
            {
                dot.AppendLine("    \"__raiz__\" [label=\"Catálogo\", shape=oval, fillcolor=\"#2C3E50\", fontcolor=white];");
                raices.Recorrer(categoria =>
                {
                    AgregarNodosCategoria(dot, categoria);
                    dot.AppendLine($"    \"__raiz__\" -> \"{EscaparTexto(categoria.Nombre)}\";");
                });
            }
            else
            {
                Categoria? inicio = ObtenerCategoria(nombreCategoriaInicio);
                if (inicio != null)
                {
                    AgregarNodosCategoria(dot, inicio);
                }
            }

            dot.AppendLine("}");
            return dot.ToString();
        }

        private void AgregarNodosCategoria(StringBuilder dot, Categoria categoria)
        {
            string id = EscaparTexto(categoria.Nombre);
            dot.AppendLine($"    \"{id}\" [label=\"{id}\"];");

            categoria.Subcategorias.Recorrer(sub =>
            {
                AgregarNodosCategoria(dot, sub);
                dot.AppendLine($"    \"{id}\" -> \"{EscaparTexto(sub.Nombre)}\";");
            });
        }

        // ------Genera el archivo DOT con los libros de una categoria, en orden ascendente por ISBN

        public string GenerarDotLibrosCategoria(string nombreCategoria)
        {
            StringBuilder dot = new StringBuilder();
            dot.AppendLine("digraph LibrosCategoria {");
            dot.AppendLine("    rankdir=LR;");
            dot.AppendLine("    node [shape=box, style=\"rounded,filled\", fillcolor=\"#FDEBD0\", fontname=\"Helvetica\"];");

            string idCategoria = EscaparTexto(nombreCategoria);
            dot.AppendLine($"    \"{idCategoria}\" [label=\"{idCategoria}\", shape=oval, fillcolor=\"#2C3E50\", fontcolor=white];");

            ListaLibros libros = ObtenerLibrosDeCategoria(nombreCategoria, false);

            NodoLibro? actual = libros.Cabeza;
            int contador = 0;
            string? idAnterior = null;

            while (actual != null)
            {
                string idLibro = $"libro_{contador}";
                string etiqueta = $"ISBN {actual.Dato.Isbn}\\n{EscaparTexto(actual.Dato.Titulo)}";

                dot.AppendLine($"    \"{idLibro}\" [label=\"{etiqueta}\"];");
                dot.AppendLine($"    \"{idCategoria}\" -> \"{idLibro}\";");

                if (idAnterior != null)
                {
                    dot.AppendLine($"    \"{idAnterior}\" -> \"{idLibro}\" [style=dashed, color=gray, constraint=false];");
                }

                idAnterior = idLibro;
                actual = actual.Siguiente;
                contador++;
            }

            dot.AppendLine("}");
            return dot.ToString();
        }

        private static string EscaparTexto(string texto)
        {
            return texto.Replace("\"", "'");
        }
    }
}