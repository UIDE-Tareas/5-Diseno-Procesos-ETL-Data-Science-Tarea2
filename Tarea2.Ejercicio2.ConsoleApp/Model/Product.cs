using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarea2.Ejercicio2.ConsoleApp.Model
{
    public class Product
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public string Category { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Image { get; set; } = null!;
        public Rating Rating { get; set; } = new();
    }
}
