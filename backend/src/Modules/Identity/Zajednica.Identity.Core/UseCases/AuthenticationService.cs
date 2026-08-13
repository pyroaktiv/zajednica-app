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
    IAccountRepository accountRepository,
    IVerificationRepository verificationRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenGenerator accessTokenGenerator,
    ISecureTokenGenerator secureTokenGenerator,
    IEmailSender emailSender,
    IAuthTokenSettings authTokenSettings) : IAuthenticationService
{
    private const int MinPasswordLength = 8;

    public void Register(RegisterAccountRequestDto requestDto)
    {
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(requestDto.Password) || requestDto.Password.Length < MinPasswordLength)
            throw new EntityValidationException($"Password must be at least {MinPasswordLength} characters.");

        var account = new Account(requestDto.Username, requestDto.Email, passwordHasher.Hash(requestDto.Password), now);

        if (accountRepository.ExistsByUsername(account.Username))
            throw new EntityValidationException("Username is already taken.");
        if (accountRepository.ExistsByEmail(account.Email))
            throw new EntityValidationException("Email is already registered.");

        if (HasProfileData(requestDto))
            account.UpdateProfile(requestDto.FirstName, requestDto.LastName, requestDto.Phone, requestDto.ContactEmail);

        accountRepository.Add(account);

        var verification = new Verification(
            account.Id, secureTokenGenerator.GenerateShort(), now.AddHours(authTokenSettings.EmailVerificationTokenHours));
        verificationRepository.Add(verification);

        SendActivationEmail(account.Email, verification.Token);
    }

    public void VerifyEmail(VerifyEmailRequestDto requestDto)
    {
        var now = DateTime.UtcNow;

        var verification = verificationRepository.GetByToken(requestDto.Token)
            ?? throw new EntityValidationException("Invalid verification token.");
        var account = accountRepository.GetById(verification.AccountId)
            ?? throw new NotFoundException("Account not found.");

        verificationRepository.Remove(verification);

        if (!verification.IsValid(now))
            throw new EntityValidationException("Verification token has expired.");

        account.VerifyEmail();
        accountRepository.Update(account);
    }

    public AuthTokensDto Login(LoginRequestDto requestDto)
    {
        var account = accountRepository.GetByUsernameOrEmail(requestDto.UsernameOrEmail);

        if (account is null || !passwordHasher.Verify(requestDto.Password, account.PasswordHash))
            throw new EntityValidationException("Invalid username/email or password.");

        if (!account.IsEmailVerified)
            throw new EntityValidationException("Email is not verified.");

        return IssueTokens(account, DateTime.UtcNow);
    }

    public AuthTokensDto Refresh(RefreshRequestDto requestDto)
    {
        var now = DateTime.UtcNow;

        var current = refreshTokenRepository.GetByToken(requestDto.RefreshToken)
            ?? throw new EntityValidationException("Invalid refresh token.");

        refreshTokenRepository.Remove(current);

        if (!current.IsValid(now))
            throw new EntityValidationException("Refresh token has expired.");

        var account = accountRepository.GetById(current.AccountId)
            ?? throw new EntityValidationException("Invalid refresh token.");

        return IssueTokens(account, now);
    }

    public void Logout(LogoutRequestDto requestDto)
    {
        var token = refreshTokenRepository.GetByToken(requestDto.RefreshToken);
        if (token is null)
            return;

        refreshTokenRepository.Remove(token);
    }

    private AuthTokensDto IssueTokens(Account account, DateTime now)
    {
        var accessToken = accessTokenGenerator.Generate(account.Id, account.Username);
        var refreshToken = new RefreshToken(account.Id, secureTokenGenerator.Generate(), now.AddDays(authTokenSettings.RefreshTokenDays));
        refreshTokenRepository.Add(refreshToken);
        return new AuthTokensDto(accessToken, refreshToken.Token);
    }

    private void SendActivationEmail(string address, string token)
    {
        var body = $"Welcome to zajednica.app! Your activation code: {token}";
        emailSender.Send(address, "Activate your zajednica.app account", body);
    }

    private static bool HasProfileData(RegisterAccountRequestDto r) =>
        !string.IsNullOrWhiteSpace(r.FirstName)
        || !string.IsNullOrWhiteSpace(r.LastName)
        || !string.IsNullOrWhiteSpace(r.Phone)
        || !string.IsNullOrWhiteSpace(r.ContactEmail);
}
