import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatIcon } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

import { Item } from '../interfaces/item';
import { ProductService } from '../services/product-service';
import { ConfirmDialogComponent } from '../../users/confirm-dialog-component/confirm-dialog-component';
import { EditProductDialogComponentTs } from '../edit-product-dialog-component.ts/edit-product-dialog-component.ts';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatIcon,
    MatButtonModule
  ],
  templateUrl: './product.html',
  styleUrl: './product.css'
})
export class Product implements OnInit, AfterViewInit {

  isMobile = false;

  displayedColumns: string[] = [
    'code',
    'name',
    'category',
    'unitPrice',
    'stockQty',
    'isActive',
    'actions'
  ];

  dataSource = new MatTableDataSource<Item>();

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private productService: ProductService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {

    /** Responsive columns */
    this.breakpointObserver
      .observe(['(max-width: 768px)'])
      .subscribe(result => {
        this.isMobile = result.matches;
        this.displayedColumns = this.isMobile
          ? ['code', 'name', 'category', 'unitPrice']
          : ['code', 'name', 'category', 'unitPrice', 'stockQty', 'isActive', 'actions'];
      });

    /** Subscribe to realtime product stream */
    this.productService.products$.subscribe(data => {
      this.dataSource.data = [...data];
      this.dataSource.paginator = this.paginator;
    });

    /** Initial API load */
    this.productService.loadProducts().subscribe();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  /** Edit Product */
  editProduct(product: Item) {
  const dialogRef = this.dialog.open(EditProductDialogComponentTs, {
    width: '450px',
    data: { ...product }
  });

  dialogRef.afterClosed().subscribe(updatedProduct => {
    if (updatedProduct) {
      this.productService.updateProduct(updatedProduct).subscribe({
        next: () => {
            this.dataSource.paginator = this.paginator;
          this.snackBar.open('Product updated successfully ✅', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top'
          });
          
        },
        error: err => {
          this.snackBar.open(err.error?.errorMessage || 'Update failed ❌', 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top'
          });
        }
      });
    }
  });
}

  /** Delete Product */
  deleteProduct(product: Item) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '380px',
      data: {
        title: 'Delete Product',
        message: `Are you sure you want to delete "${product.name}"?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.productService.deleteProduct(product.id).subscribe({
          next: (res) => {
            if (res?.success) {
              this.paginator.firstPage();
              this.snackBar.open(res.message + ' ✅', 'Close', {
                duration: 3000,
                horizontalPosition: 'right',
                verticalPosition: 'top'
              });
            }
          },
          error: err => {
            this.snackBar.open(err.error?.errorMessage || 'Delete failed', 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            });
          }
        });
      }
    });
  }
}
