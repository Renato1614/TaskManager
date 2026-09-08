import { Component, OnInit, computed, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { TaskRequest, TaskResponse, TaskStatus } from '../../shared/models/task.models';
import { TaskService } from './task.service';

@Component({
  standalone: true,
  selector: 'app-task-list',
  imports: [DatePipe, DragDropModule, RouterLink],
  template: `
    <section class="workspace">
      <div class="page-head">
        <div>
          <p class="eyebrow">Personal tasks</p>
          <h1>My tasks</h1>
        </div>
        <a class="button" routerLink="/tasks/new">New task</a>
      </div>

      @if (loading()) {
        <p class="state">Loading tasks...</p>
      } @else if (error()) {
        <p class="error">{{ error() }}</p>
      } @else if (!tasks().length) {
        <p class="state">No tasks yet.</p>
      } @else {
        <div class="kanban-board">
          @for (column of columns; track column.status) {
            <section class="kanban-column" [attr.data-status]="column.status">
              <header>
                <div>
                  <span class="status">{{ column.label }}</span>
                  <h2>{{ column.title }}</h2>
                </div>
                <strong>{{ groupedTasks()[column.status].length }}</strong>
              </header>

              <div
                class="kanban-list"
                cdkDropList
                [id]="column.status"
                [cdkDropListData]="groupedTasks()[column.status]"
                [cdkDropListConnectedTo]="columnDropListIds"
                (cdkDropListDropped)="drop($event, column.status)">
                @for (task of groupedTasks()[column.status]; track task.id) {
                  <article class="task-card" cdkDrag [cdkDragData]="task">
                    <div>
                      <h3>{{ task.title }}</h3>
                      <p>{{ task.description || 'No description' }}</p>
                    </div>
                    <footer>
                      <span>{{ task.dueDate | date }}</span>
                      <div class="actions">
                        <a [routerLink]="['/tasks', task.id, 'edit']">Edit</a>
                        <button
                          type="button"
                          class="danger"
                          [disabled]="deleting()"
                          (click)="requestDelete(task)">
                          Delete
                        </button>
                      </div>
                    </footer>
                  </article>
                } @empty {
                  <p class="empty-column">Drop tasks here</p>
                }
              </div>
            </section>
          }
        </div>
      }
    </section>

    @if (pendingDelete(); as task) {
      <div class="modal-backdrop" (click)="cancelDelete()">
        <section
          class="confirm-dialog"
          role="dialog"
          aria-modal="true"
          aria-labelledby="delete-dialog-title"
          (click)="$event.stopPropagation()">
          <p class="eyebrow">Confirm deletion</p>
          <h2 id="delete-dialog-title">Delete this task?</h2>
          <p>
            "{{ task.title }}" will be permanently removed from your board.
          </p>
          @if (deleteError()) {
            <p class="error">{{ deleteError() }}</p>
          }
          <div class="dialog-actions">
            <button type="button" class="ghost" [disabled]="deleting()" (click)="cancelDelete()">Cancel</button>
            <button type="button" class="danger" [disabled]="deleting()" (click)="confirmDelete()">
              {{ deleting() ? 'Deleting...' : 'Delete task' }}
            </button>
          </div>
        </section>
      </div>
    }
  `
})
export class TaskListComponent implements OnInit {
  protected readonly tasks = signal<TaskResponse[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly pendingDelete = signal<TaskResponse | null>(null);
  protected readonly deleting = signal(false);
  protected readonly deleteError = signal('');
  protected readonly columns: { status: TaskStatus; title: string; label: string }[] = [
    { status: 'Pending', title: 'Pending', label: 'To do' },
    { status: 'InProgress', title: 'In progress', label: 'Doing' },
    { status: 'Done', title: 'Done', label: 'Complete' }
  ];
  protected readonly columnDropListIds = this.columns.map((column) => column.status);
  protected readonly groupedTasks = computed(() => {
    const grouped: Record<TaskStatus, TaskResponse[]> = {
      Pending: [],
      InProgress: [],
      Done: []
    };

    for (const task of this.tasks()) {
      grouped[task.status].push(task);
    }

    return grouped;
  });

  constructor(private readonly taskService: TaskService) {}

  ngOnInit(): void {
    this.load();
  }

  drop(event: CdkDragDrop<TaskResponse[]>, status: TaskStatus): void {
    const task = event.item.data as TaskResponse;

    if (!task || task.status === status) {
      return;
    }

    const previousTasks = this.tasks();
    const updatedTask = { ...task, status };
    this.tasks.update((items) => items.map((item) => (item.id === task.id ? updatedTask : item)));

    this.taskService.update(task.id, this.toRequest(updatedTask)).subscribe({
      next: (savedTask) => {
        this.tasks.update((items) => items.map((item) => (item.id === savedTask.id ? savedTask : item)));
      },
      error: (response: HttpErrorResponse) => {
        this.tasks.set(previousTasks);
        this.error.set(this.getTaskErrorMessage(response, 'Could not update the task status.'));
      }
    });
  }

  requestDelete(task: TaskResponse): void {
    this.deleteError.set('');
    this.pendingDelete.set(task);
  }

  cancelDelete(): void {
    if (this.deleting()) {
      return;
    }

    this.pendingDelete.set(null);
  }

  confirmDelete(): void {
    const task = this.pendingDelete();

    if (!task) {
      return;
    }

    this.deleting.set(true);
    this.deleteError.set('');
    this.taskService.delete(task.id).subscribe({
      next: () => {
        this.tasks.update((items) => items.filter((item) => item.id !== task.id));
        this.pendingDelete.set(null);
        this.deleting.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.deleteError.set(this.getTaskErrorMessage(response, 'Could not delete the task.'));
        this.deleting.set(false);
      }
    });
  }

  private load(): void {
    this.loading.set(true);
    this.taskService.list().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load tasks.');
        this.loading.set(false);
      }
    });
  }

  private toRequest(task: TaskResponse): TaskRequest {
    return {
      title: task.title,
      description: task.description,
      status: task.status,
      dueDate: task.dueDate
    };
  }

  private getTaskErrorMessage(response: HttpErrorResponse, fallback: string): string {
    const errors = response.error?.errors;

    if (Array.isArray(errors) && errors.length > 0) {
      return errors.join(' ');
    }

    if (typeof response.error?.title === 'string') {
      return response.error.title;
    }

    return fallback;
  }
}
