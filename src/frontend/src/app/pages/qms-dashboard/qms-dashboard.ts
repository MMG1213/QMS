
import {
  Component,
  HostListener,
  OnInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import {
  MasterListService,
  DocumentType,
  Department,
  MasterListDocument,
  DocumentRevision,
  UpdateMasterListDocument,
  ML01Entry,
  ML01EntryRequest,
  ML01Revision
} from '../../services/master-list';

import {
  MasterDataService,
  MasterDataTable,
  MasterDataRow
} from '../../services/master-data.service';

import { SigninService } from '../../services/signin.service';
import { DocumentApprovalMatrixService, DocumentApprovalMatrix, SaveDocumentApprovalMatrixRequest } from '../../services/document-approval-matrix.service';
import { MasterListExportService } from '../../services/master-list-export';

// --------------------------------------------------
// ML01 DRAFT INTERFACES
// --------------------------------------------------

interface ML01RevisionDraft {
  id?: number;
  revisionNo: number;
  pageCount: number | null;
  revisionDate: string;
}

interface ML01EntryDraft {
  id: number;
  description: string;
  documentNo: string;
  revisions: ML01RevisionDraft[];
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class QmsDashboardComponent implements OnInit {

  private readonly SigninService = inject(SigninService);
  private readonly router = inject(Router);
  private readonly masterDataService = inject(MasterDataService);
  private readonly approvalMatrixService = inject(DocumentApprovalMatrixService);

  private readonly masterListExportService =
    inject(MasterListExportService);

  constructor(
    private readonly masterListService: MasterListService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  // --------------------------------------------------
  // DASHBOARD
  // --------------------------------------------------

  tabs: string[] = [
    'MasterList',
    'DCR',
    'Internal Audit',
    'QPI',
    'MRM'
  ];

  activeTab = 'MasterList';
  profileMenuOpen = false;

  private readonly currentUser = this.SigninService.getCurrentUser();

  userName =
    this.currentUser?.fullName ||
    this.currentUser?.employeeId ||
    'QMS User';

  userRole = this.currentUser?.role || 'User';

  isQmsAdmin = this.userRole === 'QMS Admin';

  get userInitial(): string {
    return this.userName.trim().charAt(0).toUpperCase() || 'U';
  }

  // --------------------------------------------------
  // EXCEL EXPORT
  // --------------------------------------------------

  isExporting = false;
  exportError = '';
  exportSuccessMessage = '';

  exportToExcel(): void {

    if (this.isExporting) {
      return;
    }

    this.isExporting = true;
    this.exportError = '';
    this.exportSuccessMessage = '';

    this.masterListExportService.exportMasterList(
      this.selectedDocumentTypeId,
      this.selectedDepartmentId
    ).subscribe({

      next: (blob: Blob) => {

        if (!blob || blob.size === 0) {
          this.exportError = 'The exported Excel file is empty.';
          this.isExporting = false;
          this.cdr.markForCheck();
          return;
        }

        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');

        link.href = url;
        link.download = 'MasterList.xlsx';

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        window.URL.revokeObjectURL(url);

        this.isExporting = false;
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Excel export failed:', error);

        this.exportError =
          'Unable to export Master List. Please check the backend API and try again.';

        this.isExporting = false;
        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // MASTER LIST FILTERS AND DOCUMENT DATA
  // --------------------------------------------------

  documentTypes: DocumentType[] = [];
  departments: Department[] = [];
  documents: MasterListDocument[] = [];

  selectedDocumentTypeId: number | null = null;
  selectedDepartmentId: number | null = null;
  searchText = '';

  isLoadingFilters = false;
  isLoadingDocuments = false;
  isLoadingMasterData = false;

  filterError = '';
  documentError = '';
  masterDataError = '';

  // --------------------------------------------------
  // ML01 - MASTER LIST OF MASTER LISTS
  // --------------------------------------------------

  ml01Entries: ML01Entry[] = [];

  isLoadingML01 = false;
  ml01Error = '';

  editingML01Id: number | null = null;
  ml01EditDraft: ML01EntryDraft | null = null;

  isAddingML01 = false;
  ml01AddDraft: ML01EntryDraft | null = null;

  isSavingML01 = false;

  ml01SaveMessage = '';
  ml01SaveError = '';

  get isML01Selected(): boolean {
    return this.selectedDocumentTypeId === 10;
  }

  // Generate columns dynamically from revision 00 to the highest revision.
  get ml01RevisionColumns(): number[] {

    const revisionNumbers = this.ml01Entries.flatMap(entry =>
      entry.revisions.map(revision => revision.revisionNo)
    );

    if (this.ml01EditDraft) {
      revisionNumbers.push(
        ...this.ml01EditDraft.revisions.map(
          revision => revision.revisionNo
        )
      );
    }

    if (this.ml01AddDraft) {
      revisionNumbers.push(
        ...this.ml01AddDraft.revisions.map(
          revision => revision.revisionNo
        )
      );
    }

    if (revisionNumbers.length === 0) {
      return [0];
    }

    const highestRevision = Math.max(0, ...revisionNumbers);

    return Array.from(
      { length: highestRevision + 1 },
      (_, index) => index
    );
  }

  // --------------------------------------------------
  // ML02–ML06 DOCUMENT EDITING
  // --------------------------------------------------

  editingDocumentId: number | null = null;
  editDraft: MasterListDocument | null = null;

  isSavingDocument = false;
  saveMessage = '';
  saveError = '';

  // --------------------------------------------------
  // ML07–ML10 SPECIAL TABLE DATA
  // --------------------------------------------------

  masterDataRows: MasterDataRow[] = [];

  editingMasterDataId: number | null = null;
  specialEditDraft: MasterDataRow | null = null;

  specialSaveMessage = '';
  specialSaveError = '';

  isAddingMasterData = false;
  isSavingMasterData = false;
  isDeletingMasterData = false;

  specialAddDraft: MasterDataRow | null = null;
  deletingMasterDataId: number | null = null;

  // --------------------------------------------------
  // SPECIAL DOCUMENT TYPE CONFIGURATION
  // --------------------------------------------------

  readonly specialDocumentTypes: Record<number, {
    table: MasterDataTable;
    title: string;
    columns: { key: string; label: string }[];
  }> = {

    6: {
      table: 'machines',
      title: 'Machines / Equipment',
      columns: [
        { key: 'machineName', label: 'Machine Name' },
        { key: 'machineCategory', label: 'Machine Category' },
        { key: 'machineCodeNum', label: 'Machine Code No.' },
        { key: 'machineSlNum', label: 'Machine Serial No.' },
        { key: 'make', label: 'Make' },
        { key: 'capacityOrSpecification', label: 'Capacity / Specification' },
        { key: 'commissionedOn', label: 'Commissioned On' },
        { key: 'loadInHP', label: 'Load in HP' },
        { key: 'location', label: 'Location' }
      ]
    },

    7: {
      table: 'standards',
      title: 'Standards',
      columns: [
        { key: 'description', label: 'Description' },
        { key: 'pages', label: 'Pages' },
        { key: 'revision', label: 'Revision' },
        { key: 'remarks', label: 'Remarks' }
      ]
    },

    8: {
      table: 'instruments',
      title: 'Testing and Measuring Instruments',
      columns: [
        { key: 'instrumentDescription', label: 'Instrument Description' },
        { key: 'instrumentCodeNum', label: 'Instrument Code No.' },
        { key: 'instrumentSlNum', label: 'Instrument Serial No.' },
        { key: 'instrumentRange', label: 'Range' },
        { key: 'make', label: 'Make' },
        { key: 'location', label: 'Location' },
        { key: 'calibrationFrequency', label: 'Calibration Frequency' },
        { key: 'remarks', label: 'Remarks' }
      ]
    },

    9: {
      table: 'qpi',
      title: 'Quality Performance Index (QPI)',
      columns: [
        { key: 'processOrDept', label: 'Process / Department' },
        { key: 'measurementTitle', label: 'Measurement Title' },
        { key: 'qualityPerformanceIndexFormula', label: 'QPI Formula' },
        { key: 'targets', label: 'Targets' },
        { key: 'responsibilities', label: 'Responsibilities' },
        { key: 'departmentReview', label: 'Department Review' },
        { key: 'qpiReview', label: 'QPI Review' }
      ]
    }
  };

  // --------------------------------------------------
  // SELECTED SPECIAL TABLE CONFIGURATION
  // --------------------------------------------------

  get selectedSpecialType(): {
    table: MasterDataTable;
    title: string;
    columns: { key: string; label: string }[];
  } | null {

    if (this.selectedDocumentTypeId === null) {
      return null;
    }

    return this.specialDocumentTypes[this.selectedDocumentTypeId] ?? null;
  }

  // --------------------------------------------------
  // ML02–ML06 REVISION COLUMNS
  // --------------------------------------------------

  get revisionColumns(): number[] {

    const revisionNumbers = this.documents.flatMap(document =>
      document.revisions.map(revision => revision.revisionNumber)
    );

    if (revisionNumbers.length === 0) {
      return [];
    }

    const highestRevision = Math.max(...revisionNumbers);
    const lowestRevision = Math.min(0, ...revisionNumbers);

    return Array.from(
      { length: highestRevision - lowestRevision + 1 },
      (_, index) => lowestRevision + index
    );
  }

  // --------------------------------------------------
  // INITIALIZATION
  // --------------------------------------------------

  ngOnInit(): void {
    this.loadFilters();
    this.loadDocuments();
    this.loadApprovalMatrixDocumentTypes();
  }

  // --------------------------------------------------
  // LOAD FILTERS
  // --------------------------------------------------

  loadFilters(): void {

    this.isLoadingFilters = true;
    this.filterError = '';

    let typesLoaded = false;
    let departmentsLoaded = false;

    const finishLoading = (): void => {

      if (typesLoaded && departmentsLoaded) {
        this.isLoadingFilters = false;
      }

      this.cdr.markForCheck();
    };

    this.masterListService.getDocumentTypes().subscribe({

      next: types => {
        this.documentTypes = types;
        typesLoaded = true;
        finishLoading();
      },

      error: error => {
        console.error('Failed to load document types:', error);
        this.filterError = 'Could not load document types.';
        typesLoaded = true;
        finishLoading();
      }
    });

    this.masterListService.getDepartments().subscribe({

      next: departments => {
        this.departments = departments;
        departmentsLoaded = true;
        finishLoading();
      },

      error: error => {
        console.error('Failed to load departments:', error);
        this.filterError = 'Could not load departments.';
        departmentsLoaded = true;
        finishLoading();
      }
    });
  }

  // --------------------------------------------------
  // LOAD DOCUMENTS / ML01
  // --------------------------------------------------

  loadDocuments(): void {

    // ML01 has its own API and must not use the ML02–ML06 endpoint.
    if (this.isML01Selected) {

      this.documents = [];
      this.isLoadingDocuments = false;
      this.documentError = '';

      this.loadML01Entries();

      this.cdr.markForCheck();
      return;
    }

    // ML07–ML10 use their own special-table APIs.
    if (this.selectedSpecialType) {

      this.documents = [];
      this.isLoadingDocuments = false;
      this.documentError = '';

      this.isLoadingML01 = false;
      this.ml01Entries = [];
      this.ml01Error = '';

      this.cdr.markForCheck();
      return;
    }

    // Normal ML02–ML06 documents.
    this.ml01Entries = [];
    this.isLoadingML01 = false;
    this.ml01Error = '';

    this.isLoadingDocuments = true;
    this.documentError = '';

    this.cdr.markForCheck();

    this.masterListService.getDocuments(
      this.selectedDocumentTypeId ?? undefined,
      this.selectedDepartmentId ?? undefined,
      this.searchText
    ).subscribe({

      next: documents => {

        this.documents = documents;
        this.isLoadingDocuments = false;

        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to load documents:', error);

        this.documents = [];

        this.documentError =
          'Could not load documents. Check that the backend API is running.';

        this.isLoadingDocuments = false;

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML01 LOAD
  // --------------------------------------------------

  loadML01Entries(): void {

    this.isLoadingML01 = true;
    this.ml01Error = '';

    this.documents = [];

    this.cdr.markForCheck();

    this.masterListService.getML01Entries().subscribe({

      next: entries => {

        // Ignore a late response if the user selected another type.
        if (!this.isML01Selected) {
          return;
        }

        this.ml01Entries = entries ?? [];
        this.isLoadingML01 = false;

        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to load ML01 entries:', error);

        if (!this.isML01Selected) {
          return;
        }

        this.ml01Entries = [];
        this.isLoadingML01 = false;

        this.ml01Error =
          error?.error?.message ??
          'Could not load ML01 records. Please check the backend API.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML01 GET REVISION
  // --------------------------------------------------

  getML01Revision(
    entry: ML01Entry | ML01EntryDraft,
    revisionNo: number
  ): ML01Revision | ML01RevisionDraft | undefined {

    return entry.revisions.find(
      revision => revision.revisionNo === revisionNo
    );
  }

  // --------------------------------------------------
  // ML01 CREATE
  // --------------------------------------------------

  startML01Add(): void {

    if (
      !this.isQmsAdmin ||
      !this.isML01Selected ||
      this.isSavingML01 ||
      this.isAddingML01 ||
      this.editingML01Id !== null ||
      this.editingDocumentId !== null ||
      this.editingMasterDataId !== null
    ) {
      return;
    }

    this.ml01AddDraft = {
      id: 0,
      description: '',
      documentNo: '',
      revisions: [
        {
          revisionNo: 0,
          pageCount: null,
          revisionDate: ''
        }
      ]
    };

    this.isAddingML01 = true;
    this.ml01SaveMessage = '';
    this.ml01SaveError = '';

    this.cdr.markForCheck();
  }

  cancelML01Add(): void {

    if (this.isSavingML01) {
      return;
    }

    this.isAddingML01 = false;
    this.ml01AddDraft = null;
    this.ml01SaveError = '';

    this.cdr.markForCheck();
  }

  saveML01Add(): void {

    const draft = this.ml01AddDraft;

    if (
      !this.isQmsAdmin ||
      !draft ||
      !this.isML01Selected ||
      this.isSavingML01
    ) {
      return;
    }

    const validationError = this.validateML01Draft(draft);

    if (validationError) {
      this.ml01SaveError = validationError;
      return;
    }

    const request = this.buildML01Request(draft);

    this.isSavingML01 = true;
    this.ml01SaveError = '';
    this.ml01SaveMessage = '';

    this.masterListService.createML01Entry(request).subscribe({

      next: () => {

        this.isSavingML01 = false;
        this.isAddingML01 = false;
        this.ml01AddDraft = null;

        this.ml01SaveMessage = 'ML01 record added successfully.';

        this.loadML01Entries();
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to create ML01 entry:', error);

        this.isSavingML01 = false;

        this.ml01SaveError =
          error?.error?.message ??
          'Could not add the ML01 record. Please try again.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML01 EDIT
  // --------------------------------------------------

  startML01Edit(entry: ML01Entry): void {

    if (
      !this.isQmsAdmin ||
      !this.isML01Selected ||
      this.isSavingML01 ||
      this.isAddingML01 ||
      this.editingML01Id !== null ||
      this.editingDocumentId !== null ||
      this.editingMasterDataId !== null
    ) {
      return;
    }

    this.editingML01Id = entry.id;

    this.ml01EditDraft = {
      id: entry.id,
      description: entry.description,
      documentNo: entry.documentNo,
      revisions: (entry.revisions ?? []).map(revision => ({
        id: revision.id,
        revisionNo: revision.revisionNo,
        pageCount: revision.pageCount,
        revisionDate: revision.revisionDate
          ? String(revision.revisionDate).substring(0, 10)
          : ''
      }))
    };

    this.ml01SaveMessage = '';
    this.ml01SaveError = '';

    this.cdr.markForCheck();
  }

  cancelML01Edit(): void {

    if (this.isSavingML01) {
      return;
    }

    this.editingML01Id = null;
    this.ml01EditDraft = null;
    this.ml01SaveError = '';

    this.cdr.markForCheck();
  }

  saveML01Edit(): void {

    const draft = this.ml01EditDraft;
    const id = this.editingML01Id;

    if (
      !this.isQmsAdmin ||
      !draft ||
      id === null ||
      !this.isML01Selected ||
      this.isSavingML01
    ) {
      return;
    }

    const validationError = this.validateML01Draft(draft);

    if (validationError) {
      this.ml01SaveError = validationError;
      return;
    }

    const request = this.buildML01Request(draft);

    this.isSavingML01 = true;
    this.ml01SaveError = '';
    this.ml01SaveMessage = '';

    this.masterListService.updateML01Entry(id, request).subscribe({

      next: () => {

        this.isSavingML01 = false;
        this.editingML01Id = null;
        this.ml01EditDraft = null;

        this.ml01SaveMessage = 'ML01 record updated successfully.';

        this.loadML01Entries();
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to update ML01 entry:', error);

        this.isSavingML01 = false;

        this.ml01SaveError =
          error?.error?.message ??
          'Could not update the ML01 record. Please try again.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML01 ADD REVISION
  // --------------------------------------------------

  addML01Revision(): void {

    const draft = this.ml01EditDraft;

    if (
      !this.isQmsAdmin ||
      !draft ||
      this.isSavingML01
    ) {
      return;
    }

    const highestRevision = draft.revisions.length > 0
      ? Math.max(...draft.revisions.map(r => r.revisionNo))
      : -1;

    const nextRevision = highestRevision + 1;

    draft.revisions.push({
      revisionNo: nextRevision,
      pageCount: null,
      revisionDate: ''
    });

    this.ml01SaveError = '';

    this.cdr.markForCheck();
  }

  // --------------------------------------------------
  // ML01 VALIDATION AND REQUEST MAPPING
  // --------------------------------------------------

  private validateML01Draft(draft: ML01EntryDraft): string | null {

    if (!draft.description.trim()) {
      return 'Description is required.';
    }

    if (!draft.documentNo.trim()) {
      return 'Document number is required.';
    }

    if (draft.revisions.length === 0) {
      return 'At least one revision is required.';
    }

    const revisionNumbers = draft.revisions.map(r => r.revisionNo);

    if (new Set(revisionNumbers).size !== revisionNumbers.length) {
      return 'Duplicate revision numbers are not allowed.';
    }

    for (const revision of draft.revisions) {

      if (
        !Number.isInteger(revision.pageCount) ||
        revision.pageCount === null ||
        revision.pageCount < 1
      ) {
        return `Enter a valid page count for revision ${revision.revisionNo}.`;
      }

      if (!revision.revisionDate) {
        return `Select a date for revision ${revision.revisionNo}.`;
      }
    }

    return null;
  }

  private buildML01Request(
    draft: ML01EntryDraft
  ): ML01EntryRequest {

    return {
      description: draft.description.trim(),
      documentNo: draft.documentNo.trim(),

      revisions: [...draft.revisions]
        .sort((a, b) => a.revisionNo - b.revisionNo)
        .map(revision => ({
          revisionNo: revision.revisionNo,
          pageCount: Number(revision.pageCount),
          revisionDate: revision.revisionDate.substring(0, 10)
        }))
    };
  }

  // --------------------------------------------------
  // LOAD ML07–ML10 SPECIAL TABLES
  // --------------------------------------------------

  loadMasterData(): void {

    const config = this.selectedSpecialType;

    this.masterDataRows = [];
    this.masterDataError = '';

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.isAddingMasterData = false;
    this.specialAddDraft = null;

    if (!config) {

      this.isLoadingMasterData = false;
      this.cdr.markForCheck();
      return;
    }

    this.isLoadingMasterData = true;
    this.cdr.markForCheck();

    this.masterDataService.getRows(config.table).subscribe({

      next: rows => {

        if (this.selectedSpecialType?.table !== config.table) {
          return;
        }

        this.masterDataRows = rows;
        this.isLoadingMasterData = false;

        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to load master data:', error);

        if (this.selectedSpecialType?.table !== config.table) {
          return;
        }

        this.masterDataRows = [];

        this.masterDataError =
          'Could not load the selected master data. Please try again.';

        this.isLoadingMasterData = false;

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML07–ML10 ADD RECORD
  // --------------------------------------------------

  startSpecialAdd(): void {

    const config = this.selectedSpecialType;

    if (
      !this.isQmsAdmin ||
      !config ||
      this.editingMasterDataId !== null ||
      this.editingDocumentId !== null ||
      this.isSavingDocument ||
      this.isSavingMasterData ||
      this.isDeletingMasterData ||
      this.isAddingMasterData
    ) {
      return;
    }

    const draft: MasterDataRow = { id: 0 };

    for (const column of config.columns) {
      draft[column.key] = null;
    }

    this.specialAddDraft = draft;
    this.isAddingMasterData = true;

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  cancelSpecialAdd(): void {

    if (this.isSavingMasterData) {
      return;
    }

    this.isAddingMasterData = false;
    this.specialAddDraft = null;
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  updateSpecialAddField(key: string, value: any): void {

    if (!this.specialAddDraft) {
      return;
    }

    if (key === 'pages' || key === 'loadInHP') {
      this.specialAddDraft[key] =
        value === '' ? null : Number(value);
    } else {
      this.specialAddDraft[key] = value;
    }
  }

  saveSpecialAdd(): void {

    const config = this.selectedSpecialType;
    const draft = this.specialAddDraft;

    if (
      !this.isQmsAdmin ||
      !config ||
      !draft ||
      this.isSavingMasterData
    ) {
      return;
    }

    const payload: Partial<MasterDataRow> = {};

    for (const column of config.columns) {

      const value = draft[column.key];

      if (
        value !== null &&
        value !== undefined &&
        String(value).trim() !== ''
      ) {
        payload[column.key] = value;
      }
    }

    if (Object.keys(payload).length === 0) {
      this.specialSaveError = 'Please enter at least one field.';
      return;
    }

    this.isSavingMasterData = true;
    this.specialSaveError = '';
    this.specialSaveMessage = '';

    this.masterDataService.createRow(
      config.table,
      payload
    ).subscribe({

      next: () => {

        this.isSavingMasterData = false;
        this.isAddingMasterData = false;
        this.specialAddDraft = null;

        this.specialSaveMessage = 'Record added successfully.';

        this.loadMasterData();
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to add master data:', error);

        this.isSavingMasterData = false;

        this.specialSaveError =
          error?.error?.message ??
          'Could not add the record. Please try again.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // ML07–ML10 DELETE RECORD
  // --------------------------------------------------

  deleteSpecialRecord(row: MasterDataRow): void {

    const config = this.selectedSpecialType;

    if (
      !this.isQmsAdmin ||
      !config ||
      this.isDeletingMasterData ||
      this.isSavingMasterData ||
      this.editingMasterDataId !== null ||
      this.isAddingMasterData
    ) {
      return;
    }

    const confirmed = window.confirm(
      'Are you sure you want to delete this record? This action cannot be undone.'
    );

    if (!confirmed) {
      return;
    }

    this.isDeletingMasterData = true;
    this.deletingMasterDataId = row.id;

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.masterDataService.deleteRow(
      config.table,
      row.id
    ).subscribe({

      next: () => {

        this.isDeletingMasterData = false;
        this.deletingMasterDataId = null;

        this.specialSaveMessage = 'Record deleted successfully.';

        this.loadMasterData();
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to delete master data:', error);

        this.isDeletingMasterData = false;
        this.deletingMasterDataId = null;

        this.specialSaveError =
          error?.error?.message ??
          'Could not delete the record. Please try again.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // DOCUMENT TYPE FILTER
  // --------------------------------------------------

  onDocumentTypeChange(value: string | number): void {

    this.selectedDocumentTypeId =
      value === '' ? null : Number(value);

    // Clear ML02–ML06 edit state.
    this.editingDocumentId = null;
    this.editDraft = null;
    this.saveMessage = '';
    this.saveError = '';

    // Clear ML01 state.
    this.editingML01Id = null;
    this.ml01EditDraft = null;
    this.isAddingML01 = false;
    this.ml01AddDraft = null;
    this.ml01SaveMessage = '';
    this.ml01SaveError = '';

    // Clear special table state.
    this.editingMasterDataId = null;
    this.specialEditDraft = null;
    this.isAddingMasterData = false;
    this.specialAddDraft = null;
    this.specialSaveMessage = '';
    this.specialSaveError = '';

    // ML01 and ML07–ML10 are not department-specific in this UI.
    if (this.selectedSpecialType || this.isML01Selected) {
      this.selectedDepartmentId = null;
    }

    this.loadDocuments();
    this.loadMasterData();

    // Keep the compact approval box in sync with the selected Document Type.
    if (this.selectedDocumentTypeId !== null) {
      this.approvalMatrixSelectedDocumentTypeId = this.selectedDocumentTypeId;
      this.isEditingApprovalMatrix = false;
      this.approvalMatrixEditBackup = null;
      this.loadApprovalMatrix();
    } else if (this.approvalMatrixDocumentTypes.length > 0) {
      // All Document Types: display the default matrix (first available type).
      this.approvalMatrixSelectedDocumentTypeId =
        this.approvalMatrixDocumentTypes[0].documentTypeId;
      this.isEditingApprovalMatrix = false;
      this.approvalMatrixEditBackup = null;
      this.loadApprovalMatrix();
    }
  }

  // --------------------------------------------------
  // DEPARTMENT FILTER
  // --------------------------------------------------

  onDepartmentChange(value: string | number): void {

    if (this.selectedSpecialType || this.isML01Selected) {
      return;
    }

    this.selectedDepartmentId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  // --------------------------------------------------
  // SEARCH
  // --------------------------------------------------

  onSearch(): void {
    this.loadDocuments();
  }

  // --------------------------------------------------
  // REFRESH
  // --------------------------------------------------

  refreshCurrentTable(): void {

    this.loadDocuments();
    this.loadMasterData();
  }

  // --------------------------------------------------
  // CLEAR FILTERS
  // --------------------------------------------------

  clearFilters(): void {

    this.selectedDocumentTypeId = null;
    this.selectedDepartmentId = null;
    this.searchText = '';

    this.editingDocumentId = null;
    this.editDraft = null;

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.isAddingMasterData = false;
    this.specialAddDraft = null;

    this.editingML01Id = null;
    this.ml01EditDraft = null;
    this.isAddingML01 = false;
    this.ml01AddDraft = null;

    this.saveMessage = '';
    this.saveError = '';

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.ml01SaveMessage = '';
    this.ml01SaveError = '';

    this.loadDocuments();
    this.loadMasterData();
  }

  // --------------------------------------------------
  // DASHBOARD NAVIGATION
  // --------------------------------------------------

  selectTab(tab: string): void {

    this.activeTab = tab;
    this.profileMenuOpen = false;

  }

  selectDocumentType(type: DocumentType): void {

    this.onDocumentTypeChange(type.documentTypeId);
  }

  // --------------------------------------------------
  // ML07–ML10 SPECIAL TABLE EDITING
  // --------------------------------------------------

  startSpecialEdit(row: MasterDataRow): void {

    if (
      !this.isQmsAdmin ||
      this.editingMasterDataId !== null ||
      this.editingDocumentId !== null ||
      this.isSavingDocument ||
      this.isSavingMasterData ||
      this.isDeletingMasterData ||
      this.isAddingMasterData
    ) {
      return;
    }

    this.editingMasterDataId = row.id;

    this.specialEditDraft = {
      ...row
    };

    if (this.specialEditDraft['commissionedOn']) {

      this.specialEditDraft['commissionedOn'] =
        String(this.specialEditDraft['commissionedOn'])
          .substring(0, 10);
    }

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  cancelSpecialEdit(): void {

    if (this.isSavingMasterData) {
      return;
    }

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  getSpecialInputType(key: string): string {

    if (key === 'commissionedOn') {
      return 'date';
    }

    if (key === 'pages' || key === 'loadInHP') {
      return 'number';
    }

    return 'text';
  }

  updateSpecialDraftField(key: string, value: any): void {

    if (!this.specialEditDraft) {
      return;
    }

    if (key === 'pages' || key === 'loadInHP') {
      this.specialEditDraft[key] =
        value === '' ? null : Number(value);
    } else {
      this.specialEditDraft[key] = value;
    }
  }

  saveSpecialEdit(): void {

    const config = this.selectedSpecialType;
    const draft = this.specialEditDraft;
    const id = this.editingMasterDataId;

    if (
      !this.isQmsAdmin ||
      !config ||
      !draft ||
      id === null ||
      this.isSavingMasterData
    ) {
      return;
    }

    const payload: Partial<MasterDataRow> = {};

    for (const column of config.columns) {

      const value = draft[column.key];

      if (
        value !== null &&
        value !== undefined &&
        String(value).trim() !== ''
      ) {
        payload[column.key] = value;
      }
    }

    if (Object.keys(payload).length === 0) {
      this.specialSaveError = 'Please enter at least one field.';
      return;
    }

    this.isSavingMasterData = true;

    this.specialSaveError = '';
    this.specialSaveMessage = '';

    this.masterDataService.updateRow(
      config.table,
      id,
      payload
    ).subscribe({

      next: () => {

        this.isSavingMasterData = false;

        this.editingMasterDataId = null;
        this.specialEditDraft = null;

        this.specialSaveMessage = 'Record updated successfully.';

        this.loadMasterData();
        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to update master data:', error);

        this.isSavingMasterData = false;

        this.specialSaveError =
          error?.error?.message ??
          'Could not update the record. Please try again.';

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // GET DOCUMENT REVISION
  // --------------------------------------------------

  getRevision(
    document: MasterListDocument,
    revisionNumber: number
  ): DocumentRevision | undefined {

    return document.revisions.find(
      revision => revision.revisionNumber === revisionNumber
    );
  }

  // --------------------------------------------------
  // DOCUMENT TYPE NAME
  // --------------------------------------------------

  getDocumentTypeName(documentTypeId: number): string {

    return this.documentTypes.find(
      type => type.documentTypeId === documentTypeId
    )?.typeName ?? '—';
  }

  // --------------------------------------------------
  // REVISION FORMATTING
  // --------------------------------------------------

  formatRevision(revisionNumber: number): string {

    return `Rev ${String(revisionNumber).padStart(3, '0')}`;
  }

  // --------------------------------------------------
  // DATE FORMATTING
  // --------------------------------------------------

  formatDate(date: string | null | undefined): string {

    if (!date) {
      return '—';
    }

    const datePart = date.substring(0, 10);

    const [year, month, day] = datePart.split('-');

    if (!year || !month || !day) {
      return date;
    }

    return `${day}/${month}/${year}`;
  }

  // --------------------------------------------------
  // ML02–ML06 DOCUMENT EDITING
  // --------------------------------------------------

  startEdit(document: MasterListDocument): void {

    if (!this.isQmsAdmin) {
      return;
    }

    if (
      this.editingDocumentId !== null ||
      this.editingMasterDataId !== null ||
      this.isSavingDocument ||
      this.isSavingMasterData ||
      this.isAddingMasterData
    ) {
      return;
    }

    console.log('Original Document:', document);

    this.editingDocumentId = document.documentId;

    this.editDraft = {

      ...document,

      documentTypeId: Number(document.documentTypeId),
      departmentId: Number(document.departmentId),

      revisions: (document.revisions ?? []).map(revision => ({
        ...revision,
        revisionDate: revision.revisionDate
          ? String(revision.revisionDate).substring(0, 10)
          : ''
      })),

      obseleteDate: document.obseleteDate
        ? String(document.obseleteDate).substring(0, 10)
        : null
    };

    console.log('Edit Draft:', this.editDraft);

    this.cdr.detectChanges();
  }

  // --------------------------------------------------
  // CANCEL DOCUMENT EDITING
  // --------------------------------------------------

  cancelEdit(): void {

    if (this.isSavingDocument) {
      return;
    }

    this.editingDocumentId = null;
    this.editDraft = null;

    this.saveMessage = '';
    this.saveError = '';

    this.cdr.markForCheck();
  }

  // --------------------------------------------------
  // DOCUMENT STATUS CHANGE
  // --------------------------------------------------

  onEditStatusChange(status: string): void {

    if (!this.editDraft) {
      return;
    }

    this.editDraft.status = status;

    if (status === 'Active') {
      this.editDraft.obseleteDate = null;
    }

    this.saveError = '';
  }

  // --------------------------------------------------
  // SAVE ML02–ML06 DOCUMENT
  // --------------------------------------------------

  saveEdit(): void {

    const draft = this.editDraft;

    if (
      !this.isQmsAdmin ||
      !draft ||
      this.editingDocumentId === null ||
      this.isSavingDocument
    ) {
      return;
    }

    if (!draft.documentNo.trim()) {
      this.saveError = 'Document number is required.';
      return;
    }

    if (!draft.title.trim()) {
      this.saveError = 'Document title is required.';
      return;
    }

    if (!draft.documentTypeId || !draft.departmentId) {
      this.saveError = 'Select a document type and department.';
      return;
    }

    if (
      draft.status !== 'Active' &&
      draft.status !== 'Obsolete'
    ) {
      this.saveError = 'Select a valid document status.';
      return;
    }

    if (
      draft.status === 'Obsolete' &&
      !draft.obseleteDate
    ) {
      this.saveError =
        'Select an obsolete date for obsolete documents.';
      return;
    }

    if (
      draft.revisions.some(revision =>
        !Number.isInteger(Number(revision.pageCount)) ||
        Number(revision.pageCount) < 1 ||
        !revision.revisionDate
      )
    ) {
      this.saveError =
        'Every revision needs a valid page count and date.';
      return;
    }

    const request: UpdateMasterListDocument = {

      documentNo: draft.documentNo.trim(),

      title: draft.title.trim(),

      documentTypeId: Number(draft.documentTypeId),

      departmentId: Number(draft.departmentId),

      retentionPeriod: draft.retentionPeriod?.trim() || null,

      status: draft.status,

      obseleteDate:
        draft.status === 'Obsolete'
          ? draft.obseleteDate?.slice(0, 10) ?? null
          : null,

      revisions: draft.revisions.map(revision => ({
        revisionNumber: revision.revisionNumber,
        pageCount: Number(revision.pageCount),
        revisionDate: revision.revisionDate.slice(0, 10)
      }))
    };

    this.isSavingDocument = true;
    this.saveError = '';
    this.saveMessage = '';

    this.masterListService
      .updateDocument(draft.documentId, request)
      .subscribe({

        next: response => {

          this.isSavingDocument = false;

          this.editingDocumentId = null;
          this.editDraft = null;

          this.saveMessage =
            response?.message ?? 'Document updated successfully.';

          this.loadDocuments();

          this.cdr.markForCheck();
        },

        error: error => {

          console.error(
            'Failed to update Master List document:',
            error
          );

          this.isSavingDocument = false;

          this.saveError =
            error?.error?.message ??
            'Could not save changes. Check the entered values and try again.';

          this.cdr.markForCheck();
        }
      });
  }

  // --------------------------------------------------
  // PROFILE MENU
  // --------------------------------------------------

  toggleProfileMenu(): void {
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  closeProfileMenu(): void {
    this.profileMenuOpen = false;
  }

  openProfile(): void {
    this.closeProfileMenu();
  }

  // --------------------------------------------------
  // SIGNUP REQUESTS
  // --------------------------------------------------

  openSignupRequests(): void {
    void this.router.navigate(['/qms/admin/signup-requests']);
  }

  // --------------------------------------------------
  // KEYBOARD ESCAPE
  // --------------------------------------------------

  @HostListener('document:keydown.escape')
  onEscape(): void {

    this.closeProfileMenu();

    if (this.isAddingML01 && !this.isSavingML01) {
      this.cancelML01Add();
      return;
    }

    if (this.editingML01Id !== null && !this.isSavingML01) {
      this.cancelML01Edit();
      return;
    }

    if (this.isAddingMasterData && !this.isSavingMasterData) {
      this.cancelSpecialAdd();
      return;
    }

    if (this.editingMasterDataId !== null) {
      this.cancelSpecialEdit();
      return;
    }

    if (
      this.editingDocumentId !== null &&
      !this.isSavingDocument
    ) {
      this.cancelEdit();
    }
  }

  // --------------------------------------------------
  // DOCUMENT APPROVAL MATRIX
  // --------------------------------------------------

  approvalMatrixDocumentTypes: DocumentType[] = [];
  approvalMatrixSelectedDocumentTypeId: number | null = null;
  approvalMatrix: DocumentApprovalMatrix = this.createEmptyApprovalMatrix(0);

  isLoadingApprovalMatrixTypes = false;
  isLoadingApprovalMatrix = false;
  isSavingApprovalMatrix = false;

  approvalMatrixLoadError = '';
  approvalMatrixSaveError = '';
  approvalMatrixSaveMessage = '';
  isEditingApprovalMatrix = false;
  private approvalMatrixEditBackup: DocumentApprovalMatrix | null = null;

  get canEditApprovalMatrix(): boolean {
    return this.userRole === 'QMS Admin';
  }

  private createEmptyApprovalMatrix(documentTypeId: number): DocumentApprovalMatrix {
    return {
      documentTypeId,
      approvedBy: null,
      checkedBy: null,
      issuedBy: null,
      approvedDate: null,
      checkedDate: null,
      issuedDate: null
    };
  }

  loadApprovalMatrixDocumentTypes(): void {
    this.isLoadingApprovalMatrixTypes = true;
    this.approvalMatrixLoadError = '';

    this.masterListService.getDocumentTypes().subscribe({
      next: (types) => {
        this.approvalMatrixDocumentTypes = types ?? [];
        this.isLoadingApprovalMatrixTypes = false;

        if (this.approvalMatrixDocumentTypes.length > 0) {
          this.approvalMatrixSelectedDocumentTypeId =
            this.selectedDocumentTypeId ?? this.approvalMatrixDocumentTypes[0].documentTypeId;
          this.loadApprovalMatrix();
        }
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoadingApprovalMatrixTypes = false;
        this.approvalMatrixLoadError = error?.error?.message ??
          'Unable to load Document Types.';
        this.cdr.detectChanges();
      }
    });
  }

  onApprovalMatrixDocumentTypeChange(): void {
    this.isEditingApprovalMatrix = false;
    this.approvalMatrixEditBackup = null;
    if (this.approvalMatrixSelectedDocumentTypeId === null) {
      this.approvalMatrix = this.createEmptyApprovalMatrix(0);
      return;
    }
    this.loadApprovalMatrix();
  }

  loadApprovalMatrix(): void {
    const documentTypeId = this.approvalMatrixSelectedDocumentTypeId;
    if (documentTypeId === null) return;

    this.isLoadingApprovalMatrix = true;
    this.approvalMatrixLoadError = '';
    this.approvalMatrixSaveError = '';
    this.approvalMatrixSaveMessage = '';

    this.approvalMatrixService.getMatrix(documentTypeId).subscribe({
      next: (response) => {
        this.approvalMatrix = {
          documentTypeId,
          approvedBy: response.approvedBy ?? null,
          checkedBy: response.checkedBy ?? null,
          issuedBy: response.issuedBy ?? null,
          approvedDate: this.approvalMatrixDateInput(response.approvedDate),
          checkedDate: this.approvalMatrixDateInput(response.checkedDate),
          issuedDate: this.approvalMatrixDateInput(response.issuedDate)
        };
        this.isLoadingApprovalMatrix = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoadingApprovalMatrix = false;
        this.approvalMatrixLoadError = error?.error?.message ??
          'Unable to load Approval Matrix data.';
        this.cdr.detectChanges();
      }
    });
  }

  startApprovalMatrixEdit(): void {
    if (!this.canEditApprovalMatrix || this.isLoadingApprovalMatrix ||
        this.approvalMatrixSelectedDocumentTypeId === null) {
      return;
    }
    this.approvalMatrixEditBackup = { ...this.approvalMatrix };
    this.isEditingApprovalMatrix = true;
    this.approvalMatrixSaveError = '';
    this.approvalMatrixSaveMessage = '';
  }

  cancelApprovalMatrixEdit(): void {
    if (this.approvalMatrixEditBackup) {
      this.approvalMatrix = { ...this.approvalMatrixEditBackup };
    }
    this.approvalMatrixEditBackup = null;
    this.isEditingApprovalMatrix = false;
    this.approvalMatrixSaveError = '';
    this.approvalMatrixSaveMessage = '';
  }

  saveApprovalMatrix(): void {
    const documentTypeId = this.approvalMatrixSelectedDocumentTypeId;
    if (!this.canEditApprovalMatrix || documentTypeId === null || this.isSavingApprovalMatrix) {
      return;
    }

    this.isSavingApprovalMatrix = true;
    this.approvalMatrixSaveError = '';
    this.approvalMatrixSaveMessage = '';

    const request: SaveDocumentApprovalMatrixRequest = {
      documentTypeId,
      approvedBy: this.approvalMatrixText(this.approvalMatrix.approvedBy),
      checkedBy: this.approvalMatrixText(this.approvalMatrix.checkedBy),
      issuedBy: this.approvalMatrixText(this.approvalMatrix.issuedBy),
      approvedDate: this.approvalMatrixDateRequest(this.approvalMatrix.approvedDate),
      checkedDate: this.approvalMatrixDateRequest(this.approvalMatrix.checkedDate),
      issuedDate: this.approvalMatrixDateRequest(this.approvalMatrix.issuedDate)
    };

    this.approvalMatrixService.saveMatrix(request).subscribe({
      next: () => {
        this.isSavingApprovalMatrix = false;
        this.isEditingApprovalMatrix = false;
        this.approvalMatrixEditBackup = null;
        this.approvalMatrixSaveMessage = 'Document Approval Matrix saved successfully.';
        this.loadApprovalMatrix();
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isSavingApprovalMatrix = false;
        this.approvalMatrixSaveError = error?.error?.message ??
          'Unable to save Approval Matrix. Please try again.';
        this.cdr.detectChanges();
      }
    });
  }

  refreshApprovalMatrix(): void {
    if (this.approvalMatrixSelectedDocumentTypeId !== null) {
      this.loadApprovalMatrix();
    } else {
      this.loadApprovalMatrixDocumentTypes();
    }
  }

  private approvalMatrixText(value: string | null): string | null {
    const trimmed = value?.trim();
    return trimmed ? trimmed : null;
  }

  private approvalMatrixDateRequest(value: string | null): string | null {
    return value ? `${value}T00:00:00` : null;
  }

  private approvalMatrixDateInput(value: string | null): string | null {
    return value ? value.substring(0, 10) : null;
  }

  // --------------------------------------------------
  // LOGOUT
  // --------------------------------------------------

  logout(): void {

    this.closeProfileMenu();

    this.SigninService.logout();

    void this.router.navigate(['/']);
  }
}
