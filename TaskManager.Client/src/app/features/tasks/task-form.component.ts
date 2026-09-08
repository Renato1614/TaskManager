import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TaskService } from './task.service';
import { TaskRequest, TaskStatus } from '../../shared/models/task.models';

@Component({
  standalone: true,
  selector: 'app-task-form',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="workspace narrow">
      <div class="page-head">
        <div>
          <p class="eyebrow">Task details</p>
          <h1>{{ taskId ? 'Edit task' : 'Create task' }}</h1>
        </div>
        <a routerLink="/tasks">Back</a>
      </div>

      <form [formGroup]="form" (ngSubmit)="submit()" class="form">
        <label>Title<input formControlName="title" maxlength="120"></label>
        @if (form.controls.title.touched && form.controls.title.hasError('required')) {
          <small>Title is required.</small>
        }
        @if (form.controls.title.touched && form.controls.title.hasError('maxlength')) {
          <small>Title must be 120 characters or fewer.</small>
        }

        <label>Description<textarea formControlName="description" maxlength="1000" rows="5"></textarea></label>
        @if (form.controls.description.touched && form.controls.description.hasError('maxlength')) {
          <small>Description must be 1000 characters or fewer.</small>
        }

        <label>Status
          <select formControlName="status">
            @for (status of statuses; track status) {
              <option [value]="status">{{ status }}</option>
            }
          </select>
        </label>
        @if (form.controls.status.touched && form.controls.status.hasError('required')) {
          <small>Status is required.</small>
        }

        <label>Due date<input type="date" formControlName="dueDate" [min]="today"></label>
        @if (shouldShowDueDateError() && form.controls.dueDate.hasError('required')) {
          <small>Due date is required.</small>
        }
        @if (shouldShowDueDateError() && form.controls.dueDate.hasError('pastDate')) {
          <small>Due date cannot be in the past.</small>
        }
        @if (error()) {
          <p class="error">{{ error() }}</p>
        }
        <button type="submit" [disabled]="form.invalid || loading()">{{ loading() ? 'Saving...' : 'Save task' }}</button>
      </form>
    </section>
  `
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly taskService = inject(TaskService);

  protected readonly statuses: TaskStatus[] = ['Pending', 'InProgress', 'Done'];
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly taskId = this.route.snapshot.paramMap.get('id');
  protected readonly today = TaskFormComponent.toDateInputValue(new Date());
  private readonly notPastDateValidator = (control: AbstractControl<string>): ValidationErrors | null => {
    const value = control.value;

    if (!value) {
      return null;
    }

    return value < this.today ? { pastDate: true } : null;
  };

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(120)]],
    description: ['', Validators.maxLength(1000)],
    status: ['Pending' as TaskStatus, Validators.required],
    dueDate: [this.today, [Validators.required, this.notPastDateValidator]]
  });

  ngOnInit(): void {
    if (!this.taskId) {
      return;
    }

    this.loading.set(true);
    this.taskService.get(this.taskId).subscribe({
      next: (task) => {
        this.form.patchValue({
          title: task.title,
          description: task.description ?? '',
          status: task.status,
          dueDate: task.dueDate
        });
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load this task.');
        this.loading.set(false);
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error.set('');
      return;
    }

    const request: TaskRequest = this.form.getRawValue();
    const save = this.taskId
      ? this.taskService.update(this.taskId, request)
      : this.taskService.create(request);

    this.loading.set(true);
    this.error.set('');
    save.subscribe({
      next: () => void this.router.navigateByUrl('/tasks'),
      error: (response: HttpErrorResponse) => {
        this.error.set(this.getSaveErrorMessage(response));
        this.loading.set(false);
      }
    });
  }

  private getSaveErrorMessage(response: HttpErrorResponse): string {
    const errors = response.error?.errors;

    if (Array.isArray(errors) && errors.length > 0) {
      return errors.join(' ');
    }

    if (typeof response.error?.title === 'string') {
      return response.error.title;
    }

    return 'Could not save the task. Please review the fields and try again.';
  }

  protected shouldShowDueDateError(): boolean {
    const dueDate = this.form.controls.dueDate;

    return dueDate.invalid && (dueDate.touched || dueDate.dirty || Boolean(this.taskId));
  }

  private static toDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
