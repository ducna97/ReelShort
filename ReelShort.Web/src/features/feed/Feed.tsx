import { useEffect, useState } from 'react';
import Button from '../../components/common/Button';
import Icon from '../../components/common/Icon';
import ReelActions from './ReelActions';
import { videos } from './videoData';
export default function Feed({ user, onLogin }) {
  const [index, setIndex] = useState(0),
    video = videos[index];
  const move = (direction) =>
    setIndex((value) => (value + direction + videos.length) % videos.length);
  useEffect(() => {
    const keys = (e) => {
      if (e.key === 'ArrowDown') move(1);
      if (e.key === 'ArrowUp') move(-1);
    };
    window.addEventListener('keydown', keys);
    return () => window.removeEventListener('keydown', keys);
  }, []);
  return (
    <main className="feed-wrap">
      <div className={`reel ${video.bg}`}>
        <div className="reel-noise" />
        <div className="feed-tabs">
          <b>Đang theo dõi</b>
          <strong>Dành cho bạn</strong>
        </div>
        <Button className="sound">♫ &nbsp; Âm thanh gốc · {video.name}</Button>
        <div className="reel-info">
          <div className="creator">
            <img src={video.avatar} alt="" />
            <div>
              <b>@{video.user}</b>
              <span>{video.name}</span>
            </div>
            <Button className="follow" onClick={onLogin}>
              {user ? 'Đang theo dõi' : 'Theo dõi'}
            </Button>
          </div>
          <p>{video.caption}</p>
          <p className="tags">{video.tag}</p>
          <small>♫ Âm thanh gốc - ReelShort</small>
        </div>
        <ReelActions video={video} />
        <Button className="scroll-control up" onClick={() => move(-1)}>
          ⌃
        </Button>
        <Button className="scroll-control down" onClick={() => move(1)}>
          ⌄
        </Button>
        <div className="play-hint">
          <Icon name="play" size={18} />
        </div>
      </div>
    </main>
  );
}
