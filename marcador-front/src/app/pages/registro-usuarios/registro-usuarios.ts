import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../servicios/auth.service';
import { RegisterDto } from '../../modelos/dto/register-dto';
import { RegisterResponseDto } from '../../modelos/dto/register-response-dto';

// Angular Material
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';


interface RoleVM { id: number; name: string; }

@Component({
  selector: 'app-registro-usuarios',
  standalone: true,
  imports: [MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    ReactiveFormsModule,
    CommonModule],
  templateUrl: './registro-usuarios.html',
  styleUrl: './registro-usuarios.css'

})


export class RegistroUsuarios {
  form: FormGroup;
  error = '';
  success = '';
 
  roles: RoleVM[] = [];

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
      roleId: ['', Validators.required]  //ID del rol
    });
  }

  ngOnInit(): void {
    this.auth.getRoles().subscribe({
      next: (res) => this.roles = res,
      error: (err) => console.error("Error al cargar roles", err)
    });
  }

  onSubmit(): void {
    if (this.form.valid) {
      const registerData: RegisterDto = {
        username: this.form.value.username,
        password: this.form.value.password,
        roleId: this.form.value.roleId
      };

      this.auth.register(registerData).subscribe({
        next: (res: RegisterResponseDto) => {
          this.success = `${res.message} (Usuario: ${res.username}, Rol: ${res.roleName})`;
          setTimeout(() => this.router.navigate(['/login']), 10000);
        },
        error: (err) => {
          this.error = err.error?.message || '❌ Error en el registro';
          console.error(err);
        }
      });
    }
  }

}
