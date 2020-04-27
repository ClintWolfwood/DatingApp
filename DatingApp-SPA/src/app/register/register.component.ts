import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  model: any = {};
  @Output() cancelRegister = new EventEmitter();
  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.authService.register(this.model).subscribe(
      () => {
        console.log('Registration Successful');
      },
      (error) => {
        console.log(error);
      }
    );
  }

  register() {}

  cancel() {
    this.cancelRegister.emit(false);
  }
}