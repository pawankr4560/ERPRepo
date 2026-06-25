import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Product } from './product/product';

const routes: Routes = [{ path: '', component: Product }];

@NgModule({
  imports: [RouterModule.forChild(routes), Product],
})
export class ProductModule {}
