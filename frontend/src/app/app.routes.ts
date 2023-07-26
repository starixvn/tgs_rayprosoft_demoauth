import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: "",
    loadComponent: () => import('./pages/index/index.component'),

  },
  {
    path: "login",
    loadComponent: () => import('./pages/login/login.component')
  }
];
