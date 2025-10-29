using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tarea2.Ejercicio2.ConsoleApp.Model;

namespace Tarea2.Ejercicio2.ConsoleApp.Services
{
    public class FakeStoreService
    {
        private readonly HttpClient client = new()
        {
            BaseAddress = new Uri("https://fakestoreapi.com/")
        };

        private readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public async Task<List<Product>> GetProductsAsync()
        {
            var response = await client.GetAsync("products");
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var products = await JsonSerializer.DeserializeAsync<List<Product>>(stream, options);
            return products ?? new List<Product>();
        }
    }
}
