using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlateformeEvaluation.Data;
using PlateformeEvaluation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlateformeEvaluation.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email;

            // Try to find a matching Candidat by email
            Candidat? candidat = null;
            if (!string.IsNullOrEmpty(email))
            {
                candidat = await _db.Candidats.Include(c => c.Tentatives).ThenInclude(t => t.Evaluation).FirstOrDefaultAsync(c => c.Email == email);
            }

            // Available evaluations (include questions and options so the quiz can be rendered inline)
            var evaluations = await _db.Evaluations
                .Include(e => e.Questions)
                    .ThenInclude(q => q.OptionsReponse)
                .OrderBy(e => e.Id)
                .ToListAsync();

            // Recent attempts for this user (if any)
            var recent = candidat != null ? candidat.Tentatives.OrderByDescending(t => t.DatePassage).Take(10).ToList() : new List<Tentative>();

            var model = new DashboardViewModel
            {
                UserName = user?.UserName ?? email ?? "Utilisateur",
                AvailableEvaluations = evaluations,
                RecentTentatives = recent
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartSession(int evaluationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var email = user.Email ?? user.UserName;
            // Find or create candidate
            var candidat = await _db.Candidats.FirstOrDefaultAsync(c => c.Email == email);
            if (candidat == null)
            {
                candidat = new Candidat { Email = email, Nom = user.UserName ?? "", Prenom = "" };
                _db.Candidats.Add(candidat);
                await _db.SaveChangesAsync();
            }

            // Create tentative
            var tentative = new Tentative
            {
                CandidatId = candidat.Id,
                DatePassage = System.DateTime.UtcNow,
                EvaluationId = evaluationId,
                Score = 0
            };
            _db.Tentatives.Add(tentative);
            await _db.SaveChangesAsync();

            // For now redirect back to Dashboard (in a real app you would redirect to the quiz-taking page)
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var email = user.Email ?? user.UserName;
            var candidat = await _db.Candidats.Include(c => c.Tentatives).ThenInclude(t => t.Evaluation).FirstOrDefaultAsync(c => c.Email == email);
            var attempts = candidat != null ? candidat.Tentatives.OrderByDescending(t => t.DatePassage).ToList() : new List<Tentative>();
            return View(attempts);
        }
    }

    public class DashboardViewModel
    {
        public string UserName { get; set; } = "Utilisateur";
        public List<Evaluation> AvailableEvaluations { get; set; } = new List<Evaluation>();
        public List<Tentative> RecentTentatives { get; set; } = new List<Tentative>();
    }
}
