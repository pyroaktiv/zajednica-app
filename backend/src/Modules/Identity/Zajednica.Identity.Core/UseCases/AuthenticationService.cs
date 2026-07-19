using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;

namespace Zajednica.Identity.Core.UseCases;


public sealed class AuthenticationService : IAuthenticationService
{
    private const int MinPasswordLength = 8;

    private readonly IAccountRepository _accounts;
    private readonly IEmailVerificationTokenRepository _emailTokens;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokens;
    private readonly ISecureTokenGenerator _secureTokens;
    private readonly IEmailSender _email;
    private readonly IAuthTokenSettings _settings;

    public AuthenticationService(
        IAccountRepository accounts,
        IEmailVerificationTokenRepository emailTokens,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokens,
        ISecureTokenGenerator secureTokens,
        IEmailSender email,
        IAuthTokenSettings settings)
    {
        _accounts = accounts;
        _emailTokens = emailTokens;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _accessTokens = accessTokens;
        _secureTokens = secureTokens;
        _email = email;
        _settings = settings;
    }

    public async Task RegisterAsync(RegisterAccountRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < MinPasswordLength)
            throw new EntityValidationException($"Password must be at least {MinPasswordLength} characters.");

        var now = DateTime.UtcNow;
        var account = new Account(request.Username, request.Email, _passwordHasher.Hash(request.Password), now);

        if (await _accounts.ExistsByUsernameAsync(account.Username, ct))
            throw new EntityValidationException("Username is already taken.");
        if (await _accounts.ExistsByEmailAsync(account.Email, ct))
            throw new EntityValidationException("Email is already registered.");

        if (HasProfileData(request))
            account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, imageUrl: null);

        await _accounts.AddAsync(account, ct);

        var verificationToken = new EmailVerificationToken(
            account.Id, _secureTokens.Generate(), now.AddHours(_settings.EmailVerificationTokenHours));
        await _emailTokens.AddAsync(verificationToken, ct);

        await SendActivationEmailAsync(account.Email, verificationToken.Token, ct);
    }

    public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var token = await _emailTokens.GetByTokenAsync(request.Token, ct)
            ?? throw new EntityValidationException("Invalid verification token.");
        var account = await _accounts.GetByIdAsync(token.AccountId, ct)
            ?? throw new NotFoundException("Account not found.");

        await _emailTokens.RemoveAsync(token, ct); // single-use: consumed even when expired

        if (!token.IsValid(now))
            throw new EntityValidationException("Verification token has expired.");

        account.VerifyEmail();
        await _accounts.UpdateAsync(account, ct);
    }

    public async Task<AuthTokens> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var account = await _accounts.GetByUsernameOrEmailAsync(request.UsernameOrEmail, ct);
        
        if (account is null
            || account.Status == AccountStatus.Deleted
            || !_passwordHasher.Verify(request.Password, account.PasswordHash))
            throw new EntityValidationException("Invalid username/email or password.");

        if (!account.IsEmailVerified)
            throw new EntityValidationException("Email is not verified.");

        return await IssueTokensAsync(account, DateTime.UtcNow, ct);
    }

    public async Task<AuthTokens> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var current = await _refreshTokens.GetByTokenAsync(request.RefreshToken, ct)
            ?? throw new EntityValidationException("Invalid refresh token.");

        await _refreshTokens.RemoveAsync(current, ct); // rotation: the presented token is single-use

        if (!current.IsValid(now))
            throw new EntityValidationException("Refresh token has expired.");

        var account = await _accounts.GetByIdAsync(current.AccountId, ct);
        if (account is null || account.Status == AccountStatus.Deleted)
            throw new EntityValidationException("Invalid refresh token.");

        return await IssueTokensAsync(account, now, ct);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        var token = await _refreshTokens.GetByTokenAsync(request.RefreshToken, ct);
        if (token is null)
            return; // idempotent: an unknown / already-revoked token is a no-op

        await _refreshTokens.RemoveAsync(token, ct);
    }

    private async Task<AuthTokens> IssueTokensAsync(Account account, DateTime now, CancellationToken ct)
    {
        var accessToken = _accessTokens.Generate(account.Id, account.Username);
        var refreshToken = new RefreshToken(account.Id, _secureTokens.Generate(), now.AddDays(_settings.RefreshTokenDays));
        await _refreshTokens.AddAsync(refreshToken, ct);
        return new AuthTokens(accessToken, refreshToken.Token);
    }

    private Task SendActivationEmailAsync(string email, string token, CancellationToken ct)
    {
        var link = string.IsNullOrWhiteSpace(_settings.EmailActivationUrl)
            ? token
            : $"{_settings.EmailActivationUrl}?token={Uri.EscapeDataString(token)}";
        var body = $"Welcome to zajednica.app! Activate your account: {link}";
        return _email.SendAsync(email, "Activate your zajednica.app account", body, ct);
    }

    private static bool HasProfileData(RegisterAccountRequest r) =>
        !string.IsNullOrWhiteSpace(r.FirstName)
        || !string.IsNullOrWhiteSpace(r.LastName)
        || !string.IsNullOrWhiteSpace(r.Phone)
        || !string.IsNullOrWhiteSpace(r.ContactEmail);
}
