import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';

import { User } from './user';
import { UserDetailsService } from '../user-details.service';

describe('User', () => {
  let component: User;
  let fixture: ComponentFixture<User>;
  const userDetailsServiceMock = {
    getAll: jasmine.createSpy('getAll').and.returnValue(of([])),
    create: jasmine.createSpy('create').and.returnValue(
      of({
        id: 1,
        firstName: 'Pawan',
        lastName: 'Kumar',
        mobile: '4723927927839238',
        address: 'Bairgania',
      })
    ),
  };
  const routerMock = {
    navigate: jasmine.createSpy('navigate'),
  };

  beforeEach(async () => {
    routerMock.navigate.calls.reset();
    userDetailsServiceMock.getAll.calls.reset();
    userDetailsServiceMock.getAll.and.returnValue(of([]));
    userDetailsServiceMock.create.calls.reset();
    userDetailsServiceMock.create.and.returnValue(
      of({
        id: 1,
        firstName: 'Pawan',
        lastName: 'Kumar',
        mobile: '4723927927839238',
        address: 'Bairgania',
      })
    );

    await TestBed.configureTestingModule({
      imports: [User],
      providers: [
        { provide: UserDetailsService, useValue: userDetailsServiceMock },
        { provide: Router, useValue: routerMock },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(User);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should navigate to create loan screen', () => {
    component.createLoan();
    expect(routerMock.navigate).toHaveBeenCalledWith(['/home/inventory/transactions']);
  });
});
