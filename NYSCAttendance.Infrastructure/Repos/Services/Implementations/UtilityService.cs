using Microsoft.EntityFrameworkCore;
using NPOI.XSSF.UserModel;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Repos.Services.Implementations;

public record class UtilityService : IUtilityService
{
    private readonly AppDbContext _context;
    public UtilityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BaseResponse<long>> CompleteOtpAsync(string Identifier, string code, CancellationToken cancellationToken)
    {
        var otp = await _context.OTPs.Where(x => x.Identifier == Identifier && x.Code == code).FirstOrDefaultAsync(cancellationToken);
        if (otp is null)
            return new BaseResponse<long>(false, "OTP is invalid. Please resend the OTP and try again.");

        otp.Status = OTPStatusEnum.Used;
        otp.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new BaseResponse<long>(true, "OTP confirmed successfully.", otp.UserId);
    }

    public byte[] ExportAttenanceData(IEnumerable<AttendanceResponse> source)
    {
        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("Sheet1");
        var rowHeader = sheet.CreateRow(0);

        var properties = typeof(AttendanceResponse).GetProperties();

        //header
        var font = workbook.CreateFont();
        font.IsBold = true;
        var style = workbook.CreateCellStyle();
        style.SetFont(font);

        var colIndex = 0;
        foreach (var property in properties)
        {
            var cell = rowHeader.CreateCell(colIndex);
            cell.SetCellValue(property.Name);
            cell.CellStyle = style;
            colIndex++;
        }
        //end header


        //content
        var rowNum = 1;
        foreach (var item in source)
        {
            var rowContent = sheet.CreateRow(rowNum);

            var colContentIndex = 0;
            foreach (var property in properties)
            {
                var cellContent = rowContent.CreateCell(colContentIndex);
                var value = property.GetValue(item, null);

                if (value == null)
                {
                    cellContent.SetCellValue("");
                }
                else if (property.PropertyType == typeof(string))
                {
                    cellContent.SetCellValue(value.ToString());
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                {
                    cellContent.SetCellValue(Convert.ToInt32(value));
                }
                else if (property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                {
                    cellContent.SetCellValue(Convert.ToInt64(value));
                }
                else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?))
                {
                    cellContent.SetCellValue(Convert.ToDouble(value));
                }
                else if (property.PropertyType == typeof(DateTimeOffset) || property.PropertyType == typeof(DateTimeOffset?))
                {
                    var dateValue = (DateTimeOffset)value;
                    cellContent.SetCellValue(dateValue.ToString("MMM dd, yyyy hh:mm tt"));
                }
                else cellContent.SetCellValue(value.ToString());

                colContentIndex++;
            }

            rowNum++;
        }
        //end content

        var stream = new MemoryStream();
        workbook.Write(stream);
        var content = stream.ToArray();

        return content;
    }

    public async Task<OTPResponse> GenerateOtpAsync(long userid, CancellationToken cancellationToken)
    {
        var code = "";
        var rand = new Random();
        code = rand.Next(100000, 999999).ToString();
        var identitifier = Guid.NewGuid().ToString().Replace("-", "");

        if (await _context.OTPs.AnyAsync(x => x.UserId == userid, cancellationToken))
            await _context.OTPs.Where(x => x.UserId == userid).ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.Code, code)
                    .SetProperty(c => c.Identifier, identitifier)
                    .SetProperty(c => c.Status, OTPStatusEnum.Active)
                    .SetProperty(c => c.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);
        else
        {
            var otp = new OTP
            {
                Code = code,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Status = OTPStatusEnum.Active,
                Identifier = identitifier,
                UserId = userid
            };
            await _context.OTPs.AddAsync(otp, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new OTPResponse(code, identitifier);
    }

    public string GeneratePassword(int length)
    {
        length -= 1;
        var number = new Random().Next(0, 9);

        var characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var password = "";
        for (var i = 0; i <= length; i++)
        {
            var random = new Random();
            var rand = random.Next(0, characters.Length - 1);
            password += characters[rand];
        }
        password = $"{password}{number}";
        return password;
    }
}
