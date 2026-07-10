using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.MasterData.Domain.Entities;

public class DocumentType : BaseAuditableEntity
{
    public string  Name              { get; set; } = string.Empty;
    public bool    IsActive          { get; set; } = true;
    /// <summary>Comma-separated allowed extensions e.g. ".pdf,.jpg". Null = all globally allowed types.</summary>
    public string? AllowedExtensions { get; set; }
    /// <summary>Maximum file size in MB. Defaults to 10.</summary>
    public int     MaxFileSizeMb     { get; set; } = 10;
}
