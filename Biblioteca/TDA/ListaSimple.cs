


namespace Biblioteca.TDA
{
    public delegate int ComparadorCategoria(Models.Categoria a, Models.Categoria b);
    public delegate bool CriterioCategoria(Models.Categoria categoria);
    public delegate void AccionCategoria(Models.Categoria categoria);

    public delegate int ComparadorLibro(Models.Libro a, Models.Libro b);
    public delegate bool CriterioLibro(Models.Libro libro);
    public delegate void AccionLibro(Models.Libro libro);

    public abstract class NodoLista
    {
    }

    public sealed class NodoCategoria : NodoLista
    {
        public Models.Categoria Dato { get; set; }
        public NodoCategoria? Siguiente { get; set; }

        public NodoCategoria(Models.Categoria dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public sealed class NodoLibro : NodoLista
    {
        public Models.Libro Dato { get; set; }
        public NodoLibro? Siguiente { get; set; }

        public NodoLibro(Models.Libro dato)
        {
            Dato = dato;
            Siguiente = null;
        }
    }

    public abstract class ListaSimple
    {
        private int cantidad;

        public int Cantidad => cantidad;
        public bool EstaVacia => cantidad == 0;

        protected void AumentarCantidad() => cantidad++;
        protected void DisminuirCantidad() => cantidad--;
    }

    public sealed class ListaCategorias : ListaSimple
    {
        public NodoCategoria? Cabeza { get; private set; }

        public void InsertarFinal(Models.Categoria dato)
        {
            NodoCategoria nuevo = new NodoCategoria(dato);

            if (Cabeza == null)
            {
                Cabeza = nuevo;
            }
            else
            {
                NodoCategoria? actual = Cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }

                actual.Siguiente = nuevo;
            }

            AumentarCantidad();
        }

        public void InsertarOrdenado(Models.Categoria dato, ComparadorCategoria comparador)
        {
            NodoCategoria nuevo = new NodoCategoria(dato);

            if (Cabeza == null || comparador(dato, Cabeza.Dato) < 0)
            {
                nuevo.Siguiente = Cabeza;
                Cabeza = nuevo;
                AumentarCantidad();
                return;
            }

            NodoCategoria? actual = Cabeza;
            while (actual.Siguiente != null && comparador(dato, actual.Siguiente.Dato) >= 0)
            {
                actual = actual.Siguiente;
            }

            nuevo.Siguiente = actual.Siguiente;
            actual.Siguiente = nuevo;
            AumentarCantidad();
        }

        public bool Eliminar(CriterioCategoria criterio)
        {
            NodoCategoria? actual = Cabeza;
            NodoCategoria? anterior = null;

            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    if (anterior == null)
                    {
                        Cabeza = actual.Siguiente;
                    }
                    else
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }

                    DisminuirCantidad();
                    return true;
                }

                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        public Models.Categoria? Buscar(CriterioCategoria criterio)
        {
            NodoCategoria? actual = Cabeza;
            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    return actual.Dato;
                }

                actual = actual.Siguiente;
            }

            return null;
        }

        public void Recorrer(AccionCategoria accion)
        {
            NodoCategoria? actual = Cabeza;
            while (actual != null)
            {
                accion(actual.Dato);
                actual = actual.Siguiente;
            }
        }
    }

    public sealed class ListaLibros : ListaSimple
    {
        public NodoLibro? Cabeza { get; private set; }

        public void InsertarFinal(Models.Libro dato)
        {
            NodoLibro nuevo = new NodoLibro(dato);

            if (Cabeza == null)
            {
                Cabeza = nuevo;
            }
            else
            {
                NodoLibro? actual = Cabeza;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }

                actual.Siguiente = nuevo;
            }

            AumentarCantidad();
        }

        public void InsertarOrdenado(Models.Libro dato, ComparadorLibro comparador)
        {
            NodoLibro nuevo = new NodoLibro(dato);

            if (Cabeza == null || comparador(dato, Cabeza.Dato) < 0)
            {
                nuevo.Siguiente = Cabeza;
                Cabeza = nuevo;
                AumentarCantidad();
                return;
            }

            NodoLibro? actual = Cabeza;
            while (actual.Siguiente != null && comparador(dato, actual.Siguiente.Dato) >= 0)
            {
                actual = actual.Siguiente;
            }

            nuevo.Siguiente = actual.Siguiente;
            actual.Siguiente = nuevo;
            AumentarCantidad();
        }

        public bool Eliminar(CriterioLibro criterio)
        {
            NodoLibro? actual = Cabeza;
            NodoLibro? anterior = null;

            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    if (anterior == null)
                    {
                        Cabeza = actual.Siguiente;
                    }
                    else
                    {
                        anterior.Siguiente = actual.Siguiente;
                    }

                    DisminuirCantidad();
                    return true;
                }

                anterior = actual;
                actual = actual.Siguiente;
            }

            return false;
        }

        public Models.Libro? Buscar(CriterioLibro criterio)
        {
            NodoLibro? actual = Cabeza;
            while (actual != null)
            {
                if (criterio(actual.Dato))
                {
                    return actual.Dato;
                }

                actual = actual.Siguiente;
            }

            return null;
        }

        public void Recorrer(AccionLibro accion)
        {
            NodoLibro? actual = Cabeza;
            while (actual != null)
            {
                accion(actual.Dato);
                actual = actual.Siguiente;
            }
        }
    }

}