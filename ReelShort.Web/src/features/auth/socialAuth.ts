interface GoogleCredentialResponse {
  credential: string;
}

interface GoogleIdentityServices {
  accounts: {
    id: {
      initialize: (options: {
        client_id: string;
        callback: (response: GoogleCredentialResponse) => void;
      }) => void;
      renderButton: (
        parent: HTMLElement,
        options: {
          theme: 'outline';
          size: 'large';
          shape: 'rectangular';
          text: 'continue_with';
          locale: string;
          width: number;
        },
      ) => void;
    };
  };
}

interface FacebookLoginResponse {
  authResponse?: {
    accessToken: string;
  };
}

interface FacebookSdk {
  init: (options: { appId: string; cookie: boolean; xfbml: boolean; version: string }) => void;
  login: (callback: (response: FacebookLoginResponse) => void, options: { scope: string }) => void;
}

declare global {
  interface Window {
    google?: GoogleIdentityServices;
    FB?: FacebookSdk;
  }
}

let googleScriptPromise: Promise<void> | undefined;
let facebookScriptPromise: Promise<void> | undefined;
let facebookAppId: string | undefined;
let googleRenderGeneration = 0;

const SDK_TIMEOUT_MS = 10_000;

function loadScript(id: string, source: string, isReady: () => boolean): Promise<void> {
  if (isReady()) return Promise.resolve();

  return new Promise((resolve, reject) => {
    const existing = document.getElementById(id) as HTMLScriptElement | null;
    const script = existing ?? document.createElement('script');
    let settled = false;

    const cleanup = () => {
      window.clearTimeout(timeoutId);
      script.removeEventListener('load', handleLoad);
      script.removeEventListener('error', handleError);
    };
    const finish = (error?: Error) => {
      if (settled) return;
      settled = true;
      cleanup();
      if (error) {
        script.remove();
        reject(error);
      } else {
        script.dataset.loaded = 'true';
        resolve();
      }
    };
    const handleLoad = () => finish(isReady() ? undefined : new Error('SDK chưa sẵn sàng.'));
    const handleError = () => finish(new Error('Không thể tải SDK.'));
    const timeoutId = window.setTimeout(
      () => finish(new Error('Tải SDK quá thời gian. Vui lòng thử lại.')),
      SDK_TIMEOUT_MS,
    );

    script.addEventListener('load', handleLoad, { once: true });
    script.addEventListener('error', handleError, { once: true });

    if (existing) {
      if (existing.dataset.loaded === 'true')
        finish(isReady() ? undefined : new Error('SDK chưa sẵn sàng.'));
      return;
    }

    script.id = id;
    script.src = source;
    script.async = true;
    script.defer = true;
    document.head.appendChild(script);
  });
}

function loadGoogleSdk() {
  googleScriptPromise ??= loadScript(
    'google-identity-services',
    'https://accounts.google.com/gsi/client',
    () => Boolean(window.google),
  ).catch((error) => {
    googleScriptPromise = undefined;
    throw error;
  });
  return googleScriptPromise;
}

function loadFacebookSdk() {
  facebookScriptPromise ??= loadScript(
    'facebook-jssdk',
    'https://connect.facebook.net/vi_VN/sdk.js',
    () => Boolean(window.FB),
  ).catch((error) => {
    facebookScriptPromise = undefined;
    throw error;
  });
  return facebookScriptPromise;
}

export async function renderGoogleButton(
  container: HTMLElement,
  clientId: string,
  onCredential: (credential: string) => void,
) {
  const generation = ++googleRenderGeneration;
  await loadGoogleSdk();

  if (!window.google) throw new Error('Google SDK chưa sẵn sàng.');
  if (generation !== googleRenderGeneration) return;

  window.google.accounts.id.initialize({
    client_id: clientId,
    callback: (response) => {
      if (generation === googleRenderGeneration) onCredential(response.credential);
    },
  });

  container.replaceChildren();
  window.google.accounts.id.renderButton(container, {
    theme: 'outline',
    size: 'large',
    shape: 'rectangular',
    text: 'continue_with',
    locale: 'vi',
    width: Math.min(container.clientWidth || 340, 340),
  });
}

export function cancelGoogleButton() {
  googleRenderGeneration += 1;
}

export async function prepareFacebookLogin(appId: string): Promise<void> {
  await loadFacebookSdk();

  if (!window.FB) throw new Error('Facebook SDK chưa sẵn sàng.');
  if (facebookAppId !== appId) {
    window.FB.init({ appId, cookie: true, xfbml: false, version: 'v26.0' });
    facebookAppId = appId;
  }
}

export function getFacebookAccessToken(appId: string): Promise<string> {
  if (!window.FB || facebookAppId !== appId) {
    return Promise.reject(new Error('Facebook SDK chưa sẵn sàng. Vui lòng thử lại.'));
  }

  return new Promise((resolve, reject) => {
    window.FB!.login(
      (response) => {
        const token = response.authResponse?.accessToken;
        if (token) resolve(token);
        else reject(new Error('Bạn đã hủy đăng nhập Facebook.'));
      },
      { scope: 'public_profile,email' },
    );
  });
}

export {};
