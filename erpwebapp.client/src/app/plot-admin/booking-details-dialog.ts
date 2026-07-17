import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogActions, MatDialogContent, MatDialogRef, MatDialogTitle } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { PlotVisitAdmin } from './plot-admin.models';

@Component({ selector:'app-booking-details-dialog', standalone:true,
  imports:[CommonModule,MatDialogTitle,MatDialogContent,MatDialogActions,MatButtonModule,MatIconModule,MatSelectModule,MatFormFieldModule],
  templateUrl:'./booking-details-dialog.html', styleUrl:'./booking-details-dialog.css' })
export class BookingDetailsDialog {
  selectedStatus:string;
  constructor(@Inject(MAT_DIALOG_DATA) public booking:PlotVisitAdmin,private ref:MatDialogRef<BookingDetailsDialog>){this.selectedStatus=booking.status;}
  update(){this.ref.close(this.selectedStatus);}
  close(){this.ref.close(null);}
}
