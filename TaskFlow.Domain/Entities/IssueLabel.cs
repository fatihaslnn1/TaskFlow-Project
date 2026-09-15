namespace TaskFlow.Domain.Entities
{
    public class IssueLabel
    {
        public int IssueId { get; set; }
        public Issue? Issue { get; set; }

        public int LabelId { get; set; }
        public Label? Label { get; set; }
    }
}