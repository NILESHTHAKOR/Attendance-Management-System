import { create } from 'zustand';

const mockNotifications = [
  { id: '1', title: 'Low Attendance Alert', message: '12 students below 75% in BCA Sem 4', type: 'warning', read: false, createdAt: '2026-04-13T10:00:00Z' },
  { id: '2', title: 'Blacklist Update', message: '3 students added to blacklist in MCA Sem 2', type: 'danger', read: false, createdAt: '2026-04-12T14:00:00Z' },
  { id: '3', title: 'New Semester Started', message: 'April 2026 semester classes are now active', type: 'info', read: true, createdAt: '2026-04-01T09:00:00Z' },
  { id: '4', title: 'Attendance Synced', message: 'Offline attendance data synced successfully', type: 'success', read: true, createdAt: '2026-03-30T16:00:00Z' },
];

export const useAppStore = create((set) => ({
  sidebarOpen: true,
  darkMode: false,
  notifications: mockNotifications,
  settings: { warningThreshold: 75, blacklistThreshold: 50 },
  toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),
  toggleDarkMode: () => {
    set((s) => {
      const next = !s.darkMode;
      document.documentElement.classList.toggle('dark', next);
      return { darkMode: next };
    });
  },
  markNotificationRead: (id) => set((s) => ({
    notifications: s.notifications.map(n => n.id === id ? { ...n, read: true } : n),
  })),
  updateSettings: (newSettings) => set((s) => ({ settings: { ...s.settings, ...newSettings } })),
}));
