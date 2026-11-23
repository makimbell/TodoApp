using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

/// <summary>
/// Resolves external OAuth/OIDC logins (Microsoft, Google) to a single internal User ID,
/// auto-linking logins that share a verified email address.
/// </summary>
public class ExternalLoginService(IDbContextFactory<AppDbContext> dbFactory)
{
    // Microsoft's fixed tenant ID for personal Microsoft accounts (MSA)
    private const string MicrosoftPersonalTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

    public async Task ResolveOrCreateUserAsync(string provider, ClaimsIdentity identity, CancellationToken cancellationToken = default)
    {
        var rawSub = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(rawSub))
        {
            return;
        }

        var email = (identity.FindFirst(ClaimTypes.Email)?.Value
                     ?? identity.FindFirst("email")?.Value)?.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(email)
            ? null
            : email.ToUpperInvariant();
        var displayName = (identity.FindFirst(ClaimTypes.Name)?.Value
                           ?? identity.FindFirst("name")?.Value)?.Trim();
        var isEmailVerified = IsEmailVerified(provider, identity);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        var existingLogin = await db.UserLogins
            .Include(ul => ul.User)
            .FirstOrDefaultAsync(ul => ul.Provider == provider && ul.ProviderKey == rawSub, cancellationToken);

        string userId;

        if (existingLogin is not null)
        {
            userId = existingLogin.UserId;

            // Backfill verified email on migrated/legacy User rows so Option B can auto-link the other provider later
            if (isEmailVerified && normalizedEmail is not null && existingLogin.User.NormalizedEmail is null)
            {
                var emailInUse = await db.Users
                    .AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != userId, cancellationToken);

                if (!emailInUse)
                {
                    existingLogin.User.Email = email;
                    existingLogin.User.NormalizedEmail = normalizedEmail;
                    if (string.IsNullOrWhiteSpace(existingLogin.User.DisplayName) && !string.IsNullOrWhiteSpace(displayName))
                    {
                        existingLogin.User.DisplayName = displayName;
                    }

                    await db.SaveChangesAsync(cancellationToken);
                }
            }
        }
        else
        {
            User? user = null;

            // Option B: auto-link by normalized email only when the provider verifies the email
            if (isEmailVerified && normalizedEmail is not null)
            {
                user = await db.Users
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
            }

            if (user is null)
            {
                user = new User
                {
                    Email = isEmailVerified ? email : null,
                    NormalizedEmail = isEmailVerified ? normalizedEmail : null,
                    DisplayName = displayName
                };
                db.Users.Add(user);
            }

            db.UserLogins.Add(new UserLogin
            {
                UserId = user.Id,
                Provider = provider,
                ProviderKey = rawSub
            });

            await db.SaveChangesAsync(cancellationToken);
            userId = user.Id;
        }

        var existingNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (existingNameId is not null)
        {
            identity.RemoveClaim(existingNameId);
        }
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    /// <summary>
    /// Permanently deletes a user account, its linked external logins, and all owned todo lists/items.
    /// </summary>
    public async Task DeleteUserAccountAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            // TodoItems cascade-delete from TodoLists; UserLogins cascade-delete from Users.
            await db.TodoLists
                .Where(l => l.OwnerId == userId)
                .ExecuteDeleteAsync(cancellationToken);

            await db.Users
                .Where(u => u.Id == userId)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static bool IsEmailVerified(string provider, ClaimsIdentity identity)
    {
        var emailVerified = identity.FindFirst("email_verified")?.Value;
        if (string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(provider, "Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            // Entra ID emits xms_edov ("1" or "true") when the email domain owner is verified
            var xmsEdov = identity.FindFirst("xms_edov")?.Value;
            if (string.Equals(xmsEdov, "1", StringComparison.Ordinal)
                || string.Equals(xmsEdov, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Personal Microsoft accounts (MSA) verify email addresses at account creation
            var tenantId = identity.FindFirst("tid")?.Value
                           ?? identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            if (string.Equals(tenantId, MicrosoftPersonalTenantId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

