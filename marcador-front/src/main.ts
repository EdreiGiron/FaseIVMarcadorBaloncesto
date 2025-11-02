import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { authInterceptor } from './app/interceptors/auth-interceptor';

import { AppComponent } from './app/app';
import { routes } from './app/app.routes';

bootstrapApplication(AppComponent, {
  ...appConfig,
  providers: [
    ...(appConfig.providers || []),  
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
}).catch(err => console.error(err));
