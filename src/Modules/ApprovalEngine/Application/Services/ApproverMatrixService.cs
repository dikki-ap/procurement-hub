using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Services;

public class ApproverMatrixService : IApproverMatrixService
{
    private readonly IApproverMatrixRepository _matrixRepo;
    private readonly ApplicationDbContext       _db;

    public ApproverMatrixService(IApproverMatrixRepository matrixRepo, ApplicationDbContext db)
    {
        _matrixRepo = matrixRepo;
        _db         = db;
    }

    public async Task<List<ApproverLevelRequest>> ResolveApproversAsync(
        Guid companyId, string referenceType, int requiredLevels, CancellationToken ct = default)
    {
        var entries = await _matrixRepo.GetByCompanyAndTypeAsync(
            companyId, referenceType, requiredLevels, ct);

        var emailSet = entries.Select(e => e.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var usersByEmail = await _db.Set<User>()
            .Where(u => emailSet.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, u => u, StringComparer.OrdinalIgnoreCase, ct);

        var result = new List<ApproverLevelRequest>();

        for (var level = 1; level <= requiredLevels; level++)
        {
            var levelEntries = entries.Where(e => e.Level == level).ToList();

            if (levelEntries.Count == 0)
                throw new BusinessRuleException(
                    "ApproverMatrix",
                    $"No approver configured for {referenceType} Level {level}. Please configure the Approver Matrix first.");

            foreach (var entry in levelEntries)
            {
                if (!usersByEmail.TryGetValue(entry.Email, out var user))
                    throw new BusinessRuleException(
                        "ApproverMatrix",
                        $"Approver '{entry.Name}' ({entry.Email}) at Level {entry.Level} has not logged into the system yet.");

                result.Add(new ApproverLevelRequest(entry.Level, user.Id, user.FullName));
            }
        }

        return result;
    }
}
