import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Item } from '../interfaces/item';

@Component({
  selector: 'app-edit-product-dialog-component.ts',
  imports: [CommonModule,
    MatDialogActions,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule],
  templateUrl: './edit-product-dialog-component.ts.html',
  styleUrl: './edit-product-dialog-component.ts.css',
})
export class EditProductDialogComponentTs {
form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditProductDialogComponentTs>,
    @Inject(MAT_DIALOG_DATA) public data: Item
  ) {
    this.form = this.fb.group({
      id: [data.id],
      code: [data.code, Validators.required],
      name: [data.name, Validators.required],
      categorie: [data.categorie, Validators.required],
      price: [data.price, Validators.required],
      stockQty: [data.stockQty, Validators.required],
      isActive: [data.status]
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
