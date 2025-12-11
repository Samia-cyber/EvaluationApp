using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlateformeEvaluation.Data;
using PlateformeEvaluation.Models;

namespace PlateformeEvaluation.Controllers
{
    public class EvaluationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EvaluationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // YOUR FRIEND'S PART — CRUD
        // ============================================================

        // GET: Evaluations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Evaluations.ToListAsync());
        }

        // GET: Evaluations/Details/5  (your friend)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (evaluation == null)
                return NotFound();

            return View(evaluation);
        }

        // GET: Evaluations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Evaluations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Titre,Description,DureeMinutes,DateCreation")] Evaluation evaluation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(evaluation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(evaluation);
        }

        // GET: Evaluations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation == null)
                return NotFound();

            return View(evaluation);
        }

        // POST: Evaluations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Description,DureeMinutes,DateCreation")] Evaluation evaluation)
        {
            if (id != evaluation.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(evaluation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EvaluationExists(evaluation.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(evaluation);
        }

        // GET: Evaluations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var evaluation = await _context.Evaluations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (evaluation == null)
                return NotFound();

            return View(evaluation);
        }

        // POST: Evaluations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evaluation = await _context.Evaluations.FindAsync(id);
            if (evaluation != null)
            {
                _context.Evaluations.Remove(evaluation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EvaluationExists(int id)
        {
            return _context.Evaluations.Any(e => e.Id == id);
        }


        // ============================================================
        // YOUR PART — PASSER, SCORING, QUESTIONS, OPTIONS
        // ============================================================

        // GET: Evaluate with questions & options
        public async Task<IActionResult> Passer(int id)
        {
            var evaluation = await _context.Evaluations
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evaluation == null)
                return NotFound();

            var vm = new PasserEvaluationViewModel
            {
                EvaluationId = evaluation.Id,
                Titre = evaluation.Titre,
                Description = evaluation.Description,
                DureeMinutes = evaluation.DureeMinutes,

                Questions = evaluation.Questions.Select(q => new QuestionAvecReponse
                {
                    QuestionId = q.Id,
                    Texte = q.Texte,
                    Options = q.Options.ToList(),
                    SelectedOptionId = null
                }).ToList()
            };

            return View(vm);
        }

        // POST: Submit answers and calculate score
        [HttpPost]
        public IActionResult Submit(PasserEvaluationViewModel model)
        {
            if (model == null || model.Questions == null)
                return BadRequest("Aucune réponse envoyée.");

            int score = 0;

            foreach (var q in model.Questions)
            {
                var correctOption = _context.OptionsReponse
                    .Where(o => o.QuestionId == q.QuestionId && o.EstCorrecte)
                    .FirstOrDefault();

                if (correctOption != null && q.SelectedOptionId == correctOption.Id)
                    score++;
            }

            ViewBag.Score = score;
            ViewBag.Total = model.Questions.Count;

            return View("NextTest", model);
        }
    }
}





// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using PlateformeEvaluation.Data;
// using PlateformeEvaluation.Models;

// namespace PlateformeEvaluation.Controllers
// {
//     public class EvaluationsController : Controller
//     {
//         private readonly ApplicationDbContext _context;

//         public EvaluationsController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: Evaluations
//         public async Task<IActionResult> Index()
//         {
//             return View(await _context.Evaluations.ToListAsync());
//         }

//         // GET: Evaluations/Details/5
//         public async Task<IActionResult> Details(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var evaluation = await _context.Evaluations
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (evaluation == null)
//             {
//                 return NotFound();
//             }

//             return View(evaluation);
//         }

//         // GET: Evaluations/Create
//         public IActionResult Create()
//         {
//             return View();
//         }

//         // POST: Evaluations/Create
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create([Bind("Id,Titre,Description,DureeMinutes,DateCreation")] Evaluation evaluation)
//         {
//             if (ModelState.IsValid)
//             {
//                 _context.Add(evaluation);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(evaluation);
//         }

//         // GET: Evaluations/Edit/5
//         public async Task<IActionResult> Edit(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var evaluation = await _context.Evaluations.FindAsync(id);
//             if (evaluation == null)
//             {
//                 return NotFound();
//             }
//             return View(evaluation);
//         }

//         // POST: Evaluations/Edit/5
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(int id, [Bind("Id,Titre,Description,DureeMinutes,DateCreation")] Evaluation evaluation)
//         {
//             if (id != evaluation.Id)
//             {
//                 return NotFound();
//             }

//             if (ModelState.IsValid)
//             {
//                 try
//                 {
//                     _context.Update(evaluation);
//                     await _context.SaveChangesAsync();
//                 }
//                 catch (DbUpdateConcurrencyException)
//                 {
//                     if (!EvaluationExists(evaluation.Id))
//                     {
//                         return NotFound();
//                     }
//                     else
//                     {
//                         throw;
//                     }
//                 }
//                 return RedirectToAction(nameof(Index));
//             }
//             return View(evaluation);
//         }

//         // GET: Evaluations/Delete/5
//         public async Task<IActionResult> Delete(int? id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var evaluation = await _context.Evaluations
//                 .FirstOrDefaultAsync(m => m.Id == id);
//             if (evaluation == null)
//             {
//                 return NotFound();
//             }

//             return View(evaluation);
//         }

//         // POST: Evaluations/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(int id)
//         {
//             var evaluation = await _context.Evaluations.FindAsync(id);
//             if (evaluation != null)
//             {
//                 _context.Evaluations.Remove(evaluation);
//             }

//             await _context.SaveChangesAsync();
//             return RedirectToAction(nameof(Index));
//         }

//         private bool EvaluationExists(int id)
//         {
//             return _context.Evaluations.Any(e => e.Id == id);
//         }
//     }
// }
