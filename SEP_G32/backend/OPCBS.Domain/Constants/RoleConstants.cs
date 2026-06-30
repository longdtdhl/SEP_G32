namespace OPCBS.Domain.Constants;

/// <summary>
/// Role name constants - use these instead of hardcoded strings
/// </summary>
public static class RoleConstants
{
    public const string Guest = "Guest";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string CustomerSupport = "CustomerSupport";
    public const string BusinessManager = "BusinessManager";
    public const string SystemAdmin = "SystemAdmin";

    public static readonly string[] AllRoles =
    {
        Guest, Patient, Doctor, CustomerSupport, BusinessManager, SystemAdmin
    };
}
