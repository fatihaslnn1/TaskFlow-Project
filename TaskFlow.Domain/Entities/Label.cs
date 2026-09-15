namespace TaskFlow.Domain.Entities
{
    public class Label
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // "urgent", "frontend", "database"
        public string Color { get; set; } = string.Empty; // "#ff0000", "#00ff00"
        public int ProjectId { get; set; } // Etiketler projeye özel olsun
        
        public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();
    }
}