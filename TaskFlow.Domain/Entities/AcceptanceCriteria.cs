namespace TaskFlow.Domain.Entities
{
    public class AcceptanceCriteria
    {
        public int Id { get; set; }
        public int FeatureRequestId { get; set; }
        public string Description { get; set; } = string.Empty; // Örn: "Kullanıcı başarılı giriş yapabilmeli."
        public bool IsCompleted { get; set; } = false;          // Kriter sağlandı mı / onaylandı mı?

        public FeatureRequest? FeatureRequest { get; set; }
    }
}