import { TestBed } from '@angular/core/testing';
import { MasterListExport } from './master-list-export';

describe('MasterListExport', () => {
  let service: MasterListExport;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MasterListExport);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
