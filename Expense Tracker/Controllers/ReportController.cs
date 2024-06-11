using Expense_Tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Expense_Tracker.Controllers
{
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Action to display the weekly report
        //public IActionResult WeeklyReport()
        //{
        //    var endDate = DateTime.Today;  // Check if this aligns with your server's date.
        //    var startDate = endDate.AddDays(-7);  // Ensure this date matches transactions in the database.

        //    Debug.WriteLine($"Date Range: {startDate} to {endDate}");

        //    var transactions = _context.Transaction
        //        .Where(t => t.Date >= startDate && t.Date <= endDate)
        //                               .Include(t => t.Category)
        //                               .ToList();

        //    Debug.WriteLine($"Transactions found: {transactions.Count}");  // Check if transactions are actually found.

        //    var reportData = transactions
        //        .GroupBy(t => t.Category.Title)
        //        .Select(group => new Report
        //        {
        //            Category = group.Key,
        //            Amount = group.Sum(t => t.Amount),
        //            Transaction = group.ToList()
        //        })
        //        .ToList();

        //    Debug.WriteLine($"Report data count: {reportData.Count}");  // This should tell us if the data grouping is working.

        //    return View(reportData);
        //}
        //public async Task<IActionResult> CategoryDetails(string category, DateTime startDate, DateTime endDate)
        //{
        //    Console.WriteLine($"Received: {category}, {startDate}, {endDate}"); // Log received data

        //    var transactions = await _context.Transaction
        //                                     .Where(t => t.Category.Title == category && t.Date >= startDate && t.Date <= endDate)
        //                                     .ToListAsync();

        //    Console.WriteLine($"Transactions found: {transactions.Count}"); // Log count of transactions

        //    return PartialView("_CategoryDetails", transactions);
        //}




        [NonAction]
        public void PopulateCategories()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }
        // GET: ReportController
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekAgo = today.AddDays(-7);

            var applicationDbContext = _context.Transaction
                                               .Include(t => t.Category)
                                               .Where(t => t.Date >= weekAgo && t.Date <= today);

            return View(await applicationDbContext.ToListAsync());
        }




        // GET: ReportController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ReportController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ReportController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ReportController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ReportController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ReportController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}