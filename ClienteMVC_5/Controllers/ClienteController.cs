using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

using ClienteMVC_5.Models;
using Newtonsoft.Json;
using PagedList;

namespace ClienteMVC_5.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index(int? pagina)
        {
            int paginaTamanho = 4;
            int paginaNumero = pagina ?? 1;

            IEnumerable<ClienteViewModel> clientes = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/");

                // HTTP GET
                var responseTask = client.GetAsync("clientes");
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var data = readTask.Result;
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy" // Define o formato de data que corresponde ao formato no JSON
                    };

                    clientes = JsonConvert.DeserializeObject<IList<ClienteViewModel>>(data, settings);
                }
                else
                {
                    clientes = Enumerable.Empty<ClienteViewModel>();
                    ModelState.AddModelError(string.Empty, "Erro no servidor. Contate o Administrador.");
                }

                return View(clientes.ToPagedList(paginaNumero, paginaTamanho));
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(ClienteViewModel cliente)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/");

                // Verifica se o CPF já existe na API
                var cpfExists = await CheckIfCPFExists(cliente.Cpf);
                if (cpfExists)
                {
                    ModelState.AddModelError("Cpf", "Este CPF já está cadastrado.");
                    return View(cliente);
                }
                //HTTP POST
                var postTask = client.PostAsJsonAsync("clientes", cliente);
                postTask.Wait();
                var result = postTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                if (cliente == null)
                {
                    ModelState.AddModelError(string.Empty, "Preencha todos os campos obrigatórios.");
                    return View(cliente);
                }
            }
            return View(cliente);
        }


        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ClienteViewModel clientes = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/clientes");

                // HTTP GET
                var responseTask = client.GetAsync($"clientes/{id}");
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var data = readTask.Result;
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy"
                    };

                    clientes = JsonConvert.DeserializeObject<ClienteViewModel>(data, settings);


                    return View(clientes);
                }
            }


            return View("Error");
        }


        [HttpPost]
        public async Task<ActionResult> Edit(ClienteViewModel cliente)
        {
            if (ModelState.IsValid)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:44394/api/clientes");

                    var response = await client.PutAsJsonAsync($"clientes/{cliente.Id}", cliente);



                    if (response.IsSuccessStatusCode)
                    {

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Erro ao editar o cliente. Contate o Administrador.");
                    }
                }
            }

            return View("Error");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClienteViewModel cliente = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/");

                //HTTP GET
                var responseTask = client.GetAsync($"clientes/{id}");
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var readTask = result.Content.ReadAsStringAsync();
                    readTask.Wait();

                    var data = readTask.Result;
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy"
                    };

                    cliente = JsonConvert.DeserializeObject<ClienteViewModel>(data, settings);

                    return View(cliente);
                }
            }

            return View("Error");
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/");

                // HTTP DELETE
                var deleteTask = client.DeleteAsync($"clientes/{id}");
                deleteTask.Wait();
                var result = deleteTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }


            return View("Error");
        }


        public ActionResult GetClientePorNome(string nome, int? pagina)
        {
            if (string.IsNullOrEmpty(nome))
            {
                return RedirectToAction("Index");
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/clientes");

                var responseTask = client.GetAsync($"clientes/Search/{nome}");
                responseTask.Wait();
                var result = responseTask.Result;

                if (result.IsSuccessStatusCode)
                {
                    var data = result.Content.ReadAsStringAsync().Result;

                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "dd/MM/yyyy"
                    };

                    var clientes = JsonConvert.DeserializeObject<List<ClienteViewModel>>(data, settings);

                    int numeroItensPorPagina = 4;
                    int numeroPagina = pagina ?? 1;
                    var clientesPagedList = new StaticPagedList<ClienteViewModel>(clientes, numeroPagina, numeroItensPorPagina, clientes.Count);

                    return View("Index", clientesPagedList);
                }
            }

            return View("Error");
        }



        private async Task<bool> CheckIfCPFExists(string cpf)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44394/api/");

                // HTTP GET para verificar se o CPF já existe
                var response = await client.GetAsync($"clientes/CheckCPFExists?cpf={cpf}");
                if (response.IsSuccessStatusCode)
                {
                    var exists = await response.Content.ReadAsAsync<bool>();
                    return exists;
                }
            }

            return false;
        }

        public ActionResult Error()
        {
            return View();
        }

    }
}