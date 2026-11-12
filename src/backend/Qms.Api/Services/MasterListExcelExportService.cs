using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;

namespace Qms.Api.Services;

public class MasterListExcelExportService
{
    private readonly QmsDbContext _context;

    public MasterListExcelExportService(QmsDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateExcelAsync(int? documentTypeId, int? departmentId, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        var documentsTypesQuery = _context.DocumentTypes.AsNoTracking().AsQueryable();

       if (documentTypeId.HasValue)
{
    documentsTypesQuery = documentsTypesQuery
        .Where(x => x.DocumentTypeId == documentTypeId.Value);
}

        var documentType = await documentsTypesQuery.OrderBy(X => X.TypeCode).ToListAsync(cancellationToken);
        if (documentType.Count == 0) throw new InvalidOperationException("No documents types were found");

        foreach (var type in documentType)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sheetName = SafeSheetName(type.TypeCode);
            var worksheet = workbook.Worksheets.Add(sheetName);

            switch (type.TypeCode)
            {
                case "ML07":
                    await ExportMachinesAsync(worksheet, type.TypeName, type.DocumentTypeId, cancellationToken);
                    break;
                case "ML08":
                    await ExportStandardsAsync(worksheet, type.TypeName, type.DocumentTypeId, cancellationToken);
                    break;
                case "ML09":
                    await ExportInstrumentsAsync(
                        worksheet,
                        type.TypeName,
                        type.DocumentTypeId,
                        cancellationToken);
                    break;

                case "ML10":
                    await ExportQpiAsync(
                        worksheet,
                        type.TypeName,
                        type.DocumentTypeId,
                        cancellationToken);
                    break;
                
                case "ML01":
    await ExportML01Async(
        worksheet,
        type.TypeName,
        type.DocumentTypeId,
        cancellationToken);
    break;

                default:
                    await ExportControlledDocumentsAsync(worksheet, type.DocumentTypeId, type.TypeCode, departmentId, cancellationToken);
                    break;
            }
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ---------------------------------------------------------
    // ML02 - ML06: Controlled documents with revision history
    // ---------------------------------------------------------

   
private async Task ExportControlledDocumentsAsync(
    IXLWorksheet worksheet,
    int typeId,
    string typeName,
    int? departmentId,
    CancellationToken cancellationToken)
{
    var query = _context.Documents
        .AsNoTracking()
        .Where(d => d.DocumentTypeId == typeId);

    if (departmentId.HasValue)
    {
        query = query.Where(d => d.DepartmentId == departmentId.Value);
    }

    var documents = await query
        .Include(d => d.Revisions)
        .OrderBy(d => d.DepartmentId)
        .ThenBy(d => d.DocumentNo)
        .ToListAsync(cancellationToken);

    var departmentNames = await _context.QMSDepartments
        .AsNoTracking()
        .ToDictionaryAsync(
            d => d.DepartmentId,
            d => d.DepartmentName,
            cancellationToken);

    var maxRevision = documents
        .SelectMany(d => d.Revisions)
        .Select(r => r.RevisionNumber)
        .DefaultIfEmpty(0)
        .Max();

    // Headers: document columns, then Rev / Pages / Date for each revision,
    // followed by the document-level columns.
    var headers = new List<string>
    {
        "Document No",
        "Description"
    };

    for (var revisionNumber = 0; revisionNumber <= maxRevision; revisionNumber++)
    {
        
        headers.Add("Pages");
        headers.Add("Date");
    }

    headers.Add("Retention Period");
    headers.Add("Status");
    headers.Add("Obsolete Date");

    WriteTitle(worksheet, typeName, headers.Count);

    var headerRow = 3;
    WriteHeaders(worksheet, headerRow, headers);

    var currentRow = headerRow + 1;

    var groupedDocuments = documents
        .GroupBy(d => d.DepartmentId)
        .OrderBy(g => departmentNames.GetValueOrDefault(g.Key, ""))
        .ToList();

    foreach (var departmentGroup in groupedDocuments)
    {
        var departmentName = departmentNames.GetValueOrDefault(
            departmentGroup.Key,
            "Unknown Department");

        // Keep the department heading row.
        worksheet.Range(currentRow, 1, currentRow, headers.Count).Merge();
        worksheet.Cell(currentRow, 1).Value = departmentName;
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Left;

        currentRow++;

        foreach (var document in departmentGroup)
        {
            var column = 1;

            worksheet.Cell(currentRow, column++).Value = document.DocumentNo;
            worksheet.Cell(currentRow, column++).Value = document.Title;

            var revisionsByNumber = document.Revisions
                .ToDictionary(r => r.RevisionNumber);

            for (var revisionNumber = 0;
                 revisionNumber <= maxRevision;
                 revisionNumber++)
            {
                if (revisionsByNumber.TryGetValue(revisionNumber, out var revision))
                {
                    

                    worksheet.Cell(currentRow, column++).Value = revision.PageCount;

                    worksheet.Cell(currentRow, column).Value = revision.RevisionDate;
                    worksheet.Cell(currentRow, column).Style.DateFormat.Format = "dd-MM-yyyy";
                    column++;
                }
                else
                {
                    // Leave Rev, Pages and Date cells empty for missing revisions.
                    column += 2;
                }
            }

            worksheet.Cell(currentRow, column++).Value =
                document.RetentionPeriod ?? "";

            worksheet.Cell(currentRow, column++).Value =
                document.Status ?? "";

            if (document.ObseleteDate.HasValue)
            {
                worksheet.Cell(currentRow, column).Value =
                    document.ObseleteDate.Value.ToDateTime(TimeOnly.MinValue);

                worksheet.Cell(currentRow, column).Style.DateFormat.Format =
                    "dd-MM-yyyy";
            }

            currentRow++;
        }

        currentRow++;
    }

    if (documents.Count == 0)
    {
        worksheet.Cell(currentRow, 1).Value = "No records found.";
    }

    FormatDataRange(worksheet, headerRow, currentRow - 1, headers.Count);
}


    // ---------------------------------------------------------
    // ML07: Machines / Equipment
    // ---------------------------------------------------------

    private async Task ExportMachinesAsync(
        IXLWorksheet worksheet,
        string typeName,
        int typeId,
        CancellationToken cancellationToken)
    {
        var records = await _context.MachinesLists
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var headers = new[]
        {
            "Machine Name",
            "Machine Category",
            "Machine Code Number",
            "Machine Serial Number",
            "Make",
            "Capacity / Specification",
            "Commissioned On",
            "Load (HP)",
            "Location",
            "Document Type"
        };

        WriteTitle(worksheet, typeName, headers.Length);
        WriteHeaders(worksheet, 3, headers);

        var row = 4;

        foreach (var item in records)
        {
            worksheet.Cell(row, 1).Value = item.MachineName;
            worksheet.Cell(row, 2).Value = item.MachineCategory;
            worksheet.Cell(row, 3).Value = item.MachineCodeNum;
            worksheet.Cell(row, 4).Value = item.MachineSlNum;
            worksheet.Cell(row, 5).Value = item.Make;
            worksheet.Cell(row, 6).Value = item.CapacityOrSpecification;

            worksheet.Cell(row, 7).Value =
                item.CommissionedOn.ToDateTime(TimeOnly.MinValue);

            worksheet.Cell(row, 7).Style.DateFormat.Format = "dd-MM-yyyy";

            if (item.LoadInHP.HasValue)
                worksheet.Cell(row, 8).Value = (double)item.LoadInHP.Value;

            worksheet.Cell(row, 9).Value = item.Location;
            worksheet.Cell(row, 10).Value = typeName;

            row++;
        }

        FormatDataRange(worksheet, 3, row - 1, headers.Length);
    }

    // ---------------------------------------------------------
    // ML08: Standards
    // ---------------------------------------------------------

    private async Task ExportStandardsAsync(
        IXLWorksheet worksheet,
        string typeName,
        int typeId,
        CancellationToken cancellationToken)
    {
        var records = await _context.Standards
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var headers = new[]
        {
            "Description",
            "Pages",
            "Revision",
            "Remarks",
            "Document Type"
        };

        WriteTitle(worksheet, typeName, headers.Length);
        WriteHeaders(worksheet, 3, headers);

        var row = 4;

        foreach (var item in records)
        {
            worksheet.Cell(row, 1).Value = item.Description;

            if (item.Pages.HasValue)
                worksheet.Cell(row, 2).Value = item.Pages.Value;

            if (item.Revision.HasValue)
                worksheet.Cell(row, 3).Value = item.Revision.Value;

            worksheet.Cell(row, 4).Value = item.Remarks;
            worksheet.Cell(row, 5).Value = typeName;

            row++;
        }

        FormatDataRange(worksheet, 3, row - 1, headers.Length);
    }

    // ---------------------------------------------------------
    // ML09: Testing and Measuring Instruments
    // ---------------------------------------------------------

    private async Task ExportInstrumentsAsync(
        IXLWorksheet worksheet,
        string typeName,
        int typeId,
        CancellationToken cancellationToken)
    {
        var records = await _context.TestingOrMeasuringInstruments
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var headers = new[]
        {
            "Instrument Description",
            "Instrument Code Number",
            "Instrument Serial Number",
            "Instrument Range",
            "Make",
            "Location",
            "Calibration Frequency",
            "Remarks",
            "Document Type"
        };

        WriteTitle(worksheet, typeName, headers.Length);
        WriteHeaders(worksheet, 3, headers);

        var row = 4;

        foreach (var item in records)
        {
            worksheet.Cell(row, 1).Value = item.InstrumentDescription;
            worksheet.Cell(row, 2).Value = item.InstrumentCodeNum;
            worksheet.Cell(row, 3).Value = item.InstrumentSlNum;
            worksheet.Cell(row, 4).Value = item.InstrumentRange;
            worksheet.Cell(row, 5).Value = item.Make;
            worksheet.Cell(row, 6).Value = item.Location;
            worksheet.Cell(row, 7).Value = item.CalibrationFrequency;
            worksheet.Cell(row, 8).Value = item.Remarks;
            worksheet.Cell(row, 9).Value = typeName;

            row++;
        }

        FormatDataRange(worksheet, 3, row - 1, headers.Length);
    }

    // ---------------------------------------------------------
    // ML10: Quality Performance Index
    // ---------------------------------------------------------

    private async Task ExportQpiAsync(
        IXLWorksheet worksheet,
        string typeName,
        int typeId,
        CancellationToken cancellationToken)
    {
        var records = await _context.QualityPerformanceIndex
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var headers = new[]
        {
            "Process / Department",
            "Measurement Title",
            "QPI Formula",
            "Targets",
            "Responsibilities",
            "Department Review",
            "QPI Review",
            "Document Type"
        };

        WriteTitle(worksheet, typeName, headers.Length);
        WriteHeaders(worksheet, 3, headers);

        var row = 4;

        foreach (var item in records)
        {
            worksheet.Cell(row, 1).Value = item.ProcessOrDept;
            worksheet.Cell(row, 2).Value = item.MeasurementTitle;
            worksheet.Cell(row, 3).Value = item.QualityPerformanceIndexFormula;
            worksheet.Cell(row, 4).Value = item.Targets;
            worksheet.Cell(row, 5).Value = item.Responsibilities;
            worksheet.Cell(row, 6).Value = item.DepartmentReview;
            worksheet.Cell(row, 7).Value = item.QPIReview;
            worksheet.Cell(row, 8).Value = typeName;

            row++;
        }

        FormatDataRange(worksheet, 3, row - 1, headers.Length);
    }
    // ---------------------------------------------------------
// ML01: Master List of Master Lists
// ---------------------------------------------------------

private async Task ExportML01Async(
    IXLWorksheet worksheet,
    string typeName,
    int typeId,
    CancellationToken cancellationToken)
{
    var entries = await _context.ML01Entries
        .AsNoTracking()
        .Where(e => e.DocumentTypeId == typeId)
        .Include(e => e.Revisions)
        .OrderBy(e => e.Id)
        .ToListAsync(cancellationToken);

    // Find the highest revision number across all entries.
    var maxRevision = entries
        .SelectMany(e => e.Revisions)
        .Select(r => r.RevisionNo)
        .DefaultIfEmpty(0)
        .Max();

    // Build dynamic Excel headers.
    var headers = new List<string>
    {
        "Sl. No.",
        "Description",
        "Document No"
    };

    for (var revisionNo = 0; revisionNo <= maxRevision; revisionNo++)
    {
        headers.Add($"Pages {revisionNo:00}");
        headers.Add($"Date {revisionNo:00}");
    }

    WriteTitle(worksheet, typeName, headers.Count);

    const int headerRow = 3;

    WriteHeaders(worksheet, headerRow, headers);

    var currentRow = headerRow + 1;
    var serialNumber = 1;

    foreach (var entry in entries)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var column = 1;

        // Basic document details.
        worksheet.Cell(currentRow, column++).Value = serialNumber++;
        worksheet.Cell(currentRow, column++).Value = entry.Description;
        worksheet.Cell(currentRow, column++).Value = entry.DocumentNo;

        // Revision history.
        for (var revisionNo = 0;
             revisionNo <= maxRevision;
             revisionNo++)
        {
            var revision = entry.Revisions
                .FirstOrDefault(r => r.RevisionNo == revisionNo);

            if (revision != null)
            {
                worksheet.Cell(currentRow, column++).Value =
                    revision.PageCount;

                worksheet.Cell(currentRow, column).Value =
                    revision.RevisionDate.ToDateTime(TimeOnly.MinValue);

                worksheet.Cell(currentRow, column).Style.DateFormat.Format =
                    "dd/MM/yyyy";

                column++;
            }
            else
            {
                // No revision available: leave both cells empty.
                column += 2;
            }
        }

        currentRow++;
    }

    if (entries.Count == 0)
    {
        worksheet.Cell(currentRow, 1).Value = "No records found.";
    }

    FormatDataRange(
        worksheet,
        headerRow,
        currentRow - 1,
        headers.Count);
}

    // ---------------------------------------------------------
    // Excel formatting helpers
    // ---------------------------------------------------------

    private static void WriteTitle(
        IXLWorksheet worksheet,
        string title,
        int columnCount)
    {
        worksheet.Range(1, 1, 1, columnCount).Merge();

        var cell = worksheet.Cell(1, 1);
        cell.Value = title;
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 16;
        cell.Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical =
            XLAlignmentVerticalValues.Center;
        cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
        cell.Style.Font.FontColor = XLColor.White;

        worksheet.Row(1).Height = 30;
    }

    private static void WriteHeaders(
        IXLWorksheet worksheet,
        int row,
        IEnumerable<string> headers)
    {
        var column = 1;

        foreach (var header in headers)
        {
            var cell = worksheet.Cell(row, column++);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        worksheet.Row(row).Height = 32;
    }

    private static void FormatDataRange(
        IXLWorksheet worksheet,
        int headerRow,
        int lastRow,
        int columnCount)
    {
        if (lastRow > headerRow)
        {
            var range = worksheet.Range(
                headerRow,
                1,
                lastRow,
                columnCount);

            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;
        }

        worksheet.SheetView.FreezeRows(3);

        worksheet.Columns(1, columnCount).AdjustToContents();

        for (var column = 1; column <= columnCount; column++)
        {
            if (worksheet.Column(column).Width > 35)
                worksheet.Column(column).Width = 35;
        }
    }

    private static string SafeSheetName(string name)
    {
        var invalidCharacters = new[] { ':', '\\', '/', '?', '*', '[', ']' };

        foreach (var character in invalidCharacters)
            name = name.Replace(character, '-');

        name = name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            name = "MasterList";

        return name.Length > 31 ? name[..31] : name;
    }
}