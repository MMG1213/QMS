
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.Models;
using System.Globalization;

namespace Qms.Api.Services;

public sealed class MasterListImportService : IMasterListImportService
{
    private readonly QmsDbContext _context;

    public MasterListImportService(QmsDbContext context)
    {
        _context = context;
    }

    public async Task<MasterListImportResult> ImportAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        // Validate the uploaded file.
        if (file == null || file.Length == 0)
        {
            return MasterListImportResult.InvalidInput(new
            {
                message = "Please select an Excel workbook."
            });
        }

        if (!string.Equals(
                Path.GetExtension(file.FileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            return MasterListImportResult.InvalidInput(new
            {
                message = "Only .xlsx files are supported."
            });
        }

        try
        {
            // Prevent accidental duplicate imports.
            if (await _context.MachinesLists.AnyAsync(cancellationToken) ||
                await _context.Standards.AnyAsync(cancellationToken) ||
                await _context.TestingOrMeasuringInstruments
                    .AnyAsync(cancellationToken) ||
                await _context.QualityPerformanceIndex
                    .AnyAsync(cancellationToken))
            {
                return MasterListImportResult.Conflict(new
                {
                    message =
                        "At least one master-list table already contains records. " +
                        "Import cancelled to prevent duplicate data."
                });
            }

            using var workbook = new XLWorkbook(file.OpenReadStream());

            // Read the same four worksheets as before.
            var machinesSheet = workbook.Worksheet("ML 07");
            var standardsSheet = workbook.Worksheet("ML 08");
            var instrumentsSheet = workbook.Worksheet("ML 09");
            var qpiSheet = workbook.Worksheet("ML_10");

            // Resolve IDs using the configured document type codes.
            var typeCodes = new[] { "ML07", "ML08", "ML09", "ML10" };

            var documentTypes = await _context.DocumentTypes
                .Where(d => typeCodes.Contains(d.TypeCode))
                .ToDictionaryAsync(
                    d => d.TypeCode,
                    d => d.DocumentTypeId,
                    cancellationToken);

            foreach (string code in typeCodes)
            {
                if (!documentTypes.ContainsKey(code))
                {
                    return MasterListImportResult.InvalidInput(new
                    {
                        message = $"Document type '{code}' was not found."
                    });
                }
            }

            // Convert the worksheets into database entities.
            var machines = ReadMachines(
                machinesSheet, documentTypes["ML07"]);

            var standards = ReadStandards(
                standardsSheet, documentTypes["ML08"]);

            var instruments = ReadInstruments(
                instrumentsSheet, documentTypes["ML09"]);

            var qpi = ReadQpi(
                qpiSheet, documentTypes["ML10"]);

            // Do not save a partial or incomplete import.
            if (machines.Count == 0 ||
                standards.Count == 0 ||
                instruments.Count == 0 ||
                qpi.Count == 0)
            {
                return MasterListImportResult.InvalidInput(new
                {
                    message =
                        "At least one worksheet produced no records. Nothing was saved.",
                    machines = machines.Count,
                    standards = standards.Count,
                    instruments = instruments.Count,
                    qualityPerformanceIndex = qpi.Count
                });
            }

            // Save all four master lists in a single transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            _context.MachinesLists.AddRange(machines);
            _context.Standards.AddRange(standards);
            _context.TestingOrMeasuringInstruments.AddRange(instruments);
            _context.QualityPerformanceIndex.AddRange(qpi);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MasterListImportResult.Success(new
            {
                message = "All four master lists were imported successfully.",
                machinesImported = machines.Count,
                standardsImported = standards.Count,
                instrumentsImported = instruments.Count,
                qualityPerformanceIndexImported = qpi.Count
            });
        }
        catch (InvalidOperationException ex)
        {
            return MasterListImportResult.InvalidInput(new
            {
                message = "A required worksheet is missing or invalid.",
                details = ex.Message
            });
        }
        catch (FormatException ex)
        {
            return MasterListImportResult.InvalidInput(new
            {
                message = "An Excel value could not be converted.",
                details = ex.Message
            });
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("===== QMS EXCEL IMPORT DATABASE ERROR =====");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("===== END DATABASE ERROR =====");

            return MasterListImportResult.Conflict(new
            {
                message = "SQL Server rejected the import.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
        catch (Exception ex) when (
            ex is not OperationCanceledException)
        {
            return MasterListImportResult.InvalidInput(new
            {
                message = "The workbook could not be imported.",
                details = ex.Message
            });
        }
    }

    // ML 07: Machines / Equipment. Data starts at row 11.
    private static List<MachinesList> ReadMachines(
        IXLWorksheet sheet,
        int documentTypeId)
    {
        var result = new List<MachinesList>();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 11; row <= lastRow; row++)
        {
            string name = CellText(sheet.Cell(row, 2));

            if (string.IsNullOrWhiteSpace(name))
                continue;

            string category = FirstNonEmpty(
                CellText(sheet.Cell(row, 3)),
                CellText(sheet.Cell(row, 4)),
                CellText(sheet.Cell(row, 5)));

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new FormatException(
                    $"ML 07 row {row}: machine category is missing.");
            }

            var commissioned = ReadDate(sheet.Cell(row, 10));

            if (commissioned == null)
            {
                throw new FormatException(
                    $"ML 07 row {row}: commissioning date is missing or invalid.");
            }

            result.Add(new MachinesList
            {
                MachineName = name,
                MachineCategory = category,
                MachineCodeNum = OptionalText(sheet.Cell(row, 6)),
                MachineSlNum = OptionalText(sheet.Cell(row, 7)),
                Make = OptionalText(sheet.Cell(row, 8)),
                CapacityOrSpecification = CellText(sheet.Cell(row, 9)),
                CommissionedOn = commissioned.Value,
                LoadInHP = ReadDecimal(sheet.Cell(row, 11)),
                Location = CellText(sheet.Cell(row, 12)),
                DocumentTypeId = documentTypeId
            });
        }

        return result;
    }

    // ML 08: Standards. Data starts at row 13.
    private static List<Standards> ReadStandards(
        IXLWorksheet sheet,
        int documentTypeId)
    {
        var result = new List<Standards>();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 13; row <= lastRow; row++)
        {
            string description = CellText(sheet.Cell(row, 2));

            if (string.IsNullOrWhiteSpace(description))
                continue;

            result.Add(new Standards
            {
                Description = description,
                Pages = ReadNullableInt(sheet.Cell(row, 3)),
                Revision = ReadNullableInt(sheet.Cell(row, 4)),
                Remarks = OptionalText(sheet.Cell(row, 5)),
                DocumentTypeId = documentTypeId
            });
        }

        return result;
    }

    // ML 09: Testing and Measuring Instruments. Data starts at row 9.
    private static List<TestingOrMeasuringInstruments> ReadInstruments(
        IXLWorksheet sheet,
        int documentTypeId)
    {
        var result = new List<TestingOrMeasuringInstruments>();
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 9; row <= lastRow; row++)
        {
            string description = CellText(sheet.Cell(row, 2));

            if (string.IsNullOrWhiteSpace(description))
                continue;

            result.Add(new TestingOrMeasuringInstruments
            {
                InstrumentDescription = description,
                InstrumentCodeNum = CellText(sheet.Cell(row, 3)),
                InstrumentSlNum = CellText(sheet.Cell(row, 4)),
                InstrumentRange = CellText(sheet.Cell(row, 5)),
                Make = CellText(sheet.Cell(row, 6)),
                Location = CellText(sheet.Cell(row, 8)),
                CalibrationFrequency = CellText(sheet.Cell(row, 9)),
                Remarks = CellText(sheet.Cell(row, 20)),
                DocumentTypeId = documentTypeId
            });
        }

        return result;
    }

