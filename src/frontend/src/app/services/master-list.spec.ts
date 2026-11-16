import { TestBed } from '@angular/core/testing';
import { MasterListService } from './master-list';

describe('MasterListService', () => {
  let service: MasterListService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MasterListService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
