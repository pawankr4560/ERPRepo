import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { UserDetails } from '../user-details';
import { UserDetailsService } from '../user-details.service';

@Component({
  selector: 'app-user',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
  ],
  templateUrl: './user.html',
  styleUrl: './user.css',
})
export class User implements OnInit {
  displayedColumns: string[] = ['id', 'firstName', 'lastName', 'mobile', 'address'];
  dataSource = new MatTableDataSource<UserDetails>([]);
  isLoading = false;
  isSaving = false;
  form: FormGroup;

  @ViewChild(MatPaginator) set paginator(value: MatPaginator) {
    if (value) {
      this.dataSource.paginator = value;
    }
  }

  constructor(
    private fb: FormBuilder,
    private userDetailsService: UserDetailsService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.form = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      mobile: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      address: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userDetailsService
      .getAll()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (users) => {
          this.dataSource.data = users ?? [];
        },
        error: (error) => this.showError('Unable to load users.', error),
      });
  }

  save(formDirective?: FormGroupDirective): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.isSaving) {
      return;
    }

    const value = this.form.getRawValue();
    this.isSaving = true;
    this.userDetailsService
      .create({
        firstName: value.firstName?.trim() ?? '',
        lastName: value.lastName?.trim() ?? '',
        mobile: value.mobile?.trim() ?? '',
        address: value.address?.trim() ?? '',
      })
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (user) => {
          this.dataSource.data = [user, ...this.dataSource.data];
          formDirective?.resetForm();
          this.form.reset({
            firstName: '',
            lastName: '',
            mobile: '',
            address: '',
          });
          this.form.markAsPristine();
          this.form.markAsUntouched();
          this.snackBar.open('User created successfully.', 'Close', { duration: 3000 });
        },
        error: (error) => this.showError('Unable to create user.', error),
      });
  }

  createLoan(): void {
    this.router.navigate(['/home/inventory/transactions']);
  }

  keepMobileDigits(): void {
    const control = this.form.get('mobile');
    const digits = `${control?.value ?? ''}`.replace(/\D/g, '').slice(0, 10);
    if (control && control.value !== digits) {
      control.setValue(digits);
    }
  }

  private showError(fallback: string, error: any): void {
    const validationErrors = error?.error?.errors
      ? Object.values(error.error.errors).flat().join(' ')
      : '';
    this.snackBar.open(
      error?.error?.errorMessage ||
        error?.error?.message ||
        validationErrors ||
        fallback,
      'Close',
      { duration: 5000, panelClass: ['error-snackbar'] }
    );
  }
}
