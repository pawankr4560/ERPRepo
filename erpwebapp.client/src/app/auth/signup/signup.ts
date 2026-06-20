import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCard, MatCardContent, MatCardHeader, MatCardModule, MatCardTitle } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router } from '@angular/router';
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
   constructor(private router: Router, private fb: FormBuilder,private snackBar: MatSnackBar,private authService:Auth) {
    this.signupForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  password: ['', Validators.required],
  confirmPassword: ['', Validators.required],
  firstName: ['', Validators.required],
  lastName: ['', Validators.required],
  gender: ['', Validators.required],
  address: ['', Validators.required],
  phone: ['', [Validators.required, Validators.maxLength(10)]],
  weight: [null, [Validators.required, Validators.min(1)]],
  height: [null, [Validators.required, Validators.min(1)]],
  calorie: [null, [Validators.required, Validators.min(1)]],
  status: [true]
  });
  }

 ngOnInit() {
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
    phone: formValue.phone,
    weight: Number(formValue.weight),
    height: Number(formValue.height),
    calorie: Number(formValue.calorie)
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
    this.router.navigate(['/auth/login']);
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
}