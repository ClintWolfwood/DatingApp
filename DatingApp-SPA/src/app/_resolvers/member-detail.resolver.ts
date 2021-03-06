import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { of, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { User } from '../_models/user';

@Injectable()
export class MemberDetailResolver implements Resolve<User> {
  constructor(
    private userUservice: UserService,
    private router: Router,
    private alertify: AlertifyService
  ) {}

  resolve(route: ActivatedRouteSnapshot): Observable<User> {
    return this.userUservice.getUser(route.params['id']).pipe(
      catchError((error) => {
        this.alertify.error('Problem Retrieving Data');
        this.router.navigate(['/members']);
        return of(null);
      })
    );
  }
}
