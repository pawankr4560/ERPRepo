import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { MenuPermission } from '../menu-permission.model';

@Component({
  selector: 'app-permission-component',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatCheckboxModule
  ],
  templateUrl: './permissioncomponent.html',
  styleUrls: ['./permissioncomponent.css']
})
export class PermissionComponent {

  permissionColumns: (keyof MenuPermission['permissions'])[] = [
    'view',
    'create',
    'edit',
    'submit',
    'approve',
    'amend',
    'post',
    'print',
    'delete'
  ];

  displayedColumns: string[] = ['menu', ...this.permissionColumns];

 menuPermissions: MenuPermission[] = [

  // ================= HOME =================
  {
    id: 1,
    name: 'Home',
    level: 0,
    permissions: this.emptyPermissions()
  },
  {
    id: 2,
    name: 'Dashboard',
    parentId: 1,
    level: 1,
    permissions: this.fullPermissions()
  },
  {
    id: 3,
    name: 'KPI Overview',
    parentId: 1,
    level: 1,
    permissions: this.fullPermissions()
  },

  // ================= FINANCE =================
  {
    id: 10,
    name: 'Finance',
    level: 0,
    permissions: this.emptyPermissions()
  },

  // ---- Finance > Master ----
  {
    id: 11,
    name: 'Master',
    parentId: 10,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 12,
    name: 'Accounts',
    parentId: 11,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 13,
    name: 'Dimensions',
    parentId: 11,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 14,
    name: 'Cost Centers',
    parentId: 11,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ---- Finance > Transactions ----
  {
    id: 20,
    name: 'Transactions',
    parentId: 10,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 21,
    name: 'Journal Entry',
    parentId: 20,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 22,
    name: 'Payment Voucher',
    parentId: 20,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 23,
    name: 'Receipt Voucher',
    parentId: 20,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ---- Finance > Reports ----
  {
    id: 30,
    name: 'Reports',
    parentId: 10,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 31,
    name: 'Trial Balance',
    parentId: 30,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 32,
    name: 'Profit & Loss',
    parentId: 30,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 33,
    name: 'Balance Sheet',
    parentId: 30,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ================= INVENTORY =================
  {
    id: 40,
    name: 'Inventory',
    level: 0,
    permissions: this.emptyPermissions()
  },

  // ---- Inventory > Master ----
  {
    id: 41,
    name: 'Item Master',
    parentId: 40,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 42,
    name: 'Products',
    parentId: 41,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 43,
    name: 'Categories',
    parentId: 41,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 44,
    name: 'UOM',
    parentId: 41,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ---- Inventory > Transactions ----
  {
    id: 50,
    name: 'Transactions',
    parentId: 40,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 51,
    name: 'Goods Receipt Note',
    parentId: 50,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 52,
    name: 'Stock Transfer',
    parentId: 50,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 53,
    name: 'Stock Adjustment',
    parentId: 50,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ---- Inventory > Reports ----
  {
    id: 60,
    name: 'Reports',
    parentId: 40,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 61,
    name: 'Stock Ledger',
    parentId: 60,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 62,
    name: 'Stock Valuation',
    parentId: 60,
    level: 2,
    permissions: this.fullPermissions()
  },

  // ================= SYSTEM =================
  {
    id: 70,
    name: 'System',
    level: 0,
    permissions: this.emptyPermissions()
  },

  {
    id: 71,
    name: 'Security',
    parentId: 70,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 72,
    name: 'User Management',
    parentId: 71,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 73,
    name: 'Role Management',
    parentId: 71,
    level: 2,
    permissions: this.fullPermissions()
  },

  {
    id: 74,
    name: 'Settings',
    parentId: 70,
    level: 1,
    permissions: this.emptyPermissions()
  },
  {
    id: 75,
    name: 'Company Setup',
    parentId: 74,
    level: 2,
    permissions: this.fullPermissions()
  },
  {
    id: 76,
    name: 'Fiscal Year',
    parentId: 74,
    level: 2,
    permissions: this.fullPermissions()
  }

];


  toggleColumn(
    permission: keyof MenuPermission['permissions'],
    checked: boolean
  ): void {
    this.menuPermissions.forEach(row => {
      if (row.level === 1) {
        row.permissions[permission] = checked;
      }
    });
    console.log(this.menuPermissions);
  }

  emptyPermissions(): MenuPermission['permissions'] {
    return {
      view: false,
      create: false,
      edit: false,
      submit: false,
      approve: false,
      amend: false,
      post: false,
      print: false,
      delete: false
    };
  }

  fullPermissions(): MenuPermission['permissions'] {
    return {
      view: true,
      create: true,
      edit: true,
      submit: true,
      approve: true,
      amend: true,
      post: false,
      print: false,
      delete: true
    };
  }
}
