import { useState } from 'react';
import Button from '../../components/common/Button';
import Icon from '../../components/common/Icon';
export default function ReelActions({ video }) {
  const [liked, setLiked] = useState(false);
  return (
    <div className="actions">
      <Button onClick={() => setLiked(!liked)} className={liked ? 'liked' : ''}>
        <span className="action-icon">
          <Icon name="heart" fill={liked ? 'currentColor' : 'none'} />
        </span>
        <b>{liked ? 'Đã thích' : video.likes}</b>
      </Button>
      <Button>
        <span className="action-icon">
          <Icon name="comment" />
        </span>
        <b>{video.comments}</b>
      </Button>
      <Button>
        <span className="action-icon">
          <Icon name="bookmark" />
        </span>
        <b>Lưu</b>
      </Button>
      <Button>
        <span className="action-icon">
          <Icon name="share" />
        </span>
        <b>{video.shares}</b>
      </Button>
    </div>
  );
}
