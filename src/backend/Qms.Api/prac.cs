using System.Xml;

/*public async Task<byte[]>GetcontroledDocumentAsync(IXLWorksheet worksheet,int ? documenttypeid,int? departmentid) {
    var query = _context.Documents.AsNoTracking().Where(d => d.DocumenTypeId == documenttypeid.value);

    if (departmentid.HasValue)
    {
        query = query.Where(e => e.DepartmentId == departmentId.Value);
    }


    var documents = query.Include(e => e.Revisions).OrderBy(e => e.DepartmentId).ThenBy(e => e.DocumentNo).ToListAsync(cancellationToken);

    var departmentNames = _context.QmsDepartments.ToDictionaryAsync(d => d.DepartmentId, d => d.DepartmentName, cancellationToken);

    var maxrevisions = documents.SelectMany(d => d.Revisions).Select(d => d.RevisionNumber).DefaultIfEmpty(0).Max();

    var headers = new List<string>{
        "Description",
        "DocumentNo"
    };
    for (int i = 0; i <= maxrevisions; i++)
    {
        headers.Add("Pages");
        headers.Add("Date");
    }

    headers.Add("RetentionPeriod");
    headers.Add("Status");
    headers.Add("obselte date");

    for (int i = 0; i < headers.count; i++)
    {
        worksheet.Cell(1, i + 1).value = headers[i];
    }

}*/