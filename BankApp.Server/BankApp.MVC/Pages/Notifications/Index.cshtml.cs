using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using BankApp.Models.Enums;
using BankApp.Models.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BankApp.MVC.Pages.Notifications;

public class IndexModel : PageModel
{
    private const int PageSize = 8;
    private static readonly object SyncRoot = new();
    private static readonly List<NotificationItem> Store = SeedNotifications();

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<NotificationItem> Notifications { get; private set; } = [];

    public int TotalCount { get; private set; }

    public int UnreadCount { get; private set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public IEnumerable<SelectListItem> NotificationTypeOptions { get; } = Enum.GetValues<NotificationType>()
        .Select(type => new SelectListItem(type.ToDisplayName(), type.ToString()));

    [BindProperty]
    public NotificationDraftInput Draft { get; set; } = new();

    public void OnGet()
    {
        LoadPage();
    }

    public IActionResult OnGetList(string? filter = null, int pageNumber = 1, int pageSize = PageSize)
    {
        NotificationPageResult result = BuildPageResult(filter ?? "all", pageNumber, pageSize);
        return new JsonResult(result);
    }

    public IActionResult OnGetUnreadCount()
    {
        lock (SyncRoot)
        {
            return new JsonResult(new { unreadCount = Store.Count(notification => !notification.IsRead) });
        }
    }

    public IActionResult OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            LoadPage();
            return RespondAjax(new JsonResult(new { success = false, errors = ModelState }), redirectFilter: Filter, redirectPage: PageNumber);
        }

        string recipientList = string.Join(", ", Draft.RecipientUserIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        NotificationItem created;
        lock (SyncRoot)
        {
            int nextId = Store.Count == 0 ? 1 : Store.Max(notification => notification.Id) + 1;
            created = new NotificationItem
            {
                Id = nextId,
                UserId = 0,
                RecipientUserIds = recipientList,
                Title = Draft.Title.Trim(),
                Message = Draft.Message.Trim(),
                Type = Draft.Type,
                ActionUrl = string.IsNullOrWhiteSpace(Draft.ActionUrl) ? null : Draft.ActionUrl.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Source = "Admin"
            };
            Store.Insert(0, created);
        }

        TempData["StatusMessage"] = "Notification created successfully.";
        return RespondAjax(new JsonResult(created.ToDto()), redirectFilter: Filter, redirectPage: 1);
    }

    public IActionResult OnPostMarkRead(int id)
    {
        NotificationItem? updated = UpdateNotification(id, notification => notification.IsRead = true);
        TempData["StatusMessage"] = "Notification marked as read.";
        return RespondAjax(updated == null ? new NotFoundResult() : new JsonResult(updated.ToDto()), redirectFilter: Filter, redirectPage: PageNumber);
    }

    public IActionResult OnPostMarkAllRead()
    {
        lock (SyncRoot)
        {
            foreach (NotificationItem notification in Store)
            {
                notification.IsRead = true;
            }
        }

        TempData["StatusMessage"] = "All notifications were marked as read.";
        return RespondAjax(new JsonResult(new { success = true }), redirectFilter: Filter, redirectPage: PageNumber);
    }

    public IActionResult OnPostDelete(int id)
    {
        bool removed = UpdateNotification(id, notification => Store.Remove(notification)) != null;
        TempData["StatusMessage"] = "Notification removed.";
        return RespondAjax(removed ? new JsonResult(new { success = true, id }) : new NotFoundResult(), redirectFilter: Filter, redirectPage: PageNumber);
    }

    public IActionResult OnPostClearAll()
    {
        lock (SyncRoot)
        {
            Store.Clear();
        }

        TempData["StatusMessage"] = "All notifications were cleared.";
        return RespondAjax(new JsonResult(new { success = true }), redirectFilter: "all", redirectPage: 1);
    }

    public string GetNotificationJson(NotificationItem item)
    {
        return JsonSerializer.Serialize(item.ToDto());
    }

    private void LoadPage()
    {
        NotificationPageResult result = BuildPageResult(Filter, PageNumber, PageSize);
        Notifications = result.Items.Select(NotificationItem.ToNotificationItem).ToList();
        TotalCount = result.TotalCount;
        UnreadCount = result.UnreadCount;
        PageNumber = result.PageNumber;
        Filter = result.Filter;
    }

    private NotificationPageResult BuildPageResult(string filter, int pageNumber, int pageSize)
    {
        List<NotificationItem> ordered;

        lock (SyncRoot)
        {
            ordered = Store
                .OrderByDescending(notification => notification.CreatedAt)
                .ToList();
        }

        int unreadCount = ordered.Count(notification => !notification.IsRead);

        if (filter.Equals("unread", StringComparison.OrdinalIgnoreCase))
        {
            ordered = ordered.Where(notification => !notification.IsRead).ToList();
        }

        int totalCount = ordered.Count;
        int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }
        else if (pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        List<NotificationItem> pageItems = ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new NotificationPageResult
        {
            Filter = filter,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            TotalPages = totalPages,
            Items = pageItems.Select(item => item.ToDto()).ToList()
        };
    }

