import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogRef } from '@angular/material/dialog';
import { UserDetails } from '../user-details';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-edit-user-dialog-component',
  imports: [ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogActions,
    MatButtonModule],
  templateUrl: './edit-user-dialog-component.html',
  styleUrl: './edit-user-dialog-component.css',
})
export class EditUserDialogComponent {

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserDetails
  ) {
    this.form = this.fb.group({
      id: [data.id],
      firstName: [data.firstName, Validators.required],
      lastName: [data.lastName, Validators.required],
      mobile: [data.mobile, [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      address: [data.address, Validators.required],
    });
  }

  save() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }

  keepMobileDigits(): void {
    const control = this.form.get('mobile');
    const digits = `${control?.value ?? ''}`.replace(/\D/g, '').slice(0, 10);
    if (control && control.value !== digits) {
      control.setValue(digits);
    }
  }

  cancel() {
    this.dialogRef.close();
  }
}
