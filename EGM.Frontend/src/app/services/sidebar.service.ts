import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  private isOpenSubject = new BehaviorSubject<boolean>(false);
  public isOpen$: Observable<boolean> = this.isOpenSubject.asObservable();

  constructor() {}

  toggleSidebar(): void {
    this.isOpenSubject.next(!this.isOpenSubject.value);
  }

  openSidebar(): void {
    this.isOpenSubject.next(true);
  }

  closeSidebar(): void {
    this.isOpenSubject.next(false);
  }

  getIsOpen(): boolean {
    return this.isOpenSubject.value;
  }
}
