namespace ProcureHub.SharedKernel.Domain;

public class Department : BaseAuditableEntity
{
    public Guid    CompanyId { get; set; }
    public string  Name      { get; set; } = string.Empty;
    public string  Code      { get; set; } = string.Empty;
    public bool    IsActive  { get; set; } = true;

    public Company?  Company  { get; set; }
}
