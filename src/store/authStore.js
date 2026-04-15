import { create } from 'zustand';
import { persist } from 'zustand/middleware';

const mockUsers = {
  'admin@college.edu': { id: '1', name: 'Dr. Rajesh Kumar', email: 'admin@college.edu', role: 'admin' },
  'faculty@college.edu': { id: '2', name: 'Prof. Anita Sharma', email: 'faculty@college.edu', role: 'faculty' },
  'student@college.edu': { id: '3', name: 'Rahul Verma', email: 'student@college.edu', role: 'student' },
};

export const useAuthStore = create(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      login: async (email, _password) => {
        await new Promise(r => setTimeout(r, 800));
        const user = mockUsers[email];
        if (user) {
          set({ user, isAuthenticated: true });
          return true;
        }
        return false;
      },
      logout: () => set({ user: null, isAuthenticated: false }),
      setRole: (role) => {
        const user = Object.values(mockUsers).find(u => u.role === role);
        if (user) set({ user, isAuthenticated: true });
      },
    }),
    { name: 'auth-storage' }
  )
);
