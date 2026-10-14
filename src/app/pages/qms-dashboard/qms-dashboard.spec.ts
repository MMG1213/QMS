import { ComponentFixture, TestBed } from '@angular/core/testing';
import { QmsDashboardComponent } from './qms-dashboard';

describe('QmsDashboardComponent', () => {
  let component: QmsDashboardComponent;
  let fixture: ComponentFixture<QmsDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QmsDashboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(QmsDashboardComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});