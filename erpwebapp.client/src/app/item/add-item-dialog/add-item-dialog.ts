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
import { Category, SubCategory } from '../interfaces/category';

export interface AddItemDialogData {
  item: Item | null;
  units: Unit[];
  categories: Category[];
  subCategories: SubCategory[];
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
  isEditMode = false;
  filteredSubCategories: SubCategory[] = [];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddItemDialog>,
    @Inject(MAT_DIALOG_DATA) public data: AddItemDialogData | null
  ) {
    const item = data?.item ?? null;
    this.isEditMode = !!item;
    const d =
      item ??
      ({
        id: '',
        code: '',
        name: '',
        categorie: '',
        categoryId: 0,
        subcategoryId: 0,
        stockQty: 0,
        uomIndex: 0,
        unitId: 0,
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
      categoryId: [d.categoryId || null, Validators.required],
      subcategoryId: [d.subcategoryId || null, Validators.required],
      uomIndex: [d.uomIndex || d.unitId || null, [Validators.required]],
      unitId: [d.unitId || d.uomIndex || null],
      locationIndex: [d.locationIndex, [Validators.required]],
      stockQty: [d.stockQty, [Validators.required, Validators.min(0)]],
      status: [d.status],
      isActive: [d.isActive ?? d.status],
      createdOn: [d.createdOn],
      isDeleted: [d.isDeleted ?? false],
      price: [d.price, [Validators.required, Validators.min(0)]],
      description: [d.description],
      image: [d.image],
    });

    this.filterSubCategories(d.categoryId);
    this.form.get('categoryId')?.valueChanges.subscribe((categoryId: number | null) => {
      this.form.patchValue({ subcategoryId: null }, { emitEvent: false });
      this.filterSubCategories(categoryId);
    });

    this.form.get('uomIndex')?.valueChanges.subscribe((unitId: number | null) => {
      this.form.patchValue({ unitId }, { emitEvent: false });
    });
  }

  get units(): Unit[] {
    return this.data?.units ?? [];
  }

  get categories(): Category[] {
    return this.data?.categories ?? [];
  }

  save() {
    if (this.form.valid) {
      const value = this.form.value as Item;
      const category = this.categories.find((item) => item.id === value.categoryId);
      const subCategory = this.filteredSubCategories.find((item) => item.id === value.subcategoryId);
      this.dialogRef.close({
        ...value,
        unitId: value.uomIndex,
        isActive: value.status,
        categorie: category?.name ?? '',
        categoryName: category?.name ?? '',
        subcategoryName: subCategory?.name ?? '',
      } as Item);
    }
  }

  cancel() {
    this.dialogRef.close(null);
  }

  private filterSubCategories(categoryId: number | null) {
    this.filteredSubCategories = (this.data?.subCategories ?? []).filter(
      (subCategory) => subCategory.categoryId === categoryId
    );
  }
}



