
using System.Text;
using System.Xml;
using Biblioteca.Models;
using Biblioteca.TDA;

namespace Biblioteca.Servicios
{
    public class CatalogoService
    {
        private ListaCategorias raices;
        private ABBIndiceCategorias indiceCategorias;
        private AVLIndiceLibros indiceLibros;

        public CatalogoService()
        {
            raices = new ListaCategorias();
            indiceCategorias = new ABBIndiceCategorias();
            indiceLibros = new AVLIndiceLibros();
        }

        public ListaCategorias Raices => raices;
        public int TotalLibros => indiceLibros.Cantidad;

        public void Reiniciar()
        {
            raices = new ListaCategorias();
            indiceCategorias = new ABBIndiceCategorias();
            indiceLibros = new AVLIndiceLibros();
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

        // ---- Nodo y cola propia para categorías pendientes de su padre ----
        private class NodoPendienteCategoria
        {
            public string Nombre;
            public string NombrePadre;
            public NodoPendienteCategoria? Siguiente;

            public NodoPendienteCategoria(string nombre, string nombrePadre)
            {
                Nombre = nombre;
                NombrePadre = nombrePadre;
                Siguiente = null;
            }
        }

        // Cola persiste entre llamadas a CargarDesdeXml, porque la carga es incremental
        private NodoPendienteCategoria? inicioPendientes = null;
        private NodoPendienteCategoria? finPendientes = null;

        private void EncolarPendiente(string nombre, string nombrePadre)
        {
            NodoPendienteCategoria nuevo = new NodoPendienteCategoria(nombre, nombrePadre);
            if (inicioPendientes == null)
            {
                inicioPendientes = nuevo;
                finPendientes = nuevo;
            }
            else
            {
                finPendientes!.Siguiente = nuevo;
                finPendientes = nuevo;
            }
        }

        // Intenta insertar los pendientes en varias pasadas, hasta que ya no
        // se resuelva ninguno más (esto cubre cadenas de varios niveles,
        // sin importar el orden en que llegaron).
        private string ProcesarPendientes(out int categoriasResueltas)
        {
            categoriasResueltas = 0;
            string advertencias = string.Empty;
            bool huboProgreso = true;

            while (huboProgreso)
            {
                huboProgreso = false;
                NodoPendienteCategoria? anterior = null;
                NodoPendienteCategoria? actual = inicioPendientes;

                while (actual != null)
                {
                    NodoPendienteCategoria? siguienteGuardado = actual.Siguiente;

                    ResultadoOperacion resultado = AgregarCategoria(actual.Nombre, actual.NombrePadre);
                    if (resultado.Exito)
                    {
                        // Se pudo insertar: se saca de la cola
                        if (anterior == null)
                    inicioPendientes = actual.Siguiente;
                        else
                            anterior.Siguiente = actual.Siguiente;

                        if (actual == finPendientes)
                            finPendientes = anterior;

                        categoriasResueltas++;
                        huboProgreso = true;
                        // anterior no avanza: actual fue removido de la cadena
                    }
                    else
                    {
                        anterior = actual;
                    }

                    actual = siguienteGuardado;
                }
            }

            // Lo que sobrevive aquí sigue esperando un padre que aún no ha llegado
            NodoPendienteCategoria? restante = inicioPendientes;
            while (restante != null)
            {
                advertencias += $"La categoría '{restante.Nombre}' quedó en espera de su padre '{restante.NombrePadre}'. ";
                restante = restante.Siguiente;
            }

            return advertencias;
        }

        // public ResultadoOperacion CargarDesdeXml(string contenidoXml)
        // {
        //     try
        //     {
        //         XmlDocument documento = new XmlDocument();
        //         documento.LoadXml(contenidoXml);

        //         int categoriasAgregadas = 0;
        //         int librosAgregados = 0;
        //         string advertencias = string.Empty;

        //         XmlNode? nodoListaCategorias = documento.SelectSingleNode("//listaCategorias");
        //         if (nodoListaCategorias != null)
        //         {
        //             foreach (XmlNode nodoCategoria in nodoListaCategorias.ChildNodes)
        //             {
        //                 if (nodoCategoria.NodeType != XmlNodeType.Element || nodoCategoria.Name != "categoria")
        //                 {
        //                     continue;
        //                 }

        //                 string nombre = nodoCategoria.InnerText.Trim();
        //                 string? padre = nodoCategoria.Attributes?["padre"]?.Value;

        //                 ResultadoOperacion resultado = AgregarCategoria(nombre, padre);
        //                 if (resultado.Exito)
        //                 {
        //                     categoriasAgregadas++;
        //                 }
        //                 else
        //                 {
        //                     advertencias += resultado.Mensaje + " ";
        //                 }
        //             }
        //         }

        //         XmlNode? nodoListaLibros = documento.SelectSingleNode("//listaLibros");
        //         if (nodoListaLibros != null)
        //         {
        //             foreach (XmlNode nodoLibro in nodoListaLibros.ChildNodes)
        //             {
        //                 if (nodoLibro.NodeType != XmlNodeType.Element || nodoLibro.Name != "libro")
        //                 {
        //                     continue;
        //                 }

        //                 string isbnTexto = nodoLibro.SelectSingleNode("ISBN")?.InnerText.Trim() ?? string.Empty;
        //                 string titulo = nodoLibro.SelectSingleNode("titulo")?.InnerText.Trim() ?? string.Empty;
        //                 string autor = nodoLibro.SelectSingleNode("autor")?.InnerText.Trim() ?? string.Empty;
        //                 string categoria = nodoLibro.SelectSingleNode("categoria")?.InnerText.Trim() ?? string.Empty;

        //                 if (!int.TryParse(isbnTexto, out int isbn))
        //                 {
        //                     advertencias += $"El libro '{titulo}' tiene un ISBN inválido. ";
        //                     continue;
        //                 }

        //                 ResultadoOperacion resultado = AgregarLibro(isbn, titulo, autor, categoria);
        //                 if (resultado.Exito)
        //                 {
        //                     librosAgregados++;
        //                 }
        //                 else
        //                 {
        //                     advertencias += resultado.Mensaje + " ";
        //                 }
        //             }
        //         }

        //         string mensaje = $"Se agregaron {categoriasAgregadas} categoría(s) y {librosAgregados} libro(s).";
        //         if (!string.IsNullOrEmpty(advertencias))
        //         {
        //             mensaje += $" Advertencias: {advertencias.Trim()}";
        //         }

        //         return ResultadoOperacion.Ok(mensaje);
        //     }
        //     catch (Exception ex)
        //     {
        //         return ResultadoOperacion.Error($"Error al procesar el archivo XML: {ex.Message}");
        //     }
        // }


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

                        if (!string.IsNullOrWhiteSpace(padre) && ObtenerCategoria(padre.Trim()) == null)
                        {
                            // El padre todavía no existe: puede aparecer más abajo
                            // en este mismo archivo, o en un entrada.xml posterior.
                            EncolarPendiente(nombre, padre.Trim());
                            continue;
                        }

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

                // Resolver pendientes (de este archivo y de archivos anteriores)
                // ANTES de procesar libros, para que puedan referenciar categorías
                // que se acaban de completar.
                int resueltasPendientes;
                advertencias += ProcesarPendientes(out resueltasPendientes);
                categoriasAgregadas += resueltasPendientes;

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

        public string GenerarDotAVLLibros()
        {
            StringBuilder dot = new StringBuilder();
            dot.AppendLine("digraph AVLLibros {");
            dot.AppendLine("    rankdir=TB;");
            dot.AppendLine("    node [shape=ellipse, style=filled, fillcolor=\"#FFE6CC\", color=\"#E87722\", fontname=\"Helvetica\"];");

            if (indiceLibros.Raiz == null)
            {
                dot.AppendLine("    \"__vacio__\" [label=\"No hay libros registrados\", shape=box];");
            }
            else
            {
                AgregarNodosAVL(dot, indiceLibros.Raiz);
            }

            dot.AppendLine("}");
            return dot.ToString();
        }

        private void AgregarNodosAVL(StringBuilder dot, NodoIndiceLibro nodo)
        {
            string id = $"libro_{nodo.Libro.Isbn}";
            string etiqueta = $"{nodo.Libro.Isbn}\\n{EscaparTexto(nodo.Libro.Titulo)}";
            string titulo = EscaparTexto($"{nodo.Libro.Titulo} (altura {nodo.Altura})");

            dot.AppendLine($"    \"{id}\" [label=\"{etiqueta}\", tooltip=\"{titulo}\"];");

            if (nodo.Izquierdo != null)
            {
                string idIzquierdo = $"libro_{nodo.Izquierdo.Libro.Isbn}";
                AgregarNodosAVL(dot, nodo.Izquierdo);
                dot.AppendLine($"    \"{id}\" -> \"{idIzquierdo}\" [label=\"izq\"];");
            }

            if (nodo.Derecho != null)
            {
                string idDerecho = $"libro_{nodo.Derecho.Libro.Isbn}";
                AgregarNodosAVL(dot, nodo.Derecho);
                dot.AppendLine($"    \"{id}\" -> \"{idDerecho}\" [label=\"der\"];");
            }
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
            return texto.Replace("\\", "\\\\").Replace("\"", "'");
        }
    }
}