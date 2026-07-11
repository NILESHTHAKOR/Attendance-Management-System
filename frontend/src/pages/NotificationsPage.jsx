import { useAppStore } from '@/store/appStore';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { AlertTriangle, Info, CheckCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

const iconMap = { warning: AlertTriangle, danger: AlertTriangle, info: Info, success: CheckCircle };
const styleMap = {
  warning: 'border-warning/30 bg-warning/5',
  danger: 'border-destructive/30 bg-destructive/5',
  info: 'border-info/30 bg-info/5',
  success: 'border-success/30 bg-success/5',
};
const iconStyle = {
  warning: 'text-warning',
  danger: 'text-destructive',
  info: 'text-info',
  success: 'text-success',
};

const NotificationsPage = () => {
  const { notifications, markNotificationRead } = useAppStore();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Notifications</h2>
          <p className="text-sm text-muted-foreground">{notifications.filter(n => !n.read).length} unread</p>
        </div>
        <Button variant="outline" size="sm" onClick={() => notifications.forEach(n => markNotificationRead(n.id))}>
          Mark all read
        </Button>
      </div>

      <div className="space-y-3">
        {notifications.map(n => {
          const Icon = iconMap[n.type];
          return (
            <Card key={n.id} className={cn('transition-all', !n.read && styleMap[n.type])}>
              <CardContent className="flex items-start gap-3 p-4">
                <Icon className={cn('h-5 w-5 mt-0.5 shrink-0', iconStyle[n.type])} />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-semibold">{n.title}</p>
                    {!n.read && <div className="h-2 w-2 rounded-full bg-primary" />}
                  </div>
                  <p className="text-sm text-muted-foreground">{n.message}</p>
                  <p className="text-xs text-muted-foreground mt-1">{new Date(n.createdAt).toLocaleDateString()}</p>
                </div>
                {!n.read && (
                  <Button variant="ghost" size="sm" onClick={() => markNotificationRead(n.id)}>
                    Mark read
                  </Button>
                )}
              </CardContent>
            </Card>
          );
        })}
      </div>
    </div>
  );
};

export default NotificationsPage;
