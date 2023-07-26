import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { catchError, throwError, lastValueFrom } from 'rxjs';

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

  ngOnInit(): void {
    this._http.get<string>("/api/account")
      .pipe(
        catchError((error: HttpErrorResponse, caught) => {
          if (error.status == 401) {
            console.log(error.message);
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
