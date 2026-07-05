namespace Shared.Infrastructure.Settings
{
    public class FirebaseSettings
    {
        public const string SectionName = "Firebase";

        public string ServiceAccountJson { get; set; } = string.Empty;
    }
}