    // ML_10: Quality Performance Index. Data starts at row 15.
    private static List<QualityPerformanceIndex> ReadQpi(
        IXLWorksheet sheet,
        int documentTypeId)
    {
        var result = new List<QualityPerformanceIndex>();
        string currentDepartment = string.Empty;
        int lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;

        for (int row = 15; row <= lastRow; row++)
        {
            if (IsRowEmpty(sheet, row, 1, 7))
                continue;

            string department = CellText(sheet.Cell(row, 1));

            // Carry department names forward for continuation rows.
            if (!string.IsNullOrWhiteSpace(department))
                currentDepartment = department;

            string title = CellText(sheet.Cell(row, 2));

            if (string.IsNullOrWhiteSpace(title))
                continue;

            if (string.IsNullOrWhiteSpace(currentDepartment))
            {
                throw new FormatException(
                    $"ML_10 row {row}: department is missing.");
            }

            result.Add(new QualityPerformanceIndex
            {
                ProcessOrDept = currentDepartment,
                MeasurementTitle = title,
                QualityPerformanceIndexFormula = CellText(sheet.Cell(row, 3)),
                Targets = CellText(sheet.Cell(row, 4)),
                Responsibilities = CellText(sheet.Cell(row, 5)),
                DepartmentReview = CellText(sheet.Cell(row, 6)),
                QPIReview = CellText(sheet.Cell(row, 7)),
                DocumentTypeId = documentTypeId
            });
        }

        return result;
    }

    // Shared Excel cell conversion helpers.

    private static string CellText(IXLCell cell)
    {
        return cell.IsEmpty()
            ? string.Empty
            : cell.GetFormattedString().Trim();
    }

    private static string? OptionalText(IXLCell cell)
    {
        string value = CellText(cell);

        if (string.IsNullOrWhiteSpace(value) ||
            value == "-" ||
            value.Equals("NA", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value;
    }

    private static string FirstNonEmpty(params string[] values)
    {
        return values.FirstOrDefault(
            value => !string.IsNullOrWhiteSpace(value))
            ?? string.Empty;
    }

    private static DateOnly? ReadDate(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        if (cell.TryGetValue<DateTime>(out var date))
            return DateOnly.FromDateTime(date);

        string value = CellText(cell);

        string[] formats =
        {
            "MMM-yy",
            "MMMM-yy",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd"
        };

        if (DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date))
        {
            return DateOnly.FromDateTime(date);
        }

        if (DateTime.TryParse(value, out date))
            return DateOnly.FromDateTime(date);

        return null;
    }

    private static decimal? ReadDecimal(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        if (cell.TryGetValue<decimal>(out var number))
            return number;

        string value = CellText(cell);

        if (string.IsNullOrWhiteSpace(value) ||
            value is "-" or "..." or "…")
        {
            return null;
        }

        if (decimal.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out number))
        {
            return number;
        }

        // Treat nonnumeric placeholders as unknown.
        return null;
    }

    private static int? ReadNullableInt(IXLCell cell)
    {
        if (cell.IsEmpty())
            return null;

        if (cell.TryGetValue<int>(out int number))
            return number;

        string value = CellText(cell);

        return int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out number)
            ? number
            : null;
    }

    private static bool IsRowEmpty(
        IXLWorksheet sheet,
        int row,
        int firstColumn,
        int lastColumn)
    {
        for (int column = firstColumn; column <= lastColumn; column++)
        {
            if (!string.IsNullOrWhiteSpace(
                    CellText(sheet.Cell(row, column))))
            {
                return false;
            }
        }

        return true;
    }
}
