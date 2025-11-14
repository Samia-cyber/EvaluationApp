using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlateformeEvaluation.Models;

namespace PlateformeEvaluation.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets pour toutes vos tables
        public DbSet<Candidat> Candidats { get; set; }
        public DbSet<Candidature> Candidatures { get; set; }
        public DbSet<Tentative> Tentatives { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<OptionReponse> OptionsReponse { get; set; }
        public DbSet<ReponseCandidat> ReponsesCandidats { get; set; }
    }
}