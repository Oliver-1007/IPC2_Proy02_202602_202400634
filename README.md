# IPC2 - Proyecto 2 · Catálogo de Librería

Sistema web (ASP.NET Core MVC, C#) para administrar de forma jerárquica el
catálogo de una cadena de librerías: categorías/subcategorías y libros,
con búsquedas eficientes y representación gráfica mediante Graphviz.

## Estudiante
- Nombre: Oliver Jorge Raxtún Morales
- Carné: 202400634

## Requisitos para ejecutar
- .NET SDK 8.0 o superior
- Graphviz instalado y el comando `dot` disponible en el PATH
  (necesario únicamente para las opciones "Ver estructura gráfica" y
  "Ver libros (ISBN asc.)")
  - Windows: https://graphviz.org/download/ (agregar la carpeta `bin` al PATH)
  - Ubuntu/Debian: `sudo apt install graphviz`
  - macOS: `brew install graphviz`

## Cómo ejecutar
```bash
dotnet restore
dotnet run
```
Luego abrir la URL que indique la consola (por ejemplo `https://localhost:5001`).

## Estructura del proyecto
```
TDA/            Estructuras de datos propias (sin usar List/Queue/Stack/etc. de .NET)
    ListaSimple.cs           TDA Lista Simple Enlazada con jerarquía de nodos y polimorfismo
    NodoIndiceCategoria.cs   Nodo del árbol binario de búsqueda de categorías
    ArbolIndiceCategorias.cs Árbol binario de búsqueda (índice por nombre de categoría)
    NodoIndiceLibro.cs       Nodo del árbol binario de búsqueda de libros
    ArbolIndiceLibros.cs     Árbol binario de búsqueda (índice por ISBN)

Modelo/         Clases del dominio (POO)
    Categoria.cs             Nodo del árbol jerárquico de categorías
    Libro.cs                 Entidad Libro
    ResultadoOperacion.cs    Resultado (éxito/mensaje) de las operaciones

Servicios/      Lógica de negocio
    CatalogoService.cs       Alta/baja/búsqueda de categorías y libros, carga XML, generación DOT
    GraphvizService.cs       Invoca el comando "dot" para generar imágenes PNG

Controllers/    Controladores MVC (Home, Categorias, Libros)
Views/          Vistas Razor (interfaz web)
wwwroot/        Archivos estáticos (CSS, imágenes generadas por Graphviz)
```

## Diseño de las estructuras de datos (TDA)
- **Árbol jerárquico de categorías**: cada `Categoria` contiene una
  `ListaCategorias` de subcategorías (mantenida siempre ordenada
  alfabéticamente al insertar) y una `ListaLibros` con los libros
  asociados directamente a ella. Las categorías raíz se guardan en otra
  `ListaCategorias` dentro de `CatalogoService`.
- **Índice de categorías por nombre** (`ArbolIndiceCategorias`): árbol
  binario de búsqueda que permite verificar la unicidad de nombres y
  localizar cualquier categoría en tiempo logarítmico promedio, sin
  recorrer todo el árbol jerárquico.
- **Índice de libros por ISBN** (`ArbolIndiceLibros`): árbol binario de
  búsqueda que permite insertar, buscar, eliminar, obtener el mínimo y el
  máximo, y recorrer los libros en orden ascendente (recorrido in-orden),
  todo en tiempo logarítmico promedio.

Ninguna de estas estructuras utiliza listas nativas ni clases genéricas de la
librería estándar de C#; todas fueron implementadas desde cero para el proyecto
con polimorfismo y nodos propios.

## Notas
- Los mensajes de éxito/error se muestran mediante `TempData` en la parte
  superior del contenido, después de cada acción.
- El botón "Inicializar sistema" (en la pantalla de Inicio) limpia toda la
  información en memoria, tal como lo pide el enunciado.
- Actualice el enlace de la vista `Views/Home/Ayuda.cshtml` cuando publique
  la documentación del proyecto.
- Recuerde crear el repositorio en GitHub con el nombre
  `IPC2_Proy02_202602_202400634`, agregar a Jorge Mejía (`jorgemejia25`)
  como colaborador si aplica, y realizar al menos 4 releases conforme
  avance el desarrollo.
