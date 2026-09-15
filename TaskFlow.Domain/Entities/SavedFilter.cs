namespace TaskFlow.Domain.Entities
{
    public class SavedFilter
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;       // Örn: "Benim Kritik İşlerim"
        public string CriteriaJson { get; set; } = string.Empty; // Filtre koşulları (JSON veya parametre metni olarak saklanır)

        public User? User { get; set; }
    }
}