import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../servicios/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  role: string | null = null;

  constructor(private auth: AuthService) { }

  ngOnInit(): void {
    this.role = this.auth.getRole(); // 'Admin' | 'User' | null
  }

  isAdmin(): boolean {
     const r = (this.auth.getRole() || '').trim().toLowerCase();
  return r === 'admin' || r === 'administrador';
  }
}
