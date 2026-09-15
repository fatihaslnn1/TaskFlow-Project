namespace TaskFlow.Domain.Entities
{
    public class IssueTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;      // Örn: "Standart Bug Şablonu"
        public string IssueType { get; set; } = string.Empty; // Hangi tür için? (Bug, Task, Feature)
        public string Content { get; set; } = string.Empty;   // Şablonun içeriği (Markdown veya Text)
    }
}