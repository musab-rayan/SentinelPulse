using Microsoft.EntityFrameworkCore;
using SentinelPulse.Models;

namespace SentinelPulse
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<OfficerModel> Officers { get; set; }
        public DbSet<FIRModel> FIRs { get; set; }
        public DbSet<CaseModel> Cases { get; set; }
        public DbSet<AlertModel> Alerts { get; set; }
        public DbSet<EvidenceModel> Evidence { get; set; }
        public DbSet<SuspectModel> Suspects { get; set; }
        public DbSet<MissingChildModel> MissingChildren { get; set; }
    }
}
