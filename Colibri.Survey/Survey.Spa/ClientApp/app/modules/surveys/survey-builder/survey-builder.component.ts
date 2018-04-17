import { Component } from '@angular/core';

// import { QuestionBase } from '../../../shared/Models/form-builder/question-base.model';

import { ControTypes } from '../../../shared/constants/control-types.constant';

import { QuestionService } from '../../../shared/services/api/question.service';


@Component({
    selector: 'survey-builder-cmp',
    templateUrl: './survey-builder.component.html',
    styleUrls: [
        './survey-builder.component.scss'
    ],
    providers: [QuestionService]
})
export class SurveyBuilderComponent {
    questions: any[];
    temporaryAnser: any;
    availableQuestions: string[] = [
        ControTypes.checkbox,
        ControTypes.dropdown,
        ControTypes.gridRadio,
        ControTypes.radio,
        ControTypes.textarea,
        ControTypes.textbox
    ];







    listOne: Array<string> = ['Coffee', 'Orange Juice', 'Red Wine', 'Unhealty drink!', 'Water'];
    listTeamOne: Array<string> = [];
    listTeamOne5: Array<string> = [];
    availableProducts: Array<Product> = [];
    shoppingBasket: Array<Product> = [];

    constructor(service: QuestionService) {

        this.listTeamOne5.push('111111111111112');
        this.listTeamOne.push('11111111111111');

        this.questions = service.getQuestions();
        this.availableProducts.push(new Product('Blue Shoes', 3, 35));
        this.availableProducts.push(new Product('Good Jacket', 1, 90));
        this.availableProducts.push(new Product('Red Shirt', 5, 12));
        this.availableProducts.push(new Product('Blue Jeans', 4, 60));
    }

    orderedProduct($event: any) {
        // const orderedProduct: Product = $event.dragData;
        // orderedProduct.quantity--;
    }

    addToBasket($event: any) {
        console.log('888888888888888888888888888888888');
        console.log($event);
        this.listTeamOne.push($event);
        console.log(this.listTeamOne);
    }

    totalCost(): number {
        return 6;
    }

    changeSort(event: any) {
        console.log(event);
    }


    checkOnMouseUp(event: any) {
        console.log('eventeventeventeventeventeventevent');
        console.log(event);
        console.log('eventeventeventeventeventeventevent');
    }
    transferDataSuccess($event: any) {
        console.log('777777777777777777777777777777777777777777777');
        console.log($event);
        this.listTeamOne.push($event);
    }


    dragOperation = false;

    containers: Array<Container> = [
        new Container(1, 'Container 1', [new Widget('1'), new Widget('2')]),
        new Container(2, 'Container 2', [new Widget('3'), new Widget('4')]),
        new Container(3, 'Container 3', [new Widget('5'), new Widget('6')])
    ];

    widgets: Array<Widget> = [];
    addTo($event: any) {
        if ($event) {
            this.widgets.push($event.dragData);
            this.listTeamOne.push($event.dragData);
        }
    }

    listBoxers: Array<string> = ['Sugar Ray Robinson',
    'Muhammad Ali', 'George Foreman', 'Joe Frazier', 'Jake LaMotta', 'Joe Louis', 'Jack Dempsey', 'Rocky Marciano', 'Mike Tyson', 'Oscar De La Hoya'];

    listTeamTwo: Array<string> = [];






    sourceList: Widget[] = [
        new Widget('1'), new Widget('2'),
        new Widget('3'), new Widget('4'),
        new Widget('5'), new Widget('6')
    ];

    targetList1: Widget[] = [];
    targetList2: Widget[] = [];

    addTo2($event: any) {
        this.targetList2.push($event.dragData);
    }

    addTo1($event: any) {
        this.targetList1.push($event.dragData);
    }
}

class Product {
    constructor(public name: string, public quantity: number, public cost: number) { }
}


class Container {
    constructor(public id: number, public name: string, public widgets: Array<Widget>) { }
}

class Widget {
    constructor(public name: string) { }
}