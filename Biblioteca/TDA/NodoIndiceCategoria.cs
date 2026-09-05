


namespace Biblioteca.TDA
{
    public class NodoIndiceCategoria
    {
        public string Nombre { get; set; }
        public Models.Categoria Referencia { get; set; }
        public NodoIndiceCategoria? Izquierdo { get; set; }
        public NodoIndiceCategoria? Derecho { get; set; }

        public NodoIndiceCategoria(string nombre, Models.Categoria referencia)
        {
            Nombre = nombre;
            Referencia = referencia;
        }
    }
}