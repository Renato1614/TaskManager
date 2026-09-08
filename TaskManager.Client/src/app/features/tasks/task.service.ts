import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { TaskRequest, TaskResponse } from '../../shared/models/task.models';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly baseUrl = `${environment.apiUrl}/api/tasks`;

  constructor(private readonly http: HttpClient) {}

  list() {
    return this.http.get<TaskResponse[]>(this.baseUrl);
  }

  get(id: string) {
    return this.http.get<TaskResponse>(`${this.baseUrl}/${id}`);
  }

  create(request: TaskRequest) {
    return this.http.post<TaskResponse>(this.baseUrl, request);
  }

  update(id: string, request: TaskRequest) {
    return this.http.put<TaskResponse>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
