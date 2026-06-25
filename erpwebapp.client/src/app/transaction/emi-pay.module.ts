import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { EmiPayComponent } from './emi-pay/emi-pay';

const routes: Routes = [{ path: '', component: EmiPayComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes), EmiPayComponent],
})
export class EmiPayModule {}
