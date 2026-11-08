import Button from '../common/Button';
import Icon from '../common/Icon';

const links = [
  { id: 'home', label: 'Dành cho bạn', icon: 'home' },
  {
    id: 'discover',
    label: 'Khám phá',
    icon: 'search',
  },
  { id: 'profile', label: 'Hồ sơ', icon: 'user' },
];
export default function Sidebar({ page, onNavigate, user, onLogin, onLogout }) {
  return (
    <aside className="sidebar">
      <Button className="brand" onClick={() => onNavigate('home')}>
        <i>R</i>ReelShort
      </Button>
      <nav>
        {links.map((link) => (
          <Button
            key={link.id}
            className={page === link.id ? 'active' : ''}
            onClick={() => onNavigate(link.id)}
          >
            <Icon name={link.icon} />
            <span>{link.label}</span>
          </Button>
        ))}
      </nav>
      <div className="sidebar-bottom">
        {user ? (
          <>
            <div className="mini-user">
              <img src={user.avatarUrl || 'https://i.pravatar.cc/80?img=11'} alt="" />
              <span>
                <b>{user.username}</b>
                <small>Đang hoạt động</small>
              </span>
            </div>
            <Button onClick={onLogout}>
              <Icon name="logout" />
              <span>Đăng xuất</span>
            </Button>
          </>
        ) : (
          <>
            <p>Đăng nhập để theo dõi các nhà sáng tạo bạn yêu thích.</p>
            <Button className="login-side" onClick={onLogin}>
              Đăng nhập
            </Button>
          </>
        )}
      </div>
    </aside>
  );
}
