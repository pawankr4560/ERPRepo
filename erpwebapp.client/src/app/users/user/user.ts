import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { UserDetails } from '../user-details';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { EditUserDialogComponent } from '../edit-user-dialog-component/edit-user-dialog-component';
import { ConfirmDialogComponent } from '../confirm-dialog-component/confirm-dialog-component';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
@Component({
  selector: 'app-user',
  imports: [MatTableModule, MatPaginatorModule,MatIconModule,MatButtonModule],
  templateUrl: './user.html',
  styleUrl: './user.css',
})
export class User implements OnInit, AfterViewInit{
  isMobile = false;
  displayedColumns: string[] = [
    'name',
    'gender',
    'weight',
    'height',
    'calorie',
    'isActive',
    'actions'
  ];

  constructor(private dialog: MatDialog,private breakpointObserver: BreakpointObserver) {
  
  }
  dataSource = new MatTableDataSource<UserDetails>(USER_DATA);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
     this.breakpointObserver
    .observe(['(max-width: 768px)'])
    .subscribe((result: { matches: boolean; }) => {
      this.isMobile = result.matches;

      this.displayedColumns = this.isMobile
        ? ['name', 'calorie', 'isActive', 'actions']
        : ['id', 'name', 'gender', 'weight', 'height', 'calorie', 'isActive', 'actions'];
    });
  }

  ngAfterViewInit() {
    this.dataSource.paginator = this.paginator;
  }

 editUser(user: UserDetails) {
  const dialogRef = this.dialog.open(EditUserDialogComponent, {
  data: { ...user }
  });

  dialogRef.afterClosed().subscribe(updatedUser => {
    if (updatedUser) {
      const index = this.dataSource.data.findIndex(u => u.id === updatedUser.id);
      if (index !== -1) {
        this.dataSource.data[index] = updatedUser;
        this.dataSource._updateChangeSubscription();
      }
    }
  });
}


deleteUser(user: UserDetails) {
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    width: '380px',
    data: {
      title: 'Delete User',
      message: `Are you sure you want to delete "${user.name}"?`
    }
  });

  dialogRef.afterClosed().subscribe(confirmed => {
    if (confirmed) {
      this.dataSource.data = this.dataSource.data.filter(
        u => u.id !== user.id
      );
    }
  });
}
}
export const USER_DATA: UserDetails[] = [
  {
    id: 1,
    email:'test1@gmail.com',
    password:'Test@123',
    confirmPassword:'Test@123',
    name: 'Rahul',
    gender: 'Male',
    phone:9829872397,
    weight: 72,
    height: 175,
    calorie: 2200,
    isActive: true
  },
  {
    id: 2,
    email:'test1@gmail.com',
    password:'Test@123',
    confirmPassword:'Test@123',
    name: 'Sukhram',
    gender: 'Male',
    phone:7991829828,
    weight: 60,
    height: 162,
    calorie: 1900,
    isActive: true
  },
  {
    id: 3,
    email:'test1@gmail.com',
    password:'Test@123',
    confirmPassword:'Test@123',
    name: 'Vikram',
    gender: 'Male',
      phone:7991829828,
    weight: 85,
    height: 180,
    calorie: 2600,
    isActive: false
  }
];