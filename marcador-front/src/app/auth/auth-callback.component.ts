// src/app/auth/auth-callback.component.ts
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../servicios/auth.service';

@Component({
    standalone: true,
    selector: 'app-auth-callback',
    template: `<div class="p-8 text-center">Procesando autenticación…</div>`,
})
export class AuthCallbackComponent implements OnInit {
    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private auth: AuthService
    ) { }

    ngOnInit(): void {
        const q = this.route.snapshot.queryParamMap;

        const access = q.get('accessToken');
        const refresh = q.get('refreshToken');
        const name = q.get('name') ?? '';
        const role = q.get('role') ?? 'USER';

        if (!access || !refresh) {
            this.router.navigate(['/login']);
            return;
        }

        this.auth.saveLoginData({
            token: access,
            refreshToken: refresh,
            username: name,
            role: { name: role }
        });

        // Redirige al dashboard
        this.router.navigate(['/dashboard']);
    }
}
