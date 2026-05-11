using SentinelPulse.Models;

namespace SentinelPulse.Data;

public static class MockData
{
    public static List<OfficerModel> Officers { get; } = new()
    {
        new() { Id = 1, Name = "Inspector Ahmed Khan",  BadgeNumber = "SP-1042", Role = "Admin",   Email = "ahmed.khan@sentinelpulse.gov" },
        new() { Id = 2, Name = "Officer Sara Malik",    BadgeNumber = "SP-2188", Role = "Officer", Email = "sara.malik@sentinelpulse.gov" },
        new() { Id = 3, Name = "Officer Bilal Raza",    BadgeNumber = "SP-2231", Role = "Officer", Email = "bilal.raza@sentinelpulse.gov" },
        new() { Id = 4, Name = "Officer Hina Saeed",    BadgeNumber = "SP-2305", Role = "Officer", Email = "hina.saeed@sentinelpulse.gov" },
    };

    public static List<FIRModel> FIRs { get; } = new()
    {
        new() { CaseId = "CASE-001", CitizenName = "Mohammad Asif", CitizenCNIC = "35202-1234567-1", PhoneNumber = "+92-300-1234567", CrimeType = "Theft",       Description = "Mobile phone snatched near Liberty Market.", Location = "Liberty Market", District = "Lahore",     Status = "Investigation", DateFiled = DateTime.Now.AddDays(-2) },
        new() { CaseId = "CASE-002", CitizenName = "Ayesha Rahim",  CitizenCNIC = "42101-7654321-2", PhoneNumber = "+92-321-9876543", CrimeType = "Assault",     Description = "Physical altercation outside cafe.",         Location = "Clifton Block 4",District = "Karachi",    Status = "Open",          DateFiled = DateTime.Now.AddDays(-1) },
        new() { CaseId = "CASE-003", CitizenName = "Usman Tariq",   CitizenCNIC = "61101-1112223-3", PhoneNumber = "+92-333-5556677", CrimeType = "Burglary",    Description = "Home break-in, jewelry missing.",            Location = "F-7 Sector",     District = "Islamabad",  Status = "Open",          DateFiled = DateTime.Now.AddHours(-6) },
        new() { CaseId = "CASE-004", CitizenName = "Fatima Noor",   CitizenCNIC = "35202-4445556-4", PhoneNumber = "+92-345-1112233", CrimeType = "Cybercrime",  Description = "Online banking fraud, PKR 250,000 lost.",    Location = "DHA Phase 5",    District = "Lahore",     Status = "Investigation", DateFiled = DateTime.Now.AddDays(-3) },
        new() { CaseId = "CASE-005", CitizenName = "Imran Shah",    CitizenCNIC = "17301-9998887-5", PhoneNumber = "+92-301-7778899", CrimeType = "Vandalism",   Description = "Public property damaged near park.",         Location = "University Road",District = "Peshawar",   Status = "Closed",        DateFiled = DateTime.Now.AddDays(-7) },
    };

    public static List<CaseModel> Cases { get; } = new()
    {
        new() { CaseId = "CASE-001", Title = "Mobile snatching - Liberty",  AssignedOfficer = "Inspector Ahmed Khan", Status = "Investigation", Priority = "Medium", DateOpened = DateTime.Now.AddDays(-2),  LastUpdated = DateTime.Now.AddHours(-3) },
        new() { CaseId = "CASE-002", Title = "Assault at Clifton Cafe",     AssignedOfficer = "Officer Sara Malik",   Status = "Open",          Priority = "High",   DateOpened = DateTime.Now.AddDays(-1),  LastUpdated = DateTime.Now.AddHours(-1) },
        new() { CaseId = "CASE-003", Title = "F-7 Burglary Investigation",  AssignedOfficer = "Officer Bilal Raza",   Status = "Open",          Priority = "High",   DateOpened = DateTime.Now.AddHours(-6), LastUpdated = DateTime.Now.AddHours(-2) },
        new() { CaseId = "CASE-004", Title = "Banking Fraud - DHA",         AssignedOfficer = "Officer Hina Saeed",   Status = "Investigation", Priority = "High",   DateOpened = DateTime.Now.AddDays(-3),  LastUpdated = DateTime.Now.AddHours(-12) },
        new() { CaseId = "CASE-005", Title = "Vandalism - University Road", AssignedOfficer = "Officer Sara Malik",   Status = "Closed",        Priority = "Low",    DateOpened = DateTime.Now.AddDays(-7),  LastUpdated = DateTime.Now.AddDays(-1) },
        new() { CaseId = "CASE-006", Title = "Vehicle Theft - Saddar",      AssignedOfficer = "Officer Bilal Raza",   Status = "Investigation", Priority = "Medium", DateOpened = DateTime.Now.AddDays(-4),  LastUpdated = DateTime.Now.AddHours(-20) },
        new() { CaseId = "CASE-007", Title = "Domestic Disturbance",        AssignedOfficer = "Officer Hina Saeed",   Status = "Closed",        Priority = "Low",    DateOpened = DateTime.Now.AddDays(-10), LastUpdated = DateTime.Now.AddDays(-2) },
    };

    public static DashboardViewModel GetDashboard() => new()
    {
        ActiveCases = Cases.Count(c => c.Status != "Closed"),
        PendingFIRs = FIRs.Count(f => f.Status == "Open"),
        TotalOfficers = Officers.Count,
        ClosedCases = Cases.Count(c => c.Status == "Closed"),
        RecentFIRs = FIRs.OrderByDescending(f => f.DateFiled).Take(5).ToList(),
        CrimeDistribution = new()
        {
            { "Theft", 12 },
            { "Assault", 7 },
            { "Burglary", 9 },
            { "Cybercrime", 5 },
            { "Vandalism", 3 },
            { "Fraud", 6 },
        }
    };
}
