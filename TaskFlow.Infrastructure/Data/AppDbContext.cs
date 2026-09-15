using TaskFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Label> Labels { get; set; }
        public DbSet<IssueLabel> IssueLabels { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<FeatureRequest> FeatureRequests { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<IssueRelation> IssueRelations { get; set; }
        public DbSet<Sprint> Sprints { get; set; }         // (19. Madde)
        public DbSet<Milestone> Milestones { get; set; }   // (19. Madde)
        public DbSet<AcceptanceCriteria> AcceptanceCriterias { get; set; } // (20. Madde)
        public DbSet<SavedFilter> SavedFilters { get; set; }               // (23. Madde)
        public DbSet<IssueTemplate> IssueTemplates { get; set; }           // (24. Madde)

        // YENİ EKLENEN PDF TABLOLARI (26. Madde)
        public DbSet<IssueType> IssueTypes { get; set; }
        public DbSet<IssueStatus> IssueStatuses { get; set; }
        public DbSet<IssuePriority> IssuePriorities { get; set; }
        public DbSet<Mention> Mentions { get; set; }
        public DbSet<SprintIssue> SprintIssues { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Query Filters (25. Madde - Soft Delete olanları gizle)
            modelBuilder.Entity<Project>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Issue>().HasQueryFilter(i => !i.IsDeleted);

            modelBuilder.Entity<WorkLog>()
                .Property(w => w.HoursSpent)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Issue>()
                .Property(i => i.EstimatedEffort)
                .HasColumnType("decimal(18,2)");

            // --- YENİ EKLENEN KISIM: Varsayılan Değerler (Foreign Key Hatalarını Önler) ---
            modelBuilder.Entity<Issue>()
                .Property(i => i.IssueStatusId)
                .HasDefaultValue(1); // Todo

            modelBuilder.Entity<Issue>()
                .Property(i => i.IssuePriorityId)
                .HasDefaultValue(2); // Medium

            modelBuilder.Entity<Issue>()
                .Property(i => i.IssueTypeId)
                .HasDefaultValue(1); // Task
            // --------------------------------------------------------------------------

            // Label ve Issue Arasındaki Many-to-Many İlişkisi
            modelBuilder.Entity<IssueLabel>()
                .HasKey(il => new { il.IssueId, il.LabelId });

            modelBuilder.Entity<IssueLabel>()
                .HasOne(il => il.Issue)
                .WithMany(i => i.IssueLabels)
                .HasForeignKey(il => il.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IssueLabel>()
                .HasOne(il => il.Label)
                .WithMany(l => l.IssueLabels)
                .HasForeignKey(il => il.LabelId)
                .OnDelete(DeleteBehavior.Cascade);

            // SprintIssue (Many-to-Many) İlişkisi
            modelBuilder.Entity<SprintIssue>()
                .HasKey(si => new { si.SprintId, si.IssueId });

            modelBuilder.Entity<SprintIssue>()
                .HasOne(si => si.Sprint)
                .WithMany(s => s.SprintIssues)
                .HasForeignKey(si => si.SprintId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SprintIssue>()
                .HasOne(si => si.Issue)
                .WithMany(i => i.SprintIssues)
                .HasForeignKey(si => si.IssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Issue Relations
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.ParentIssue)
                .WithMany(i => i.ChildIssues)
                .HasForeignKey(i => i.ParentIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IssueRelation>()
                .HasOne(ir => ir.SourceIssue)
                .WithMany(i => i.SourceRelations)
                .HasForeignKey(ir => ir.SourceIssueId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IssueRelation>()
                .HasOne(ir => ir.TargetIssue)
                .WithMany(i => i.TargetRelations)
                .HasForeignKey(ir => ir.TargetIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sprint ve Milestone
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Sprint)
                .WithMany(s => s.Issues)
                .HasForeignKey(i => i.SprintId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Milestone)
                .WithMany(m => m.Issues)
                .HasForeignKey(i => i.MilestoneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Acceptance Criteria
            modelBuilder.Entity<AcceptanceCriteria>()
                .HasOne(ac => ac.FeatureRequest)
                .WithMany(fr => fr.AcceptanceCriterias)
                .HasForeignKey(ac => ac.FeatureRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Saved Filter
            modelBuilder.Entity<SavedFilter>()
                .HasOne(sf => sf.User)
                .WithMany()
                .HasForeignKey(sf => sf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mentions İlişkisi
            modelBuilder.Entity<Mention>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================= SEED DATA (Başlangıç Verileri) =================

            modelBuilder.Entity<IssueType>().HasData(
                new IssueType { Id = 1, Name = "Task" },
                new IssueType { Id = 2, Name = "Bug" },
                new IssueType { Id = 3, Name = "Feature" }
            );

            modelBuilder.Entity<IssueStatus>().HasData(
                new IssueStatus { Id = 1, Name = "Todo" },
                new IssueStatus { Id = 2, Name = "In Progress" },
                new IssueStatus { Id = 3, Name = "Done" }
            );

            modelBuilder.Entity<IssuePriority>().HasData(
                new IssuePriority { Id = 1, Name = "Low" },
                new IssuePriority { Id = 2, Name = "Medium" },
                new IssuePriority { Id = 3, Name = "High" },
                new IssuePriority { Id = 4, Name = "Critical" }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "PM" },
                new Role { Id = 3, Name = "Developer" },
                new Role { Id = 4, Name = "Reporter" },
                new Role { Id = 5, Name = "Customer" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Admin",
                    Email = "admin@taskflow.com",
                    PasswordHash = PasswordHasher.HashPassword("Admin123!"),
                    IsActive = true,
                    RoleId = 1
                }
            );

            // Issue Şablonları (24. Madde)
            modelBuilder.Entity<IssueTemplate>().HasData(
                new IssueTemplate
                {
                    Id = 1,
                    Name = "Standart Bug Şablonu",
                    IssueType = "Bug",
                    Content = "• **Beklenen Davranış:**\n\n• **Gerçekleşen Davranış:**\n\n• **Tekrarlama Adımları:**\n1. \n2. \n3. \n\n• **Ortam:**\n\n• **Browser:**\n\n• **OS:**\n"
                },
                new IssueTemplate
                {
                    Id = 2,
                    Name = "Standart Feature Şablonu",
                    IssueType = "Feature",
                    Content = "• **Kullanıcı Hikayesi (User Story):**\n\n• **Kabul Kriterleri (Acceptance Criteria):**\n- \n- \n- "
                }
            );

            // Foreign Key İlişkileri
            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Reporter)
                .WithMany()
                .HasForeignKey(i => i.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Issue>()
                .HasOne(i => i.Assignee)
                .WithMany()
                .HasForeignKey(i => i.AssigneeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}