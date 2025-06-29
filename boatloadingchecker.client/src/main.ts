import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { importProvidersFrom } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { HomeComponent } from './app/home/home.component';

const routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent }
];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    importProvidersFrom(HttpClientModule)
  ]
})
  .catch(err => console.error(err));
