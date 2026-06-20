import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Subject, takeUntil } from 'rxjs';

import { ConfirmDialogComponent } from '../../../users/confirm-dialog-component/confirm-dialog-component';
import { CarDialog } from '../car-dialog/car-dialog';
import { Car, CarCategory, CarDialogMode } from '../interfaces/car';
import { CarService } from '../services/car-service';

@Component({
  selector: 'app-car-master',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './car-master.html',
  styleUrl: './car-master.css',
})
export class CarMaster implements OnInit, AfterViewInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  isLoading = false;
  categories: CarCategory[] = [];
  displayedColumns = [
    'image',
    'car',
    'year',
    'category',
    'transmission',
    'seats',
    'pricePerDay',
    'status',
    'actions',
  ];

  readonly dataSource = new MatTableDataSource<Car>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private carService: CarService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.breakpointObserver
      .observe(['(max-width: 768px)'])
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ matches }) => {
        this.displayedColumns = matches
          ? ['car', 'pricePerDay', 'status', 'actions']
          : [
              'image',
              'car',
              'year',
              'category',
              'transmission',
              'seats',
              'pricePerDay',
              'status',
              'actions',
            ];
      });

    this.carService.cars$
      .pipe(takeUntil(this.destroy$))
      .subscribe((cars) => {
        this.dataSource.data = [...cars];
        this.dataSource.paginator = this.paginator;
      });

    this.loadData();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openCreateDialog(): void {
    if (this.categories.length === 0) {
      this.notify('No car categories are available. Reload the page and try again.');
      return;
    }
    this.openDialog('create');
  }

  viewCar(car: Car): void {
    this.openDialog('view', car);
  }

  editCar(car: Car): void {
    this.openDialog('edit', car);
  }

  deleteCar(car: Car): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '380px',
      data: {
        title: 'Delete Car',
        message: `Are you sure you want to delete "${car.brand} ${car.model}"?`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.carService.deleteCar(car.id).subscribe({
        next: () => {
          this.paginator?.firstPage();
          this.notify('Car deleted successfully');
        },
        error: (error) => this.notify(this.errorMessage(error, 'Delete failed')),
      });
    });
  }

  private loadData(): void {
    this.isLoading = true;
    this.carService.loadCategories().subscribe({
      next: (categories) => {
        this.categories = categories ?? [];
        this.carService.loadCars().subscribe({
          next: () => (this.isLoading = false),
          error: (error) => {
            this.isLoading = false;
            this.notify(this.errorMessage(error, 'Unable to load cars'));
          },
        });
      },
      error: (error) => {
        this.isLoading = false;
        this.notify(this.errorMessage(error, 'Unable to load car categories'));
      },
    });
  }

  private openDialog(mode: CarDialogMode, car?: Car): void {
    const dialogRef = this.dialog.open(CarDialog, {
      width: '720px',
      maxWidth: '95vw',
      data: { mode, car, categories: this.categories },
    });

    if (mode === 'view') return;

    dialogRef.afterClosed().subscribe((result: Car | null) => {
      if (!result) return;

      const request =
        mode === 'create'
          ? this.carService.createCar(result)
          : this.carService.updateCar(result);

      request.subscribe({
        next: () =>
          this.notify(
            mode === 'create'
              ? 'Car created successfully'
              : 'Car updated successfully'
          ),
        error: (error) =>
          this.notify(
            this.errorMessage(
              error,
              mode === 'create' ? 'Create failed' : 'Update failed'
            )
          ),
      });
    });
  }

  private notify(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }

  private errorMessage(error: any, fallback: string): string {
    const validationErrors = error?.error?.errors;
    const firstValidationError = validationErrors
      ? (Object.values(validationErrors).flat() as string[])[0]
      : null;

    return (
      error?.error?.message ||
      error?.error?.errorMessage ||
      firstValidationError ||
      fallback
    );
  }
}
