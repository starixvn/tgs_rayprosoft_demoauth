import { ActivatedRoute, Router } from '@angular/router';
import { AfterViewChecked, AfterViewInit, Component, ElementRef, OnInit, Renderer2, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';

import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, MatInputModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export default class LoginComponent implements OnInit {
  public loginChallenge: string | null = null;
  public forms: FormGroup;
  private _route: ActivatedRoute = inject(ActivatedRoute);
  private _fromBuilder = inject(FormBuilder);

  constructor() {
    this.forms = this._fromBuilder.group({
      userName: [''],
      password: ['']
    });
  }

  async ngOnInit(): Promise<void> {
    const queryParams = await firstValueFrom(this._route.queryParamMap);

    this.loginChallenge = queryParams.get("loginChallenge");
  }
}
