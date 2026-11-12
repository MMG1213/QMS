
using ClosedXML.Excel;

namespace Qms.Api.Services;

public class MasterListExcelPreviewService
{
    public object Preview(string filePath)
    {
        using var workbook = new XLWorkbook(filePath);

        var worksheet = workbook.Worksheet("ML 03");

        var departments = new List<object>();
        var documents = new List<object>();
        var errors = new List<string>();

        string? currentDepartment = null;
        string? currentDepartmentCode = null;

        for (int row = 13; row <= worksheet.LastRowUsed()!.RowNumber(); row++)
        {
            string description =
                worksheet.Cell(row, 2).GetString().Trim();

                string firstColumn =
    worksheet.Cell(row, 1).GetString().Trim();

            string documentNo =
                worksheet.Cell(row, 3).GetString().Trim();

            // Department heading rows contain a department name
            // but do not contain a document number.
           if (firstColumn.Contains(
        "Department",
        StringComparison.OrdinalIgnoreCase)
    && string.IsNullOrWhiteSpace(documentNo))
{
    currentDepartment = firstColumn;

    int openBracket = firstColumn.LastIndexOf('(');
    int closeBracket = firstColumn.LastIndexOf(')');

    currentDepartmentCode =
        openBracket >= 0 && closeBracket > openBracket
            ? firstColumn[(openBracket + 1)..closeBracket].Trim()
            : null;

    departments.Add(new
    {
        DepartmentName = currentDepartment,
        DepartmentCode = currentDepartmentCode
    });

    continue;
}

            // Ignore rows that do not represent documents.
            if (string.IsNullOrWhiteSpace(documentNo))
                continue;

            if (string.IsNullOrWhiteSpace(currentDepartmentCode))
            {
                errors.Add($"Row {row}: department could not be identified.");
                continue;
            }

            var revisions = new List<object>();

            // D/E, F/G, H/I ... P/Q
            // Revision numbers are 000, 001, 002 ... 006.
            for (int revision = 0; revision <= 6; revision++)
            {
                int pagesColumn = 4 + (revision * 2);
                int dateColumn = pagesColumn + 1;

                var pagesCell = worksheet.Cell(row, pagesColumn);
                var dateCell = worksheet.Cell(row, dateColumn);

                // Both cells empty means this revision is absent.
                if (pagesCell.IsEmpty() && dateCell.IsEmpty())
                    continue;

                if (!int.TryParse(pagesCell.GetString(), out int pages)
                    || pages <= 0)
                {
                    errors.Add(
                        $"Row {row}, {documentNo}, revision {revision:000}: " +
                        "page count is missing or invalid.");
                    continue;
                }

                DateTime revisionDate;

                if (dateCell.TryGetValue<DateTime>(out var parsedDate))
                {
                    revisionDate = parsedDate;
                }
                else if (!DateTime.TryParse(
                    dateCell.GetString(),
                    out revisionDate))
                {
                    errors.Add(
                        $"Row {row}, {documentNo}, revision {revision:000}: " +
                        $"invalid date '{dateCell.GetString()}'.");
                    continue;
                }

                revisions.Add(new
                {
                    RevisionNumber = revision,
                    PageCount = pages,
                    RevisionDate = revisionDate.ToString("yyyy-MM-dd")
                });
            }

            documents.Add(new
            {
                Row = row,
                DocumentNo = documentNo,
                Title = description,
                DepartmentName = currentDepartment,
                DepartmentCode = currentDepartmentCode,
                Revisions = revisions
            });
        }

        return new
        {
            Sheet = "ML 03",
            DocumentType = "Work Procedures",
            Departments = departments,
            Documents = documents,
            Errors = errors,
            DocumentCount = documents.Count,
            ErrorCount = errors.Count
        };
    }
}
