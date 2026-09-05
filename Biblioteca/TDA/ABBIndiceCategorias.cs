


namespace Biblioteca.TDA
{
    public class ABBIndiceCategorias
    {
        private NodoIndiceCategoria? raiz;

        public void Insertar(string nombre, Models.Categoria referencia)
        {
            raiz = InsertarRecursivo(raiz, nombre, referencia);
        }

        private NodoIndiceCategoria InsertarRecursivo(NodoIndiceCategoria? nodo, string nombre, Models.Categoria referencia)
        {
            if (nodo == null)
            {
                return new NodoIndiceCategoria(nombre, referencia);
            }

            int comparacion = string.Compare(nombre, nodo.Nombre, StringComparison.OrdinalIgnoreCase);

            if (comparacion < 0)
            {
                nodo.Izquierdo = InsertarRecursivo(nodo.Izquierdo, nombre, referencia);
            }
            else if (comparacion > 0)
            {
                nodo.Derecho = InsertarRecursivo(nodo.Derecho, nombre, referencia);
            }
            // Si es igual (nombre repetido) no se inserta; los nombres son únicos.

            return nodo;
        }

        public bool Existe(string nombre)
        {
            return Buscar(nombre) != null;
        }

        public Models.Categoria? Buscar(string nombre)
        {
            NodoIndiceCategoria? actual = raiz;

            while (actual != null)
            {
                int comparacion = string.Compare(nombre, actual.Nombre, StringComparison.OrdinalIgnoreCase);

                if (comparacion == 0)
                {
                    return actual.Referencia;
                }

                actual = comparacion < 0 ? actual.Izquierdo : actual.Derecho;
            }

            return null;
        }
    }
}