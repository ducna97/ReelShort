import { useEffect, useRef, useState } from 'react';
import { login, register, socialLogin } from '../../api';
import Button from '../../components/common/Button';
import Modal from '../../components/common/Modal';
import type { Credentials, RegisterCredentials, User } from '../../types';
import {
  cancelGoogleButton,
  getFacebookAccessToken,
  prepareFacebookLogin,
  renderGoogleButton,
} from './socialAuth';

type AuthMode = 'login' | 'register';

interface AuthModalProps {
  onClose: () => void;
  onSuccess: (user: User) => void;
}

export default function AuthModal({ onClose, onSuccess }: AuthModalProps) {
  const [mode, setMode] = useState<AuthMode>('login');
  const [form, setForm] = useState<RegisterCredentials>({
    username: '',
    email: '',
    password: '',
  });
  const [message, setMessage] = useState('');
  const [busy, setBusy] = useState(false);
  const [socialBusy, setSocialBusy] = useState<'google' | 'facebook' | null>(null);
  const [facebookReady, setFacebookReady] = useState(false);
  const [facebookLoadFailed, setFacebookLoadFailed] = useState(false);
  const googleButtonRef = useRef<HTMLDivElement>(null);
  const mountedRef = useRef(true);
  const requestLockedRef = useRef(false);
  const requestControllerRef = useRef<AbortController | null>(null);
  const requestOwnerRef = useRef(0);

  const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  const facebookAppId = import.meta.env.VITE_FACEBOOK_APP_ID;

  const isRegister = mode === 'register';
  const update = (field: keyof RegisterCredentials) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((value) => ({ ...value, [field]: e.target.value }));

  const beginRequest = () => {
    if (requestLockedRef.current) return null;
    requestLockedRef.current = true;
    requestOwnerRef.current += 1;
    return requestOwnerRef.current;
  };

  const ownsRequest = (owner: number) => mountedRef.current && requestOwnerRef.current === owner;

  const endRequest = (owner: number) => {
    if (requestOwnerRef.current !== owner) return;
    requestLockedRef.current = false;
    requestControllerRef.current = null;
  };

  const sendSocialToken = async (
    provider: 'google' | 'facebook',
    accessToken: string,
    owner: number,
  ) => {
    const controller = new AbortController();
    requestControllerRef.current = controller;
    setMessage('');
    setSocialBusy(provider);
    try {
      const result = await socialLogin(provider, accessToken, controller.signal);
      if (ownsRequest(owner)) onSuccess(result);
    } catch (error) {
      if (ownsRequest(owner) && !(error instanceof DOMException && error.name === 'AbortError')) {
        setMessage(error instanceof Error ? error.message : 'Đăng nhập mạng xã hội thất bại.');
      }
    } finally {
      if (ownsRequest(owner)) setSocialBusy(null);
      endRequest(owner);
    }
  };

  useEffect(() => {
    mountedRef.current = true;

    return () => {
      mountedRef.current = false;
      requestOwnerRef.current += 1;
      requestControllerRef.current?.abort();
      requestLockedRef.current = false;
      cancelGoogleButton();
    };
  }, []);

  useEffect(() => {
    const container = googleButtonRef.current;
    if (container && googleClientId) {
      renderGoogleButton(container, googleClientId, (credential) => {
        const owner = beginRequest();
        if (mountedRef.current && owner !== null) void sendSocialToken('google', credential, owner);
      }).catch((error) => {
        if (mountedRef.current) {
          setMessage(error instanceof Error ? error.message : 'Không thể tải đăng nhập Google.');
        }
      });
    }

    return () => {
      cancelGoogleButton();
    };
  }, [googleClientId]);

  useEffect(() => {
    let cancelled = false;
    setFacebookReady(false);
    setFacebookLoadFailed(false);

    if (!facebookAppId) {
      setFacebookLoadFailed(true);
      setMessage('Thiếu VITE_FACEBOOK_APP_ID trong cấu hình frontend.');
      return () => undefined;
    }

    prepareFacebookLogin(facebookAppId)
      .then(() => {
        if (!cancelled && mountedRef.current) {
          setFacebookReady(true);
          setFacebookLoadFailed(false);
        }
      })
      .catch((error) => {
        if (!cancelled && mountedRef.current) {
          setFacebookLoadFailed(true);
          setMessage(error instanceof Error ? error.message : 'Không thể tải đăng nhập Facebook.');
        }
      });

    return () => {
      cancelled = true;
    };
  }, [facebookAppId]);

  const loginFacebook = async () => {
    if (!facebookAppId) {
      setMessage('Thiếu VITE_FACEBOOK_APP_ID trong cấu hình frontend.');
      return;
    }
    if (!facebookReady) {
      if (!facebookLoadFailed) {
        setMessage('Facebook SDK đang tải. Vui lòng chờ trong giây lát.');
        return;
      }

      setFacebookLoadFailed(false);
      setMessage('Đang tải lại Facebook SDK...');
      try {
        await prepareFacebookLogin(facebookAppId);
        if (mountedRef.current) {
          setFacebookReady(true);
          setMessage('Facebook đã sẵn sàng. Hãy nhấn đăng nhập lần nữa.');
        }
      } catch (error) {
        if (mountedRef.current) {
          setFacebookLoadFailed(true);
          setMessage(error instanceof Error ? error.message : 'Không thể tải đăng nhập Facebook.');
        }
      }
      return;
    }
    const owner = beginRequest();
    if (owner === null) return;

    setMessage('');
    setSocialBusy('facebook');
    try {
      const accessToken = await getFacebookAccessToken(facebookAppId);
      if (ownsRequest(owner)) await sendSocialToken('facebook', accessToken, owner);
    } catch (error) {
      if (mountedRef.current) {
        setMessage(error instanceof Error ? error.message : 'Không thể đăng nhập Facebook.');
        setSocialBusy(null);
      }
      endRequest(owner);
    }
  };

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    const owner = beginRequest();
    if (owner === null) return;

    const controller = new AbortController();
    requestControllerRef.current = controller;
    setMessage('');
    setBusy(true);

    try {
      if (isRegister) {
        await register(form, controller.signal);
        if (!ownsRequest(owner)) return;
        setMode('login');
        setMessage('Tạo tài khoản thành công. Hãy đăng nhập để tiếp tục.');
        setForm((value) => ({ ...value, password: '' }));
        return;
      }

      const loginPayload: Credentials = {
        email: form.email,
        password: form.password,
      };
      const result = await login(loginPayload, controller.signal);

      if (ownsRequest(owner)) onSuccess(result);
    } catch (error) {
      if (ownsRequest(owner) && !(error instanceof DOMException && error.name === 'AbortError')) {
        setMessage(error instanceof Error ? error.message : 'Đã xảy ra lỗi.');
      }
    } finally {
      if (ownsRequest(owner)) setBusy(false);
      endRequest(owner);
    }
  };
  return (
    <Modal onClose={onClose}>
      <Button className="close" onClick={onClose}>
        ×
      </Button>
      <div className="auth-logo">
        <i>R</i>
      </div>
      <h2>{isRegister ? 'Tạo tài khoản mới' : 'Chào mừng trở lại'}</h2>
      <p>
        {isRegister
          ? 'Tham gia cộng đồng ReelShort ngay hôm nay.'
          : 'Đăng nhập để xem những reel mới nhất.'}
      </p>
      <div className={`social-login${busy || socialBusy ? ' busy' : ''}`}>
        {googleClientId ? (
          <div ref={googleButtonRef} className="google-button-slot" />
        ) : (
          <Button
            className="social-button google"
            disabled={Boolean(socialBusy)}
            onClick={() => setMessage('Thiếu VITE_GOOGLE_CLIENT_ID trong cấu hình frontend.')}
          >
            <span className="google-mark">G</span>
            Tiếp tục với Google
          </Button>
        )}
        <Button
          className="social-button facebook"
          disabled={
            busy ||
            Boolean(socialBusy) ||
            (Boolean(facebookAppId) && !facebookReady && !facebookLoadFailed)
          }
          onClick={loginFacebook}
        >
          <span className="facebook-mark">f</span>
          {socialBusy === 'facebook'
            ? 'Đang kết nối...'
            : !facebookAppId
              ? 'Thiếu cấu hình Facebook'
              : facebookReady
                ? 'Tiếp tục với Facebook'
                : facebookLoadFailed
                  ? 'Thử tải lại Facebook'
                  : 'Đang tải Facebook...'}
        </Button>
      </div>
      <div className="auth-divider">
        <span>hoặc tiếp tục bằng email</span>
      </div>
      <form onSubmit={submit}>
        {isRegister && (
          <label>
            Tên người dùng
            <input
              required
              minLength={3}
              value={form.username}
              onChange={update('username')}
              placeholder="reelshorter"
            />
          </label>
        )}
        <label>
          Email
          <input
            required
            type="email"
            value={form.email}
            onChange={update('email')}
            placeholder="you@example.com"
          />
        </label>
        <label>
          Mật khẩu
          <input
            required
            minLength={6}
            type="password"
            value={form.password}
            onChange={update('password')}
            placeholder="••••••••"
          />
        </label>
        {message && (
          <div className={message.startsWith('Tạo') ? 'notice success' : 'notice'}>{message}</div>
        )}
        <Button type="submit" className="submit" disabled={busy || Boolean(socialBusy)}>
          {busy ? 'Đang xử lý...' : isRegister ? 'Tạo tài khoản' : 'Đăng nhập'}
        </Button>
      </form>
      <div className="auth-switch">
        {isRegister ? 'Đã có tài khoản?' : 'Chưa có tài khoản?'}{' '}
        <Button
          onClick={() => {
            setMode(isRegister ? 'login' : 'register');
            setMessage('');
          }}
        >
          {isRegister ? 'Đăng nhập' : 'Đăng ký'}
        </Button>
      </div>
    </Modal>
  );
}
