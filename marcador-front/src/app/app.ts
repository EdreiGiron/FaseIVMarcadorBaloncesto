import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from './servicios/auth.service';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';

// Angular Material
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class AppComponent {
  isLoggedIn$: Observable<boolean>;
  constructor(private auth: AuthService) {
    this.isLoggedIn$ = this.auth.isLoggedIn$;
  }

  onLogout(): void {
    this.auth.logout();
  }
}
