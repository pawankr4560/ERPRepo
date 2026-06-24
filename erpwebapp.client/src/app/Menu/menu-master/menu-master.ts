import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { Menu } from '../menu';
import { MenuItem } from '../menu-item';

@Component({
  selector: 'app-menu-master',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTableModule,
  ],
  templateUrl: './menu-master.html',
  styleUrl: './menu-master.css',
})
export class MenuMaster implements OnInit {
  displayedColumns = ['id', 'title', 'iconClass', 'route', 'parent', 'isActive', 'orderNumber', 'actions'];
  dataSource = new MatTableDataSource<MenuItem>([]);
  form: FormGroup;
  isLoading = false;
  isSaving = false;
  editingMenuId: number | null = null;

  @ViewChild(MatPaginator) set paginator(value: MatPaginator) {
    if (value) {
      this.dataSource.paginator = value;
    }
  }

  constructor(
    private fb: FormBuilder,
    private menuService: Menu,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      iconClass: ['', Validators.required],
      route: [''],
      parentId: [0],
      isActive: [true],
      orderNumber: [1, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit(): void {
    this.loadMenus();
  }

  get parentMenus(): MenuItem[] {
    return this.dataSource.data.filter((menu) => menu.id !== this.editingMenuId);
  }

  loadMenus(): void {
    this.isLoading = true;
    this.menuService
      .getAllMenus()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (menus) => {
          this.dataSource.data = menus ?? [];
        },
        error: (error) => this.showError('Unable to load menus.', error),
      });
  }

  save(formDirective?: FormGroupDirective): void {
    this.form.markAllAsTouched();
    if (this.form.invalid || this.isSaving) {
      return;
    }

    const value = this.form.getRawValue();
    const parentId = Number(value.parentId || 0);
    this.isSaving = true;
    const request = {
      title: value.title?.trim() ?? '',
      iconClass: value.iconClass?.trim() ?? '',
      route: value.route?.trim() || null,
      parentId,
      isActive: !!value.isActive,
      orderNumber: Number(value.orderNumber || 1),
    };

    const isEditMode = this.editingMenuId !== null;
    const saveRequest = isEditMode
      ? this.menuService.updateMenu(this.editingMenuId!, request)
      : this.menuService.createMenu(request);

    saveRequest
      .pipe(finalize(() => (this.isSaving = false)))
      .subscribe({
        next: (menu) => {
          this.dataSource.data = this.editingMenuId
            ? this.dataSource.data.map((item) => (item.id === menu.id ? menu : item))
            : [menu, ...this.dataSource.data];
          this.resetForm(formDirective);
          this.snackBar.open(
            isEditMode ? 'Menu updated successfully.' : 'Menu created successfully.',
            'Close',
            { duration: 3000 }
          );
        },
        error: (error) => this.showError(isEditMode ? 'Unable to update menu.' : 'Unable to create menu.', error),
      });
  }

  edit(menu: MenuItem): void {
    this.editingMenuId = menu.id;
    this.form.reset({
      title: menu.title,
      iconClass: menu.iconClass,
      route: menu.route ?? '',
      parentId: menu.parentId ?? 0,
      isActive: menu.isActive,
      orderNumber: menu.orderNumber,
    });
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }

  cancelEdit(formDirective?: FormGroupDirective): void {
    this.resetForm(formDirective);
  }

  delete(menu: MenuItem): void {
    if (!confirm(`Delete menu "${menu.title}"?`)) {
      return;
    }

    this.menuService.deleteMenu(menu.id).subscribe({
      next: () => {
        this.dataSource.data = this.dataSource.data.filter((item) => item.id !== menu.id);
        if (this.editingMenuId === menu.id) {
          this.resetForm();
        }
        this.snackBar.open('Menu deleted successfully.', 'Close', { duration: 3000 });
      },
      error: (error) => this.showError('Unable to delete menu.', error),
    });
  }

  getParentTitle(parentId?: number | null): string {
    if (!parentId) {
      return '-';
    }

    return this.dataSource.data.find((menu) => menu.id === parentId)?.title ?? '-';
  }

  private showError(fallback: string, error: any): void {
    this.snackBar.open(
      error?.error?.errorMessage ||
        error?.error?.message ||
        error?.error?.data?.message ||
        fallback,
      'Close',
      { duration: 5000, panelClass: ['error-snackbar'] }
    );
  }

  private resetForm(formDirective?: FormGroupDirective): void {
    this.editingMenuId = null;
    formDirective?.resetForm({
      title: '',
      iconClass: '',
      route: '',
      parentId: 0,
      isActive: true,
      orderNumber: 1,
    });
    this.form.reset({
      title: '',
      iconClass: '',
      route: '',
      parentId: 0,
      isActive: true,
      orderNumber: 1,
    });
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }
}
