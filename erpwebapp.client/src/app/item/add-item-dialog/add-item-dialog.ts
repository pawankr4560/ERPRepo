import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { Item } from '../interfaces/item';
import { Unit } from '../interfaces/unit';

export interface AddItemDialogData {
  item: Item | null;
  units: Unit[];
}

@Component({
  selector: 'app-add-item-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogActions,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
  ],
  templateUrl: './add-item-dialog.html',
  styleUrl: './add-item-dialog.css',
})
export class AddItemDialog {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddItemDialog>,
    @Inject(MAT_DIALOG_DATA) public data: AddItemDialogData | null
  ) {
    const item = data?.item ?? null;
    const d =
      item ??
      ({
        id: '',
        code: '',
        name: '',
        categorie: '',
        stockQty: 0,
        uomIndex: 0,
        locationIndex: 0,
        status: true,
        price: 0,
        description: '',
        image: '',
      } as Item);

    this.form = this.fb.group({
      id: [d.id],
      code: [d.code, Validators.required],
      name: [d.name, Validators.required],
      categorie: [d.categorie, Validators.required],
      uomIndex: [d.uomIndex || null, [Validators.required]],
      locationIndex: [d.locationIndex, [Validators.required]],
      stockQty: [d.stockQty, [Validators.required, Validators.min(0)]],
      status: [d.status],
      price: [d.price, [Validators.required, Validators.min(0)]],
      description: [d.description],
      image: [d.image],
    });
  }

  get units(): Unit[] {
    return this.data?.units ?? [];
  }

  save() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value as Item);
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }
}

