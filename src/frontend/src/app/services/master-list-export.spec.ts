import { TestBed } from '@angular/core/testing';
import { MasterListExportService } from './master-list-export';

describe('MasterListExport', () => {
  let service: MasterListExportService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MasterListExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
