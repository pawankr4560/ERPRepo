import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditProductDialogComponentTs } from './edit-product-dialog-component.ts';

describe('EditProductDialogComponentTs', () => {
  let component: EditProductDialogComponentTs;
  let fixture: ComponentFixture<EditProductDialogComponentTs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditProductDialogComponentTs]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditProductDialogComponentTs);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
