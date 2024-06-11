using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Retrieve or initialize start and end dates
            DateTime startDate = TempData["StartDate"] != null ? (DateTime)TempData["StartDate"] : DateTime.Today.AddDays(-7);
            DateTime endDate = TempData["EndDate"] != null ? (DateTime)TempData["EndDate"] : DateTime.Today;

            // Ensure TempData values persist for subsequent requests during this session
            TempData.Keep("StartDate");
            TempData.Keep("EndDate");

            // Pass dates back to view for display or further user actions
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Fetch transactions within the given date range
            var selectedTransactions = await _context.Transaction
                .Include(x => x.Category)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToListAsync();

            // Calculate financial summaries
            int totalIncome = selectedTransactions
                .Where(t => t.Category.Type == "Income")
                .Sum(t => t.Amount);

            int totalExpense = selectedTransactions
                .Where(t => t.Category.Type == "Expense")
                .Sum(t => t.Amount);

            int balance = totalIncome - totalExpense;

            // Format the financial data for display
            ViewBag.TotalIncome = totalIncome.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
            ViewBag.TotalExpense = totalExpense.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
            ViewBag.Balance = balance.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));

            // Prepare data for the Expense By Category Doughnut Chart
            ViewBag.DoughnutChartData = selectedTransactions
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => t.Category.CategoryId)
                .Select(group => new
                {
                    categoryTitleWithIcon = group.First().Category.Icon + " " + group.First().Category.Title,
                    amount = group.Sum(t => t.Amount),
                    formattedAmount = group.Sum(t => t.Amount).ToString("C0", CultureInfo.CreateSpecificCulture("en-US"))
                })
                .OrderByDescending(result => result.amount)
                .ToList();

            // Prepare data for the Income vs Expense Spline Chart
            ViewBag.SplineChartData = PrepareSplineChartData(selectedTransactions, startDate, endDate);

            // Recent Transactions
            ViewBag.RecentTransactions = await _context.Transaction
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> FilterDates(DateTime? date_start, DateTime? date_end)
        {
            if (!date_start.HasValue || !date_end.HasValue)
            {
                TempData["Error"] = "Both start date and end date are required.";
                return RedirectToAction("Index");
            }

            if (date_start > date_end)
            {
                TempData["Error"] = "Start date cannot be greater than end date.";
                return RedirectToAction("Index");
            }

            if (date_start > DateTime.Now || date_end > DateTime.Now)
            {
                TempData["Error"] = "Dates cannot be in the future.";
                return RedirectToAction("Index");
            }

            TempData["StartDate"] = date_start.Value;
            TempData["EndDate"] = date_end.Value;

            return RedirectToAction("Index");
        }

        private IEnumerable<dynamic> PrepareSplineChartData(List<Transaction> transactions, DateTime startDate, DateTime endDate)
        {
            var dates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                  .Select(offset => startDate.AddDays(offset).ToString("dd-MMM"))
                                  .ToList();

            var incomeData = transactions
                .Where(t => t.Category.Type == "Income")
                .GroupBy(t => t.Date.ToString("dd-MMM"))
                .ToDictionary(group => group.Key, group => group.Sum(t => t.Amount));

            var expenseData = transactions
                .Where(t => t.Category.Type == "Expense")
                .GroupBy(t => t.Date.ToString("dd-MMM"))
                .ToDictionary(group => group.Key, group => group.Sum(t => t.Amount));

            return dates.Select(day => new
            {
                day,
                income = incomeData.ContainsKey(day) ? incomeData[day] : 0,
                expense = expenseData.ContainsKey(day) ? expenseData[day] : 0
            });
        }
    
}

public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;

    }
}
