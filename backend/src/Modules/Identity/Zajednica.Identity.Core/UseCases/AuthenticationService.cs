using Zajednica.BuildingBlocks.Core.Exceptions;
using Zajednica.BuildingBlocks.Core.Security;
using Zajednica.Identity.Api.Dto;
using Zajednica.Identity.Api.Public;
using Zajednica.Identity.Core.Domain;
using Zajednica.Identity.Core.Infrastructural;
using Zajednica.Identity.Core.Domain.RepositoryInterfaces;
using Zajednica.Identity.Core.Infrastructural.RepositoryInterfaces;

namespace Zajednica.Identity.Core.UseCases;

public sealed class AuthenticationService(
    IAccountRepository accounts,
    IVerificationRepository verifications,
    IRefreshTokenRepository refreshTokens,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokens,
    ISecureTokenGenerator secureTokens,
    IEmailSender email,
    IAuthTokenSettings settings) : IAuthenticationService
{
    public void Register(RegisterAccountRequest request)
    {
        var now = DateTime.UtcNow;
        var account = Account.Register(request.Username, request.Email, request.Password, passwordHasher, now);

        if (accounts.ExistsByUsername(account.Username))
            throw new EntityValidationException("Username is already taken.");
        if (accounts.ExistsByEmail(account.Email))
            throw new EntityValidationException("Email is already registered.");

        if (HasProfileData(request))
            account.UpdateProfile(request.FirstName, request.LastName, request.Phone, request.ContactEmail, imageUrl: null);

        accounts.Add(account);

        var verification = new Verification(
            account.Id, secureTokens.GenerateShort(), now.AddHours(settings.EmailVerificationTokenHours));
        verifications.Add(verification);

        SendActivationEmail(account.Email, verification.Token);
    }

    public void VerifyEmail(VerifyEmailRequest request)
    {
        var now = DateTime.UtcNow;

        var verification = verifications.GetByToken(request.Token)
            ?? throw new EntityValidationException("Invalid verification token.");
        var account = accounts.GetById(verification.AccountId)
            ?? throw new NotFoundException("Account not found.");

        verifications.Remove(verification);

        if (!verification.IsValid(now))
            throw new EntityValidationException("Verification token has expired.");

        account.VerifyEmail();
        accounts.Update(account);
    }

    public AuthTokens Login(LoginRequest request)
    {
        var account = accounts.GetByUsernameOrEmail(request.UsernameOrEmail);

        if (account is null || !passwordHasher.Verify(request.Password, account.Password))
            throw new EntityValidationException("Invalid username/email or password.");

        if (!account.IsEmailVerified)
            throw new EntityValidationException("Email is not verified.");

        return IssueTokens(account, DateTime.UtcNow);
    }

    public AuthTokens Refresh(RefreshRequest request)
    {
        var now = DateTime.UtcNow;

        var current = refreshTokens.GetByToken(request.RefreshToken)
            ?? throw new EntityValidationException("Invalid refresh token.");

        refreshTokens.Remove(current);

        if (!current.IsValid(now))
            throw new EntityValidationException("Refresh token has expired.");

        var account = accounts.GetById(current.AccountId)
            ?? throw new EntityValidationException("Invalid refresh token.");

        return IssueTokens(account, now);
    }

    public void Logout(LogoutRequest request)
    {
        var token = refreshTokens.GetByToken(request.RefreshToken);
        if (token is null)
            return;

        refreshTokens.Remove(token);
    }

    private AuthTokens IssueTokens(Account account, DateTime now)
    {
        var accessToken = accessTokens.Generate(account.Id, account.Username);
        var refreshToken = new RefreshToken(account.Id, secureTokens.Generate(), now.AddDays(settings.RefreshTokenDays));
        refreshTokens.Add(refreshToken);
        return new AuthTokens(accessToken, refreshToken.Token);
    }

    private void SendActivationEmail(string address, string token)
    {
        var body = $"Welcome to zajednica.app! Your activation code: {token}";
        email.Send(address, "Activate your zajednica.app account", body);
    }

    private static bool HasProfileData(RegisterAccountRequest r) =>
        !string.IsNullOrWhiteSpace(r.FirstName)
        || !string.IsNullOrWhiteSpace(r.LastName)
        || !string.IsNullOrWhiteSpace(r.Phone)
        || !string.IsNullOrWhiteSpace(r.ContactEmail);
}
