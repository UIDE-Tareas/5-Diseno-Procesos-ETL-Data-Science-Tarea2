namespace Tarea2.Ejercicio1.WebApi
{
    public class BoePoliceCandidate
    {
        public long Id { get; set; }
        public required string Order { get; set; } = null!;
        public required string Dni { get; set; } = null!;
        public required string Name { get; set; } = null!;
        public required string LastName { get; set; } = null!;
        public required string NamePlusLastName { get; set; } = null!;
        public required string LastNamePlusName { get; set; } = null!;
        public required decimal FinalScore { get; set; }
        public required int DocumentPageNumber { get; set; }
        public required int PdfPageNumber { get; set; }

    }
}
