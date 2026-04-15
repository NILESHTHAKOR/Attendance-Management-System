import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';
import { useAppStore } from '@/store/appStore';
import { cn } from '@/lib/utils';
import {
  LayoutDashboard, Users, BookOpen, CalendarCheck, AlertTriangle,
  BarChart3, Bell, Settings, LogOut, Menu, X, GraduationCap,
  ChevronLeft, FileText, Shield, ClipboardList
} from 'lucide-react';
import { Button } from '@/components/ui/button';

const navItems = [
  { label: 'Dashboard', path: '/dashboard', icon: LayoutDashboard, roles: ['admin', 'faculty', 'student'] },
  { label: 'Students', path: '/students', icon: Users, roles: ['admin', 'faculty'] },
  { label: 'Classes', path: '/classes', icon: BookOpen, roles: ['admin', 'faculty'] },
  { label: 'Attendance', path: '/attendance', icon: CalendarCheck, roles: ['admin', 'faculty'] },
  { label: 'My Attendance', path: '/my-attendance', icon: ClipboardList, roles: ['student'] },
  { label: 'Blacklist', path: '/blacklist', icon: AlertTriangle, roles: ['admin', 'faculty'] },
  { label: 'Reports', path: '/reports', icon: BarChart3, roles: ['admin', 'faculty'] },
  { label: 'Notifications', path: '/notifications', icon: Bell, roles: ['admin', 'faculty', 'student'] },
  { label: 'Audit Logs', path: '/audit-logs', icon: FileText, roles: ['admin'] },
  { label: 'Settings', path: '/settings', icon: Settings, roles: ['admin'] },
];

export const AppSidebar = () => {
  const { user, logout } = useAuthStore();
  const { sidebarOpen, toggleSidebar } = useAppStore();
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);

  const filteredItems = navItems.filter(item => user && item.roles.includes(user.role));
  const unreadCount = useAppStore(s => s.notifications.filter(n => !n.read).length);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const sidebarContent = (
    <div className="flex flex-col h-full">
      <div className="flex items-center gap-3 px-4 py-5 border-b border-sidebar-border">
        <div className="rounded-lg bg-sidebar-primary p-2">
          <GraduationCap className="h-5 w-5 text-sidebar-primary-foreground" />
        </div>
        {sidebarOpen && (
          <div className="animate-fade-in">
            <h1 className="text-sm font-bold text-sidebar-primary-foreground">AttendanceMS</h1>
            <p className="text-[10px] text-sidebar-foreground/60">Management System</p>
          </div>
        )}
        <Button variant="ghost" size="icon" onClick={toggleSidebar} className="ml-auto hidden lg:flex text-sidebar-foreground hover:bg-sidebar-accent">
          <ChevronLeft className={cn('h-4 w-4 transition-transform', !sidebarOpen && 'rotate-180')} />
        </Button>
      </div>

      <nav className="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        {filteredItems.map(item => {
          const active = location.pathname === item.path;
          return (
            <Link
              key={item.path}
              to={item.path}
              onClick={() => setMobileOpen(false)}
              className={cn(
                'flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors',
                active
                  ? 'bg-sidebar-primary text-sidebar-primary-foreground'
                  : 'text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground'
              )}
            >
              <item.icon className="h-4 w-4 shrink-0" />
              {sidebarOpen && <span className="animate-fade-in">{item.label}</span>}
              {item.label === 'Notifications' && unreadCount > 0 && sidebarOpen && (
                <span className="ml-auto rounded-full bg-destructive px-2 py-0.5 text-[10px] font-bold text-destructive-foreground">
                  {unreadCount}
                </span>
              )}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-sidebar-border p-3">
        {sidebarOpen && user && (
          <div className="flex items-center gap-3 px-3 py-2 mb-2 animate-fade-in">
            <div className="rounded-full bg-sidebar-primary/20 p-2">
              <Shield className="h-3.5 w-3.5 text-sidebar-primary" />
            </div>
            <div className="min-w-0">
              <p className="text-xs font-semibold text-sidebar-accent-foreground truncate">{user.name}</p>
              <p className="text-[10px] text-sidebar-foreground/60 capitalize">{user.role}</p>
            </div>
          </div>
        )}
        <button
          onClick={handleLogout}
          className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-sidebar-foreground hover:bg-destructive/10 hover:text-destructive transition-colors"
        >
          <LogOut className="h-4 w-4 shrink-0" />
          {sidebarOpen && <span>Logout</span>}
        </button>
      </div>
    </div>
  );

  return (
    <>
      <button
        onClick={() => setMobileOpen(true)}
        className="fixed top-4 left-4 z-50 lg:hidden rounded-lg bg-card border p-2 shadow-md"
      >
        <Menu className="h-5 w-5" />
      </button>

      {mobileOpen && (
        <div className="fixed inset-0 z-50 lg:hidden">
          <div className="absolute inset-0 bg-foreground/20 backdrop-blur-sm" onClick={() => setMobileOpen(false)} />
          <aside className="absolute left-0 top-0 h-full w-64 bg-sidebar-background animate-slide-in-left">
            <button onClick={() => setMobileOpen(false)} className="absolute top-4 right-4 text-sidebar-foreground">
              <X className="h-5 w-5" />
            </button>
            {sidebarContent}
          </aside>
        </div>
      )}

      <aside className={cn(
        'hidden lg:flex flex-col h-screen bg-sidebar-background border-r border-sidebar-border transition-all duration-300 sticky top-0',
        sidebarOpen ? 'w-60' : 'w-16'
      )}>
        {sidebarContent}
      </aside>
    </>
  );
};
