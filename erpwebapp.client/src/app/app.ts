import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, signal, ViewChild, ViewContainerRef } from '@angular/core';
import {ReactiveFormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';
import { combineLatest, concatMap, debounceTime, exhaustMap, forkJoin, fromEvent, mergeMap, OperatorFunction, Subject, switchMap, takeUntil, tap, timer } from 'rxjs';
import { DynamicComponent } from './dynamic-component/dynamic-component';
interface Post {
  userId: number;
  id: number;
  title: string;
  body: string;
}
@Component({
  selector: 'app-root',
  imports: [RouterOutlet,CommonModule,ReactiveFormsModule,HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('MyAngularApp');
 /**
  *
  */
 constructor(private http:HttpClient) {
 
//  combineLatest({
//   user: this.http.get('https://jsonplaceholder.typicode.com/users'),
//   comments: this.http.get('https://jsonplaceholder.typicode.com/comments'),
//   posts: this.http.get('https://jsonplaceholder.typicode.com/comments'),
// })
// .subscribe(({ user, posts, comments }) => {
//   console.log(user, posts, comments);
// });
 }

//  combineLatest()
//  {
//   const firstTimer = timer(0, 5); 
//   const secondTimer = timer(500, 1); 
//   const combinedTimers = combineLatest([firstTimer, secondTimer]);
//   firstTimer.subscribe(x=>{
//    console.log(x);
//   });

//   secondTimer.subscribe(x=>{
//    console.log(x);
//   });
//  }

  // STEP 3.1: Get reference to the container
  @ViewChild('container', { read: ViewContainerRef })
  container!: ViewContainerRef;

  // STEP 3.2: Load component dynamically
  loadComponent() {
    // Optional: clear old component
    this.container.clear();

    // STEP 3.3: Create component
    const componentRef =
      this.container.createComponent(DynamicComponent);

    // STEP 3.4: Pass data to dynamic component
    componentRef.instance.title = 'Loaded from AppComponent';
    console.log(componentRef.instance.title);
    
  }

  // STEP 3.5: Remove dynamic component
  clearComponent() {
    this.container.clear();
  }
}
