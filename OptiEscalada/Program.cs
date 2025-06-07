using OptiEscalada;
using System.Net;
using System.Text;
using System.Text.Json;

class Program
{
    static List<Elemento> elementos = new List<Elemento>
    {
        new Elemento("El", 5, 3),
        new Elemento("E2", 3, 5),
        new Elemento("E3", 5, 2),
        new Elemento("E4", 1, 8),
        new Elemento("ES", 2, 3),
    };

    static void Main()
    {
        Console.WriteLine("Servidor iniciado en http://localhost:8080");
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        while (true)
        {
            var context = listener.GetContext();
            Task.Run(() => HandleRequest(context));
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        try
        {
            string url = string.Empty;

            if (context.Request.Url != null)
            {
                url = context.Request.Url.AbsolutePath;
            }

            Console.WriteLine($"Request: {url}");

            if (url == "/" || url == "/index.html")
            {
                string html = GetHtmlPage();
                byte[] buffer = Encoding.UTF8.GetBytes(html);
                context.Response.ContentType = "text/html; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (url == "/elementos")
            {
                string json = JsonSerializer.Serialize(elementos);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            else if (url == "/agregar" && context.Request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();
                var nuevo = JsonSerializer.Deserialize<Elemento>(body);
                if (nuevo != null)
                {
                    elementos.Add(nuevo);
                }
                context.Response.StatusCode = 200;
            }

            else if (url == "/eliminar" && context.Request.HttpMethod == "DELETE")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();
                var toDelete = JsonSerializer.Deserialize<Elemento>(body);

                if (toDelete != null)
                {
                    elementos.RemoveAll(e => e.Nombre == toDelete.Nombre);
                    context.Response.StatusCode = 200;
                }
                else
                {
                    context.Response.StatusCode = 400;
                    Console.WriteLine("El objeto a eliminar es nulo.");
                }

            }
            else if (url == "/optimizar" && context.Request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                string body = reader.ReadToEnd();
                var request = JsonSerializer.Deserialize<OptimizacionRequest>(body);

                if (request != null)
                {
                    var resultados = OptimizarTodasCombinaciones(elementos, request.PesoMaximo, request.MinCalorias);
                    string jsonResponse = JsonSerializer.Serialize(resultados);

                    byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
                    context.Response.ContentType = "application/json";
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    Console.WriteLine("Parámetros inválidos en la solicitud.");
                }

            }
            else
            {
                context.Response.StatusCode = 404;
                byte[] buffer = Encoding.UTF8.GetBytes("404 - Not Found");
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            context.Response.StatusCode = 500;
            byte[] buffer = Encoding.UTF8.GetBytes("500 - Error interno");
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }

    static List<ResultadoOptimizacion> OptimizarTodasCombinaciones(List<Elemento> elementos, int pesoMaximo, int minCalorias)
    {
        List<ResultadoOptimizacion> resultados = new();
        int n = elementos.Count;
        int combinaciones = 1 << n;

        for (int mask = 1; mask < combinaciones; mask++)
        {
            int pesoTotal = 0;
            int caloriasTotales = 0;
            List<Elemento> seleccion = new();

            for (int i = 0; i < n; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    pesoTotal += elementos[i].Peso;
                    caloriasTotales += elementos[i].Calorias;
                    seleccion.Add(elementos[i]);
                }
            }

            if (pesoTotal <= pesoMaximo && caloriasTotales >= minCalorias)
            {
                resultados.Add(new ResultadoOptimizacion(seleccion, pesoTotal, caloriasTotales));
            }
        }

        return resultados;
    }

    static string GetHtmlPage()
    {
        return @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Optimizador de elementos</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
    <style>
        body {
            background: linear-gradient(to right, #0f2027, #203a43, #2c5364);
            color: white;
            font-family: 'Segoe UI', sans-serif;
        }
        .card {
            background-color: #f8f9fa;
            color: #212529;
        }
        .form-control, .btn {
            border-radius: 0.5rem;
        }
    </style>
</head>
<body class='container py-5'>

    <h1 class='mb-4'>Optimizador de elementos</h1>

    <!-- Formulario para agregar/editar elementos -->
    <form id='addItemForm' class='row g-3 mb-4'>
        <div class='col-md-3'>
            <input type='text' class='form-control' placeholder='Nombre del elemento' id='nombre' required>
        </div>
        <div class='col-md-2'>
            <input type='number' class='form-control' placeholder='Peso (kg)' id='peso' required>
        </div>
        <div class='col-md-2'>
            <input type='number' class='form-control' placeholder='Calorías' id='calorias' required>
        </div>
        <input type='hidden' id='editIndex' value='-1'>
        <div class='col-md-2'>
            <button type='submit' class='btn btn-success w-100'>Guardar</button>
        </div>
        <div class='col-md-2'>
            <button type='button' class='btn btn-secondary w-100' onclick='resetForm()'>Cancelar</button>
        </div>
    </form>

    <!-- Formulario para optimizar -->
    <form id='optimizationForm' class='row g-3 mb-4'>
        <div class='col-md-4'>
            <input type='number' id='minCalories' class='form-control' placeholder='Calorías mínimas' required>
        </div>
        <div class='col-md-4'>
            <input type='number' id='maxWeight' class='form-control' placeholder='Peso máximo (kg)' required>
        </div>
        <div class='col-md-4'>
            <button type='submit' class='btn btn-primary w-100'>Optimizar</button>
        </div>
    </form>

    <h2>Elementos disponibles</h2>
    <ul id='itemsList' class='list-group mb-4'></ul>

    <h2>Resultados de optimización</h2>
    <div id='resultSection'></div>

    <script>
        async function loadItems() {
            const res = await fetch('/elementos');
            const items = await res.json();
            const list = document.getElementById('itemsList');
            list.innerHTML = '';
            items.forEach((item, index) => {
                const li = document.createElement('li');
                li.className = 'list-group-item bg-dark text-light d-flex justify-content-between align-items-center';
                li.innerHTML = `
                    <div>
                        <strong>${item.Nombre}</strong> - Peso: ${item.Peso} kg, Calorías: ${item.Calorias}
                    </div>
                    <div>
                        <button class='btn btn-sm btn-danger' onclick='deleteItem(${index})'>Eliminar</button>
                    </div>`;
                list.appendChild(li);
            });
        }

async function deleteItem(index) {
    const res = await fetch('/elementos');
    const items = await res.json();
    const item = items[index];

    await fetch('/eliminar', {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(item)
    });
    await loadItems();
}
        
        function resetForm() {
            document.getElementById('addItemForm').reset();
            document.getElementById('editIndex').value = -1;
        }

        document.getElementById('addItemForm').addEventListener('submit', async e => {
            e.preventDefault();
            const nombre = document.getElementById('nombre').value;
            const peso = parseInt(document.getElementById('peso').value);
            const calorias = parseInt(document.getElementById('calorias').value);
            const index = parseInt(document.getElementById('editIndex').value);

            if (index === -1) {
                await fetch('/agregar', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Nombre: nombre, Peso: peso, Calorias: calorias })
                });
            } else {
                await fetch('/editar/' + index, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ Nombre: nombre, Peso: peso, Calorias: calorias })
                });
            }

            resetForm();
            await loadItems();
        });

        document.getElementById('optimizationForm').addEventListener('submit', async e => {
            e.preventDefault();
            const minCalories = parseInt(document.getElementById('minCalories').value);
            const maxWeight = parseInt(document.getElementById('maxWeight').value);

            const response = await fetch('/optimizar', {
                method: 'POST',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({MinCalorias: minCalories, PesoMaximo: maxWeight})
            });

            const results = await response.json();
            const resultSection = document.getElementById('resultSection');
            resultSection.innerHTML = '';

            if (results.length === 0) {
                resultSection.innerHTML = `<div class='alert alert-warning'>No se encontraron combinaciones válidas.</div>`;
                return;
            }

            results.forEach((result, index) => {
                const div = document.createElement('div');
                div.className = 'card mb-3';
                div.innerHTML = `
                    <div class='card-body'>
                        <h5 class='card-title'>Opción #${index + 1}</h5>
                        <p><strong>Peso total:</strong> ${result.PesoTotal} kg</p>
                        <p><strong>Calorías totales:</strong> ${result.CaloriasTotales}</p>
                        <ul class='list-group'>
                            ${result.ElementosSeleccionados.map(e => `<li class='list-group-item'>${e.Nombre} - Peso: ${e.Peso} kg, Calorías: ${e.Calorias}</li>`).join('')}
                        </ul>
                    </div>`;
                resultSection.appendChild(div);
            });
        });

        loadItems();
    </script>
</body>
</html>
";
    }
    public record OptimizacionRequest(int MinCalorias, int PesoMaximo);
    public record ResultadoOptimizacion(List<Elemento> ElementosSeleccionados, int PesoTotal, int CaloriasTotales);
}
