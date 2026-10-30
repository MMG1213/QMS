namespace Qms.Api.Models;

public class TestingOrMeasuringInstruments
{
    public int Id {get; set;}

    public string InstrumentDescription {get; set;} = string.Empty;
    
    public string InstrumentCodeNum {get; set;} = string.Empty;

    public string InstrumentSlNum {get; set;} = string.Empty;

    public string InstrumentRange {get; set;} = string.Empty;

    public string Make {get; set;} = string.Empty;

    public string Location {get; set;} = string.Empty;

    public string CalibrationFrequency {get; set;} = string.Empty;

    public string Remarks {get; set;} = string.Empty;

    public int DocumentTypeId {get; set;}

    public DocumentType?DocumentType {get; set;}
}