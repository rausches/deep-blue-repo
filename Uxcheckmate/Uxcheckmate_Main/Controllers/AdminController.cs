using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.ViewModels;
using Uxcheckmate_Main.Areas.Identity.Data;


namespace Uxcheckmate_Main.Controllers;

public class AdminController : Controller
{
    private readonly AuthDbContext _authContext;
    private readonly UxCheckmateDbContext _appContext;

    public AdminController(AuthDbContext authContext, UxCheckmateDbContext appContext)
    {
        _authContext = authContext;
        _appContext = appContext;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _authContext.Users.ToListAsync();
        var reports = await _appContext.Reports.ToListAsync();

        var model = users.Select(user => new AdminUserViewModel
        {
            User = user,
            Reports = reports.Where(r => r.UserID == user.Id).ToList()
        }).ToList();

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminFeedback()
    {
        var grouped = await _appContext.UserFeedbacks
            .GroupBy(f => f.UserID)
            .Select(g => new AdminFeedbackViewModel
            {
                UserId = g.Key,
                Feedbacks = g.ToList()
            }).ToListAsync();
        return View(grouped);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFeedback(int id)
    {
        var feedback = await _appContext.UserFeedbacks.FindAsync(id);
        if (feedback != null)
        {
            _appContext.UserFeedbacks.Remove(feedback);
            await _appContext.SaveChangesAsync();
        }
        return RedirectToAction("AdminFeedback");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAllFeedback(string userId)
    {
        var userFeedbacks = _appContext.UserFeedbacks.Where(f => f.UserID == userId);
        _appContext.UserFeedbacks.RemoveRange(userFeedbacks);
        await _appContext.SaveChangesAsync();
        return RedirectToAction("AdminFeedback");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var report = await _appContext.Reports.FindAsync(id);
        if (report != null)
        {
            _appContext.Reports.Remove(report);
            await _appContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _authContext.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Delete associated reports
        var userReports = _appContext.Reports.Where(r => r.UserID == id);
        _appContext.Reports.RemoveRange(userReports);

        // Delete associated feedback (if needed)
        var userFeedbacks = _appContext.UserFeedbacks.Where(f => f.UserID == id);
        _appContext.UserFeedbacks.RemoveRange(userFeedbacks);

        await _appContext.SaveChangesAsync();

        // Remove user from identity
        _authContext.Users.Remove(user);
        await _authContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
