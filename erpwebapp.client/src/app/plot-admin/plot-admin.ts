import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSelectModule } from '@angular/material/select';
import { finalize, forkJoin } from 'rxjs';
import { ConfirmDialogComponent } from '../users/confirm-dialog-component/confirm-dialog-component';
import { PlotAdmin, PlotAmenity, PlotVisitAdmin } from './plot-admin.models';
import { PlotAdminService } from './plot-admin.service';
import { PlotDialog } from './plot-dialog';
import { BookingDetailsDialog } from './booking-details-dialog';

@Component({ selector:'app-plot-admin', standalone:true,
  imports:[CommonModule,FormsModule,MatButtonModule,MatFormFieldModule,MatIconModule,MatInputModule,MatPaginatorModule,MatProgressSpinnerModule,MatSnackBarModule,MatTableModule,MatTabsModule,MatSelectModule],
  templateUrl:'./plot-admin.html', styleUrl:'./plot-admin.css' })
export class PlotAdminComponent implements OnInit, AfterViewInit {
  columns=['plot','location','area','price','status','active','actions']; dataSource=new MatTableDataSource<PlotAdmin>([]);
  visitColumns=['plot','customer','schedule','remarks','status','bookingActions']; visits:PlotVisitAdmin[]=[]; amenities:PlotAmenity[]=[];
  loading=false; bookingsLoading=false; search=''; bookingSearch=''; bookingStatus=''; @ViewChild(MatPaginator) paginator!:MatPaginator;
  constructor(private service:PlotAdminService,private dialog:MatDialog,private snack:MatSnackBar){}
  ngOnInit(){this.load();this.loadBookings();}
  ngAfterViewInit(){this.dataSource.paginator=this.paginator;}
  load(){this.loading=true;forkJoin({plots:this.service.list(this.search),amenities:this.service.amenities()}).pipe(finalize(()=>this.loading=false)).subscribe({next:x=>{this.dataSource.data=x.plots;this.amenities=x.amenities;this.dataSource.paginator=this.paginator;},error:e=>this.notify(e.error?.errorMessage||'Unable to load plot listings')});}
  open(plot?:PlotAdmin){this.dialog.open(PlotDialog,{width:'820px',maxWidth:'96vw',data:{plot:plot??null,amenities:this.amenities}}).afterClosed().subscribe(value=>{if(!value)return;this.loading=true;(plot?this.service.update(value):this.service.create(value)).pipe(finalize(()=>this.loading=false)).subscribe({next:()=>{this.notify(plot?'Plot updated':'Plot added');this.load();},error:e=>this.notify(e.error?.errorMessage||'Unable to save plot')});});}
  remove(plot:PlotAdmin){this.dialog.open(ConfirmDialogComponent,{width:'380px',data:{title:'Delete Plot',message:`Delete "${plot.title}"?`}}).afterClosed().subscribe(ok=>{if(!ok||!plot.id)return;this.service.delete(plot.id).subscribe({next:()=>{this.notify('Plot deleted');this.load();},error:e=>this.notify(e.error?.errorMessage||'Unable to delete plot')});});}
  notify(message:string){this.snack.open(message,'Close',{duration:3000});}
  setVisitStatus(visit:PlotVisitAdmin,status:string){if(status===visit.status)return;this.service.updateVisitStatus(visit.id,status).subscribe({next:()=>{this.notify('Booking status updated');this.loadBookings();},error:e=>this.notify(e.error?.errorMessage||'Unable to update booking')});}
  viewBooking(visit:PlotVisitAdmin){this.dialog.open(BookingDetailsDialog,{width:'760px',maxWidth:'96vw',data:visit}).afterClosed().subscribe(status=>{if(status&&status!==visit.status)this.setVisitStatus(visit,status);});}
  loadBookings(){this.bookingsLoading=true;this.service.visits(this.bookingStatus,this.bookingSearch).pipe(finalize(()=>this.bookingsLoading=false)).subscribe({next:x=>this.visits=x,error:e=>this.notify(e.error?.errorMessage||'Unable to load bookings')});}
  clearBookingFilters(){this.bookingSearch='';this.bookingStatus='';this.loadBookings();}
  get totalPlots(){return this.dataSource.data.length} get activePlots(){return this.dataSource.data.filter(x=>x.isActive).length} get pendingVisits(){return this.visits.filter(x=>x.status==='Pending').length}
  get confirmedVisits(){return this.visits.filter(x=>x.status==='Confirmed').length}
}
