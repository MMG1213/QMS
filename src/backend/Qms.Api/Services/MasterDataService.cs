
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class MasterDataService : IMasterDataService
{
    private readonly QmsDbContext _db;

    public MasterDataService(QmsDbContext db)
    {
        _db = db;
    }

    private async Task<int> GetDocumentTypeIdAsync(
        string typeCode,
        CancellationToken cancellationToken)
    {
        var documentTypeId = await _db.DocumentTypes
            .Where(d => d.TypeCode == typeCode)
            .Select(d => (int?)d.DocumentTypeId)
            .SingleOrDefaultAsync(cancellationToken);

        return documentTypeId
            ?? throw new InvalidOperationException(
                $"Document Type '{typeCode}' was not found.");
    }

    // ───────────── Machines / Equipment (ML07) ─────────────

    public async Task<List<MachineListDto>> GetMachinesAsync(
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML07", cancellationToken);

        return await _db.MachinesLists
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .Select(x => new MachineListDto
            {
                Id = x.Id,
                MachineName = x.MachineName,
                MachineCategory = x.MachineCategory,
                MachineCodeNum = x.MachineCodeNum,
                MachineSlNum = x.MachineSlNum,
                Make = x.Make,
                CapacityOrSpecification = x.CapacityOrSpecification,
                CommissionedOn = x.CommissionedOn,
                LoadInHP = x.LoadInHP,
                Location = x.Location
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<MachineListDto> CreateMachineAsync(
        MachineListDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML07", cancellationToken);

        var entity = new MachinesList
        {
            DocumentTypeId = typeId,
            MachineName = request.MachineName,
            MachineCategory = request.MachineCategory,
            MachineCodeNum = request.MachineCodeNum,
            MachineSlNum = request.MachineSlNum,
            Make = request.Make,
            CapacityOrSpecification = request.CapacityOrSpecification,
            CommissionedOn = request.CommissionedOn,
            LoadInHP = request.LoadInHP,
            Location = request.Location
        };

        _db.MachinesLists.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new MachineListDto
        {
            Id = entity.Id,
            MachineName = entity.MachineName,
            MachineCategory = entity.MachineCategory,
            MachineCodeNum = entity.MachineCodeNum,
            MachineSlNum = entity.MachineSlNum,
            Make = entity.Make,
            CapacityOrSpecification = entity.CapacityOrSpecification,
            CommissionedOn = entity.CommissionedOn,
            LoadInHP = entity.LoadInHP,
            Location = entity.Location
        };
    }

    public async Task<MachineListDto?> UpdateMachineAsync(
        int id,
        MachineListDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML07", cancellationToken);

        var entity = await _db.MachinesLists
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return null;

        entity.MachineName = request.MachineName;
        entity.MachineCategory = request.MachineCategory;
        entity.MachineCodeNum = request.MachineCodeNum;
        entity.MachineSlNum = request.MachineSlNum;
        entity.Make = request.Make;
        entity.CapacityOrSpecification = request.CapacityOrSpecification;
        entity.CommissionedOn = request.CommissionedOn;
        entity.LoadInHP = request.LoadInHP;
        entity.Location = request.Location;

        await _db.SaveChangesAsync(cancellationToken);

        return new MachineListDto
        {
            Id = entity.Id,
            MachineName = entity.MachineName,
            MachineCategory = entity.MachineCategory,
            MachineCodeNum = entity.MachineCodeNum,
            MachineSlNum = entity.MachineSlNum,
            Make = entity.Make,
            CapacityOrSpecification = entity.CapacityOrSpecification,
            CommissionedOn = entity.CommissionedOn,
            LoadInHP = entity.LoadInHP,
            Location = entity.Location
        };
    }

    public async Task<bool> DeleteMachineAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML07", cancellationToken);

        var entity = await _db.MachinesLists
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return false;

        _db.MachinesLists.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ───────────────── Standards (ML08) ──────────────────

    public async Task<List<StandardDto>> GetStandardsAsync(
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML08", cancellationToken);

        return await _db.Standards
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .Select(x => new StandardDto
            {
                Id = x.Id,
                Description = x.Description,
                Pages = x.Pages,
                Revision = x.Revision,
                Remarks = x.Remarks
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StandardDto> CreateStandardAsync(
        StandardDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML08", cancellationToken);

        var entity = new Standards
        {
            DocumentTypeId = typeId,
            Description = request.Description,
            Pages = request.Pages,
            Revision = request.Revision,
            Remarks = request.Remarks
        };

        _db.Standards.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new StandardDto
        {
            Id = entity.Id,
            Description = entity.Description,
            Pages = entity.Pages,
            Revision = entity.Revision,
            Remarks = entity.Remarks
        };
    }

    public async Task<StandardDto?> UpdateStandardAsync(
        int id,
        StandardDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML08", cancellationToken);

        var entity = await _db.Standards
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return null;

        entity.Description = request.Description;
        entity.Pages = request.Pages;
        entity.Revision = request.Revision;
        entity.Remarks = request.Remarks;

        await _db.SaveChangesAsync(cancellationToken);

        return new StandardDto
        {
            Id = entity.Id,
            Description = entity.Description,
            Pages = entity.Pages,
            Revision = entity.Revision,
            Remarks = entity.Remarks
        };
    }

    public async Task<bool> DeleteStandardAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML08", cancellationToken);

        var entity = await _db.Standards
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return false;

        _db.Standards.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ───── Testing / Measuring Instruments (ML09) ────────

    public async Task<List<TestingOrMeasuringInstrumentDto>>
        GetInstrumentsAsync(
            CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML09", cancellationToken);

        return await _db.TestingOrMeasuringInstruments
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .Select(x => new TestingOrMeasuringInstrumentDto
            {
                Id = x.Id,
                InstrumentDescription = x.InstrumentDescription,
                InstrumentCodeNum = x.InstrumentCodeNum,
                InstrumentSlNum = x.InstrumentSlNum,
                InstrumentRange = x.InstrumentRange,
                Make = x.Make,
                Location = x.Location,
                CalibrationFrequency = x.CalibrationFrequency,
                Remarks = x.Remarks
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TestingOrMeasuringInstrumentDto> CreateInstrumentAsync(
        TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML09", cancellationToken);

        var entity = new TestingOrMeasuringInstruments
        {
            DocumentTypeId = typeId,
            InstrumentDescription = request.InstrumentDescription,
            InstrumentCodeNum = request.InstrumentCodeNum,
            InstrumentSlNum = request.InstrumentSlNum,
            InstrumentRange = request.InstrumentRange,
            Make = request.Make,
            Location = request.Location,
            CalibrationFrequency = request.CalibrationFrequency,
            Remarks = request.Remarks
        };

        _db.TestingOrMeasuringInstruments.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new TestingOrMeasuringInstrumentDto
        {
            Id = entity.Id,
            InstrumentDescription = entity.InstrumentDescription,
            InstrumentCodeNum = entity.InstrumentCodeNum,
            InstrumentSlNum = entity.InstrumentSlNum,
            InstrumentRange = entity.InstrumentRange,
            Make = entity.Make,
            Location = entity.Location,
            CalibrationFrequency = entity.CalibrationFrequency,
            Remarks = entity.Remarks
        };
    }

    public async Task<TestingOrMeasuringInstrumentDto?> UpdateInstrumentAsync(
        int id,
        TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML09", cancellationToken);

        var entity = await _db.TestingOrMeasuringInstruments
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return null;

        entity.InstrumentDescription = request.InstrumentDescription;
        entity.InstrumentCodeNum = request.InstrumentCodeNum;
        entity.InstrumentSlNum = request.InstrumentSlNum;
        entity.InstrumentRange = request.InstrumentRange;
        entity.Make = request.Make;
        entity.Location = request.Location;
        entity.CalibrationFrequency = request.CalibrationFrequency;
        entity.Remarks = request.Remarks;

        await _db.SaveChangesAsync(cancellationToken);

        return new TestingOrMeasuringInstrumentDto
        {
            Id = entity.Id,
            InstrumentDescription = entity.InstrumentDescription,
            InstrumentCodeNum = entity.InstrumentCodeNum,
            InstrumentSlNum = entity.InstrumentSlNum,
            InstrumentRange = entity.InstrumentRange,
            Make = entity.Make,
            Location = entity.Location,
            CalibrationFrequency = entity.CalibrationFrequency,
            Remarks = entity.Remarks
        };
    }

    public async Task<bool> DeleteInstrumentAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML09", cancellationToken);

        var entity = await _db.TestingOrMeasuringInstruments
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return false;

        _db.TestingOrMeasuringInstruments.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ───────────── Quality Performance Index (ML10) ──────

    public async Task<List<QualityPerformanceIndexDto>> GetQpiAsync(
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML10", cancellationToken);

        return await _db.QualityPerformanceIndex
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == typeId)
            .OrderBy(x => x.Id)
            .Select(x => new QualityPerformanceIndexDto
            {
                Id = x.Id,
                ProcessOrDept = x.ProcessOrDept,
                MeasurementTitle = x.MeasurementTitle,
                QualityPerformanceIndexFormula =
                    x.QualityPerformanceIndexFormula,
                Targets = x.Targets,
                Responsibilities = x.Responsibilities,
                DepartmentReview = x.DepartmentReview,
                QPIReview = x.QPIReview
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<QualityPerformanceIndexDto> CreateQpiAsync(
        QualityPerformanceIndexDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML10", cancellationToken);

        var entity = new QualityPerformanceIndex
        {
            DocumentTypeId = typeId,
            ProcessOrDept = request.ProcessOrDept,
            MeasurementTitle = request.MeasurementTitle,
            QualityPerformanceIndexFormula =
                request.QualityPerformanceIndexFormula,
            Targets = request.Targets,
            Responsibilities = request.Responsibilities,
            DepartmentReview = request.DepartmentReview,
            QPIReview = request.QPIReview
        };

        _db.QualityPerformanceIndex.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new QualityPerformanceIndexDto
        {
            Id = entity.Id,
            ProcessOrDept = entity.ProcessOrDept,
            MeasurementTitle = entity.MeasurementTitle,
            QualityPerformanceIndexFormula =
                entity.QualityPerformanceIndexFormula,
            Targets = entity.Targets,
            Responsibilities = entity.Responsibilities,
            DepartmentReview = entity.DepartmentReview,
            QPIReview = entity.QPIReview
        };
    }

    public async Task<QualityPerformanceIndexDto?> UpdateQpiAsync(
        int id,
        QualityPerformanceIndexDto request,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML10", cancellationToken);

        var entity = await _db.QualityPerformanceIndex
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return null;

        entity.ProcessOrDept = request.ProcessOrDept;
        entity.MeasurementTitle = request.MeasurementTitle;
        entity.QualityPerformanceIndexFormula =
            request.QualityPerformanceIndexFormula;
        entity.Targets = request.Targets;
        entity.Responsibilities = request.Responsibilities;
        entity.DepartmentReview = request.DepartmentReview;
        entity.QPIReview = request.QPIReview;

        await _db.SaveChangesAsync(cancellationToken);

        return new QualityPerformanceIndexDto
        {
            Id = entity.Id,
            ProcessOrDept = entity.ProcessOrDept,
            MeasurementTitle = entity.MeasurementTitle,
            QualityPerformanceIndexFormula =
                entity.QualityPerformanceIndexFormula,
            Targets = entity.Targets,
            Responsibilities = entity.Responsibilities,
            DepartmentReview = entity.DepartmentReview,
            QPIReview = entity.QPIReview
        };
    }

    public async Task<bool> DeleteQpiAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var typeId = await GetDocumentTypeIdAsync("ML10", cancellationToken);

        var entity = await _db.QualityPerformanceIndex
            .FirstOrDefaultAsync(
                x => x.Id == id && x.DocumentTypeId == typeId,
                cancellationToken);

        if (entity is null)
            return false;

        _db.QualityPerformanceIndex.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
