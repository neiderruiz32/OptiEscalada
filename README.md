# OptiEscalada

## Descripción
OptiEscalada es una aplicación de consola que implementa un servidor HTTP básico para gestionar una lista de elementos, cada uno con propiedades de nombre, peso y calorías. La aplicación permite agregar, eliminar y optimizar combinaciones de elementos bajo restricciones de peso máximo y calorías mínimas. Incluye también una interfaz web simple para interactuar con el servidor.

## Funcionalidades

- Servidor HTTP corriendo en `http://localhost:8080`
- Endpoints disponibles:
  - `/` o `/index.html`: Página web con interfaz para gestionar elementos y optimizar combinaciones.
  - `/elementos`: Obtiene la lista actual de elementos en formato JSON (GET).
  - `/agregar`: Agrega un nuevo elemento (POST).
  - `/eliminar`: Elimina un elemento (DELETE).
  - `/optimizar`: Devuelve todas las combinaciones de elementos que cumplen con el peso máximo y calorías mínimas indicadas (POST).

## Modelo de Datos

- `Elemento`
  - `Nombre` (string): Nombre identificador del elemento.
  - `Peso` (int): Peso del elemento.
  - `Calorias` (int): Calorías del elemento.

- `OptimizacionRequest`
  - `PesoMaximo` (int): Peso máximo permitido para las combinaciones.
  - `MinCalorias` (int): Calorías mínimas requeridas para las combinaciones.

- `ResultadoOptimizacion`
  - `ElementosSeleccionados` (lista de Elemento): Elementos en la combinación.
  - `PesoTotal` (int): Peso total de la combinación.
  - `CaloriasTotales` (int): Calorías totales de la combinación.

## Tecnologías utilizadas

- C# (.NET 6+)
- HttpListener para servidor HTTP básico
- Serialización JSON con System.Text.Json
- HTML, CSS (Bootstrap 5) para la interfaz web

## Cómo ejecutar el proyecto

1. Clona este repositorio:
2. Compila y ejecuta el proyecto (desde consola o Visual Studio):
3. Abre tu navegador web en: http://localhost:8080
4. Usa la interfaz para agregar, eliminar elementos y optimizar combinaciones.

## Contacto

Para dudas o sugerencias, puedes contactarme vía email o abrir un issue en el repositorio.  
**Neider Alfonso Ruiz Garcia**  
Correo: neiderruizgarcias@gmail.com

---

¡Gracias por usar OptiEscalada!





