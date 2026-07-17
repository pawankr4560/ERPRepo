import { HttpContextToken } from '@angular/common/http';

/** Skip the application-wide blocking loader for background HTTP requests. */
export const SKIP_GLOBAL_LOADING = new HttpContextToken<boolean>(() => false);

/**
 * Global blocking loading is opt-in. Feature screens should normally render their
 * own local loading state so navigation and page context stay visible.
 */
export const USE_GLOBAL_LOADING = new HttpContextToken<boolean>(() => false);
