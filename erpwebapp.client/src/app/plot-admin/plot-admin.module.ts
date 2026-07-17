import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PlotAdminComponent } from './plot-admin';
const routes:Routes=[{path:'',component:PlotAdminComponent}];
@NgModule({imports:[RouterModule.forChild(routes),PlotAdminComponent]})
export class PlotAdminModule{}
