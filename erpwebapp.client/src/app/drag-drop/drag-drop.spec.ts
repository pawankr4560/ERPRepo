import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DragDrop } from './drag-drop';

describe('DragDrop', () => {
  let component: DragDrop;
  let fixture: ComponentFixture<DragDrop>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DragDrop]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DragDrop);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
