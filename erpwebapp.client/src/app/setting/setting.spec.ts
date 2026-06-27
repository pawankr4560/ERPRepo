import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of } from 'rxjs';

import { Setting } from './setting';
import { InterestSettingService } from './interest-setting.service';

describe('Setting', () => {
  let component: Setting;
  let fixture: ComponentFixture<Setting>;
  let settingService: jasmine.SpyObj<InterestSettingService>;

  beforeEach(async () => {
    settingService = jasmine.createSpyObj<InterestSettingService>(
      'InterestSettingService',
      ['load', 'loadBookingPaymentCharges', 'save', 'saveBookingPaymentCharges']
    );
    settingService.load.and.returnValue(of('Reducing'));
    settingService.loadBookingPaymentCharges.and.returnValue(
      of({ fixedCharge: 25, percentageCharge: 2 })
    );

    await TestBed.configureTestingModule({
      imports: [Setting],
      providers: [
        { provide: InterestSettingService, useValue: settingService },
        { provide: MatSnackBar, useValue: jasmine.createSpyObj('MatSnackBar', ['open']) },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Setting);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
    expect(component.fixedCharge).toBe(25);
    expect(component.percentageCharge).toBe(2);
  });
});
