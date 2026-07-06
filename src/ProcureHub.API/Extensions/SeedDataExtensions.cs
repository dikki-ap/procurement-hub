using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.API.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedMasterDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var company = await SeedCompanyAsync(db);
        if (company is null) return;

        await SeedCurrenciesAsync(db);
        await SeedUnitsOfMeasureAsync(db, company.Id);
        await SeedPaymentTermsAsync(db, company.Id);
        await SeedMaterialCategoriesAsync(db, company.Id);
        await SeedLocationsAsync(db, company.Id);
        await SeedApprovalPoliciesAsync(db, company.Id);
    }

    private static async Task<Company?> SeedCompanyAsync(ApplicationDbContext db)
    {
        var existing = await db.Companies.FirstOrDefaultAsync();
        if (existing is not null) return existing;

        var company = new Company
        {
            Name     = "ProcureHub Manufacturing Co.",
            Code     = "PRCH",
            Type     = "Internal",
            IsActive = true,
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return company;
    }

    private static async Task SeedCurrenciesAsync(ApplicationDbContext db)
    {
        if (await db.Set<Currency>().AnyAsync()) return;

        db.Set<Currency>().AddRange(
            new Currency { Code = "IDR", Name = "Indonesian Rupiah", Symbol = "Rp", ExchangeRate = 1m,     IsBase = true  },
            new Currency { Code = "USD", Name = "US Dollar",         Symbol = "$",  ExchangeRate = 16000m, IsBase = false },
            new Currency { Code = "EUR", Name = "Euro",              Symbol = "€",  ExchangeRate = 17500m, IsBase = false },
            new Currency { Code = "SGD", Name = "Singapore Dollar",  Symbol = "S$", ExchangeRate = 12000m, IsBase = false }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedUnitsOfMeasureAsync(ApplicationDbContext db, Guid companyId)
    {
        if (await db.Set<UnitOfMeasure>().AnyAsync()) return;

        db.Set<UnitOfMeasure>().AddRange(
            new UnitOfMeasure { CompanyId = companyId, Code = "KG",   Name = "Kilogram" },
            new UnitOfMeasure { CompanyId = companyId, Code = "TON",  Name = "Ton"      },
            new UnitOfMeasure { CompanyId = companyId, Code = "PCS",  Name = "Pieces"   },
            new UnitOfMeasure { CompanyId = companyId, Code = "LTR",  Name = "Liter"    },
            new UnitOfMeasure { CompanyId = companyId, Code = "MTR",  Name = "Meter"    },
            new UnitOfMeasure { CompanyId = companyId, Code = "BOX",  Name = "Box"      },
            new UnitOfMeasure { CompanyId = companyId, Code = "SET",  Name = "Set"      },
            new UnitOfMeasure { CompanyId = companyId, Code = "UNIT", Name = "Unit"     }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedPaymentTermsAsync(ApplicationDbContext db, Guid companyId)
    {
        if (await db.Set<PaymentTerm>().AnyAsync()) return;

        db.Set<PaymentTerm>().AddRange(
            new PaymentTerm { CompanyId = companyId, Code = "COD",   Name = "Cash on Delivery", Days = 0,  Description = "Payment upon delivery"                },
            new PaymentTerm { CompanyId = companyId, Code = "NET7",  Name = "Net 7 Days",        Days = 7                                                       },
            new PaymentTerm { CompanyId = companyId, Code = "NET14", Name = "Net 14 Days",       Days = 14                                                      },
            new PaymentTerm { CompanyId = companyId, Code = "NET30", Name = "Net 30 Days",       Days = 30                                                      },
            new PaymentTerm { CompanyId = companyId, Code = "NET60", Name = "Net 60 Days",       Days = 60                                                      },
            new PaymentTerm { CompanyId = companyId, Code = "DP50",  Name = "Down Payment 50%",  Days = 30, Description = "50% upfront, remainder on delivery"  },
            new PaymentTerm { CompanyId = companyId, Code = "DP30",  Name = "Down Payment 30%",  Days = 30, Description = "30% upfront, remainder on delivery"  }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedMaterialCategoriesAsync(ApplicationDbContext db, Guid companyId)
    {
        if (await db.Set<MaterialCategory>().AnyAsync()) return;

        db.Set<MaterialCategory>().AddRange(
            new MaterialCategory { CompanyId = companyId, Code = "RM",    Name = "Raw Material",         IsStrategic = true  },
            new MaterialCategory { CompanyId = companyId, Code = "SP",    Name = "Spare Parts",          IsStrategic = false },
            new MaterialCategory { CompanyId = companyId, Code = "MRO",   Name = "Maintenance & Repair", IsStrategic = false },
            new MaterialCategory { CompanyId = companyId, Code = "CAPEX", Name = "Capital Expenditure",  IsStrategic = true  },
            new MaterialCategory { CompanyId = companyId, Code = "PKG",   Name = "Packaging",            IsStrategic = false },
            new MaterialCategory { CompanyId = companyId, Code = "CONS",  Name = "Consumables",          IsStrategic = false }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedLocationsAsync(ApplicationDbContext db, Guid companyId)
    {
        if (await db.Set<Location>().AnyAsync()) return;

        db.Set<Location>().AddRange(
            new Location { CompanyId = companyId, Name = "Main Warehouse",     Type = "warehouse", City = "Jakarta",   Country = "Indonesia" },
            new Location { CompanyId = companyId, Name = "Production Plant A", Type = "plant",     City = "Cikarang",  Country = "Indonesia" },
            new Location { CompanyId = companyId, Name = "Head Office",        Type = "office",    City = "South Jakarta", Country = "Indonesia" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedApprovalPoliciesAsync(ApplicationDbContext db, Guid companyId)
    {
        if (await db.Set<ApprovalPolicy>().AnyAsync()) return;

        // IsStrategicOverride = true  → +1 level if the document is a strategic item
        // IsSingleSourceOverride = true → +1 level if the document is single source
        db.Set<ApprovalPolicy>().AddRange(

            // ── PR Policies ──────────────────────────────────────────────────
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PR",
                Name = "PR Low Value (< 50M)",
                MinValue = 0, MaxValue = 49_999_999.9999m,
                RequiredLevels = 1, IsStrategicOverride = false, IsSingleSourceOverride = false,
            },
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PR",
                Name = "PR Medium Value (50M - 500M)",
                MinValue = 50_000_000, MaxValue = 499_999_999.9999m,
                RequiredLevels = 2, IsStrategicOverride = true, IsSingleSourceOverride = true,
            },
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PR",
                Name = "PR High Value (> 500M)",
                MinValue = 500_000_000, MaxValue = null,
                RequiredLevels = 3, IsStrategicOverride = false, IsSingleSourceOverride = false,
            },

            // ── PO Policies ──────────────────────────────────────────────────
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PO",
                Name = "PO Low Value (< 50M)",
                MinValue = 0, MaxValue = 49_999_999.9999m,
                RequiredLevels = 1, IsStrategicOverride = false, IsSingleSourceOverride = false,
            },
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PO",
                Name = "PO Medium Value (50M - 500M)",
                MinValue = 50_000_000, MaxValue = 499_999_999.9999m,
                RequiredLevels = 2, IsStrategicOverride = true, IsSingleSourceOverride = true,
            },
            new ApprovalPolicy
            {
                CompanyId = companyId, ReferenceType = "PO",
                Name = "PO High Value (> 500M)",
                MinValue = 500_000_000, MaxValue = null,
                RequiredLevels = 3, IsStrategicOverride = false, IsSingleSourceOverride = false,
            }
        );
        await db.SaveChangesAsync();
    }
}
