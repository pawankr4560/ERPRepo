import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIcon } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BreakpointObserver } from '@angular/cdk/layout';

import { Item } from '../interfaces/item';
import { ItemService } from '../services/item-service';
import { ConfirmDialogComponent } from '../../users/confirm-dialog-component/confirm-dialog-component';
import { AddItemDialog } from '../add-item-dialog/add-item-dialog';
import { MatSortModule } from '@angular/material/sort';

@Component({
  selector: 'app-item-master',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatIcon,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSortModule,
  ],
  templateUrl: './item-master.html',
  styleUrl: './item-master.css',
})
export class ItemMaster implements OnInit, AfterViewInit {
  isMobile = false;
  isLoading = false;

  displayedColumns: string[] = [
    'code',
    'name',
    'categorie',
    'price',
    'stockQty',
    'status',
    'actions',
  ];

  dataSource = new MatTableDataSource<Item>([]);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private itemService: ItemService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.breakpointObserver.observe(['(max-width: 768px)']).subscribe((result) => {
      this.isMobile = result.matches;
      this.displayedColumns = this.isMobile
        ? ['code', 'name', 'categorie', 'price', 'actions']
        : ['code', 'name', 'categorie', 'price', 'stockQty', 'status', 'actions'];
    });

    this.itemService.items$.subscribe((items) => {
      this.dataSource.data = [...(items ?? [])];
      this.dataSource.paginator = this.paginator;
    });

    this.isLoading = true;
    this.itemService.loadItems().subscribe({
      complete: () => (this.isLoading = false),
      error: () => (this.isLoading = false),
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  openAddDialog() {
    const dialogRef = this.dialog.open(AddItemDialog, {
      width: '520px',
      maxWidth: 'calc(100vw - 24px)',
      maxHeight: 'calc(100vh - 24px)',
      data: null,
    });

    dialogRef.afterClosed().subscribe((created: Item | null) => {
      if (!created) return;

      this.itemService.createItem(created).subscribe({
        next: (res) => {
          if (res?.success) {
            this.snackBar.open('Item created successfully ✅', 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err?.error?.errorMessage || 'Create failed ❌', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
      });
    });
  }

  deleteItem(item: Item) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '380px',
      data: {
        title: 'Delete Item',
        message: `Are you sure you want to delete "${item.name}"?`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.itemService.deleteItem(item.id).subscribe({
        next: (res) => {
          if (res?.success) {
            this.paginator.firstPage();
            this.snackBar.open(res?.message + ' ✅', 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
          }
        },
        error: (err) => {
          this.snackBar.open(err?.error?.errorMessage || 'Delete failed', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        },
      });
    });
  }
}

