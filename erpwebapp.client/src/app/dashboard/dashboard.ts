import { CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray } from '@angular/cdk/drag-drop';
import { BreakpointObserver } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule,
    MatCardModule,
    MatGridListModule,
    CdkDropList, CdkDrag,
    MatIconModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  cols = 4;
  constructor(private breakpointObserver: BreakpointObserver) {
    this.breakpointObserver.observe(['(max-width: 768px)']).subscribe(res => {
      this.cols = res.matches ? 1 : 4;
    });
  }

  charts = [
    { title: 'Sales Overview', body: '📊 Interactive Chart Placeholder' },
    { title: 'User Growth', body: '📈 Interactive Chart Placeholder' },
  ];
  kpis = [
    { title: 'Products', value: '128', trend: '▲ 12%', icon: 'inventory_2', bg: 'bg-primary', trendClass: 'up' },
    { title: 'Users', value: '54', trend: '▲ 8%', icon: 'group', bg: 'bg-accent', trendClass: 'up' },
    { title: 'Orders', value: '320', trend: '▼ 4%', icon: 'shopping_cart', bg: 'bg-warn', trendClass: 'down' },
    { title: 'Revenue', value: '₹4.5L', trend: '▲ 18%', icon: 'trending_up', bg: 'bg-success', trendClass: 'up' },
  ];
  
  drop(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.kpis, event.previousIndex, event.currentIndex);
  }
   dropChart(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.charts, event.previousIndex, event.currentIndex);
  }
}
