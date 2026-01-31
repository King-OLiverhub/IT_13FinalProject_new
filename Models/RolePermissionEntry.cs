namespace IT_13FinalProject.Models
{
    public class RolePermissionEntry
    {
        public int RolePermissionEntryId { get; set; }
        public string RoleKey { get; set; } = string.Empty;
        public string PermissionKey { get; set; } = string.Empty;
        public bool IsAllowed { get; set; }
    }
}
