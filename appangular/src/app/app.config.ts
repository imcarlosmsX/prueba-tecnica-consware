import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { registerLocaleData } from '@angular/common';
import localeEsCo from '@angular/common/locales/es-CO';

import { apiErrorInterceptor } from './core/api-error.interceptor';

// Sin el locale colombiano, los pipes de moneda y fecha usan el formato de Estados Unidos y un
// monto se muestra como "$950,000" en vez de "$950.000", que es al revés de como se lee aquí.
registerLocaleData(localeEsCo);

// Sin router: el panel es una sola pantalla y agregar rutas sería estructura sin destino.
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([apiErrorInterceptor])),
    { provide: LOCALE_ID, useValue: 'es-CO' },
  ],
};
