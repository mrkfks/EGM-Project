import { Routes } from '@angular/router';
import { Province } from './pages/province/province';
import { Login } from './pages/login/login';
import { Olay } from './pages/olay/olay';


export const routes: Routes = [
    { path: '', redirectTo: 'province', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'olay', component: Olay },
    { path: 'province', component: Province },
    { path: 'province/:id', component: Province },
];
