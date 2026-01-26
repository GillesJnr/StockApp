using Microsoft.EntityFrameworkCore;
using StockManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace StockManagement.Infrastructure.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StockManagement.Infrastructure.Persistence.StockDbContext>();

        // Apply migrations for relational providers
        try
        {
            if (db.Database.IsRelational())
            {
                await db.Database.MigrateAsync(cancellationToken);
            }
        }
        catch
        {
            // ignore migration errors in development
        }

        if (!await db.RetentionPolicies.AnyAsync(cancellationToken))
        {
            var defaults = new List<RetentionPolicy>
            {
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.CreditCard, RetentionDays = 90, Notes = "Default 90 days" },
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.CreditCardPin, RetentionDays = 90 },
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.ReturnedCheque, RetentionDays = 60 },
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.CapturedCard, RetentionDays = 60 },
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.ChequeBook, RetentionDays = 90 },
                new RetentionPolicy { Type = StockManagement.Domain.Enums.ControlledItemType.DebitCard, RetentionDays = 90 }
            };
            await db.RetentionPolicies.AddRangeAsync(defaults, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.CaptureReasons.AnyAsync(cancellationToken))
        {
            var reasons = new List<CaptureReason>
            {
                new CaptureReason { Code = "PIN_TRIES_EXCEEDED", Description = "PIN tries exceeded" },
                new CaptureReason { Code = "AT_ISSUERS_REQUEST", Description = "At Issuer's Request" },
                new CaptureReason { Code = "RESTRICTED", Description = "Restricted" },
                new CaptureReason { Code = "CARD_CAPTURE", Description = "Card Capture" },
                new CaptureReason { Code = "LOST_STOLEN", Description = "Lost/Stolen Card" },
                new CaptureReason { Code = "PLAIN_CARD", Description = "Plain Card/Cardboard", ContactFraudOps = true, RaiseRiskEvent = true },
                new CaptureReason { Code = "FRAUDULENT", Description = "Fraudulent Card", ContactFraudOps = true, RaiseRiskEvent = true }
            };
            await db.CaptureReasons.AddRangeAsync(reasons, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