    private static NotificationItem? UpdateNotification(int id, Action<NotificationItem> updateAction)
    {
        lock (SyncRoot)
        {
            NotificationItem? notification = Store.FirstOrDefault(item => item.Id == id);
            if (notification != null)
            {
                updateAction(notification);
            }

            return notification;
        }
    }

    private IActionResult RespondAjax(IActionResult fallbackResult, string? redirectFilter = null, int? redirectPage = null)
    {
        if (IsAjaxRequest())
        {
            return fallbackResult;
        }

        return RedirectToPage(new
        {
            filter = redirectFilter ?? Filter,
            pageNumber = redirectPage ?? PageNumber
        });
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase) ||
               Request.Headers.Accept.Any(value => value.Contains("application/json", StringComparison.OrdinalIgnoreCase));
    }

    private static List<NotificationItem> SeedNotifications()
    {
        return
        [
            new NotificationItem
            {
                Id = 1,
                UserId = 1001,
                RecipientUserIds = "1001",
                Title = "Large transfer approved",
                Message = "Your transfer of $2,450.00 was approved and sent successfully. The recipient should receive the funds shortly.",
                Type = NotificationType.OutboundTransfer,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-12),
                ActionUrl = "/Transactions/Details/2450",
                Source = "System"
            },
            new NotificationItem
            {
                Id = 2,
                UserId = 1001,
                RecipientUserIds = "1001",
                Title = "Security alert",
                Message = "We detected a login from a new device. Review your recent activity and change your password if this wasn't you.",
                Type = NotificationType.SuspiciousActivity,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                ActionUrl = "/Profile/Security",
                Source = "System"
            },
            new NotificationItem
            {
                Id = 3,
                UserId = 1001,
                RecipientUserIds = "1001",
                Title = "Low balance reminder",
                Message = "Your checking account balance is below the configured threshold. Consider transferring funds to avoid overdraft fees.",
                Type = NotificationType.LowBalance,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                ActionUrl = "/Accounts",
                Source = "System"
            },
            new NotificationItem
            {
                Id = 4,
                UserId = 1001,
                RecipientUserIds = "1001, 1008",
                Title = "Upcoming payment due",
                Message = "A scheduled payment is due tomorrow. Please confirm that the destination account and amount are still correct.",
                Type = NotificationType.DuePayment,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ActionUrl = "/Payments/Scheduled",
                Source = "System"
            },
            new NotificationItem
            {
                Id = 5,
                UserId = 1008,
                RecipientUserIds = "1008",
                Title = "Direct deposit received",
                Message = "Your paycheck was deposited successfully. The transaction is now available in your account history.",
                Type = NotificationType.InboundTransfer,
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ActionUrl = "/Transactions",
                Source = "System"
            },
            new NotificationItem
            {
                Id = 6,
                UserId = 0,
                RecipientUserIds = "1001, 1008, 1022",
                Title = "Spring cashback promotion",
                Message = "Earn 2% cashback on eligible debit card purchases through the end of the month. Tap to review offer details.",
                Type = NotificationType.Payment,
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ActionUrl = "/Offers",
                Source = "Admin"
            }
        ];
    }

    public sealed class NotificationDraftInput
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(4000)]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; } = NotificationType.SuspiciousActivity;

        [Required, StringLength(200)]
        [Display(Name = "Target user IDs")]
        public string RecipientUserIds { get; set; } = string.Empty;

        [Url]
        [StringLength(300)]
        public string? ActionUrl { get; set; }
    }

    public sealed class NotificationItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RecipientUserIds { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ActionUrl { get; set; }
        public string Source { get; set; } = "System";

        public string DisplayType => Type.ToDisplayName();
        public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("f");
        public string Snippet => Message.Length <= 160 ? Message : Message[..157] + "...";
        public string ReadClass => IsRead ? "notification-read" : "notification-unread";
        public string BadgeClass => IsRead ? "bg-secondary" : "bg-primary";

        public NotificationDto ToDto() => new()
        {
            Id = Id,
            UserId = UserId,
            RecipientUserIds = RecipientUserIds,
            Title = Title,
            Message = Message,
            Type = DisplayType,
            IsRead = IsRead,
            CreatedAt = CreatedAtDisplay,
            ActionUrl = ActionUrl,
            Source = Source
        };

        public static NotificationItem ToNotificationItem(NotificationDto dto) => new()
        {
            Id = dto.Id,
            UserId = dto.UserId,
            RecipientUserIds = dto.RecipientUserIds,
            Title = dto.Title,
            Message = dto.Message,
            Type = Enum.TryParse<NotificationType>(dto.Type, true, out NotificationType type) ? type : NotificationType.SuspiciousActivity,
            IsRead = dto.IsRead,
            CreatedAt = DateTime.TryParse(dto.CreatedAt, out DateTime createdAt) ? createdAt : DateTime.UtcNow,
            ActionUrl = dto.ActionUrl,
            Source = dto.Source ?? "System"
        };
    }

    public sealed class NotificationPageResult
    {
        public string Filter { get; set; } = "all";
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
        public int TotalPages { get; set; }
        public List<NotificationDto> Items { get; set; } = [];
    }

    public sealed class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RecipientUserIds { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
