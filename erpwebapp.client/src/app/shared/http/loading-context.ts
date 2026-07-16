import { HttpContextToken } from '@angular/common/http';

/** Skip the application-wide blocking loader for background HTTP requests. */
export const SKIP_GLOBAL_LOADING = new HttpContextToken<boolean>(() => false);
