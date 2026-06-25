import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Emi } from './emi/emi';

const routes: Routes = [{ path: '', component: Emi }];

@NgModule({
  imports: [RouterModule.forChild(routes), Emi],
})
export class EmiModule {}
