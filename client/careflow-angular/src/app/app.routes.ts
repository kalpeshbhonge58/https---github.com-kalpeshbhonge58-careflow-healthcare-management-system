import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';
import { LoginComponent } from './features/auth/login.component';
import { UnauthorizedComponent } from './features/auth/unauthorized.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'dashboard' },
	{ path: 'login', component: LoginComponent },
	{ path: 'unauthorized', component: UnauthorizedComponent },
	{ path: 'dashboard', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Your dashboard' } },
	{ path: 'admin/dashboard', component: DashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'], pageTitle: 'Admin dashboard' } },
	{ path: 'doctor/dashboard', component: DashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Doctor'], pageTitle: 'Doctor dashboard' } },
	{ path: 'receptionist/dashboard', component: DashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Receptionist'], pageTitle: 'Reception desk' } },
	{ path: 'patient/dashboard', component: DashboardComponent, canActivate: [authGuard, roleGuard], data: { roles: ['Patient'], pageTitle: 'Patient dashboard' } },
	{ path: 'patients', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Patients' } },
	{ path: 'doctors', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Doctors' } },
	{ path: 'appointments', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Appointments' } },
	{ path: 'medical-records', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Medical records' } },
	{ path: 'prescriptions', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Prescriptions' } },
	{ path: 'billing', component: DashboardComponent, canActivate: [authGuard], data: { pageTitle: 'Billing' } },
	{ path: '**', redirectTo: 'dashboard' }
];
