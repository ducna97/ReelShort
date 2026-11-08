import { useState } from 'react';
import AuthModal from './features/auth/AuthModal';
import Feed from './features/feed/Feed';
import Profile from './features/profile/Profile';
import MobileNav from './components/layout/MobileNav';
import Sidebar from './components/layout/Sidebar';

const STORAGE_KEY = 'reelshort_user';

export default function App() {
  const [page, setPage] = useState('home');
  const [authOpen, setAuthOpen] = useState(false);
  const [user, setUser] = useState(() => {
    try {
      return JSON.parse(localStorage.getItem(STORAGE_KEY));
    } catch {
      return null;
    }
  });
  const saveUser = (value) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(value));
    setUser(value);
    setAuthOpen(false);
  };
  const logout = () => {
    localStorage.removeItem(STORAGE_KEY);
    setUser(null);
    setPage('home');
  };
  return (
    <div className="app">
      <Sidebar
        page={page}
        onNavigate={setPage}
        user={user}
        onLogin={() => setAuthOpen(true)}
        onLogout={logout}
      />
      {page === 'profile' ? (
        <Profile user={user} onLogin={() => setAuthOpen(true)} />
      ) : (
        <Feed user={user} onLogin={() => !user && setAuthOpen(true)} />
      )}
      <MobileNav onNavigate={setPage} onCreate={() => setAuthOpen(true)} />
      {authOpen && <AuthModal onClose={() => setAuthOpen(false)} onSuccess={saveUser} />}
    </div>
  );
}
