namespace Biblioteca.TDA
{
    public class AVLIndiceLibros
    {
        private NodoIndiceLibro? raiz;
        private int cantidad;

        public int Cantidad => cantidad;
        public NodoIndiceLibro? Raiz => raiz;

        public void Insertar(Models.Libro libro)
        {
            bool insertado = false;
            raiz = InsertarRecursivo(raiz, libro, ref insertado);

            if (insertado)
            {
                cantidad++;
            }
        }

        private NodoIndiceLibro InsertarRecursivo(
            NodoIndiceLibro? nodo,
            Models.Libro libro,
            ref bool insertado)
        {
            if (nodo == null)
            {
                insertado = true;
                return new NodoIndiceLibro(libro);
            }

            if (libro.Isbn < nodo.Libro.Isbn)
            {
                nodo.Izquierdo = InsertarRecursivo(nodo.Izquierdo, libro, ref insertado);
            }
            else if (libro.Isbn > nodo.Libro.Isbn)
            {
                nodo.Derecho = InsertarRecursivo(nodo.Derecho, libro, ref insertado);
            }
            else
            {
                return nodo;
            }

            ActualizarAltura(nodo);
            return Balancear(nodo);
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

                actual = isbn < actual.Libro.Isbn
                    ? actual.Izquierdo
                    : actual.Derecho;
            }

            return null;
        }

        public bool Eliminar(int isbn)
        {
            bool eliminado = false;
            raiz = EliminarRecursivo(raiz, isbn, ref eliminado);

            if (eliminado)
            {
                cantidad--;
            }

            return eliminado;
        }

        private NodoIndiceLibro? EliminarRecursivo(
            NodoIndiceLibro? nodo,
            int isbn,
            ref bool eliminado)
        {
            if (nodo == null)
            {
                return null;
            }

            if (isbn < nodo.Libro.Isbn)
            {
                nodo.Izquierdo = EliminarRecursivo(nodo.Izquierdo, isbn, ref eliminado);
            }
            else if (isbn > nodo.Libro.Isbn)
            {
                nodo.Derecho = EliminarRecursivo(nodo.Derecho, isbn, ref eliminado);
            }
            else
            {
                eliminado = true;

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
                bool eliminadoSucesor = false;
                nodo.Derecho = EliminarRecursivo(
                    nodo.Derecho,
                    sucesor.Libro.Isbn,
                    ref eliminadoSucesor);
            }

            ActualizarAltura(nodo);
            return Balancear(nodo);
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
            if (raiz == null)
            {
                return null;
            }

            return ObtenerMinimoNodo(raiz).Libro;
        }

        public Models.Libro? ObtenerMaximo()
        {
            if (raiz == null)
            {
                return null;
            }

            NodoIndiceLibro actual = raiz;
            while (actual.Derecho != null)
            {
                actual = actual.Derecho;
            }

            return actual.Libro;
        }

        public void RecorrerAscendente(AccionLibro accion)
        {
            RecorrerInOrden(raiz, accion);
        }

        private void RecorrerInOrden(NodoIndiceLibro? nodo, AccionLibro accion)
        {
            if (nodo == null)
            {
                return;
            }

            RecorrerInOrden(nodo.Izquierdo, accion);
            accion(nodo.Libro);
            RecorrerInOrden(nodo.Derecho, accion);
        }

        private static int ObtenerAltura(NodoIndiceLibro? nodo)
        {
            return nodo?.Altura ?? 0;
        }

        private static void ActualizarAltura(NodoIndiceLibro nodo)
        {
            int alturaIzquierda = ObtenerAltura(nodo.Izquierdo);
            int alturaDerecha = ObtenerAltura(nodo.Derecho);
            int mayorAltura = alturaIzquierda > alturaDerecha
                ? alturaIzquierda
                : alturaDerecha;
            nodo.Altura = 1 + mayorAltura;
        }

        private static int ObtenerBalance(NodoIndiceLibro nodo)
        {
            return ObtenerAltura(nodo.Izquierdo) - ObtenerAltura(nodo.Derecho);
        }

        private static NodoIndiceLibro Balancear(NodoIndiceLibro nodo)
        {
            int balance = ObtenerBalance(nodo);

            if (balance > 1)
            {
                if (ObtenerBalance(nodo.Izquierdo!) < 0)
                {
                    nodo.Izquierdo = RotarIzquierda(nodo.Izquierdo!);
                }

                return RotarDerecha(nodo);
            }

            if (balance < -1)
            {
                if (ObtenerBalance(nodo.Derecho!) > 0)
                {
                    nodo.Derecho = RotarDerecha(nodo.Derecho!);
                }

                return RotarIzquierda(nodo);
            }

            return nodo;
        }

        private static NodoIndiceLibro RotarDerecha(NodoIndiceLibro nodo)
        {
            NodoIndiceLibro nuevaRaiz = nodo.Izquierdo!;
            NodoIndiceLibro? subarbol = nuevaRaiz.Derecho;

            nuevaRaiz.Derecho = nodo;
            nodo.Izquierdo = subarbol;

            ActualizarAltura(nodo);
            ActualizarAltura(nuevaRaiz);
            return nuevaRaiz;
        }

        private static NodoIndiceLibro RotarIzquierda(NodoIndiceLibro nodo)
        {
            NodoIndiceLibro nuevaRaiz = nodo.Derecho!;
            NodoIndiceLibro? subarbol = nuevaRaiz.Izquierdo;

            nuevaRaiz.Izquierdo = nodo;
            nodo.Derecho = subarbol;

            ActualizarAltura(nodo);
            ActualizarAltura(nuevaRaiz);
            return nuevaRaiz;
        }
    }
}
