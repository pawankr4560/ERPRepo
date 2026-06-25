import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoanComponent } from './loan-component/loan-component';
import { LoanDetailsComponent } from './loan-details/loan-details';

const routes: Routes = [
  { path: '', component: LoanComponent },
  { path: ':id', component: LoanDetailsComponent },
];

@NgModule({
  imports: [RouterModule.forChild(routes), LoanComponent, LoanDetailsComponent],
})
export class LoanModule {}
