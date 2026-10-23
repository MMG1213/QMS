import { TestBed } from '@angular/core/testing';
import { MasterList } from './master-list';

describe('MasterList', () => {
  let service: MasterList;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MasterList);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
