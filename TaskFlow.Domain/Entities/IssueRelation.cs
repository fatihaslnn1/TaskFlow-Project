namespace TaskFlow.Domain.Entities
{
    public class IssueRelation
    {
        public int Id { get; set; }
        
        public int SourceIssueId { get; set; }
        public Issue? SourceIssue { get; set; }

        public int TargetIssueId { get; set; }
        public Issue? TargetIssue { get; set; }

        public string RelationType { get; set; } = string.Empty; 
        // "RelatedTo", "Blocks", "DependsOn", "DuplicateOf" vb.
    }
}