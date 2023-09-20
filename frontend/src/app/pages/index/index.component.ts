import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, DOCUMENT } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { catchError, lastValueFrom, throwError } from 'rxjs';

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

  ngOnInit(): void {
    this._http.get<string>("/api/account")
      .pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status == 401) {
            const loginChallenge = error.headers.get("Login_Challenge");

            if (loginChallenge)
              this._router.navigate(["/login"], { queryParams: { loginChallenge } });

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
