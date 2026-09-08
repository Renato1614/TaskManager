export type TaskStatus = 'Pending' | 'InProgress' | 'Done';

export interface TaskResponse {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate: string;
  userId: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate: string;
}
