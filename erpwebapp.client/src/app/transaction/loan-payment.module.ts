import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { LoanPaymentComponent } from './loan-payment-component/loan-payment-component';

const routes: Routes = [{ path: '', component: LoanPaymentComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), LoanPaymentComponent],
})
export class LoanPaymentModule {}
