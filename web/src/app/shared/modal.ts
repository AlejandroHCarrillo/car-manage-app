import { afterRenderEffect, Component, ElementRef, input, output, viewChild } from '@angular/core';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.html',
})
export class Modal {
  private readonly dialog = viewChild<ElementRef<HTMLDialogElement>>('dialogRef');

  readonly open = input(false);
  readonly size = input<'md' | 'lg'>('lg');
  readonly closed = output<void>();

  constructor() {
    afterRenderEffect(() => {
      const dialog = this.dialog()?.nativeElement;
      if (!dialog) {
        return;
      }

      if (this.open()) {
        if (!dialog.open) {
          dialog.showModal();
        }
      } else if (dialog.open) {
        dialog.close();
      }
    });
  }

  protected onNativeClose(): void {
    if (this.open()) {
      this.closed.emit();
    }
  }
}
