/**
 * Servicio de autenticación del frontend.
 *
 * Responsabilidades:
 * - Login contra el API y almacenamiento de tokens (access/refresh) en localStorage.
 * - Lectura del usuario/rol actual desde almacenamiento local.
 * - Renovación de access token vía refresh token.
 * - Validación de token (ping a /auth/validate).
 * - Registro de usuarios.
 *
 * Notas:
 * - La base URL del API se toma de `Global.url`.
 * - Access token se guarda en `auth_token`; refresh en `refresh_token`.
 * - Expone un BehaviorSubject para conocer si el usuario está logueado.
 */
import { Injectable } from '@angular/core';

type Sonido = 'click' | 'warn' | 'horn';

@Injectable({ providedIn: 'root' })
export class AudioService {
  private enabled = true;
  private ctx?: AudioContext;

  setEnabled(v: boolean) { this.enabled = v; }

  private ensureCtx(): AudioContext {
    if (this.ctx) return this.ctx;
    const AC: any = (window as any).AudioContext || (window as any).webkitAudioContext;
    this.ctx = new AC();
    return this.ctx!;
  }

  play(kind: Sonido) {
    if (!this.enabled) return;
    const ctx = this.ensureCtx();
    const now = ctx.currentTime;

    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.connect(gain);
    gain.connect(ctx.destination);

    const startGain = 0.0001;

    switch (kind) {
      case 'click':
        osc.type = 'square';
        osc.frequency.setValueAtTime(800, now);
        gain.gain.setValueAtTime(startGain, now);
        gain.gain.exponentialRampToValueAtTime(0.25, now + 0.005);
        gain.gain.exponentialRampToValueAtTime(startGain, now + 0.06);
        osc.start(now); osc.stop(now + 0.07);
        break;

      case 'warn':
        osc.type = 'sine';
        osc.frequency.setValueAtTime(1000, now);
        gain.gain.setValueAtTime(startGain, now);
        gain.gain.exponentialRampToValueAtTime(0.6, now + 0.01);
        gain.gain.exponentialRampToValueAtTime(startGain, now + 0.18);
        osc.start(now); osc.stop(now + 0.2);
        break;

      case 'horn':
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(330, now);
        gain.gain.setValueAtTime(startGain, now);
        gain.gain.exponentialRampToValueAtTime(0.85, now + 0.05);
        gain.gain.exponentialRampToValueAtTime(startGain, now + 1.2);
        osc.start(now); osc.stop(now + 1.25);
        break;
    }
  }
}
