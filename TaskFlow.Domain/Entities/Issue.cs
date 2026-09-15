namespace TaskFlow.Domain.Entities
{
    public class Issue : ISoftDeletable
    {
        public int Id { get; set; }
        public string IssueKey { get; set; } = string.Empty; 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // --- KONTROLCÜLERİN KULLANDIĞI STRING ALANLAR (Hataları önler) ---
        public string Status { get; set; } = "Todo"; 
        public string Priority { get; set; } = "Medium"; 
        public string IssueType { get; set; } = "Task"; 
        // -----------------------------------------------------------------

        // --- PDF İÇİN YENİ VERİTABANI İLİŞKİLERİ (26. Madde Tabloları) ---
        public int IssueStatusId { get; set; } = 1;
        public IssueStatus? IssueStatusEntity { get; set; }

        public int IssuePriorityId { get; set; } = 2;
        public IssuePriority? IssuePriorityEntity { get; set; }

        public int IssueTypeId { get; set; } = 1;
        public IssueType? IssueTypeEntity { get; set; }
        // -----------------------------------------------------------------

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int ReporterId { get; set; }
        public User? Reporter { get; set; }

        public int? AssigneeId { get; set; }
        public User? Assignee { get; set; }

        // Sprint ve Milestone İlişkileri (19. Madde)
        public int? SprintId { get; set; }
        public Sprint? Sprint { get; set; }

        public int? MilestoneId { get; set; }
        public Milestone? Milestone { get; set; }

        public DateTime? DueDate { get; set; }                  
        public decimal? EstimatedEffort { get; set; }        

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ISoftDeletable Alanları (25. Madde)
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }

        public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
        public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();
        public ICollection<SprintIssue> SprintIssues { get; set; } = new List<SprintIssue>();
        public ICollection<Mention> Mentions { get; set; } = new List<Mention>();

        // Parent / Child İlişkisi İçin
        public int? ParentIssueId { get; set; }
        public Issue? ParentIssue { get; set; }
        public ICollection<Issue> ChildIssues { get; set; } = new List<Issue>();

        // Diğer İlişkiler İçin
        public ICollection<IssueRelation> SourceRelations { get; set; } = new List<IssueRelation>();
        public ICollection<IssueRelation> TargetRelations { get; set; } = new List<IssueRelation>();
    }
}