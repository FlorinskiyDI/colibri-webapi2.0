import { Component, ViewChild, Input } from '@angular/core';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { MessageService } from 'primeng/components/common/messageservice';
import { Router } from '@angular/router';

/* model-api */ import { GroupApiModel } from 'shared/models/entities/api/group.api.model';
/* service-api */ import { GroupsApiService } from 'shared/services/api/groups.api.service';
/* helper */ import { FormGroupHelper } from 'shared/helpers/form-group.helper';
/* model-api */ import { SearchQueryApiModel } from 'shared/models/entities/api/page-search-entry.api.model';

@Component({
    selector: 'cmp-group-form-update',
    templateUrl: './group-form-update.component.html',
    styleUrls: ['./group-form-update.component.scss'],
    providers: [ MessageService ]
})

export class GroupFormUpdateComponent {
    @Input() itemId: string;
    // form variable
    @ViewChild('ngFormGroup') ngFormGroup: any;
    formGroup: FormGroup;
    drpdwnGroups: any[] = [];

    treeSelected: any;
    treeItems: any[] = [];
    treeloading = false;
    isSubGroup = false;

    constructor(
        private router: Router,
        private messageService: MessageService,
        private groupsApiService: GroupsApiService
    ) {
    }

    ngOnInit() {
        this.componentInit(this.itemId);
        this.formInitBuild();
    }
    ngOnDestroy() { this.componentClear(); }

    private componentInit(itemId: any) {
        this.isSubGroup = false;
        this.treeItems = [];
        this._getAllRoot();
        this._getItem(itemId);
    }
    private componentClear() {
        this.formInitBuild();
        this.drpdwnGroups = [];
    }

    private formInitBuild(data: any = {}): void {
        this.formGroup = new FormGroup({
            'name': new FormControl(data && data.name, [Validators.required]),
            'groupID': new FormControl(null, [Validators.required]),
            'isSubgroup': new FormControl(null, [Validators.required]),
            'parentId': new FormControl(data && data.parentId),
            'description': new FormControl(data.description),
        });
    }

    public formSubmit() {
        if (!this.ngFormGroup.valid) {
            FormGroupHelper.setFormControlsAsTouched(this.formGroup);
            return;
        }
        this.groupsApiService
            .update(Object.assign({}, this.ngFormGroup.value))
            .subscribe(
                (response: any) => {
                    // this.dialogChange();
                    this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Group was created successfully' });
                    // this.router.navigate(['is/groups']);
                },
                (error: any) => { }
            );
    }
    public formCancel() {
        this.router.navigate(['is/groups']);
    }
    
    _getItem(itemId: any) {
        this.groupsApiService.get(itemId).subscribe(
            (data: any) => {
                this.formInitBuild(data);
            });
    }

    _getAllRoot() {
        this.treeloading = true;
        const that = this;
        this.groupsApiService.getAllRoot(null).subscribe((data: any) => {
            this.treeItems = data.itemList.map((item: any) => {
                return {
                    'label': item.name,
                    'data': { 'id': item.id },
                    'leaf': false
                };
            });
            that.treeloading = false;
        });
    }

    onChckboxChange(data: any) {
        console.log(data);
        console.log(this.isSubGroup);
        if (this.isSubGroup) {
            this._getAllRoot();
            this.formGroup.controls['parentId'].setValidators([Validators.required]);
            this.formGroup.controls['parentId'].markAsUntouched();
            this.formGroup.controls['parentId'].updateValueAndValidity();
        } else {
            this.formGroup.controls['parentId'].setValue(null);
            this.formGroup.controls['parentId'].clearValidators();
            this.formGroup.controls['parentId'].updateValueAndValidity();
            this.treeItems = [];
        }
    }

    onNodeSelect(event: any) {
        this.formGroup.controls['parentId'].setValue(event.node.data.id);
    }

    onNodeExpand(event: any) {
        this.treeloading = true;
        const that = this;
        this.groupsApiService.getSubgroups(new SearchQueryApiModel(), event.node.data.id).subscribe((data: Array<GroupApiModel>) => {
            event.node.children = data.map((item: GroupApiModel) => {
                return {
                    'label': item.name,
                    'data': { 'id': item.id },
                    'leaf': false
                };
            });
            that.treeloading = false;
        });
    }

}
