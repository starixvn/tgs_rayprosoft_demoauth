import { Component, AfterViewInit, ElementRef, inject, Renderer2, AfterViewChecked } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export default class LoginComponent
  implements AfterViewInit, AfterViewChecked {

  private _elementRef: ElementRef = inject(ElementRef);
  private _location: Location = inject(Location);

  ngAfterViewInit(): void {
    let state = <{ html: string }>this._location.getState();
    console.log(state);
    if (state !== undefined) {
      let loginHtml: string = state.html;
      let doc = new DOMParser().parseFromString(loginHtml, 'text/html');

      this._elementRef.nativeElement.insertAdjacentHTML("beforeend", doc.body.getElementsByTagName("form")[0].outerHTML);
    }
  }

  ngAfterViewChecked(): void {
    document.forms[0].submit();
  }
}
