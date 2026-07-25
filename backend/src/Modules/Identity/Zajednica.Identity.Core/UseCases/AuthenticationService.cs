using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Security;
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

    public void Register(RegisterAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < MinPasswordLength)
            throw new EntityValidationException($"Password must be at least {MinPasswordLength} characters.");

        var now = DateTime.UtcNow;
        var account = new Account(request.Username, request.Email, _passwordHasher.Hash(request.Password), now);

        if (_accounts.ExistsByUsername(account.Username))
            throw new EntityValidationException("Username is already taken.");
        if (_accounts.ExistsByEmail(account.Email))
            throw new EntityValidationException("Email is already registered.");

        if (HasProfileData(request))
            account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, imageUrl: null);

        _accounts.Add(account);

        var verificationToken = new EmailVerificationToken(
            account.Id, _secureTokens.Generate(), now.AddHours(_settings.EmailVerificationTokenHours));
        _emailTokens.Add(verificationToken);

        SendActivationEmail(account.Email, verificationToken.Token);
    }

    public void VerifyEmail(VerifyEmailRequest request)
    {
        var now = DateTime.UtcNow;

        var token = _emailTokens.GetByToken(request.Token)
            ?? throw new EntityValidationException("Invalid verification token.");
        var account = _accounts.GetById(token.AccountId)
            ?? throw new NotFoundException("Account not found.");

        _emailTokens.Remove(token);

        if (!token.IsValid(now))
            throw new EntityValidationException("Verification token has expired.");

        account.VerifyEmail();
        _accounts.Update(account);
    }

    public AuthTokens Login(LoginRequest request)
    {
        var account = _accounts.GetByUsernameOrEmail(request.UsernameOrEmail);

        if (account is null || !_passwordHasher.Verify(request.Password, account.PasswordHash))
            throw new EntityValidationException("Invalid username/email or password.");

        if (!account.IsEmailVerified)
            throw new EntityValidationException("Email is not verified.");

        return IssueTokens(account, DateTime.UtcNow);
    }

    public AuthTokens Refresh(RefreshRequest request)
    {
        var now = DateTime.UtcNow;

        var current = _refreshTokens.GetByToken(request.RefreshToken)
            ?? throw new EntityValidationException("Invalid refresh token.");

        _refreshTokens.Remove(current);

        if (!current.IsValid(now))
            throw new EntityValidationException("Refresh token has expired.");

        var account = _accounts.GetById(current.AccountId)
            ?? throw new EntityValidationException("Invalid refresh token.");

        return IssueTokens(account, now);
    }

    public void Logout(LogoutRequest request)
    {
        var token = _refreshTokens.GetByToken(request.RefreshToken);
        if (token is null)
            return;

        _refreshTokens.Remove(token);
    }

    private AuthTokens IssueTokens(Account account, DateTime now)
    {
        var accessToken = _accessTokens.Generate(account.Id, account.Username);
        var refreshToken = new RefreshToken(account.Id, _secureTokens.Generate(), now.AddDays(_settings.RefreshTokenDays));
        _refreshTokens.Add(refreshToken);
        return new AuthTokens(accessToken, refreshToken.Token);
    }

    private void SendActivationEmail(string email, string token)
    {
        var link = string.IsNullOrWhiteSpace(_settings.EmailActivationUrl)
            ? token
            : $"{_settings.EmailActivationUrl}?token={Uri.EscapeDataString(token)}";
        var body = $"Welcome to zajednica.app! Activate your account: {link}";
        _email.Send(email, "Activate your zajednica.app account", body);
    }

    private static bool HasProfileData(RegisterAccountRequest r) =>
        !string.IsNullOrWhiteSpace(r.FirstName)
        || !string.IsNullOrWhiteSpace(r.LastName)
        || !string.IsNullOrWhiteSpace(r.Phone)
        || !string.IsNullOrWhiteSpace(r.ContactEmail);
}
