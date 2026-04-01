import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: 'faaliyet-yonetimi',
    loadComponent: () =>
      import('./pages/faaliyet-yonetimi/faaliyet-yonetimi').then(
        (m) => m.FaaliyetYonetimiComponent
      ),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}