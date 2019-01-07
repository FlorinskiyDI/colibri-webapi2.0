import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { Subject } from 'rxjs/Subject';
import { filter } from 'rxjs/operators';

@Component({
    selector: 'cmp-layout-identity',
    templateUrl: 'layout-identity.component.html',
    styleUrls: ['./layout-identity.component.scss']
})

export class LayoutIdentityComponent implements OnInit {
    eventSidebarToggle: Subject<any> = new Subject<any>();
    breadcrumbs$: any;

    constructor(
        private router: Router,
        private activatedRoute: ActivatedRoute
    ) {}

    ngOnInit() {
        this.breadcrumbs$ = this.router.events.pipe(
            filter((event: any) => event instanceof NavigationEnd))
            .distinctUntilChanged()
            .map((event: any) => {
                return this.buildBreadCrumb(this.activatedRoute.root);
            });

        this.breadcrumbs$.subscribe(
            (data: any) => {
                // debugger
                console.log(data);
            }
        );
    }

    sidebarToggle() {
        this.eventSidebarToggle.next();
    }

    private buildBreadCrumb(
        route: ActivatedRoute,
        url: string = '',
        breadcrumbs: Array<BreadCrumb> = []
    ): Array<BreadCrumb> {
        // If no routeConfig is avalailable we are on the root path
        const breadcrumbData = route.routeConfig && route.routeConfig.data && route.routeConfig.data['breadcrumb'];

        let newBreadcrumbs: any = [...breadcrumbs];
        let nextUrl = '';

        const label = route.routeConfig ? breadcrumbData : null;
        const path = route.routeConfig ? route.routeConfig.path : '';
        // In the routeConfig the complete path is not available,
        // so we rebuild it each time
        nextUrl = `${url}${path}/`;
        const breadcrumb = {
            label: label,
            url: nextUrl
        };
        if (breadcrumbData) {
            console.log(route);
            newBreadcrumbs = [...breadcrumbs, breadcrumb];
        }
        // const newBreadcrumbs = [...breadcrumbs, breadcrumb];
        if (route.firstChild) {
            // If we are not on our current path yet,
            // there will be more children to look after, to build our breadcumb
            return this.buildBreadCrumb(route.firstChild, nextUrl, newBreadcrumbs);
        }
        return newBreadcrumbs;
    }
}

export interface BreadCrumb {
    label: string;
    url: string;
}