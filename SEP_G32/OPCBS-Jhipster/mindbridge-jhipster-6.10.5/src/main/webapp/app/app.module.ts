import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import './vendor';
import { OpcbsSharedModule } from 'app/shared/shared.module';
import { OpcbsCoreModule } from 'app/core/core.module';
import { OpcbsAppRoutingModule } from './app-routing.module';
import { OpcbsHomeModule } from './home/home.module';
import { OpcbsEntityModule } from './entities/entity.module';
// jhipster-needle-angular-add-module-import JHipster will add new module here
import { MainComponent } from './layouts/main/main.component';
import { NavbarComponent } from './layouts/navbar/navbar.component';
import { FooterComponent } from './layouts/footer/footer.component';
import { PageRibbonComponent } from './layouts/profiles/page-ribbon.component';
import { ErrorComponent } from './layouts/error/error.component';

@NgModule({
  imports: [
    BrowserModule,
    OpcbsSharedModule,
    OpcbsCoreModule,
    OpcbsHomeModule,
    // jhipster-needle-angular-add-module JHipster will add new module here
    OpcbsEntityModule,
    OpcbsAppRoutingModule,
  ],
  declarations: [MainComponent, NavbarComponent, ErrorComponent, PageRibbonComponent, FooterComponent],
  bootstrap: [MainComponent],
})
export class OpcbsAppModule {}
