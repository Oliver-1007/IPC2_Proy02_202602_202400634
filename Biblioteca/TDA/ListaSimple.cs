


namespace Biblioteca.TDA
{
    public class ListaSimple<T>
    {
        private Nodo<T>? cabeza;
        private int cantidad;

        public int Cantidad => cantidad;
        public bool EstaVacia => cabeza == null;
        public Nodo<T>? Cabeza => cabeza;

        public void InsertarFinal(T dato)
        {
            Nodo<T> nuevo = new Nodo<T>(dato);

            if (cabeza == null)
            {
                cabeza = nuevo;
            }
            else
            {
                Nodo<T> actual = cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevo;
            }

            cantidad++;
        }

        public void InsertarOrdenado(T dato, Comparison<T> comparador)
        {
            Nodo<T> nuevo = new Nodo<T>(dato);

            if (cabeza == null || comparador(dato, cabeza.Dato) < 0)
            {
                nuevo.Siguiente = cabeza;
                cabeza = nuevo;
                cantidad++;
                return;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null && comparador(dato, actual.Siguiente.Dato) >= 0)
            {
                actual = actual.Siguiente;
            }

            nuevo.Siguiente = actual.Siguiente;
            actual.Siguiente = nuevo;
            cantidad++;
        }


        public bool Eliminar(Func<T, bool> criterio)
        {
            Nodo<T>? actual = cabeza;
            Nodo<T>? anterior = null;

            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    if (anterior == null)
                    {
                        cabeza = actual.Siguiente;
                    }
                    else
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }
                    cantidad--;
                    return true;
                }

                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        public T? Buscar(Func<T, bool> criterio)
        {
            Nodo<T>? actual = cabeza;
            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    return actual.Dato;
                }
                actual = actual.Siguiente;
            }
            return default;
        }

        public void Recorrer(Action<T> accion)
        {
            Nodo<T>? actual = cabeza;
            while (actual != null)
            {
                accion(actual.Dato);
                actual = actual.Siguiente;
            }
        }
    }
}