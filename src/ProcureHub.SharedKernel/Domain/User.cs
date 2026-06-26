namespace ProcureHub.SharedKernel.Domain;

public class User : BaseAuditableEntity
{
    public Guid CompanyId { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
}
