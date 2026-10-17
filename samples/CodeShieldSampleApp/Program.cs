using System;

namespace CodeShieldSampleApp;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== CodeShield Sample Application ===");
        
        var vault = new SecretVault("RootAdminKey_2026_Secure");
        vault.Unlock("RootAdminKey_2026_Secure");

        var processor = new PaymentProcessor("https://api.payments.internal/v2/checkout");
        bool paid = processor.Charge("user_98124", 150.00m);

        var license = new LicenseValidator();
        bool valid = license.ValidateLicense("LIC-ABCD-9988-XYZ");

        Console.WriteLine($"All tests completed: Vault Unlocked: {vault.IsUnlocked}, Payment: {paid}, License: {valid}");
    }
}

public class SecretVault
{
    private readonly string _secretMasterKey;
    private bool _unlocked;

    public bool IsUnlocked => _unlocked;

    public SecretVault(string masterKey)
    {
        _secretMasterKey = masterKey;
        _unlocked = false;
    }

    public bool Unlock(string keyAttempt)
    {
        if (string.Equals(keyAttempt, _secretMasterKey, StringComparison.Ordinal))
        {
            _unlocked = true;
            Console.WriteLine("[Vault] Successfully unlocked with secret key.");
            return true;
        }

        Console.WriteLine("[Vault] Access denied. Invalid secret key provided.");
        return false;
    }
}

public class PaymentProcessor
{
    private readonly string _endpointUrl;
    private const string ApiSecretToken = "sk_live_920491823091283091283";

    public PaymentProcessor(string endpointUrl)
    {
        _endpointUrl = endpointUrl;
    }

    public bool Charge(string customerId, decimal amount)
    {
        Console.WriteLine($"[Payment] Submitting charge for customer '{customerId}', amount: ${amount:F2}");
        Console.WriteLine($"[Payment] Dispatching securely to endpoint: {_endpointUrl}");
        Console.WriteLine($"[Payment] Using Auth Token: {ApiSecretToken}");
        return true;
    }
}

public class LicenseValidator
{
    private const string ExpectedProductPrefix = "LIC-ABCD";

    public bool ValidateLicense(string licenseCode)
    {
        if (string.IsNullOrWhiteSpace(licenseCode))
        {
            return false;
        }

        bool isValid = licenseCode.StartsWith(ExpectedProductPrefix, StringComparison.OrdinalIgnoreCase);
        Console.WriteLine($"[License] License '{licenseCode}' validation result: {isValid}");
        return isValid;
    }
}
