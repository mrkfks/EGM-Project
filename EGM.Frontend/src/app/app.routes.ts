import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Province } from './pages/province/province';
import { Login } from './pages/login/login';
import { Olay } from './pages/olay/olay';
import { authGuard } from './guards/auth.guard';


export const routes: Routes = [
    {path: '', redirectTo:'login', pathMatch: 'full'},
    {path: 'login', component: Login},
    {path: 'home', component: Home, canActivate: [authGuard]},
    {path: 'olay', component: Olay, canActivate: [authGuard]},
    {path: 'province/:id', component: Province, canActivate: [authGuard]}
];
