import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DOCUMENT } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { catchError, throwError, lastValueFrom } from 'rxjs';
import { Router } from '@angular/router';

@Component({
  selector: 'app-index',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './index.component.html',
  styleUrls: ['./index.component.scss']
})
export default class IndexComponent
  implements OnInit {

  private _http: HttpClient = inject(HttpClient);
  private _router: Router = inject(Router);
  private _document: Document = inject(DOCUMENT);

  ngOnInit(): void {
    this._http.get<string>("/api/account")
      .pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status == 401) {
            const redirectUrl = error.headers.get("Location");
            const w: Window = this._document.defaultView!;
            const location: Location = w.location;

            if (redirectUrl)
              location.href = redirectUrl;

            return throwError(() => new Error("Need to Login!"));
          }
          else
            return throwError(() => new Error("Api fail!"));
        })
      )
      .subscribe(res => {
        console.log(res);
      });
  }
}
