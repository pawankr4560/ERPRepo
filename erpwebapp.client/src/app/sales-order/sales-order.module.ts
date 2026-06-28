import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SalesOrderComponent } from './sales-order';

const routes: Routes = [{ path: '', component: SalesOrderComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), SalesOrderComponent],
})
export class SalesOrderModule {}
