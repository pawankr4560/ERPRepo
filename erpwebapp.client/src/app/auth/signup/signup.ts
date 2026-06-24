import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardModule, MatCardTitle } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router } from '@angular/router';
import { UserDetails } from '../../users/user-details';
import { Auth } from '../auth';
import { AddressApiResponse, SignupModel } from '../interfaces/login-model';
import { MatSnackBar } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, filter, switchMap } from 'rxjs';
import { MatOption } from '@angular/material/select';
import {  MatAutocompleteModule } from '@angular/material/autocomplete';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-signup',
  imports: [ ReactiveFormsModule,
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatOption,
    MatCardContent,
    MatButtonModule,
    MatCardTitle,
    MatCardHeader,
    MatCard,
    MatAutocompleteModule
  ],
  templateUrl: './signup.html',
  styleUrl: './signup.css',
})
export class Signup implements OnInit {
  signupForm: FormGroup;
  addresses: any[] = [];
  users: UserDetails[] = [];
  private readonly defaultRedirectUrl = '/auth/login';

   constructor(private router: Router, private route: ActivatedRoute, private fb: FormBuilder,private snackBar: MatSnackBar,private authService:Auth) {
    this.signupForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required],
  confirmPassword: ['', Validators.required],
  firstName: ['', Validators.required],
  lastName: ['', Validators.required],
  gender: ['', Validators.required],
  address: ['', Validators.required],
  phone: ['', [Validators.required, Validators.maxLength(10)]],
  status: [true]
  });
  }

 ngOnInit() {
  if (this.route.snapshot.queryParamMap.get('returnUrl')?.includes('/home/inventory/transactions')) {
    this.router.navigate(['/home/users']);
    return;
  }

  this.signupForm.get('address')!
    .valueChanges
    .pipe(
      debounceTime(500),
      distinctUntilChanged(),
      filter(val => val && val.length > 2),
      switchMap(value =>
        this.authService.searchAddress(value)
      )
    )
    .subscribe({
      next: (res: AddressApiResponse) => {
        this.addresses = res?.data?.predictions ?? [];
      },
      error: (err) => {
        console.error(err);
        this.addresses = [];
      }
    });
}
  
  signupUser() {
    if (this.signupForm.invalid) return;
     
     const formValue = this.signupForm.value;

  const user: SignupModel = {
    email: formValue.email,
    password: formValue.password,
    confirmPassword: formValue.password,
    firstName: formValue.firstName,
    lastName: formValue.lastName,
    gender: formValue.gender,
    address: formValue.address,
    phone: formValue.phone
  };
  this.authService.signup(user).pipe().subscribe({
   next: (res) => {
    if(res.success)
    {
       this.snackBar.open(res.message+ '✅', 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
});
    }
    console.log('Signup success', res);
    this.router.navigateByUrl(this.getSuccessRedirectUrl());
  },
  error: (err) => {
       this.snackBar.open(err.error.errorMessage+ '❌', 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
});
    console.error('Signup failed', err);
  }
  });

}

 redirect()
  {
    this.router.navigate(['/auth/login']);
  }

  private getSuccessRedirectUrl(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl?.startsWith('/home/') ? returnUrl : this.defaultRedirectUrl;
  }
}
