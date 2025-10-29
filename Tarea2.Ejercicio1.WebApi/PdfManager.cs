using Microsoft.AspNetCore.Routing.Matching;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Tarea2.Ejercicio1.WebApi
{
    public partial class PdfManager
    {
        private const string PDF_URI = "https://www.boe.es/boe/dias/2023/06/12/pdfs/BOE-A-2023-13811.pdf";
        public static readonly string OUTPUT_DIR = Path.Combine(AppContext.BaseDirectory, "Downloads");
        public static readonly string OUTPUT_PDF_FILE = Path.Combine(OUTPUT_DIR, "BOE-A-2023-13811.pdf");
        public static readonly string OUTPUT_JSON_FILE = Path.Combine(OUTPUT_DIR, "BOE-A-2023-13811.json");

        private static async Task DownloadPDFAsync()
        {
            try
            {
                if (!Directory.Exists(OUTPUT_DIR))
                    Directory.CreateDirectory(OUTPUT_DIR);
                using (HttpClient client = new())
                {
                    Console.WriteLine($"Descargando PDF desde: {PDF_URI}");
                    byte[] pdfBytes = await client.GetByteArrayAsync(PDF_URI);
                    await File.WriteAllBytesAsync(OUTPUT_PDF_FILE, pdfBytes);
                }
                Console.WriteLine($"✅ PDF guardado en: {OUTPUT_PDF_FILE}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al descargar el PDF: {ex.Message}");
                throw;
            }
        }

        private static async Task ReadAllPagesAsync()
        {
            using PdfDocument document = PdfDocument.Open(OUTPUT_PDF_FILE);
            int totalPages = document.NumberOfPages;
            Console.WriteLine($"📘 El PDF tiene {totalPages} páginas.\n");
            List<BoePdfPageData> boePdfPages = new();
            foreach (Page page in document.GetPages())
            {
                Regex regex = RealPageNumberRegex();
                BoePdfPageData boePdfPage = new BoePdfPageData
                {
                    Text = page.Text,
                    PdfPageNumber = page.Number,
                    DocumentPageNumber = int.Parse(regex.Match(page.Text).Groups[2].Value),
                };
                boePdfPages.Add(boePdfPage);
            }
            string json = JsonSerializer.Serialize(boePdfPages);
            await File.WriteAllTextAsync(OUTPUT_JSON_FILE, json);
            Console.WriteLine($"✅ JSON guardado en: {OUTPUT_JSON_FILE}");
        }

        private static async Task SaveAllRecordsAsync(BoePoliceDbContext dbContext)
        {
            List<BoePdfPageData> pages = JsonSerializer.Deserialize<List<BoePdfPageData>>(await File.ReadAllTextAsync(OUTPUT_JSON_FILE))!;
            List<BoePoliceCandidate> candidates = new();
            foreach (BoePdfPageData page in pages)
            {
                PoliceCandidateRegex().Matches(page.Text).ToList().ForEach(match =>
                {
                    string name = match.Groups[4].Value.Trim();
                    string lastName = match.Groups[3].Value.Trim();
                    BoePoliceCandidate candidate = new BoePoliceCandidate
                    {
                        Order = match.Groups[1].Value,
                        Dni = match.Groups[2].Value,
                        LastName = lastName,
                        Name = name,
                        FinalScore = decimal.Parse(match.Groups[5].Value.Replace(",", ".")),
                        DocumentPageNumber = page.DocumentPageNumber,
                        PdfPageNumber = page.PdfPageNumber,
                        NamePlusLastName = $"{name} {lastName}",
                        LastNamePlusName = $"{lastName} {name}"
                    };
                    candidates.Add(candidate);
                });
            }

            if (File.Exists(BoePoliceDbContext.DB_FILE))
            {
                Console.WriteLine("📚 La base de datos ya existe. Eliminando.");
                File.Delete(BoePoliceDbContext.DB_FILE);
            }
            Console.WriteLine($"📘 Creando base de datos en {BoePoliceDbContext.DB_FILE}.");
            dbContext.Database.EnsureCreated();
            Console.WriteLine("📦 Cargando registros.");
            PrintTable(candidates);
            dbContext.PoliceCandidates.AddRange(candidates);
            dbContext.SaveChanges();
        }

        static void PrintTable(List<BoePoliceCandidate> data)
        {
            Console.WriteLine(new string('═', 138));
            Console.WriteLine($"{"ID",-5} {"Order",-6} {"DNI",-10} {"LastName",-30} {"Name",-30} {"FinalScore",-12} {"DocPg",-8} {"PdfPg",-8}");
            Console.WriteLine(new string('─', 138));
            foreach (var c in data)
            {
                Console.WriteLine($"{c.Id,-5} {c.Order,-6} {c.Dni,-10} {c.LastName,-30} {c.Name,-30} {c.FinalScore,-12:F8} {c.DocumentPageNumber,-8} {c.PdfPageNumber,-8}");
            }
            Console.WriteLine(new string('═', 120));
        }

        public static async Task ProcessPDF(BoePoliceDbContext dbContext)
        {
            await DownloadPDFAsync();
            await ReadAllPagesAsync();
            await SaveAllRecordsAsync(dbContext);
        }

        // Pág. 123456
        [GeneratedRegex(@"(Pág\.\s?)(\d{1,6})")]
        private static partial Regex RealPageNumberRegex();

        // 1733**0066***SEGURA SANCHEZ, ALEJANDRO JOSE.71,18318234
        [GeneratedRegex(@"(\d{1,3})(\*\*\d{4}\*\*\*)([\w\sAÉÍÓÚÑ-]*)\,\s([\w\sAÉÍÓÚÑ-]*)\.(\d{1,3}\,\d{8})")]
        private static partial Regex PoliceCandidateRegex();

    }
}
