// src/app/caracteristicas/renombrar-equipos.dialog.ts
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup,
  FormControl
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

export interface RenombrarData {
  localActual: string;
  visitanteActual: string;
}

export interface RenombrarResultado {
  local: string;
  visitante: string;
}

// 👇 Tipado explícito del formulario (typed forms)
type RenombrarForm = {
  local: FormControl<string>;
  visitante: FormControl<string>;
};

@Component({
  selector: 'app-renombrar-equipos-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  template: `
  <h2 mat-dialog-title class="dlg-title">Renombrar equipos</h2>

  <mat-dialog-content class="dlg-content">
    <form [formGroup]="fm" (ngSubmit)="guardar()" class="dlg-form">

      <mat-form-field appearance="fill" class="full">
        <mat-label>Nombre Local</mat-label>
        <input matInput formControlName="local" required maxlength="40" />
        <mat-hint align="start">Máximo 40 caracteres</mat-hint>
        <mat-error *ngIf="fm.controls.local.hasError('required')">Campo obligatorio</mat-error>
        <mat-error *ngIf="fm.controls.local.hasError('maxlength')">Máximo 40 caracteres</mat-error>
      </mat-form-field>

      <mat-form-field appearance="fill" class="full">
        <mat-label>Nombre Visitante</mat-label>
        <input matInput formControlName="visitante" required maxlength="40" />
        <mat-hint align="start">Máximo 40 caracteres</mat-hint>
        <mat-error *ngIf="fm.controls.visitante.hasError('required')">Campo obligatorio</mat-error>
        <mat-error *ngIf="fm.controls.visitante.hasError('maxlength')">Máximo 40 caracteres</mat-error>
      </mat-form-field>

    </form>
  </mat-dialog-content>

  <mat-dialog-actions align="end" class="dlg-actions">
    <button mat-button (click)="cerrar()">Cancelar</button>
    <button mat-flat-button color="primary" (click)="guardar()" [disabled]="fm.invalid">Guardar</button>
  </mat-dialog-actions>
`,
})
export class RenombrarEquiposDialog {
  // ✅ Controles no-nullables y tipados como string
  readonly fm: FormGroup<RenombrarForm>;

  constructor(
    private fb: FormBuilder,
    private ref: MatDialogRef<RenombrarEquiposDialog, RenombrarResultado>,
    @Inject(MAT_DIALOG_DATA) public data: RenombrarData
  ) {
    this.fm = this.fb.group<RenombrarForm>({
      local: this.fb.control<string>(this.data.localActual ?? '', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(40)]
      }),
      visitante: this.fb.control<string>(this.data.visitanteActual ?? '', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(40)]
      })
    });
  }

  cerrar(): void {
    this.ref.close();
  }

  guardar(): void {
    const { local, visitante } = this.fm.getRawValue(); // ambos son string (no null)
    this.ref.close({ local: local.trim(), visitante: visitante.trim() });
  }
}
