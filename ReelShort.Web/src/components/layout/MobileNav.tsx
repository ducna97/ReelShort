import Button from '../common/Button';
import Icon from '../common/Icon';
export default function MobileNav({ onNavigate, onCreate }) {
  return (
    <footer className="mobile-nav">
      <Button onClick={() => onNavigate('home')}>
        <Icon name="home" />
        Trang chủ
      </Button>
      <Button className="create" onClick={onCreate}>
        <Icon name="plus" />
      </Button>
      <Button onClick={() => onNavigate('profile')}>
        <Icon name="user" />
        Hồ sơ
      </Button>
    </footer>
  );
}
