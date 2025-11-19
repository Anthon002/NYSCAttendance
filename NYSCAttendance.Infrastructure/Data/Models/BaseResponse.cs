namespace NYSCAttendance.Infrastructure.Data.Models;

public sealed record BaseResponse(bool Status, string Message);
public sealed record BaseResponse<T>(bool Status, string Message, T? Value = default!);

public sealed record PaginatedResponse<T> where T : class
{

    public PaginatedResponse(T[] records, long totalRecordsCount, int page = 1, int pageSize = 10)
    {
        Records = records;
        CurrentPage = page;
        CurrentRecordCount = Records.Length;
        PageSize = pageSize;
        TotalRecordCount = totalRecordsCount;
        TotalPages = (int)Math.Ceiling((double)totalRecordsCount / pageSize);
    }

    public T[] Records { get; set; }
    public int CurrentPage { get; set; }
    public int CurrentRecordCount { get; set; }
    public int PageSize { get; set; }
    public long TotalRecordCount { get; set; }
    public int TotalPages { get; set; } 
}

public sealed record BrevoRequest(string HTMLContent, string Subject, string RecipientName, string RecipientEmail);
public sealed record MailRequest
{
    public string FirstName { get; set; } = default!;
    public string Email { get; set; } = default!;
};


public sealed record OTPResponse(string Code, string Identifier);