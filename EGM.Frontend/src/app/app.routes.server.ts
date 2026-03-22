import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    // Parametreli rotalar SSR prerender'da getPrerenderParams gerektirmeden
    // client-side render edilir; bu sayede province/:id hatası ortadan kalkar.
    path: 'province/:id',
    renderMode: RenderMode.Client
  },
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
