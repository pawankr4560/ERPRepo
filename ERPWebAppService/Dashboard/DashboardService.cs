using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using ERPWebAppData.Entity;
using ERPWebAppModels.Dashboard;

namespace WebApp.Service.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly WebAppDbContext _context;

        public DashboardService(WebAppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(string userId)
        {
            // 1. Total Income: Sum of Loan Payments (associated with User's Loans) + Sum of Booking Payments (associated with User's Bookings)
            // Wait, we query paid loan payments:
            var loanIncome = await _context.LoanPayment
                .Include(p => p.Loan)
                .Where(p => p.Loan != null
                    && p.Loan.UserId == userId
                    && (p.PaymentStatus == "Paid" || p.PaymentStatus == "Success"))
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            // Query paid booking payments:
            var bookingIncome = await _context.BookingPayments
                .Include(bp => bp.Booking)
                .Where(bp => bp.Booking.UserId == userId && (bp.Status == "Paid" || bp.Status == "Success"))
                .SumAsync(bp => (decimal?)bp.Amount) ?? 0;

            var totalIncome = loanIncome + bookingIncome;

            // 2. Pending EMI: Sum of EMIAmount for unpaid schedules for the user's loans
            var pendingEmi = await _context.LoanEMISchedule
                .Join(
                    _context.Loan,
                    schedule => schedule.LoanId,
                    loan => loan.Id,
                    (schedule, loan) => new { schedule, loan })
                .Where(x => x.loan.UserId == userId && !x.schedule.IsPaid)
                .SumAsync(x => (decimal?)x.schedule.EMIAmount) ?? 0;

            // 3. Bookings: Count of active bookings for the user
            var bookingsCount = await _context.Bookings
                .Where(b => b.UserId == userId && (b.Status == "Active" || b.Status == "Confirmed"))
                .CountAsync();

            // 4. Inventory Alerts: Count of low stock inventory items (global)
            var inventoryAlertsCount = await _context.InventoryItems
                .Where(i => i.CurrentStock <= i.LowStockThreshold)
                .CountAsync();

            return new DashboardSummaryDto
            {
                TotalIncome = totalIncome,
                PendingEmi = pendingEmi,
                Bookings = bookingsCount,
                InventoryAlerts = inventoryAlertsCount
            };
        }

        public async Task<List<RecentActivityDto>> GetRecentActivityAsync(string userId, int count = 10)
        {
            var logs = await _context.ActivityLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedOn)
                .Take(count)
                .ToListAsync();

            return logs.Select(l => new RecentActivityDto
            {
                Id = l.Id.ToString(),
                Title = l.Title,
                Subtitle = l.Subtitle,
                TimeAgo = GetTimeAgo(l.CreatedOn),
                IconName = l.IconName,
                HexColor = l.HexColor
            }).ToList();
        }

        public async Task LogActivityAsync(string userId, string title, string subtitle, string iconName, string hexColor)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Title = title,
                Subtitle = subtitle,
                IconName = iconName,
                HexColor = hexColor,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _context.ActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        private string GetTimeAgo(DateTime createdOn)
        {
            var diff = DateTime.UtcNow - createdOn;
            if (diff.TotalDays >= 365) return $"{(int)(diff.TotalDays / 365)}y ago";
            if (diff.TotalDays >= 30) return $"{(int)(diff.TotalDays / 30)}mo ago";
            if (diff.TotalDays >= 1) return $"{(int)diff.TotalDays}d ago";
            if (diff.TotalHours >= 1) return $"{(int)diff.TotalHours}h ago";
            if (diff.TotalMinutes >= 1) return $"{(int)diff.TotalMinutes}m ago";
            return "Just now";
        }
    }
}
