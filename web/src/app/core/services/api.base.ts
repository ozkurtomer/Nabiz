import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';

export abstract class ApiBase {
  protected http = inject(HttpClient);
  protected url(yol: string) { return `${environment.apiUrl}/${yol}`; }
}
