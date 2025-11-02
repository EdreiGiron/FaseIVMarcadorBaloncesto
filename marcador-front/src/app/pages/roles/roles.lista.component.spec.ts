import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RolesListaComponent } from './roles.lista.component';

describe('RolesListaComponent', () => {
  let component: RolesListaComponent;
  let fixture: ComponentFixture<RolesListaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RolesListaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RolesListaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
