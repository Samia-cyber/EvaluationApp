using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Administrateur")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            // 🔍 SEARCH + RECENT TENTATIVES (ONE SOURCE OF TRUTH)
            var recentTentativesQuery = _db.Tentatives
                .Include(t => t.Candidat)
                .Include(t => t.Evaluation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                recentTentativesQuery = recentTentativesQuery.Where(t =>
                    t.Candidat.Nom.Contains(search) ||
                    t.Candidat.Prenom.Contains(search) ||
                    t.Evaluation.Titre.Contains(search)
                );
            }

            var recentTentatives = await recentTentativesQuery
                .OrderByDescending(t => t.DatePassage)
                .Take(10)
                .ToListAsync();

            // 👤 USER
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // 👥 CANDIDAT
            var candidat = await _db.Candidats
                .Include(c => c.Tentatives)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // 📅 START OF WEEK (MONDAY)
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-diff);

            // 📊 STATS
            var totalCandidats = await _db.Candidats.CountAsync();

            var totalEvaluationsTerminees = await _db.Tentatives
                .CountAsync(t => t.Score > 0);

            var evaluationsTermineesCetteSemaine = await _db.Tentatives
                .CountAsync(t => t.Score > 0 && t.DatePassage >= startOfWeek);

            var scoreMoyen = Math.Round(
                await _db.Tentatives
                    .Where(t => t.Score > 0)
                    .AverageAsync(t => (double?)t.Score) ?? 0,
                2
            );

            var inProgressCount = await _db.Tentatives
                .CountAsync(t => t.Score == 0);

            var tentativesNonTerminees = await _db.Tentatives
                .CountAsync(t => t.Score == 0);

            // ACTIVITÉS RÉCENTES
            var activitesTentatives = recentTentatives
                .Select(t => new
                {
                    Type = "evaluation",
                    ActionBy = t.Candidat != null ? $"{t.Candidat.Prenom} {t.Candidat.Nom}" : "Candidat",
                    ActionText = "a terminé l'évaluation de",
                    TargetName = t.Evaluation?.Titre ?? "Évaluation",
                    Date = t.DatePassage,
                    Badge = "évaluation"
                })
                .ToList();

            var activitesEntretiens = await _db.Entretiens
                .Include(e => e.Candidat)
                .OrderByDescending(e => e.DateCreation)
                .Take(5)
                .Select(e => new
                {
                    Type = "entretien",
                    ActionBy = "Équipe RH",
                    ActionText = "a planifié un entretien avec",
                    TargetName = e.Candidat != null ? $"{e.Candidat.Prenom} {e.Candidat.Nom}" : "Candidat",
                    Date = e.DateCreation,
                    Badge = "entretien"
                })
                .ToListAsync();

            var activitesRecentes = activitesTentatives
                .Concat(activitesEntretiens)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();

            ViewBag.ActivitesRecentes = activitesRecentes;

            // 🧠 VIEWMODEL (CLEAN & SINGLE)
            var model = new DashboardViewModel
            {
                UserName = candidat != null
                    ? $"{candidat.Prenom} {candidat.Nom}".Trim()
                    : user?.UserName ?? "Utilisateur",

                TotalCandidats = totalCandidats,
                TotalEvaluationsTerminees = totalEvaluationsTerminees,
                EvaluationsTermineesCetteSemaine = evaluationsTermineesCetteSemaine,
                ScoreMoyen = scoreMoyen,
                InProgressCount = inProgressCount,
                TentativesNonTerminees = tentativesNonTerminees,
                RecentTentatives = recentTentatives
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> StartSession(int evaluationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userId = user.Id;
            var email = user.Email ?? user.UserName;

            var candidat = await _db.Candidats.FirstOrDefaultAsync(c => c.UserId == userId)
                           ?? await _db.Candidats.FirstOrDefaultAsync(c => c.Email == email);

            if (candidat == null)
            {
                candidat = new Candidat
                {
                    Email = email,
                    Nom = user.UserName ?? "",
                    Prenom = "",
                    UserId = userId
                };
                _db.Candidats.Add(candidat);
            }
            else
            {
                candidat.UserId = userId;
            }

            await _db.SaveChangesAsync();

            var tentative = new Tentative
            {
                CandidatId = candidat.Id,
                EvaluationId = evaluationId,
                DatePassage = DateTime.UtcNow,
                Score = 0
            };

            _db.Tentatives.Add(tentative);
            await _db.SaveChangesAsync();

            return RedirectToAction("PasserEvaluation", "Evaluations", new { id = evaluationId });
        }

        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var candidat = await _db.Candidats
                .Include(c => c.Tentatives)
                    .ThenInclude(t => t.Evaluation)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var attempts = candidat != null
                ? candidat.Tentatives.OrderByDescending(t => t.DatePassage).ToList()
                : new List<Tentative>();

            return View(attempts);
        }
    }
}
