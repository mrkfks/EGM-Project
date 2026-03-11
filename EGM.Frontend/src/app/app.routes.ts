import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Login } from './pages/login/login';
import { Olay } from './pages/olay/olay';


export const routes: Routes = [
    {path: '', redirectTo:'login', pathMatch: 'full'},
    {path: 'home', component: Home},
    {path: 'login', component:Login},
    {path: 'olay', component:Olay}

];
