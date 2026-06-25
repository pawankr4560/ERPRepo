import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ItemMaster } from './item-master/item-master';

const routes: Routes = [{ path: '', component: ItemMaster }];

@NgModule({
  imports: [RouterModule.forChild(routes), ItemMaster],
})
export class ItemModule {}
