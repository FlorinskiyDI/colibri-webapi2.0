import { Component, ViewChild, ElementRef } from '@angular/core';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { FormControl } from '@angular/forms';
import { TreeDragDropService } from 'primeng/components/common/api';
import { ConfirmationService } from 'primeng/api';
import { MessageService } from 'primeng/components/common/messageservice';
import { Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { map, startWith } from 'rxjs/operators';
import { MatAutocompleteSelectedEvent, MatChipInputEvent, MatAutocomplete } from '@angular/material';

// /* service-transfer */ import { GroupManageTransferService } from '../group-manage/group-manage.transfer.service';
/* service-api */ import { GroupsApiService } from 'shared/services/api/groups.api.service';
/* model-control */ import { DialogDataModel } from 'shared/models/controls/dialog-data.model';
/* model-api */ import { GroupApiModel } from 'shared/models/entities/api/group.api.model';
/* model-api */ import { PageSearchEntryApiModel } from 'shared/models/entities/api/page-search-entry.api.model';
/* constant */ import { ModalTypes } from 'shared/constants/modal-types.constant';
// /* directive */ import { ModalService } from 'shared/directives/modal/modal.service';

@Component({
    selector: 'cmp-group-grid',
    templateUrl: './group-grid.component.html',
    styleUrls: ['./group-grid.component.scss'],
    providers: [
        TreeDragDropService,
        ConfirmationService,
        MessageService
    ]
})
export class GroupGridComponent {

    visible = true;
    selectable = true;
    removable = true;
    addOnBlur = true;
    separatorKeysCodes: number[] = [ENTER, COMMA];
    fruitCtrl = new FormControl();
    filteredFruits: Observable<string[]>;
    fruits: string[] = ['Lemon'];
    allFruits: string[] = ['Apple', 'Lemon', 'Lime', 'Orange', 'Strawberry'];

    @ViewChild('fruitInput') fruitInput: ElementRef<HTMLInputElement>;
    @ViewChild('auto') matAutocomplete: MatAutocomplete;




    // modal
    MODAL_GROUP_CREATE = ModalTypes.GROUP_CREATE;



    dialogGroupCreateConfig: DialogDataModel<any>;
    selectedGroup: any;
    tbItems: any[] = [];
    tbCols: any[] = [];
    tbLoading = true;
    tbTotalItemCount: number;
    isNodeSelected = false;
    constructor(
        private router: Router,
        private messageService: MessageService,
        private confirmationService: ConfirmationService,
        private groupsApiService: GroupsApiService,
    ) {
        // this._requestGetRootGroups();

        this.filteredFruits = this.fruitCtrl.valueChanges.pipe(
            startWith(null),
            map((fruit: string | null) => fruit ? this._filter(fruit) : this.allFruits.slice()));
    }



    add(event: MatChipInputEvent): void {
        // Add fruit only when MatAutocomplete is not open
        // To make sure this does not conflict with OptionSelected Event
        if (!this.matAutocomplete.isOpen) {
            const input = event.input;
            const value = event.value;

            // Add our fruit
            if ((value || '').trim()) {
                this.fruits.push(value.trim());
            }

            // Reset the input value
            if (input) {
                input.value = '';
            }

            this.fruitCtrl.setValue(null);
        }
    }

    remove(fruit: string): void {
        const index = this.fruits.indexOf(fruit);

        if (index >= 0) {
            this.fruits.splice(index, 1);
        }
    }

    selected(event: MatAutocompleteSelectedEvent): void {
        this.fruits.push(event.option.viewValue);
        this.fruitInput.nativeElement.value = '';
        this.fruitCtrl.setValue(null);
    }

    private _filter(value: string): string[] {
        const filterValue = value.toLowerCase();

        return this.allFruits.filter(fruit => fruit.toLowerCase().indexOf(filterValue) === 0);
    }










    ngOninit() {
        this.tbCols = [
            { field: 'id', header: 'id' }
        ];

        this.tbLoading = true;
    }

    public createGroup() {
        this.dialogGroupCreateOpen();
    }
    public removeGroup(data: any) {
        console.log(`removeGroup - ${data.id}`);
        const that = this;
        this.confirmationService.confirm({
            message: 'Are you sure that you want to remove this group?',
            accept: () => {
                that.selectedGroup = null;
                this.groupsApiService.delete(data.id).subscribe(
                    (response: any) => {
                        this._requestGetSubGroups(true);
                        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Group was removed successfully' });
                    },
                    (error: any) => {
                        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error' });
                    }
                );
            }
        });
    }
    public selectGroup(data: any) {
        if (data.node && data.node.data && data.node.data.id) {
            // this.groupManageTransferService.sendSelectedGroupId(data.node.data.id);
        }
    }
    public searchGroups(data: any) {
        // const that = this;
        if (data === '') {
            this._requestGetSubGroups(true);
        } else {
            // this._stub2().then(function (value: any) {
            //     that.items = value;
            //     that.treeloading = false;
            //     that.selectedGroup = value[0];
            // });
        }
    }



    // #region - GROUP DIALOG actions
    public dialogGroupCreateOpen() { this.dialogGroupCreateConfig = new DialogDataModel<any>(true); }
    public dialogGroupCreateOnChange() {
        this._requestGetSubGroups(true);
        console.log('dialogGroupCreateOnChange');
    }
    public dialogGroupCreateOnCancel() { console.log('dialogGroupCreateOnCancel'); }
    public dialogGroupCreateOnHide() { console.log('dialogGroupCreateOnHide'); }
    // #endregion



    loadNodes(event: any) {
        debugger
        this.tbLoading = true;
        const searchEntry = {
            pageNumber: event.first > 0 ? event.first : 1,
            pageLength: event.rows,
            orderStatement: (event.sortField && event.sortOrder) ? { columName: event.sortField, reverse: event.sortOrder > 0 } : null

        } as PageSearchEntryApiModel;
        this._requestGetRootGroups(searchEntry);
    }

    onNodeSelect(event: any) {
        console.log(event.node.data.name);
        this.isNodeSelected = true;
    }

    onNodeUnselect(event: any) {
        this.isNodeSelected = false;
        console.log(event.node.data.name);
    }

    onNodeExpand(event: any) {
        const node = event.node;
        const that = this;
        //
        this.tbLoading = true;
        this.groupsApiService.getSubGroups(event.node.data.id).subscribe((data: any) => {
            node.children = data.map((item: GroupApiModel) => { return { 'data': { 'id': item.id, 'name': item.name }, 'leaf': false }; });
            that.tbLoading = false;
            this.tbItems = [...this.tbItems];
        });
    }

    goToGroupView(groupId: string) {
        this.router.navigate(['/manage/groups/' + groupId]);
    }

    _requestGetRootGroups(searchEntry: PageSearchEntryApiModel) {
        this.tbLoading = true;
        this.groupsApiService.getRoot(searchEntry).subscribe((response: any) => {
            this.tbLoading = false;
            this.tbItems = response.items.map((item: GroupApiModel) => {
                return {
                    'label': item.name,
                    'data': { 'id': item.id, 'name': item.name },
                    'leaf': false
                };
            });
            this.tbTotalItemCount = response.totalItemCount;
            this.selectedGroup = this.tbItems[0];
            // if (data.length > 0) {
            //     // this.groupManageTransferService.sendSelectedGroupId(data[0].id);
            // }
        });
    }

    _requestGetSubGroups(changeSelectedGroup: boolean, subGroupId: string = null) {
        this.tbLoading = true;
        this.groupsApiService.getSubGroups(subGroupId).subscribe((data: Array<GroupApiModel>) => {
            this.tbItems = data.map((item: GroupApiModel) => {
                return {
                    'label': item.name,
                    'data': { 'id': item.id },
                    'leaf': false
                };
            });
            this.tbLoading = false;

            if (changeSelectedGroup) {
                this.selectedGroup = this.tbItems[0];
                if (data.length > 0) {
                    // this.groupManageTransferService.sendSelectedGroupId(data[0].id);
                }
            }

        });
    }
}
