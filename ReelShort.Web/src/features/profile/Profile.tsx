import Button from '../../components/common/Button';
import Icon from '../../components/common/Icon';
import { videos } from '../feed/videoData';
export default function Profile({ user, onLogin }) {
  const current = user || { username: 'thuytrang', avatarUrl: 'https://i.pravatar.cc/180?img=47' };
  return (
    <main className="profile">
      <section className="profile-header">
        <img className="profile-avatar" src={current.avatarUrl} alt="" />
        <div>
          <h1>@{current.username}</h1>
          <p>{user?.email || 'Những khoảnh khắc nhỏ tạo nên câu chuyện lớn.'}</p>
          <div className="profile-stats">
            <span>
              <b>24</b> Đang theo dõi
            </span>
            <span>
              <b>15.8K</b> Người theo dõi
            </span>
            <span>
              <b>231.4K</b> Lượt thích
            </span>
          </div>
          <Button className="edit-profile" onClick={onLogin}>
            {user ? 'Chỉnh sửa hồ sơ' : 'Đăng nhập để theo dõi'}
          </Button>
        </div>
      </section>
      <div className="profile-tabs">
        <b>Video</b>
        <span>Đã thích</span>
      </div>
      <section className="grid">
        {videos.concat(videos).map((video, index) => (
          <article className={`tile ${video.bg}`} key={`${video.id}-${index}`}>
            <div className="tile-bottom">
              <Icon name="play" size={16} />
              <b>{video.likes}</b>
            </div>
          </article>
        ))}
      </section>
    </main>
  );
}
