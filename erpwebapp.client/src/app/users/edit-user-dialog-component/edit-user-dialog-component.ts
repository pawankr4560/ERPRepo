import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogRef } from '@angular/material/dialog';
import { UserDetails } from '../user-details';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-edit-user-dialog-component',
  imports: [ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogActions,
    MatButtonModule,
    MatSelectModule],
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
      name: [data.name, Validators.required],
      gender: [data.gender, Validators.required],
      weight: [data.weight, Validators.required],
      height: [data.height, Validators.required],
      calorie: [data.calorie, Validators.required],
      isActive: [data.isActive]
    });
  }

  save() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }

  cancel() {
    this.dialogRef.close();
  }
}
