export interface Club {
  id: number;
  name: string;
  description?: string | null;
  category?: string | null;
  contact?: string | null;
  logoUrl?: string | null;
  status?: string;
  createdAt?: string;
}

export interface ClubOption {
  id: number;
  name: string;
}

export interface User {
  id: number;
  name: string;
  email: string;
  role: string;
}

export interface Profile {
  id: number;
  name: string;
  email: string;
  role: string;
  faculty?: string | null;
  department?: string | null;
  bio?: string | null;
  avatarUrl?: string | null;
}

export interface ClubApplication {
  id: number;
  clubId: number;
  clubName: string;
  status: string;
  note?: string | null;
  qrCodeBase64?: string | null;
  createdAt: string;
}

export interface EventItem {
  id: number;
  clubId: number;
  clubName: string;
  title: string;
  description?: string | null;
  location?: string | null;
  startAt: string;
  endAt: string;
  capacity: number;
  status: string;
}

export interface AdminEvent {
  id: number;
  clubId: number;
  title: string;
  description?: string | null;
  location?: string | null;
  startAt: string;
  endAt: string;
  capacity: number;
  status: string;
}

export interface Announcement {
  id: number;
  clubId: number;
  title: string;
  content: string;
  status: string;
  createdAt: string;
}

export interface NotificationItem {
  id: number;
  type: string;
  payload: Record<string, unknown>;
  createdAt: string;
  readAt?: string | null;
}

export interface EventRegistration {
  id: number;
  eventId: number;
  status: string;
  createdAt: string;
}

export interface AdminRegistration {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  status: string;
  createdAt: string;
}

export interface SksClubApplication {
  id: number;
  clubId: number;
  clubName: string;
  submittedByUserId: number;
  submittedByName: string;
  submittedByEmail: string;
  status: string;
  reviewNote?: string | null;
  createdAt: string;
  reviewedAt?: string | null;
}

export interface SksEventDocument {
  id: number;
  eventId: number;
  eventTitle: string;
  clubId: number;
  clubName: string;
  fileName: string;
  status: string;
  reviewNote?: string | null;
  createdAt: string;
  reviewedAt?: string | null;
}

export interface CommentItem {
  id: number;
  userId: number;
  userName: string;
  body: string;
  createdAt: string;
  status: string;
}

export interface ReactionSummary {
  total: number;
  liked: boolean;
}

export interface ViewStat {
  entityId: number;
  name: string;
  total: number;
  unique: number;
}

export interface ClubStats {
  clubId: number;
  clubName: string;
  eventCount: number;
  registrationCount: number;
}

export interface DauPoint {
  date: string;
  count: number;
}

export interface AdminStats {
  clubs: ClubStats[];
  dauTrend: DauPoint[];
  views: {
    clubs: ViewStat[];
    events: ViewStat[];
  };
  totalRegistrations: number;
}
