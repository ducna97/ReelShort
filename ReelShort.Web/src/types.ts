export type Page = 'home' | 'discover' | 'profile';

export interface User {
  id: string;
  username: string;
  email: string;
  avatarUrl?: string;
  accessToken?: string;
  refreshToken?: string;
}

export interface Credentials {
  email: string;
  password: string;
}

export interface RegisterCredentials extends Credentials {
  username: string;
}

export interface Video {
  id: number;
  user: string;
  name: string;
  avatar: string;
  caption: string;
  tag: string;
  likes: string;
  comments: string;
  shares: string;
  bg: 'sunset' | 'city' | 'ocean';
}
