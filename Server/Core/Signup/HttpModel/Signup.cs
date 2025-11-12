namespace Server.Signup;

public sealed class SignupResponse
{
    public string UserId { get; init; } = default!;
    public string Username { get; init; } = default!;
    public string Email { get; init; } = default!;
    public bool RequiresEmailVerification { get; init; }
    public VerificationInfo? Verification { get; init; }
    public string NextStep { get; init; } = "verify_email";

    public sealed class VerificationInfo
    {
        public string Method { get; init; } = "email";
        public string SentTo { get; init; } = default!;
        public DateTimeOffset ExpiresAt { get; init; }
    }
}

public sealed class VerifyEmailResult
{
    public bool Success { get; }
    public string? Error { get; }

    private VerifyEmailResult(bool success, string? error)
    {
        Success = success;
        Error = error;
    }

    public static VerifyEmailResult RSuccess() => new VerifyEmailResult(true, null);
    public static VerifyEmailResult RFailure(string error) => new VerifyEmailResult(false, error);
}

public sealed class AppUrls
{
    public string PublicBaseUrl { get; set; } = "";
}
