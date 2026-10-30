
namespace Qms.Api.DTOs;

// Machines / Equipment
public class MachineListDto
{
    public int Id { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string MachineCategory { get; set; } = string.Empty;
    public string? MachineCodeNum { get; set; }
    public string? MachineSlNum { get; set; }
    public string? Make { get; set; }
    public string CapacityOrSpecification { get; set; } = string.Empty;
    public DateOnly CommissionedOn { get; set; }
    public decimal? LoadInHP { get; set; }
    public string Location { get; set; } = string.Empty;
}

// Standards
public class StandardDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? Pages { get; set; }
    public int? Revision { get; set; }
    public string? Remarks { get; set; }
}

// Testing and Measuring Instruments
public class TestingOrMeasuringInstrumentDto
{
    public int Id { get; set; }
    public string InstrumentDescription { get; set; } = string.Empty;
    public string InstrumentCodeNum { get; set; } = string.Empty;
    public string InstrumentSlNum { get; set; } = string.Empty;
    public string InstrumentRange { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CalibrationFrequency { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

// Quality Performance Index
public class QualityPerformanceIndexDto
{
    public int Id { get; set; }
    public string ProcessOrDept { get; set; } = string.Empty;
    public string MeasurementTitle { get; set; } = string.Empty;
    public string QualityPerformanceIndexFormula { get; set; } = string.Empty;
    public string Targets { get; set; } = string.Empty;
    public string Responsibilities { get; set; } = string.Empty;
    public string DepartmentReview { get; set; } = string.Empty;
    public string QPIReview { get; set; } = string.Empty;
}
