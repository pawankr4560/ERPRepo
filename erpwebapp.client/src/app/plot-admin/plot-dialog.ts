import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { PlotAdmin, PlotAmenity } from './plot-admin.models';
interface PlotDialogData { plot: PlotAdmin | null; amenities: PlotAmenity[]; }

@Component({
  selector: 'app-plot-dialog', standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogTitle, MatDialogContent, MatDialogActions,
    MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './plot-dialog.html', styleUrl: './plot-dialog.css'
})
export class PlotDialog {
  readonly form;
  constructor(private fb: FormBuilder, private ref: MatDialogRef<PlotDialog>,
    @Inject(MAT_DIALOG_DATA) public data: PlotDialogData) {
    const plot = data.plot;
    this.form = this.fb.nonNullable.group({
      id: [plot?.id ?? ''], title: [plot?.title ?? '', [Validators.required, Validators.maxLength(150)]],
      location: [plot?.location ?? '', [Validators.required, Validators.maxLength(250)]],
      areaSqFt: [plot?.areaSqFt ?? 0, [Validators.required, Validators.min(.01)]],
      price: [plot?.price ?? 0, [Validators.required, Validators.min(0)]],
      status: [plot?.status ?? 'Available', Validators.required], propertyType: [plot?.propertyType ?? 'Residential', Validators.required],
      roadWidth: [plot?.roadWidth ?? ''], electricity: [plot?.electricity ?? ''], water: [plot?.water ?? ''],
      registration: [plot?.registration ?? ''], description: [plot?.description ?? ''], thumbnailUrl: [plot?.thumbnailUrl ?? ''],
      sellerName: [plot?.sellerName ?? '', Validators.required], sellerPhone: [plot?.sellerPhone ?? '', Validators.required],
      isActive: [plot?.isActive ?? true],
      imageUrlsText: [(plot?.imageUrls ?? []).join('\n')],
      amenityIds: [plot?.amenityIds ?? [] as number[]]
    });
  }
  save(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const value = this.form.getRawValue();
    const { imageUrlsText, ...plot } = value;
    this.ref.close({ ...plot, imageUrls: imageUrlsText.split(/\r?\n|,/).map(x => x.trim()).filter(Boolean) });
  }
  cancel(): void { this.ref.close(null); }
}
