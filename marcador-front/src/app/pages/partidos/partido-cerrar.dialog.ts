import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

type DatosDialog = {
  localNombre: string;
  visitanteNombre: string;
  marcadorLocal?: number | null;
  marcadorVisitante?: number | null;
};

@Component({
  standalone: true,
  selector: 'app-partido-cerrar-dialog',
  imports: [CommonModule, FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Cerrar partido</h2>
    <div mat-dialog-content class="wrap">
      <mat-form-field appearance="outline">
        <mat-label>{{data.localNombre}}</mat-label>
        <input matInput type="number" [(ngModel)]="ml">
      </mat-form-field>

      <mat-form-field appearance="outline">
        <mat-label>{{data.visitanteNombre}}</mat-label>
        <input matInput type="number" [(ngModel)]="mv">
      </mat-form-field>
    </div>

    <div mat-dialog-actions align="end">
      <button mat-button (click)="cerrar()">Cancelar</button>
      <button mat-raised-button color="primary" (click)="guardar()" [disabled]="!valido()">Guardar</button>
    </div>
  `,
  styles: [`.wrap{display:flex;gap:12px;margin-top:8px;}`]
})
export class PartidoCerrarDialog {
  data = inject<DatosDialog>(MAT_DIALOG_DATA);
  ref = inject(MatDialogRef<PartidoCerrarDialog, { ml: number; mv: number } | null>);
  ml = this.data.marcadorLocal ?? 0;
  mv = this.data.marcadorVisitante ?? 0;

  valido() { return Number.isFinite(this.ml) && Number.isFinite(this.mv); }
  cerrar() { this.ref.close(null); }
  guardar() { this.ref.close({ ml: Number(this.ml), mv: Number(this.mv) }); }
}
