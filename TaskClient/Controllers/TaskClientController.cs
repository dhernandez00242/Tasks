using Microsoft.AspNetCore.Mvc;
using TaskClient.Models;
using Newtonsoft.Json;
using System.Text;
using System.Security.Policy;

namespace TaskClient.Controllers
{
    public class TaskClientController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;


        public TaskClientController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {

            _configuration = configuration;
            string? ClientStringConnection = _configuration["UrlApis"];

            if (!string.IsNullOrEmpty(ClientStringConnection))
            {
                _httpClient = httpClientFactory.CreateClient();
                _httpClient.BaseAddress = new Uri(ClientStringConnection);
            }
            else
            {
                throw new Exception("La cadena de conexión es nula o vacía.");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            string url = _httpClient.BaseAddress + "/Listar";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {

                var content = await response.Content.ReadAsStringAsync();

                var tareas = JsonConvert.DeserializeObject<IEnumerable<TaskViewModel>>(content);
                return View("Index", tareas);
            }

            return View(new List<TaskViewModel>());
        }

        public IActionResult create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskViewModel tarea)
        {
            if (ModelState.IsValid)
            {
                var json = JsonConvert.SerializeObject(tarea);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                string url = _httpClient.BaseAddress + "/Crear";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");// Manejar el caso de creación exitosa.
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear la tarea.");
                }
            }
            return View(tarea);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            string url = _httpClient.BaseAddress + $"/Obtener/{id}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tarea = JsonConvert.DeserializeObject<TaskViewModel>(content);

                return View(tarea);
            }
            else
            {
                return RedirectToAction("Details");
            }
        }


        /// <summary>
        ///  Editar  tarea
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tarea"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string url = _httpClient.BaseAddress + $"/Obtener/{id}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {

                    var content = await response.Content.ReadAsStringAsync();
                    var tarea = JsonConvert.DeserializeObject<TaskViewModel>(content);
                    return View(tarea);
                }
                else
                {
                    return RedirectToAction("Details");
                }

            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, TaskViewModel tarea)
        {

            if (ModelState.IsValid)
            {
                var Json = JsonConvert.SerializeObject(tarea);
                var content = new StringContent(Json, Encoding.UTF8, "application/json");

                var url = _httpClient.BaseAddress + $"/Editar/{id}";
                var response = await _httpClient.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", new { id });
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Error al actualizar la tarea ");
                }
            }

            return View(tarea);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {

            string url = _httpClient.BaseAddress + $"/Eliminar/{id}";
            var response = await _httpClient.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Error al eliminar la tarea.";
                return RedirectToAction("Index");
            }
        }


        /// <summary>
        /// ReOrdenar lista
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> ReOrdenar(int id, int nuevoOrden)
        {
            try
            {
                string url = _httpClient.BaseAddress + $"/Obtener/{id}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Error al obtener los datos de la tarea.");

                var content = await response.Content.ReadAsStringAsync();

                url = _httpClient.BaseAddress + $"/ReOrdenar/{id}/{nuevoOrden}";

                response = await _httpClient.PutAsync(url, new StringContent(content));

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Error al reordenar la tarea.");

                return await Details(id);

            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }

        }


        /// <summary>
        /// Marcar : Marca tarea como [Hecho]
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<IActionResult> Marcar(int id)
        {
            try
            {
                string url = _httpClient.BaseAddress + $"/Obtener/{id}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Error al obtener los datos de la tarea.");

                var content = await response.Content.ReadAsStringAsync();

                url = _httpClient.BaseAddress + $"/Marcar/{id}";

                response = await _httpClient.PutAsync(url, new StringContent(content));

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("Error al marcar la tarea.");

               // return await Details(id);
                return RedirectToAction("Index");

            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}
