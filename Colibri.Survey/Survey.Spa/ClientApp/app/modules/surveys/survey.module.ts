import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

/* module */ import { SharedModule } from 'shared/shared.module';
/* module */ import { SurveyRoutingModule } from './survey.routes';
/* component */ import { SurveyComponent } from './survey.component';
/* component */ import { SurveyBuilderComponent } from './survey-builder/survey-builder.component';

/* component */ import { BuilderComponent } from './builder/builder.component';
/* component */ import { FormBuilderComponent } from './builder/form-builder/form-builder.component';
/* component */ import { SurveyGridComponent } from './survey-grid/survey-grid.component';

/* component */ import { SurveyFormBuilderComponent } from './survey-builder/survey-form-builder/survey-form-builder.component';
/* component */ import { SurveyFormQuestionComponent } from './survey-builder/survey-form-builder/survey-form-question/survey-form-question.component';
/* component */ import { PagingComponent } from './survey-builder/paging/paging.component';
/* component */ import { PagingFormComponent } from './builder/form-builder/paging-form/paging-form.component';

/* component */ import { QuestionOptionComponent } from './survey-builder/question-option/question-option.component';

/* component */ import { QuestionFormBuilderComponent } from './builder/form-builder/question-form-builder/question-form-builder.component';
/* component */ import { QuestionOptionsComponent } from './builder/form-builder/question-options/question-options.component';

// /* component */ import { ReportModule} from './report/report.module';


import { DndModule } from 'ng2-dnd';
@NgModule({
    imports: [
        SurveyRoutingModule,
        SharedModule,
        DndModule.forRoot(),
        // ReportModule
    ],
    declarations: [
        FormBuilderComponent,
        SurveyComponent,
        SurveyBuilderComponent,
        BuilderComponent,
        SurveyFormBuilderComponent,
        QuestionOptionsComponent,
        PagingComponent,
        SurveyFormQuestionComponent,
        QuestionOptionComponent,
        QuestionFormBuilderComponent,
        PagingFormComponent,
        SurveyGridComponent
        // GroupGridComponent
    ],
    entryComponents: [
        FormBuilderComponent,
      ],
    providers: [],
    schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SurveyModule { }
