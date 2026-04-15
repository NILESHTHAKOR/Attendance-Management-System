import { useAppStore } from '@/store/appStore';
import { useAuthStore } from '@/store/authStore';
import { Bell, Moon, Sun, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useNavigate } from 'react-router-dom';

export const TopNavbar = () => {
  const { darkMode, toggleDarkMode, notifications } = useAppStore();
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const unreadCount = notifications.filter(n => !n.read).length;

  return (
    <header className="sticky top-0 z-40 flex h-14 items-center gap-4 border-b bg-card/80 backdrop-blur-md px-4 lg:px-6">
      <div className="flex-1 lg:ml-0 ml-12">
        <div className="relative max-w-sm">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input placeholder="Search students, classes..." className="pl-9 h-9 bg-muted/50 border-0" />
        </div>
      </div>
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={toggleDarkMode} className="h-9 w-9">
          {darkMode ? <Sun className="h-4 w-4" /> : <Moon className="h-4 w-4" />}
        </Button>
        <Button variant="ghost" size="icon" className="h-9 w-9 relative" onClick={() => navigate('/notifications')}>
          <Bell className="h-4 w-4" />
          {unreadCount > 0 && (
            <span className="absolute -top-0.5 -right-0.5 h-4 w-4 rounded-full bg-destructive text-[10px] font-bold text-destructive-foreground flex items-center justify-center">
              {unreadCount}
            </span>
          )}
        </Button>
        <div className="hidden sm:flex items-center gap-2 ml-2 pl-2 border-l">
          <div className="rounded-full bg-primary/10 h-8 w-8 flex items-center justify-center">
            <span className="text-xs font-bold text-primary">{user?.name?.charAt(0)}</span>
          </div>
          <div className="hidden md:block">
            <p className="text-xs font-semibold">{user?.name}</p>
            <p className="text-[10px] text-muted-foreground capitalize">{user?.role}</p>
          </div>
        </div>
      </div>
    </header>
  );
};
