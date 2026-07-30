namespace Zajednica.BuildingBlocks.Core.Security;

public interface ISecureTokenGenerator
{
    string Generate();
    string GenerateShort();
}
