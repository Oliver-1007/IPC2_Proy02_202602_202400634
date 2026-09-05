


namespace Biblioteca.TDA
{
    public class ABBIndiceLibros
    {
        private NodoIndiceLibro? raiz;
        private int cantidad;

        public int Cantidad => cantidad;

        public void Insertar(Models.Libro libro)
        {
            raiz = InsertarRecursivo(raiz, libro);
            cantidad++;
        }

        private NodoIndiceLibro InsertarRecursivo(NodoIndiceLibro? nodo, Models.Libro libro)
        {
            if (nodo == null)
            {
                return new NodoIndiceLibro(libro);
            }

            if (libro.Isbn < nodo.Libro.Isbn)
            {
                nodo.Izquierdo = InsertarRecursivo(nodo.Izquierdo, libro);
            }
            else if (libro.Isbn > nodo.Libro.Isbn)
            {
                nodo.Derecho = InsertarRecursivo(nodo.Derecho, libro);
            }

            return nodo;
        }

        public Models.Libro? Buscar(int isbn)
        {
            NodoIndiceLibro? actual = raiz;

            while (actual != null)
            {
                if (isbn == actual.Libro.Isbn)
                {
                    return actual.Libro;
                }
                actual = isbn < actual.Libro.Isbn ? actual.Izquierdo : actual.Derecho;
            }

            return null;
        }

        public bool Eliminar(int isbn)
        {
            if (Buscar(isbn) == null)
            {
                return false;
            }

            raiz = EliminarRecursivo(raiz, isbn);
            cantidad--;
            return true;
        }

        private NodoIndiceLibro? EliminarRecursivo(NodoIndiceLibro? nodo, int isbn)
        {
            if (nodo == null)
            {
                return null;
            }

            if (isbn < nodo.Libro.Isbn)
            {
                nodo.Izquierdo = EliminarRecursivo(nodo.Izquierdo, isbn);
            }
            else if (isbn > nodo.Libro.Isbn)
            {
                nodo.Derecho = EliminarRecursivo(nodo.Derecho, isbn);
            }
            else
            {
                if (nodo.Izquierdo == null)
                {
                    return nodo.Derecho;
                }
                if (nodo.Derecho == null)
                {
                    return nodo.Izquierdo;
                }

                NodoIndiceLibro sucesor = ObtenerMinimoNodo(nodo.Derecho);
                nodo.Libro = sucesor.Libro;
                nodo.Derecho = EliminarSucesor(nodo.Derecho, sucesor.Libro.Isbn);
            }

            return nodo;
        }

        private NodoIndiceLibro? EliminarSucesor(NodoIndiceLibro? nodo, int isbn)
        {
            if (nodo == null) return null;

            if (isbn < nodo.Libro.Isbn)
            {
                nodo.Izquierdo = EliminarSucesor(nodo.Izquierdo, isbn);
            }
            else if (isbn > nodo.Libro.Isbn)
            {
                nodo.Derecho = EliminarSucesor(nodo.Derecho, isbn);
            }
            else
            {
                return nodo.Derecho; // el sucesor mínimo nunca tiene hijo izquierdo
            }

            return nodo;
        }

        private NodoIndiceLibro ObtenerMinimoNodo(NodoIndiceLibro nodo)
        {
            NodoIndiceLibro actual = nodo;
            while (actual.Izquierdo != null)
            {
                actual = actual.Izquierdo;
            }
            return actual;
        }

        public Models.Libro? ObtenerMinimo()
        {
            if (raiz == null) return null;
            return ObtenerMinimoNodo(raiz).Libro;
        }

        public Models.Libro? ObtenerMaximo()
        {
            if (raiz == null) return null;

            NodoIndiceLibro actual = raiz;
            while (actual.Derecho != null)
            {
                actual = actual.Derecho;
            }
            return actual.Libro;
        }

        public void RecorrerAscendente(Action<Models.Libro> accion)
        {
            RecorrerInOrden(raiz, accion);
        }

        private void RecorrerInOrden(NodoIndiceLibro? nodo, Action<Models.Libro> accion)
        {
            if (nodo == null) return;
            RecorrerInOrden(nodo.Izquierdo, accion);
            accion(nodo.Libro);
            RecorrerInOrden(nodo.Derecho, accion);
        }
    }
}