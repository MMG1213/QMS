
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import {
  DcrService,
  DCRCreateRequest,
  DCRCreateResponse
} from '../../../services/dcr.service';

import {
  MasterListService,
  DocumentType
} from '../../../services/master-list';

@Component({
  selector: 'app-raise-dcr',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './raise-dcr.html',
  styleUrl: './raise-dcr.css'
})
export class RaiseDcr implements OnInit {

  // Document types loaded from the Master List API
  documentTypes: DocumentType[] = [];

  // Form fields
  requestDate: string = this.getTodayDate();
  standard: string = '';
  requestType: string = '';
  documentTypeName: string = '';

  documentId: number | null = null;
  documentTitle: string = '';
  documentNo: string = '';

  descriptionBeforechange: string = '';
  descriptionAfterchange: string = '';
  reasonforchange: string = '';

  // Loading and submission states
  loadingDocumentTypes: boolean = false;
  submitting: boolean = false;

  // Messages
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private dcrService: DcrService,
    private masterListService: MasterListService
  ) {}

  ngOnInit(): void {
    this.loadDocumentTypes();
  }

  // Get today's date in YYYY-MM-DD format
  getTodayDate(): string {
    const today = new Date();

    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  // Load active document types from the backend
  loadDocumentTypes(): void {
    this.loadingDocumentTypes = true;
    this.errorMessage = '';

    this.masterListService.getDocumentTypes().subscribe({
      next: (response: DocumentType[]) => {

        this.documentTypes = response.filter(
          (type: DocumentType) => type.isActive
        );

        this.loadingDocumentTypes = false;
      },

      error: (error: HttpErrorResponse) => {
        console.error('Error loading document types:', error);

        this.errorMessage = 'Unable to load document types. Please try again.';

        this.loadingDocumentTypes = false;
      }
    });
  }

  // Check whether the selected request type needs an existing document
  get isExistingDocumentRequest(): boolean {
    return this.requestType === 'Revision' ||
           this.requestType === 'Obsolete';
  }

  // Submit DCR
  submitDCR(): void {

    this.successMessage = '';
    this.errorMessage = '';

    // Basic validation
    if (!this.requestDate) {
      this.errorMessage = 'Please select the request date.';
      return;
    }

    if (!this.standard.trim()) {
      this.errorMessage = 'Please select a standard.';
      return;
    }

    if (!this.requestType) {
      this.errorMessage = 'Please select a request type.';
      return;
    }

    if (!this.documentTypeName) {
      this.errorMessage = 'Please select a document type.';
      return;
    }

    if (
      this.isExistingDocumentRequest &&
      (!this.documentId || this.documentId <= 0)
    ) {
      this.errorMessage = 'Please enter a valid existing Document ID.';
      return;
    }

    // Prepare request payload
    const request: DCRCreateRequest = {
      requestDate: this.requestDate,
      standard: this.standard,
      requestType: this.requestType,
      documentTypeName: this.documentTypeName,

      documentId: this.isExistingDocumentRequest
        ? this.documentId
        : null,

      documentTitle: this.documentTitle.trim() || null,
      documentNo: this.documentNo.trim() || null,

      descriptionBeforechange:
        this.descriptionBeforechange.trim() || null,

      descriptionAfterchange:
        this.descriptionAfterchange.trim() || null,

      reasonforchange:
        this.reasonforchange.trim() || null
    };

    this.submitting = true;

    // Send request to backend
    this.dcrService.createDCR(request).subscribe({

      next: (response: DCRCreateResponse) => {

        console.log('DCR submission response:', response);

        this.successMessage = 'DCR submitted successfully!';
        this.errorMessage = '';

        this.resetForm();

        this.submitting = false;
      },

      error: (error: HttpErrorResponse) => {

        console.error('DCR submission error:', error);

        const errorBody = error.error as {
          message?: string;
          title?: string;
        } | null;

        this.errorMessage =
          errorBody?.message ||
          errorBody?.title ||
          'Failed to submit DCR. Please try again.';

        this.submitting = false;
      }
    });
  }

  // Reset form fields
  resetForm(): void {

    this.requestDate = this.getTodayDate();
    this.standard = '';
    this.requestType = '';
    this.documentTypeName = '';

    this.documentId = null;
    this.documentTitle = '';
    this.documentNo = '';

    this.descriptionBeforechange = '';
    this.descriptionAfterchange = '';
    this.reasonforchange = '';

    this.successMessage = '';
    this.errorMessage = '';
  }
}
