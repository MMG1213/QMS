import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DCRCreatorDto { creatorEmployeeId: string; creatorEmployeeName: string; }
export interface ApprovalLevelDto { id: number; levelNumber: number; approvalType: string; approverEmployeeId: string; approverEmployeeName: string; redirectedApproverEmployeeId: string | null; redirectedApproverEmployeeName: string | null; isActive: boolean; }
export interface DepartmentApprovalMappingDto { id: number; departmentId: number; departmentName: string; configVersion: number; isActive: boolean; createdByEmployeeId: string; createdByEmployeeName: string; createdAtUtc: string; updatedAtUtc: string | null; dcrCreators: DCRCreatorDto[]; approvalLevels: ApprovalLevelDto[]; }
export interface CreateApprovalLevelDto { levelNumber: number; approvalType: string; approverEmployeeId: string; redirectedApproverEmployeeId: string | null; isActive: boolean; }
export interface CreateDepartmentApprovalMappingDto { departmentId: number; creatorEmployeeIds: string[]; approvalLevels: CreateApprovalLevelDto[]; }
export interface UpdateDepartmentApprovalMappingDto { isActive: boolean; creatorEmployeeIds: string[]; approvalLevels: CreateApprovalLevelDto[]; }
export interface ApprovalMappingEmployeeDto { employeeId: string; fullName: string; role: string; departmentId: number | null; }

@Injectable({ providedIn: 'root' })
export class DepartmentApprovalMappingService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/department-approval-mappings';
  getAllMappings(): Observable<DepartmentApprovalMappingDto[]> { return this.http.get<DepartmentApprovalMappingDto[]>(this.apiUrl); }
  getMappingById(id: number): Observable<DepartmentApprovalMappingDto> { return this.http.get<DepartmentApprovalMappingDto>(`${this.apiUrl}/${id}`); }
  getEmployeesByDepartment(departmentId: number): Observable<ApprovalMappingEmployeeDto[]> { return this.http.get<ApprovalMappingEmployeeDto[]>(`${this.apiUrl}/employees/${departmentId}`); }
  createMapping(request: CreateDepartmentApprovalMappingDto): Observable<DepartmentApprovalMappingDto> { return this.http.post<DepartmentApprovalMappingDto>(this.apiUrl, request); }
  updateMapping(id: number, request: UpdateDepartmentApprovalMappingDto): Observable<DepartmentApprovalMappingDto> { return this.http.put<DepartmentApprovalMappingDto>(`${this.apiUrl}/${id}`, request); }
  deleteMapping(id: number): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
